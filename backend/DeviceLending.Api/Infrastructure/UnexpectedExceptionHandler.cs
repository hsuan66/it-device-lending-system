using Microsoft.AspNetCore.Diagnostics;

namespace DeviceLending.Api.Infrastructure;

// 其他未預期的錯誤（程式錯誤、資料庫連不上等）：回傳 500 與 code 9999
// 例外細節只寫進 log，不放進回應，避免把程式內部資訊暴露給外部
// 要註冊在 PostgresExceptionHandler 之後，前面的處理器都不處理時才輪到它
public class UnexpectedExceptionHandler(ILogger<UnexpectedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 程式壞掉了，記 Error 並附上完整的例外（含堆疊）
        logger.LogError(
            exception,
            "未預期的錯誤 {ErrorCode}：{Method} {Path}",
            ErrorCodes.UnexpectedError, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(ErrorCodes.UnexpectedError, "未預期的錯誤，詳細內容請查看後端 log"),
            cancellationToken);
        return true;
    }
}
