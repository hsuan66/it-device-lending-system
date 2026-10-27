using DeviceLending.Api.Data;
using DeviceLending.Api.Models.Dtos;

namespace DeviceLending.Api.Services;

public interface IEquipmentTypeService
{
    Task<IReadOnlyList<EquipmentTypeResponse>> GetAllAsync(CancellationToken cancellationToken);
}

public class EquipmentTypeService(IEquipmentTypeRepository repository) : IEquipmentTypeService
{
    public async Task<IReadOnlyList<EquipmentTypeResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var types = await repository.GetAllAsync(cancellationToken);

        // Entity 轉成 DTO
        return types
            .Select(t => new EquipmentTypeResponse(t.Id, t.Name))
            .ToList();
    }
}
