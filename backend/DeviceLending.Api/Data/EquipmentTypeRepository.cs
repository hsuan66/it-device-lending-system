using DeviceLending.Api.Models.Entities;
using Npgsql;

namespace DeviceLending.Api.Data;

public interface IEquipmentTypeRepository
{
    Task<IReadOnlyList<EquipmentType>> GetAllAsync(CancellationToken cancellationToken);
}

public class EquipmentTypeRepository(NpgsqlDataSource dataSource) : IEquipmentTypeRepository
{
    public async Task<IReadOnlyList<EquipmentType>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, name
                FROM equipment_type
            ORDER BY id
            """;

        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<EquipmentType>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new EquipmentType
            {
                Id = reader.GetInt16(0),
                Name = reader.GetString(1)
            });
        }

        return result;
    }
}
