-- =============================================================
-- 02_ddl.sql
-- 建立資料表、約束、索引
-- 執行方式：連到 device_lending 資料庫，開 Query Tool 執行本檔
-- 注意：會先刪除既有的資料表（包含所有資料）
-- =============================================================

SET client_encoding = 'UTF8';

COMMENT ON DATABASE device_lending IS 'IT 設備借用系統';

-- 預設時區設為台北，TIMESTAMPTZ 顯示時會轉為 +08:00（重新連線後生效）
ALTER DATABASE device_lending SET timezone TO 'Asia/Taipei';

-- 清除舊版物件（舊版曾建立查詢用的 view、function 與 update_time trigger）
DROP FUNCTION IF EXISTS fn_equipment_type_list();
DROP FUNCTION IF EXISTS fn_equipment_list(INTEGER, VARCHAR);
DROP FUNCTION IF EXISTS fn_equipment_get(INTEGER);
DROP FUNCTION IF EXISTS fn_borrow_record_list(INTEGER, VARCHAR, VARCHAR);
DROP FUNCTION IF EXISTS fn_borrow_record_get(INTEGER);
DROP VIEW     IF EXISTS v_borrow_record CASCADE;
DROP VIEW     IF EXISTS v_equipment     CASCADE;

-- 刪除資料表（先刪參照別人的，再刪被參照的）
DROP TABLE IF EXISTS borrow_record  CASCADE;
DROP TABLE IF EXISTS equipment      CASCADE;
DROP TABLE IF EXISTS equipment_type CASCADE;

DROP FUNCTION IF EXISTS trg_set_update_time() CASCADE;

-- -------------------------------------------------------------
-- equipment_type 設備類型
-- -------------------------------------------------------------
CREATE TABLE equipment_type (
    id   SMALLINT    GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(50) NOT NULL,

    CONSTRAINT pk_equipment_type          PRIMARY KEY (id),
    CONSTRAINT uq_equipment_type_name     UNIQUE (name),
    CONSTRAINT ck_equipment_type_name     CHECK (btrim(name) <> '')
);

COMMENT ON TABLE  equipment_type      IS '設備類型';
COMMENT ON COLUMN equipment_type.id   IS '類型 ID';
COMMENT ON COLUMN equipment_type.name IS '類型名稱，例如「筆記型電腦」';

-- -------------------------------------------------------------
-- equipment 設備
-- -------------------------------------------------------------
CREATE TABLE equipment (
    id                INTEGER      GENERATED ALWAYS AS IDENTITY,
    asset_code        VARCHAR(30)  NOT NULL,
    name              VARCHAR(100) NOT NULL,
    equipment_type_id SMALLINT     NOT NULL,
    is_archive        BOOLEAN      NOT NULL DEFAULT false,
    create_time       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    update_time       TIMESTAMPTZ  NOT NULL DEFAULT now(),

    CONSTRAINT pk_equipment                PRIMARY KEY (id),
    CONSTRAINT uq_equipment_asset_code     UNIQUE (asset_code),
    CONSTRAINT fk_equipment_equipment_type FOREIGN KEY (equipment_type_id)
        REFERENCES equipment_type (id) ON DELETE RESTRICT,
    CONSTRAINT ck_equipment_asset_code     CHECK (btrim(asset_code) <> ''),
    CONSTRAINT ck_equipment_name           CHECK (btrim(name) <> '')
);

CREATE INDEX ix_equipment_equipment_type_id ON equipment (equipment_type_id);

COMMENT ON TABLE  equipment                   IS '設備';
COMMENT ON COLUMN equipment.id                IS '設備 ID';
COMMENT ON COLUMN equipment.asset_code        IS '財產編號';
COMMENT ON COLUMN equipment.name              IS '設備名稱';
COMMENT ON COLUMN equipment.equipment_type_id IS '設備類型';
COMMENT ON COLUMN equipment.is_archive        IS '設備是否刪除(軟刪除)';
COMMENT ON COLUMN equipment.create_time       IS '建立時間';
COMMENT ON COLUMN equipment.update_time       IS '更新時間';

-- -------------------------------------------------------------
-- borrow_record 借用紀錄
-- -------------------------------------------------------------
CREATE TABLE borrow_record (
    id                 INTEGER     GENERATED ALWAYS AS IDENTITY,
    equipment_id       INTEGER     NOT NULL,
    borrower_name      VARCHAR(50) NOT NULL,
    borrow_time        TIMESTAMPTZ NOT NULL,
    expect_return_time TIMESTAMPTZ NOT NULL,
    return_time        TIMESTAMPTZ NULL,
    create_time        TIMESTAMPTZ NOT NULL DEFAULT now(),
    update_time        TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT pk_borrow_record                    PRIMARY KEY (id),
    CONSTRAINT fk_borrow_record_equipment          FOREIGN KEY (equipment_id)
        REFERENCES equipment (id) ON DELETE RESTRICT,
    CONSTRAINT ck_borrow_record_borrower_name      CHECK (btrim(borrower_name) <> ''),
    CONSTRAINT ck_borrow_record_expect_return_time CHECK (expect_return_time > borrow_time),
    CONSTRAINT ck_borrow_record_return_time        CHECK (return_time IS NULL OR return_time >= borrow_time)
);

CREATE INDEX ix_borrow_record_equipment_id ON borrow_record (equipment_id);
CREATE INDEX ix_borrow_record_borrow_time  ON borrow_record (borrow_time DESC);

-- 同一台設備同時間只能有一筆「借用中」的紀錄
CREATE UNIQUE INDEX ux_borrow_record_active
    ON borrow_record (equipment_id)
    WHERE return_time IS NULL;

COMMENT ON TABLE  borrow_record                    IS '借用紀錄';
COMMENT ON COLUMN borrow_record.id                 IS '紀錄 ID';
COMMENT ON COLUMN borrow_record.equipment_id       IS '借用設備';
COMMENT ON COLUMN borrow_record.borrower_name      IS '借用人姓名';
COMMENT ON COLUMN borrow_record.borrow_time        IS '實際借出時間';
COMMENT ON COLUMN borrow_record.expect_return_time IS '預計歸還時間';
COMMENT ON COLUMN borrow_record.return_time        IS '實際歸還時間，NULL 代表借用中';
COMMENT ON COLUMN borrow_record.create_time        IS '系統建立紀錄的時間';
COMMENT ON COLUMN borrow_record.update_time        IS '更新時間';
