# PetService 後端 API 專案

## 專案簡介

此專案為 PetService 寵物服務平台的後端，採用 ASP.NET Core Web API 架構開發，負責處理會員管理、訂單處理、寵物旅館與散步服務、金流整合（ECPay）、權限控管等功能。前端透過 RESTful API 與本服務溝通。

## 技術架構

- **語言與框架**：C#, ASP.NET Core 7
- **資料庫**：Microsoft SQL Server
- **ORM**：Entity Framework Core
- **API 樣式**：RESTful API
- **驗證方式**：JWT（Json Web Token）
- **金流整合**：ECPay
- **日誌紀錄**：Console logging（可擴充為 Serilog）
- **開發工具**：Visual Studio / Rider / VSCode

## 資料庫設計

使用 EF Core Code First 設計資料表，主要實體如下：

- `TMember`：會員資料表
- `TOrder`：訂單主表
- `TOrderDetail_Walk`：散步服務明細
- `TOrderDetail_Hotel`：旅館服務明細
- `TEmployee`：服務人員資料
- `THotel`：旅館資訊
- `TPaymentLog`：金流紀錄

## 資料夾結構簡介

```
PetService_Project_Api/
├── Controllers/         # 各模組 API 控制器
├── DTO/                # 資料傳輸物件（DTO）
│   ├── AuthDTO/
│   ├── OrderDTO/
│   ├── HotelDTO/
│   └── WalkDTO/
├── Models/             # EF Core 實體與 DbContext
├── Services/           # 業務邏輯（選擇性）
├── Migrations/         # 資料庫遷移紀錄
├── Program.cs          # 入口點
└── appsettings.json    # 設定檔
```

## 環境變數與設定

請在根目錄的 `appsettings.json` 設定資料庫與金流參數：

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=PetServiceDb;Trusted_Connection=True;"
},
"JWT": {
  "SecretKey": "YourSuperSecretKey",
  "Issuer": "PetService",
  "Audience": "PetServiceUsers"
},
"ECPay": {
  "MerchantID": "2000132",
  "HashKey": "YourHashKey",
  "HashIV": "YourHashIV",
  "ReturnURL": "https://yourdomain.com/api/payment/return"
}
```

## API 功能說明

### 認證與授權
- 註冊 /api/Auth/Register
- 登入 /api/Auth/Login
- JWT 驗證與 Claims 解析

### 散步服務 API
- 建立訂單：POST /api/WalkOrder
- 查詢訂單：GET /api/WalkOrder/{id}

### 旅館預訂 API
- 查詢旅館清單：GET /api/Hotel
- 建立訂單：POST /api/HotelOrder

### 訂單整合
- 查詢全部訂單：GET /api/Order?memberId=xxx
- 取得訂單詳情：GET /api/Order/{id}?type=walk/hotel

### 金流整合
- 發起綠界付款：POST /api/PaymentRequest
- 處理金流回傳：POST /api/Payment/Return

## 開發與執行

### 使用 EF Core 進行資料庫遷移與建立：
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 啟動專案：
```bash
dotnet run
```

## 測試工具

- Postman / Thunder Client for API 測試
- Swagger（可整合）

## 授權

本專案採用 MIT 授權條款，詳見 [LICENSE](LICENSE)

## 聯繫方式

- 作者：郭維哲（Wizkuo）
- 前端專案連結：[https://github.com/sa585951/PetService-project](https://github.com/sa585951/PetService-project)
