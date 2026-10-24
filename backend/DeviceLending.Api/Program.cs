using Scalar.AspNetCore; // 新增

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
