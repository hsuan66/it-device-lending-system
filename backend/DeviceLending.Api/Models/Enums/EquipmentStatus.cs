namespace DeviceLending.Api.Models.Enums;

// 設備狀態：資料表沒有此欄位，由 borrow_record 推算
// 借用中、逾期的代碼與 BorrowRecordStatus 相同
public enum EquipmentStatus
{
    Borrowed = 1,  // 借用中：有未歸還的紀錄，且尚未超過預計歸還時間
    Overdue = 2,   // 逾期：有未歸還的紀錄，且已超過預計歸還時間
    Available = 3  // 可借用：沒有未歸還的借用紀錄
}
