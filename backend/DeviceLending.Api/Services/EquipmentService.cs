using DeviceLending.Api.Data;
using DeviceLending.Api.Models.Dtos;

namespace DeviceLending.Api.Services;

public interface IEquipmentService
{
    Task<IReadOnlyList<GetEquipmentResponse>> GetListAsync(GetEquipmentRequest query, CancellationToken cancellationToken);
    Task CreateAsync(PostEquipmentRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(int id, PutEquipmentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

// 欄位檢查（空值、類型存在、財產編號重複、借用中不可刪除）由預存程序負責
public class EquipmentService(IEquipmentRepository repository) : IEquipmentService
{
    public async Task<IReadOnlyList<GetEquipmentResponse>> GetListAsync(
        GetEquipmentRequest query, CancellationToken cancellationToken)
    {
        var items = await repository.GetListAsync(
            query.TypeId, query.Status, query.IncludeArchived, cancellationToken);

        // Entity 轉成 DTO
        return items
            .Select(e => new GetEquipmentResponse(
                e.Id, e.AssetCode, e.Name, e.EquipmentTypeId, e.EquipmentTypeName, e.Status, e.IsArchive))
            .ToList();
    }

    public Task CreateAsync(PostEquipmentRequest request, CancellationToken cancellationToken)
    {
        return repository.CreateAsync(
            request.AssetCode, request.Name, request.EquipmentTypeId, cancellationToken);
    }

    public Task UpdateAsync(int id, PutEquipmentRequest request, CancellationToken cancellationToken)
    {
        return repository.UpdateAsync(
            id, request.AssetCode, request.Name, request.EquipmentTypeId, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }
}
