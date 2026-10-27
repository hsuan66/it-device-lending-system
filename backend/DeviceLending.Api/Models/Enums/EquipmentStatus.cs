namespace DeviceLending.Api.Models.Enums;

// 設備狀態：資料表沒有此欄位，由 borrow_record 推算
public enum EquipmentStatus
{
    Available = 1, // 可借用：沒有未歸還的借用紀錄
    Borrowed = 2,  // 借用中：有未歸還的紀錄，且尚未超過預計歸還時間
    Overdue = 3    // 逾期：有未歸還的紀錄，且已超過預計歸還時間
}
