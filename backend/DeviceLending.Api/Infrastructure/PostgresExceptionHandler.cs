using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace DeviceLending.Api.Infrastructure;

// 把預存程序丟出的自訂錯誤代碼（SQLSTATE）轉成對應的 HTTP 狀態碼
public class PostgresExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 不是資料庫錯誤，交給預設處理（500）
        if (exception is not PostgresException pgException)
        {
            return false;
        }

        int? statusCode = pgException.SqlState switch
        {
            "DL400" => StatusCodes.Status400BadRequest,
            "DL404" => StatusCodes.Status404NotFound,
            "DL409" => StatusCodes.Status409Conflict,
            _ => null
        };

        // 不是我們自訂的錯誤代碼，交給預設處理（500）
        if (statusCode is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = pgException.MessageText
            }
        });
    }
}
