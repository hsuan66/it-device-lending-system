using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Dtos;

// GET /api/Equipment 回傳給前端的格式
public record GetEquipmentResponse(
    int Id,
    string AssetCode,
    string Name,
    int EquipmentTypeId,
    string EquipmentTypeName,
    EquipmentStatus Status);
