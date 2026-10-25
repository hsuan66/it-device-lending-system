namespace DeviceLending.Api.Models.Entities;

// 對應資料表 equipment_type，欄位型別跟資料庫一致
public class EquipmentType
{
    public short Id { get; set; } // SMALLINT → short
    public string Name { get; set; } = string.Empty;  // VARCHAR(50) → string
}
