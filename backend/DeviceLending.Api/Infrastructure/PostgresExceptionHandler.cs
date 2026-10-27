using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace DeviceLending.Api.Infrastructure;

// 把預存程序丟出的自訂錯誤代碼（SQLSTATE，例如 U2006）轉成 HTTP 狀態碼與 code
// 回應的 message 是給開發者看的訊息；前端依 code 顯示給使用者看的訊息
public class PostgresExceptionHandler(ILogger<PostgresExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 不是資料庫錯誤，交給 UnexpectedExceptionHandler（500）
        if (exception is not PostgresException pgException)
        {
            return false;
        }

        // 不是我們自訂的錯誤代碼，交給 UnexpectedExceptionHandler（500）
        if (!ErrorCodes.TryParseSqlState(pgException.SqlState, out var errorCode, out var statusCode))
        {
            return false;
        }

        // 業務規則擋下的錯誤不是程式壞掉，記 Warning 就好
        logger.LogWarning(
            "業務錯誤 {ErrorCode}：{Method} {Path}，{Message}",
            errorCode, httpContext.Request.Method, httpContext.Request.Path, pgException.MessageText);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(errorCode, pgException.MessageText), cancellationToken);
        return true;
    }
}
