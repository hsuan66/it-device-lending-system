using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Entities;

// 設備清單查詢結果：equipment JOIN equipment_type，再加上推算出的狀態
public class EquipmentListItem
{
    public int Id { get; set; } // INTEGER → int
    public string AssetCode { get; set; } = string.Empty; // VARCHAR(30) → string
    public string Name { get; set; } = string.Empty; // VARCHAR(100) → string
    public short EquipmentTypeId { get; set; } // SMALLINT → short
    public string EquipmentTypeName { get; set; } = string.Empty; // equipment_type.name
    public EquipmentStatus Status { get; set; } // 由 borrow_record 推算
}
