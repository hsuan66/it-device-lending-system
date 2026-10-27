using System.ComponentModel.DataAnnotations;

namespace DeviceLending.Api.Models.Dtos;

// POST /api/Equipment 前端送來的格式
// 長度限制對齊資料表欄位，避免超長字串送進資料庫變成 500
public class PostEquipmentRequest
{
    [Required]
    [MaxLength(30)]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "財產編號只能包含英文大寫、數字和 -")]
    public string AssetCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    // 對應 SMALLINT，沒傳時預設 0 也會被擋下
    [Range(1, short.MaxValue)]
    public int EquipmentTypeId { get; init; }
}
