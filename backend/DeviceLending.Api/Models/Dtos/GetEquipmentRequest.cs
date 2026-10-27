using System.ComponentModel.DataAnnotations;
using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Dtos;

// GET /api/Equipment 的篩選條件，例如 ?typeId=1&status=2，不傳代表不篩選
public class GetEquipmentRequest
{
    public int? TypeId { get; init; }

    // 不在 EquipmentStatus 裡的數字（例如 9）會回 400
    [EnumDataType(typeof(EquipmentStatus))]
    public EquipmentStatus? Status { get; init; }
}
