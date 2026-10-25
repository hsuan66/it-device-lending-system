using System.ComponentModel.DataAnnotations;
using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Dtos;

// GET /api/EquipmentBorrowRecord 的篩選條件，例如 ?equipmentId=1&borrowerName=王&status=2，不傳代表不篩選
public class GetEquipmentBorrowRecordRequest
{
    public int? EquipmentId { get; init; }

    // 借用人姓名，部分符合即可
    [MaxLength(50)]
    public string? BorrowerName { get; init; }

    // 不在 BorrowRecordStatus 裡的數字 會回 400
    [EnumDataType(typeof(BorrowRecordStatus))]
    public BorrowRecordStatus? Status { get; init; }
}
