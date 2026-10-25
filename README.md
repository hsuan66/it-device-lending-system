# IT 設備借用系統（IT Device Lending System）

## 系統介紹
這是一個簡易的資訊設備借用管理系統。管理者可以在網頁上：

1. 查看現有設備(依設備類型、狀態篩選設備)
2. 新增、修改、刪除可借用的設備
3. 新增借用紀錄
4. 歸還設備
5. 查看借用歷史

## 使用的技術

| 層級 | 技術 |
| --- | --- |
| 前端 | Vue 3 |
| 後端 | ASP.NET Core 10 |
| 資料庫 | PostgreSQL |

## 資料庫 Table 設計

### `equipment_type` 設備類型

| 欄位 | 型別 | 說明 |
|------|------|------|
| id | SMALLINT, PK, IDENTITY | 類型 ID |
| name | VARCHAR(50), NOT NULL, UNIQUE | 類型名稱，例如「筆記型電腦」 |

### `equipment` 設備

| 欄位 | 型別 | 說明 |
|------|------|------|
| id | INTEGER, PK, IDENTITY | 設備 ID |
| asset_code | VARCHAR(30), NOT NULL, UNIQUE | 財產編號 |
| name | VARCHAR(100), NOT NULL | 設備名稱 |
| equipment_type_id | SMALLINT, NOT NULL, FK → equipment_type | 設備類型 |
| is_archive | BOOLEAN, NOT NULL, DEFAULT false | 設備是否刪除(軟刪除) |
| create_time | TIMESTAMPTZ, NOT NULL, DEFAULT now() | 建立時間 |
| update_time | TIMESTAMPTZ, NOT NULL, DEFAULT now() | 更新時間 |


### `borrow_record` 借用紀錄

| 欄位 | 型別 | 說明 |
|------|------|------|
| id | INTEGER, PK, IDENTITY | 紀錄 ID |
| equipment_id | INTEGER, NOT NULL, FK → equipment | 借用設備 |
| borrower_name | VARCHAR(50), NOT NULL | 借用人姓名 |
| borrow_time | TIMESTAMPTZ, NOT NULL | 實際借出時間 |
| expect_return_time | TIMESTAMPTZ, NOT NULL | 預計歸還時間 |
| return_time | TIMESTAMPTZ, NULL | 實際歸還時間，NULL 代表借用中 |
| create_time | TIMESTAMPTZ, NOT NULL, DEFAULT now() | 系統建立紀錄的時間 |
| update_time | TIMESTAMPTZ, NOT NULL, DEFAULT now() | 更新時間 |

## 如何起動前端、後端、資料庫

### 資料庫

SQL 腳本放在 `DB/`：

| 檔案 | 內容 |
|------|------|
| `01_create_database.sql` | 建立資料庫 `device_lending` |
| `02_ddl.sql` | 資料表、約束、索引 |
| `03_dml_seed.sql` | 初始資料／測試資料 |
| `04_procedures.sql` | API 使用的異動預存程序（PROCEDURE） |

建置步驟（以 pgAdmin 為例，每個檔案打開後整份執行即可）：

1. 連上 `postgres` 資料庫，開 Query Tool 執行 `01_create_database.sql`
2. 切換到 `device_lending` 資料庫，依序執行 `02_ddl.sql` → `03_dml_seed.sql` → `04_procedures.sql`

之後只修改預存程序時，重跑 `04` 即可；想清空資料重新建立，重跑 `02` ～ `04`。

## API 說明

### 類型

| Method | URL | 說明 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/Type/Equipment` | 取得所有設備類型 | 後端 SQL |

### 設備

| Method | URL | 說明 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/Equipment` | 查詢設備清單 | 後端 SQL |
| POST | `/api/Equipment` | 新增設備 | `sp_equipment_create` |
| PUT | `/api/Equipment/{id}` | 修改設備 | `sp_equipment_update` |
| DELETE | `/api/Equipment/{id}` | 刪除設備（軟刪除）| `sp_equipment_delete` |

### 借用紀錄

| Method | URL | 說明 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/EquipmentBorrowRecord` | 查詢借用歷史 | 後端 SQL |
| POST | `/api/EquipmentBorrowRecord` | 新增借用紀錄 | `sp_borrow_record_create` |
| POST | `/api/EquipmentBorrowRecord/{id}/Return` | 歸還設備 | `sp_borrow_record_return` |