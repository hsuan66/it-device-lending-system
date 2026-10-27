using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Entities;

// 借用紀錄查詢結果：borrow_record JOIN equipment、equipment_type，再加上推算出的狀態
public class EquipmentBorrowRecordListItem
{
    public int Id { get; set; } // INTEGER → int
    public int EquipmentId { get; set; } // INTEGER → int
    public string AssetCode { get; set; } = string.Empty; // equipment.asset_code
    public string EquipmentName { get; set; } = string.Empty; // equipment.name
    public string EquipmentTypeName { get; set; } = string.Empty; // equipment_type.name
    public string BorrowerName { get; set; } = string.Empty; // VARCHAR(50) → string
    public DateTimeOffset BorrowTime { get; set; } // TIMESTAMPTZ → DateTimeOffset
    public DateTimeOffset ExpectReturnTime { get; set; } // TIMESTAMPTZ → DateTimeOffset
    public DateTimeOffset? ReturnTime { get; set; } // NULL 代表尚未歸還
    public BorrowRecordStatus Status { get; set; } // 由 return_time、expect_return_time 推算
}
