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
| 前端 | Vue 3、TypeScript、Vite、Element Plus、Vue Router |
| 後端 | ASP.NET Core 10 Web API、ADO.NET（Npgsql）、NLog |
| 資料庫 | PostgreSQL |

### 執行需求

執行前請先安裝以下軟體：

| 軟體 | 版本 | 用途 |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | 建置與執行後端 |
| [Node.js](https://nodejs.org/) | 22.18 以上或 24.12 以上 | 安裝套件與執行前端 |
| [PostgreSQL](https://www.postgresql.org/download/) | 18 | 資料庫 |
| [pgAdmin](https://www.pgadmin.org/)（建議） | 隨 PostgreSQL 安裝程式附帶 | 執行 `DB/` 的 SQL 腳本 |

開發時使用的版本：.NET SDK 10.0.402、Node.js 24.22.1、PostgreSQL 18.6。

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

設備和借用紀錄都沒有狀態欄位，狀態由借用紀錄的歸還時間、預計歸還時間即時推算，避免狀態和借用紀錄不一致。

## 如何啟動前端、後端、資料庫

請先安裝[執行需求](#執行需求)中的軟體，再依照「資料庫 → 後端 → 前端」的順序啟動。

### 1. 資料庫

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

### 2. 後端

連線字串預設為 `Host=localhost;Port=5432;Database=device_lending;Username=postgres`（`appsettings.json`），**密碼不放在程式碼裡**，開發時用 .NET 的 user-secrets 設定：

```bash
cd backend/DeviceLending.Api

# 設定連線字串，只需執行一次（請換成自己的 PostgreSQL 密碼；主機、埠號、帳號不同時一併修改）
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=device_lending;Username=postgres;Password=你的密碼"

# 啟動後端
dotnet run
```

user-secrets 內建於 .NET SDK，不需另外安裝。也可以改用環境變數設定連線字串（只在目前的終端機視窗有效）：

```
# Windows PowerShell
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=device_lending;Username=postgres;Password=你的密碼"

# macOS／Linux
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=device_lending;Username=postgres;Password=你的密碼"

dotnet run
```

- 啟動後 API 位於 `http://localhost:5123`
- 開發環境可開啟 `http://localhost:5123/scalar` 查看與測試所有 API
- log 檔預設寫入執行目錄下的 `logs/`（`dotnet run` 時為 `bin/Debug/net10.0/logs/`），可在 `appsettings.json` 的 `LogDirectory` 改成其他資料夾

### 3. 前端

另開一個終端機：

```bash
cd frontend
npm install
npm run dev
```

- 啟動後用瀏覽器開啟 `http://localhost:5173`

## API 說明

### Type 類型

| Method | URL | 用途 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/Type/Equipment` | 取得所有設備類型 | 後端 SQL |

### Equipment 設備

| Method | URL | 用途 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/Equipment` | 查詢設備清單，可依設備類型（`typeId`）、狀態（`status`：1 借用中、2 逾期、3 可借用）篩選；預設不含已刪除的設備，帶 `includeArchived=true` 可一併列出 | 後端 SQL |
| POST | `/api/Equipment` | 新增設備 | `sp_equipment_create` |
| PUT | `/api/Equipment/{id}` | 修改設備 | `sp_equipment_update` |
| DELETE | `/api/Equipment/{id}` | 刪除設備（軟刪除） | `sp_equipment_delete` |

### EquipmentBorrowRecord 借用紀錄

| Method | URL | 用途 | 資料庫存取 |
|--------|-----|------|------|
| GET | `/api/EquipmentBorrowRecord` | 查詢借用歷史，可依設備（`equipmentId`）、借用人（`borrowerName`）、狀態（`status`：1 借用中、2 逾期、3 已歸還）篩選 | 後端 SQL |
| POST | `/api/EquipmentBorrowRecord` | 借出設備 | `sp_borrow_record_create` |
| POST | `/api/EquipmentBorrowRecord/{id}/Return` | 歸還設備 | `sp_borrow_record_return` |

完整的參數與 request body 可在後端啟動後開啟 `http://localhost:5123/scalar` 查看。
