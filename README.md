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

## API 說明

### 類型

| Method | URL | 說明 |
|--------|-----|------|
| GET | `/api/type/equipment` | 取得所有設備類型 |

### 設備

| Method | URL | 說明 |
|--------|-----|------|
| GET | `/api/equipment` | 查詢設備清單 |
| POST | `/api/equipment` | 新增設備 |
| PUT | `/api/equipment/{id}` | 修改設備 |
| DELETE | `/api/equipment/{id}` | 刪除設備（軟刪除）|

### 借用紀錄

| Method | URL | 說明 |
|--------|-----|------|
| GET | `/api/equipment/borrowRecord` | 查詢借用歷史 |
| POST | `/api/equipment/borrowRecord` | 新增借用紀錄 |
| POST | `/api/equipment/borrowRecords/{id}/return` | 歸還設備 |