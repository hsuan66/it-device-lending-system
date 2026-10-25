using System.ComponentModel.DataAnnotations;

namespace DeviceLending.Api.Models.Dtos;

// POST /api/EquipmentBorrowRecord 前端送來的格式
// 長度限制對齊資料表欄位，避免超長字串送進資料庫變成 500
public class PostEquipmentBorrowRecordRequest
{
    // 沒傳時預設 0 也會被擋下
    [Range(1, int.MaxValue)]
    public int EquipmentId { get; init; }

    [Required]
    [MaxLength(50)]
    public string BorrowerName { get; init; } = string.Empty;

    // 用可為 null 的型別，[Required] 才擋得住沒傳的情況
    [Required]
    public DateTimeOffset? ExpectReturnTime { get; init; }

    // 實際借出時間，不傳代表現在
    public DateTimeOffset? BorrowTime { get; init; }
}
