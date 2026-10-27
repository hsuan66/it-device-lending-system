namespace DeviceLending.Api.Infrastructure;

// API 失敗時統一的回應格式，例如 { "code": 2006, "message": "借用設備失敗：設備借用中（...）" }
// code：錯誤代碼，前端依代碼顯示給使用者看的訊息
// message：給開發者看的訊息，會帶資料 id
public record ApiErrorResponse(int Code, string Message);
