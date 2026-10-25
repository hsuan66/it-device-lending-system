using Scalar.AspNetCore;
using Npgsql;
using DeviceLending.Api.Infrastructure;
using DeviceLending.Api.Data;
using DeviceLending.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 資料庫連線：開發時從 user-secrets 讀取，正式環境從環境變數讀取
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("找不到連線字串 ConnectionStrings:Default");

// NpgsqlDataSource 管理連線池，整個程式共用一個
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

// 錯誤處理：資料庫自訂錯誤代碼轉成 HTTP 狀態碼，回傳格式統一用 ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PostgresExceptionHandler>();

// Repository 與 Service：每個 HTTP 請求建立一份
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeRepository>();
builder.Services.AddScoped<IEquipmentTypeService, EquipmentTypeService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 新增：允許 Vue 開發伺服器跨來源呼叫 API
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 新增：API 測試頁 /scalar
}

app.UseHttpsRedirection();

app.UseCors("Frontend"); // 新增

app.UseAuthorization();

app.MapControllers();

app.Run();
