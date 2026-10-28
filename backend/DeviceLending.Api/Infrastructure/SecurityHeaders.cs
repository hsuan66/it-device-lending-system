namespace DeviceLending.Api.Infrastructure;

// 安全性回應標頭（XSS 防護的一環）
// API 只回傳 JSON，不應該被瀏覽器當成網頁執行：
// * X-Content-Type-Options: nosniff：瀏覽器不可猜測內容型別，JSON 不會被當成 HTML 或 script
// * Content-Security-Policy: default-src 'none'：就算回應被當成網頁開啟，也不載入、不執行任何內容
// * frame-ancestors 'none'：不可被其他網站用 iframe 嵌入（防點擊劫持）
// 畫面本身的 XSS 防護在前端：Vue 的 {{ }} 會自動跳脫、不使用 v-html，正式版另有 CSP（見 frontend/vite.config.ts）
public static class SecurityHeaders
{
    private const string ApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // 用 OnStarting 在回應送出前才寫入：UseExceptionHandler 處理例外時會清空標頭，錯誤回應也要帶到
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";

                // 只限 /api：開發環境的 /scalar 測試頁需要執行 script，不能套用
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    headers.ContentSecurityPolicy = ApiContentSecurityPolicy;
                }

                return Task.CompletedTask;
            });

            await next(context);
        });
    }
}
