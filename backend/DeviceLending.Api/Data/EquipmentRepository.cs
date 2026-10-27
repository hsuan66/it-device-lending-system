using DeviceLending.Api.Models.Entities;
using DeviceLending.Api.Models.Enums;
using Npgsql;
using NpgsqlTypes;

namespace DeviceLending.Api.Data;

public interface IEquipmentRepository
{
    Task<IReadOnlyList<EquipmentListItem>> GetListAsync(
        int? typeId, EquipmentStatus? status, CancellationToken cancellationToken);

    Task CreateAsync(
        string assetCode, string name, int equipmentTypeId, CancellationToken cancellationToken);

    Task UpdateAsync(
        int id, string assetCode, string name, int equipmentTypeId, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

public class EquipmentRepository(NpgsqlDataSource dataSource) : IEquipmentRepository
{
    public async Task<IReadOnlyList<EquipmentListItem>> GetListAsync(
        int? typeId, EquipmentStatus? status, CancellationToken cancellationToken)
    {
        // 子查詢先算出狀態，外層再依類型、狀態篩選
        // 同一台設備最多只有一筆未歸還紀錄（ux_borrow_record_active），LEFT JOIN 不會重複
        const string sql = """
            SELECT id
                , asset_code
                , name
                , equipment_type_id
                , equipment_type_name
                , status
            FROM (
                SELECT equipment.id
                    , equipment.asset_code
                    , equipment.name
                    , equipment.equipment_type_id
                    , equipment_type.name AS equipment_type_name
                    , CASE WHEN borrow_record.id IS NULL THEN 3
                        WHEN borrow_record.expect_return_time < now() THEN 2
                        ELSE 1
                    END AS status
                FROM equipment
                JOIN equipment_type
                    ON equipment_type.id = equipment.equipment_type_id
                LEFT JOIN borrow_record
                    ON borrow_record.equipment_id = equipment.id
                    AND borrow_record.return_time IS NULL
                WHERE NOT equipment.is_archive
                ) x
            WHERE (@type_id IS NULL OR x.equipment_type_id = @type_id)
            AND (@status  IS NULL OR x.status = @status)
            ORDER BY x.id
            """;

        await using var command = dataSource.CreateCommand(sql);

        // 參數可能是 NULL，要指定型別，否則 PostgreSQL 無法判斷參數型別
        command.Parameters.Add(new NpgsqlParameter("type_id", NpgsqlDbType.Integer)
        {
            Value = (object?)typeId ?? DBNull.Value
        });
        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Integer)
        {
            Value = status is null ? DBNull.Value : (int)status.Value
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<EquipmentListItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new EquipmentListItem
            {
                Id = reader.GetInt32(0),
                AssetCode = reader.GetString(1),
                Name = reader.GetString(2),
                EquipmentTypeId = reader.GetInt16(3),
                EquipmentTypeName = reader.GetString(4),
                Status = (EquipmentStatus)reader.GetInt32(5)
            });
        }

        return result;
    }

    public async Task CreateAsync(
        string assetCode, string name, int equipmentTypeId, CancellationToken cancellationToken)
    {
        // 最後一個參數是 INOUT p_id，簽章需要所以傳 NULL，帶回的 ID 不使用
        const string sql = "CALL sp_equipment_create(@asset_code, @name, @equipment_type_id, NULL)";

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("asset_code", assetCode);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("equipment_type_id", equipmentTypeId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        int id, string assetCode, string name, int equipmentTypeId, CancellationToken cancellationToken)
    {
        const string sql = "CALL sp_equipment_update(@id, @asset_code, @name, @equipment_type_id)";

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("asset_code", assetCode);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("equipment_type_id", equipmentTypeId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "CALL sp_equipment_delete(@id)";

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
