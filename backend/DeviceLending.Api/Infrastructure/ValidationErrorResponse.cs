using Microsoft.AspNetCore.Mvc;

namespace DeviceLending.Api.Infrastructure;

// 欄位驗證失敗（DTO 的 [Required]、[MaxLength] 等）時的回應：
// code 1001，message 列出每個欄位的錯誤，並記錄 log
public static class ValidationErrorResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var httpContext = context.HttpContext;

        // 例如「AssetCode：財產編號只能包含英文大寫、數字和 -；Name：The Name field is required.」
        var errors = string.Join("；", context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => $"{e.Key}：{string.Join("、", e.Value!.Errors.Select(x => x.ErrorMessage))}"));

        // static class 不能當 ILogger<T> 的 T，改用 ILoggerFactory 以類別全名建立 logger
        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(ValidationErrorResponse).FullName!)
            .LogWarning(
                "欄位驗證失敗 {ErrorCode}：{Method} {Path}，{Errors}",
                ErrorCodes.ValidationFailed, httpContext.Request.Method, httpContext.Request.Path, errors);

        return new BadRequestObjectResult(
            new ApiErrorResponse(ErrorCodes.ValidationFailed, $"欄位驗證失敗：{errors}"));
    }
}
