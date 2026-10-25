-- =============================================================
-- 01_create_database.sql
-- 建立資料庫 device_lending
-- 執行方式：連到預設的 postgres 資料庫，開 Query Tool 執行本檔
-- 執行完後把連線切換到 device_lending，再依序執行 02 → 03 → 04
-- 注意：若資料庫已存在會出現 already exists 錯誤，
--       需要重建時請先刪除 device_lending 資料庫，或直接重跑 02 ～ 04
-- =============================================================

CREATE DATABASE device_lending
    WITH ENCODING   = 'UTF8'
         TEMPLATE   = template0
         LC_COLLATE = 'C'
         LC_CTYPE   = 'C';
