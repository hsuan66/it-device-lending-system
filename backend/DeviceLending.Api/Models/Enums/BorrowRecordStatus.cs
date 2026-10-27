namespace DeviceLending.Api.Models.Enums;

// 借用紀錄狀態：資料表沒有此欄位，由 return_time、expect_return_time 推算
public enum BorrowRecordStatus
{
    Borrowed = 1, // 借用中：尚未歸還，且尚未超過預計歸還時間
    Overdue = 2,  // 逾期：尚未歸還，且已超過預計歸還時間
    Returned = 3  // 已歸還：有實際歸還時間
}
