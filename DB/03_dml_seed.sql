-- =============================================================
-- 03_dml_seed.sql
-- 初始資料 / 測試資料
-- 執行方式：連到 device_lending 資料庫，開 Query Tool 執行本檔
-- 注意：會先清空三張資料表並重設 ID
-- =============================================================

SET client_encoding = 'UTF8';

BEGIN;

TRUNCATE TABLE borrow_record, equipment, equipment_type RESTART IDENTITY CASCADE;

-- -------------------------------------------------------------
-- 設備類型
-- -------------------------------------------------------------
INSERT INTO equipment_type (name) VALUES
    ('筆記型電腦'),
    ('桌上型電腦'),
    ('螢幕'),
    ('平板電腦'),
    ('投影機'),
    ('周邊設備');

-- -------------------------------------------------------------
-- 設備
-- -------------------------------------------------------------
INSERT INTO equipment (asset_code, name, equipment_type_id, is_archive)
SELECT v.asset_code, v.name, t.id, v.is_archive
  FROM (VALUES
        (1, 'NB-2024-001', 'Lenovo ThinkPad T14 Gen 4', '筆記型電腦', false),
        (2, 'NB-2024-002', 'Dell Latitude 5440', '筆記型電腦', false),
        (3, 'NB-2023-001', 'ASUS ExpertBook B9', '筆記型電腦', true),
        (4, 'PC-2024-001', 'HP EliteDesk 800 G9', '桌上型電腦', false),
        (5, 'MON-2024-001', 'Dell P2423D 24 吋螢幕', '螢幕', false),
        (6, 'MON-2024-002', 'LG 27UP850 27 吋 4K 螢幕', '螢幕', false),
        (7, 'TAB-2024-001', 'Apple iPad Air 11 吋', '平板電腦', false),
        (8, 'PJ-2023-001', 'Epson EB-FH52 投影機', '投影機', false),
        (9, 'ACC-2024-001', 'Logitech MX Keys 無線鍵盤', '周邊設備', false),
        (10, 'ACC-2024-002', 'Jabra Evolve2 65 耳機', '周邊設備', false),
        (11, 'PC-2024-002', 'Lenovo ThinkCentre M70q', '桌上型電腦', false),
        (12, 'PJ-2024-001', 'BenQ MH560 投影機', '投影機', false)
       ) AS v (ord, asset_code, name, type_name, is_archive)
  JOIN equipment_type t
    ON t.name = v.type_name
 ORDER BY v.ord;

-- -------------------------------------------------------------
-- 借用紀錄（時間以執行當下為基準）
--   已歸還：第 1 ～ 8 筆（第 2、8 筆晚於預計時間歸還；第 5 筆為已刪除設備的歷史紀錄）
--   借用中：NB-2024-002、PJ-2024-001（未逾期）
--   逾期中：TAB-2024-001、ACC-2024-002
-- -------------------------------------------------------------
INSERT INTO borrow_record (equipment_id, borrower_name, borrow_time, expect_return_time, return_time)
SELECT e.id, v.borrower_name, v.borrow_time, v.expect_return_time, v.return_time
  FROM (VALUES
        (1, 'NB-2024-001', '王小明', now() - interval '30 days', now() - interval '23 days', now() - interval '24 days'),
        (2, 'MON-2024-001', '陳美華', now() - interval '20 days', now() - interval '13 days', now() - interval '12 days'),
        (3, 'PJ-2023-001', '林志豪', now() - interval '10 days', now() - interval '9 days', now() - interval '9 days'),
        (4, 'NB-2024-001', '張雅婷', now() - interval '8 days', now() - interval '5 days', now() - interval '5 days'),
        (5, 'NB-2023-001', '吳建宏', now() - interval '60 days', now() - interval '53 days', now() - interval '54 days'),
        (6, 'PC-2024-001', '劉怡君', now() - interval '45 days', now() - interval '38 days', now() - interval '40 days'),
        (7, 'ACC-2024-001', '蔡宗翰', now() - interval '25 days', now() - interval '18 days', now() - interval '19 days'),
        (8, 'MON-2024-002', '鄭佩珊', now() - interval '15 days', now() - interval '8 days', now() - interval '6 days'),
        (9, 'NB-2024-002', '李大同', now() - interval '2 days', now() + interval '5 days', NULL::TIMESTAMPTZ),
        (10, 'PJ-2024-001', '周俊傑', now() - interval '1 day', now() + interval '2 days', NULL::TIMESTAMPTZ),
        (11, 'TAB-2024-001', '黃淑芬', now() - interval '10 days', now() - interval '3 days', NULL::TIMESTAMPTZ),
        (12, 'ACC-2024-002', '許志明', now() - interval '7 days', now() - interval '1 day', NULL::TIMESTAMPTZ)
       ) AS v (ord, asset_code, borrower_name, borrow_time, expect_return_time, return_time)
  JOIN equipment e
    ON e.asset_code = v.asset_code
 ORDER BY v.ord;

COMMIT;
