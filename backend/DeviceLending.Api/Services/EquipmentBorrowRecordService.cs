using DeviceLending.Api.Data;
using DeviceLending.Api.Models.Dtos;

namespace DeviceLending.Api.Services;

public interface IEquipmentBorrowRecordService
{
    Task<IReadOnlyList<GetEquipmentBorrowRecordResponse>> GetListAsync(
        GetEquipmentBorrowRecordRequest query, CancellationToken cancellationToken);
    Task CreateAsync(PostEquipmentBorrowRecordRequest request, CancellationToken cancellationToken);
    Task ReturnAsync(int id, CancellationToken cancellationToken);
}

// 欄位檢查（空值、時間先後、設備存在、借用中不可再借、已歸還不可再還）由預存程序負責
public class EquipmentBorrowRecordService(IEquipmentBorrowRecordRepository repository) : IEquipmentBorrowRecordService
{
    public async Task<IReadOnlyList<GetEquipmentBorrowRecordResponse>> GetListAsync(
        GetEquipmentBorrowRecordRequest query, CancellationToken cancellationToken)
    {
        // 借用人去掉前後空白，空字串視為不篩選
        var borrowerName = string.IsNullOrWhiteSpace(query.BorrowerName) ? null : query.BorrowerName.Trim();

        var items = await repository.GetListAsync(query.EquipmentId, borrowerName, query.Status, cancellationToken);

        // Entity 轉成 DTO
        return items
            .Select(r => new GetEquipmentBorrowRecordResponse(
                r.Id, r.EquipmentId, r.AssetCode, r.EquipmentName, r.EquipmentTypeName, r.BorrowerName,
                r.BorrowTime, r.ExpectReturnTime, r.ReturnTime, r.Status))
            .ToList();
    }

    public Task CreateAsync(PostEquipmentBorrowRecordRequest request, CancellationToken cancellationToken)
    {
        // ExpectReturnTime 有 [Required]，通過模型驗證後一定有值
        return repository.CreateAsync(
            request.EquipmentId, request.BorrowerName, request.ExpectReturnTime!.Value, cancellationToken);
    }

    public Task ReturnAsync(int id, CancellationToken cancellationToken)
    {
        return repository.ReturnAsync(id, cancellationToken);
    }
}
