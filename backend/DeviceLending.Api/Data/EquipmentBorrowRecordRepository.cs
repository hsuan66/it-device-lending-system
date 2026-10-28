using DeviceLending.Api.Models.Entities;
using DeviceLending.Api.Models.Enums;
using Npgsql;
using NpgsqlTypes;

namespace DeviceLending.Api.Data;

public interface IEquipmentBorrowRecordRepository
{
    Task<IReadOnlyList<EquipmentBorrowRecordListItem>> GetListAsync(
        int? equipmentId, string? borrowerName, BorrowRecordStatus? status, CancellationToken cancellationToken);

    Task CreateAsync(
        int equipmentId, string borrowerName, DateTimeOffset expectReturnTime, CancellationToken cancellationToken);

    Task ReturnAsync(int id, CancellationToken cancellationToken);
}

public class EquipmentBorrowRecordRepository(NpgsqlDataSource dataSource) : IEquipmentBorrowRecordRepository
{
    public async Task<IReadOnlyList<EquipmentBorrowRecordListItem>> GetListAsync(
        int? equipmentId, string? borrowerName, BorrowRecordStatus? status, CancellationToken cancellationToken)
    {
        // 子查詢先算出狀態，外層再依設備、借用人、狀態篩選
        // 已刪除設備的借用紀錄仍會列出，保留完整歷史
        // 借用人用 strpos 做「包含」比對，使用者輸入 % 或 _ 不會變成萬用字元
        const string sql = """
            SELECT id
                , equipment_id
                , asset_code
                , equipment_name
                , equipment_type_name
                , borrower_name
                , borrow_time
                , expect_return_time
                , return_time, status
            FROM (
                SELECT borrow_record.id
                    , borrow_record.equipment_id
                    , equipment.asset_code
                    , equipment.name AS equipment_name
                    , equipment_type.name AS equipment_type_name
                    , borrow_record.borrower_name
                    , borrow_record.borrow_time
                    , borrow_record.expect_return_time
                    , borrow_record.return_time,
                    CASE WHEN borrow_record.return_time IS NOT NULL THEN 3
                        WHEN borrow_record.expect_return_time < now() THEN 2
                        ELSE 1
                    END AS status
                FROM borrow_record
                JOIN equipment
                    ON equipment.id = borrow_record.equipment_id
                JOIN equipment_type
                    ON equipment_type.id = equipment.equipment_type_id
                ) x
            WHERE (@equipment_id IS NULL OR x.equipment_id = @equipment_id)
            AND (@borrower_name IS NULL OR strpos(x.borrower_name, @borrower_name) > 0)
            AND (@status IS NULL OR x.status = @status)
            ORDER BY x.borrow_time DESC, x.id DESC
            """;

        await using var command = dataSource.CreateCommand(sql);

        // 參數可能是 NULL，要指定型別，否則 PostgreSQL 無法判斷參數型別
        command.Parameters.Add(new NpgsqlParameter("equipment_id", NpgsqlDbType.Integer)
        {
            Value = (object?)equipmentId ?? DBNull.Value
        });
        command.Parameters.Add(new NpgsqlParameter("borrower_name", NpgsqlDbType.Varchar)
        {
            Value = (object?)borrowerName ?? DBNull.Value
        });
        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Integer)
        {
            Value = status is null ? DBNull.Value : (int)status.Value
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<EquipmentBorrowRecordListItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new EquipmentBorrowRecordListItem
            {
                Id = reader.GetInt32(0),
                EquipmentId = reader.GetInt32(1),
                AssetCode = reader.GetString(2),
                EquipmentName = reader.GetString(3),
                EquipmentTypeName = reader.GetString(4),
                BorrowerName = reader.GetString(5),
                BorrowTime = reader.GetFieldValue<DateTimeOffset>(6),
                ExpectReturnTime = reader.GetFieldValue<DateTimeOffset>(7),
                ReturnTime = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
                Status = (BorrowRecordStatus)reader.GetInt32(9)
            });
        }

        return result;
    }

    public async Task CreateAsync(
        int equipmentId, string borrowerName, DateTimeOffset expectReturnTime, CancellationToken cancellationToken)
    {
        // 借出時間由預存程序以目前時間寫入；最後一個是 INOUT p_id，簽章需要所以傳 NULL，帶回的 ID 不使用
        const string sql =
            "CALL sp_borrow_record_create(@equipment_id, @borrower_name, @expect_return_time, NULL)";

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("equipment_id", equipmentId);
        command.Parameters.AddWithValue("borrower_name", borrowerName);

        // TIMESTAMPTZ 只接受 UTC，先轉成 UTC 再傳
        command.Parameters.Add(new NpgsqlParameter("expect_return_time", NpgsqlDbType.TimestampTz)
        {
            Value = expectReturnTime.ToUniversalTime()
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ReturnAsync(int id, CancellationToken cancellationToken)
    {
        // 第 2 個參數是 INOUT p_return_time，傳 NULL 代表以現在時間歸還，帶回的時間不使用
        const string sql = "CALL sp_borrow_record_return(@id, NULL)";

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
