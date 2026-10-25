-- =============================================================
-- 04_procedures.sql
-- API 使用的異動預存程序 (PROCEDURE)
-- 執行方式：連到 device_lending 資料庫，開 Query Tool 執行本檔
--
-- 設計說明
--   * 新增 / 修改 / 刪除 / 借用 / 歸還使用 PROCEDURE，以 CALL sp_xxx(...) 呼叫，
--     需要回傳的值（新增後的 ID、歸還時間）透過 INOUT 參數帶回
--   * 查詢由後端以 Npgsql 撰寫 SQL 實作，不在資料庫建立查詢函式
--   * 一次 CALL 即在一個交易內執行，失敗會整體回滾；
--     借用 / 歸還 / 修改 / 刪除以 SELECT ... FOR UPDATE 鎖定資料列，避免併發重複借出
--   * 沒有 update_time trigger，UPDATE 時需自行寫入 update_time = now()
--
-- 自訂錯誤代碼（SQLSTATE），後端可依此對應 HTTP 狀態碼：
--   DL400  參數不合法                          → 400 Bad Request
--   DL404  資料不存在或已刪除                  → 404 Not Found
--   DL409  狀態衝突（借用中、已歸還、編號重複）→ 409 Conflict
-- =============================================================

SET client_encoding = 'UTF8';

-- 清除舊版以 FUNCTION 實作的異動程式
DROP FUNCTION IF EXISTS fn_equipment_create(VARCHAR, VARCHAR, INTEGER);
DROP FUNCTION IF EXISTS fn_equipment_update(INTEGER, VARCHAR, VARCHAR, INTEGER);
DROP FUNCTION IF EXISTS fn_equipment_delete(INTEGER);
DROP FUNCTION IF EXISTS fn_borrow_record_create(INTEGER, VARCHAR, TIMESTAMPTZ, TIMESTAMPTZ);
DROP FUNCTION IF EXISTS fn_borrow_record_return(INTEGER, TIMESTAMPTZ);

-- -------------------------------------------------------------
-- POST /api/equipment  新增設備
--   p_id：傳入 NULL，執行後帶回新增的設備 ID
--   CALL sp_equipment_create('NB-2025-001', 'MacBook Pro', 1, NULL);
-- -------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_equipment_create(
    p_asset_code        VARCHAR,
    p_name              VARCHAR,
    p_equipment_type_id INTEGER,
    INOUT p_id          INTEGER DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF coalesce(btrim(p_asset_code), '') = '' THEN
        RAISE EXCEPTION '財產編號不可為空' USING ERRCODE = 'DL400';
    END IF;

    IF coalesce(btrim(p_name), '') = '' THEN
        RAISE EXCEPTION '設備名稱不可為空' USING ERRCODE = 'DL400';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM equipment_type t WHERE t.id = p_equipment_type_id) THEN
        RAISE EXCEPTION '設備類型不存在：%', p_equipment_type_id USING ERRCODE = 'DL404';
    END IF;

    BEGIN
        INSERT INTO equipment (asset_code, name, equipment_type_id)
        VALUES (btrim(p_asset_code), btrim(p_name), p_equipment_type_id)
        RETURNING equipment.id INTO p_id;
    EXCEPTION
        WHEN unique_violation THEN
            RAISE EXCEPTION '財產編號已存在：%', btrim(p_asset_code) USING ERRCODE = 'DL409';
    END;
END;
$$;

-- -------------------------------------------------------------
-- PUT /api/equipment/{id}  修改設備
--   CALL sp_equipment_update(1, 'NB-2024-001', 'ThinkPad T14', 1);
-- -------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_equipment_update(
    p_id                INTEGER,
    p_asset_code        VARCHAR,
    p_name              VARCHAR,
    p_equipment_type_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM 1
       FROM equipment e
      WHERE e.id = p_id
        AND NOT e.is_archive
        FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION '設備不存在或已刪除：%', p_id USING ERRCODE = 'DL404';
    END IF;

    IF coalesce(btrim(p_asset_code), '') = '' THEN
        RAISE EXCEPTION '財產編號不可為空' USING ERRCODE = 'DL400';
    END IF;

    IF coalesce(btrim(p_name), '') = '' THEN
        RAISE EXCEPTION '設備名稱不可為空' USING ERRCODE = 'DL400';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM equipment_type t WHERE t.id = p_equipment_type_id) THEN
        RAISE EXCEPTION '設備類型不存在：%', p_equipment_type_id USING ERRCODE = 'DL404';
    END IF;

    BEGIN
        UPDATE equipment e
           SET asset_code        = btrim(p_asset_code),
               name              = btrim(p_name),
               equipment_type_id = p_equipment_type_id,
               update_time       = now()
         WHERE e.id = p_id;
    EXCEPTION
        WHEN unique_violation THEN
            RAISE EXCEPTION '財產編號已存在：%', btrim(p_asset_code) USING ERRCODE = 'DL409';
    END;
END;
$$;

-- -------------------------------------------------------------
-- DELETE /api/equipment/{id}  刪除設備（軟刪除）
--   借用中的設備不可刪除；歷史借用紀錄保留不動
--   CALL sp_equipment_delete(1);
-- -------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_equipment_delete(
    p_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM 1
       FROM equipment e
      WHERE e.id = p_id
        AND NOT e.is_archive
        FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION '設備不存在或已刪除：%', p_id USING ERRCODE = 'DL404';
    END IF;

    IF EXISTS (
        SELECT 1
          FROM borrow_record br
         WHERE br.equipment_id = p_id
           AND br.return_time IS NULL
    ) THEN
        RAISE EXCEPTION '設備借用中，無法刪除：%', p_id USING ERRCODE = 'DL409';
    END IF;

    UPDATE equipment e
       SET is_archive  = true,
           update_time = now()
     WHERE e.id = p_id;
END;
$$;

-- -------------------------------------------------------------
-- POST /api/equipment/borrowRecord  新增借用紀錄
--   p_borrow_time：傳 NULL 時以目前時間為借出時間
--   p_id：傳入 NULL，執行後帶回新增的借用紀錄 ID
--   CALL sp_borrow_record_create(1, '王小明', now() + interval '7 days', NULL, NULL);
-- -------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_borrow_record_create(
    p_equipment_id       INTEGER,
    p_borrower_name      VARCHAR,
    p_expect_return_time TIMESTAMPTZ,
    p_borrow_time        TIMESTAMPTZ DEFAULT NULL,
    INOUT p_id           INTEGER     DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_borrow_time TIMESTAMPTZ := coalesce(p_borrow_time, now());
BEGIN
    -- 鎖住設備，避免同一台設備被同時借出
    PERFORM 1
       FROM equipment e
      WHERE e.id = p_equipment_id
        AND NOT e.is_archive
        FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION '設備不存在或已刪除：%', p_equipment_id USING ERRCODE = 'DL404';
    END IF;

    IF coalesce(btrim(p_borrower_name), '') = '' THEN
        RAISE EXCEPTION '借用人姓名不可為空' USING ERRCODE = 'DL400';
    END IF;

    IF p_expect_return_time IS NULL OR p_expect_return_time <= v_borrow_time THEN
        RAISE EXCEPTION '預計歸還時間必須晚於借出時間' USING ERRCODE = 'DL400';
    END IF;

    IF EXISTS (
        SELECT 1
          FROM borrow_record br
         WHERE br.equipment_id = p_equipment_id
           AND br.return_time IS NULL
    ) THEN
        RAISE EXCEPTION '設備借用中，無法借出：%', p_equipment_id USING ERRCODE = 'DL409';
    END IF;

    INSERT INTO borrow_record (equipment_id, borrower_name, borrow_time, expect_return_time)
    VALUES (p_equipment_id, btrim(p_borrower_name), v_borrow_time, p_expect_return_time)
    RETURNING borrow_record.id INTO p_id;
END;
$$;

-- -------------------------------------------------------------
-- POST /api/equipment/borrowRecords/{id}/return  歸還設備
--   p_return_time：傳 NULL 時以目前時間為歸還時間，執行後帶回實際歸還時間
--   CALL sp_borrow_record_return(1, NULL);
-- -------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_borrow_record_return(
    p_id                INTEGER,
    INOUT p_return_time TIMESTAMPTZ DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_borrow_time TIMESTAMPTZ;
    v_old_return  TIMESTAMPTZ;
BEGIN
    SELECT br.borrow_time, br.return_time
      INTO v_borrow_time, v_old_return
      FROM borrow_record br
     WHERE br.id = p_id
       FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION '借用紀錄不存在：%', p_id USING ERRCODE = 'DL404';
    END IF;

    IF v_old_return IS NOT NULL THEN
        RAISE EXCEPTION '此借用紀錄已歸還：%', p_id USING ERRCODE = 'DL409';
    END IF;

    p_return_time := coalesce(p_return_time, now());

    IF p_return_time < v_borrow_time THEN
        RAISE EXCEPTION '歸還時間不可早於借出時間' USING ERRCODE = 'DL400';
    END IF;

    UPDATE borrow_record br
       SET return_time = p_return_time,
           update_time = now()
     WHERE br.id = p_id;
END;
$$;
