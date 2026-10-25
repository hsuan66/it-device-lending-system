using DeviceLending.Api.Models.Enums;

namespace DeviceLending.Api.Models.Dtos;

// GET /api/EquipmentBorrowRecord 回傳給前端的格式
public record GetEquipmentBorrowRecordResponse(
    int Id,
    int EquipmentId,
    string AssetCode,
    string EquipmentName,
    string EquipmentTypeName,
    string BorrowerName,
    DateTimeOffset BorrowTime,
    DateTimeOffset ExpectReturnTime,
    DateTimeOffset? ReturnTime,
    BorrowRecordStatus Status);
