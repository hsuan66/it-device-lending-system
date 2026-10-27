using System.Text.RegularExpressions;

namespace DeviceLending.Api.Infrastructure;

// 錯誤代碼：放在回應的 code，前端依代碼顯示給使用者看的訊息
// 1xxx 共用、2xxx 設備、3xxx 借用紀錄
// 預存程序以 SQLSTATE「U + 4 位數字」丟出（例如 U2006），U 開頭是 SQL 標準留給自訂錯誤的範圍
public static partial class ErrorCodes
{
    public const int ValidationFailed = 1001;        // 欄位驗證失敗（DTO，由後端產生）
    public const int RequiredFieldEmpty = 1002;      // 必填欄位為空
    public const int EquipmentTypeNotFound = 2001;   // 設備類型不存在
    public const int EquipmentNotFound = 2002;       // 設備不存在或已刪除
    public const int AssetCodeDuplicated = 2003;     // 財產編號已存在
    public const int AssetCodeArchived = 2004;       // 財產編號屬於已刪除的設備
    public const int EquipmentBorrowedCannotDelete = 2005; // 設備借用中，無法刪除
    public const int EquipmentBorrowedCannotBorrow = 2006; // 設備借用中，無法借出
    public const int BorrowRecordNotFound = 3001;    // 借用紀錄不存在
    public const int BorrowRecordReturned = 3002;    // 借用紀錄已歸還
    public const int ExpectReturnTimeInvalid = 3003; // 預計歸還時間必須晚於借出時間
    public const int ReturnTimeInvalid = 3004;       // 歸還時間不可早於借出時間
    public const int UnexpectedError = 9999;         // 未預期的錯誤（500，由後端產生）

    // 錯誤代碼對應的 HTTP 狀態碼
    private static readonly Dictionary<int, int> HttpStatuses = new()
    {
        [ValidationFailed] = StatusCodes.Status400BadRequest,
        [RequiredFieldEmpty] = StatusCodes.Status400BadRequest,
        [EquipmentTypeNotFound] = StatusCodes.Status404NotFound,
        [EquipmentNotFound] = StatusCodes.Status404NotFound,
        [AssetCodeDuplicated] = StatusCodes.Status409Conflict,
        [AssetCodeArchived] = StatusCodes.Status409Conflict,
        [EquipmentBorrowedCannotDelete] = StatusCodes.Status409Conflict,
        [EquipmentBorrowedCannotBorrow] = StatusCodes.Status409Conflict,
        [BorrowRecordNotFound] = StatusCodes.Status404NotFound,
        [BorrowRecordReturned] = StatusCodes.Status409Conflict,
        [ExpectReturnTimeInvalid] = StatusCodes.Status400BadRequest,
        [ReturnTimeInvalid] = StatusCodes.Status400BadRequest,
    };

    // 自訂的 SQLSTATE：U 加 4 位數字
    [GeneratedRegex(@"^U(\d{4})$")]
    private static partial Regex SqlStatePattern();

    // 從預存程序的 SQLSTATE 取出錯誤代碼與 HTTP 狀態碼；不是已定義的代碼時回傳 false
    public static bool TryParseSqlState(string sqlState, out int errorCode, out int statusCode)
    {
        errorCode = 0;
        statusCode = 0;

        var match = SqlStatePattern().Match(sqlState);
        if (!match.Success)
        {
            return false;
        }

        errorCode = int.Parse(match.Groups[1].Value);
        return HttpStatuses.TryGetValue(errorCode, out statusCode);
    }
}
