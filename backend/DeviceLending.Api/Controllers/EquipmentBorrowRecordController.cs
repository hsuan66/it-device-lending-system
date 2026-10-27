using DeviceLending.Api.Models.Dtos;
using DeviceLending.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceLending.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentBorrowRecordController(IEquipmentBorrowRecordService service) : ControllerBase
{
    // GET /api/EquipmentBorrowRecord?equipmentId=1&borrowerName=王&status=2  查詢借用歷史（可依設備、借用人、狀態篩選）
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetEquipmentBorrowRecordResponse>>> GetBorrowRecordList(
        [FromQuery] GetEquipmentBorrowRecordRequest query, CancellationToken cancellationToken)
    {
        var records = await service.GetListAsync(query, cancellationToken);
        return Ok(records);
    }

    // POST /api/EquipmentBorrowRecord  新增借用紀錄
    [HttpPost]
    public async Task<IActionResult> CreateBorrowRecord(
        PostEquipmentBorrowRecordRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    // POST /api/EquipmentBorrowRecord/{id}/Return  歸還設備
    [HttpPost("{id:int}/Return")]
    public async Task<IActionResult> ReturnEquipment(int id, CancellationToken cancellationToken)
    {
        await service.ReturnAsync(id, cancellationToken);
        return NoContent();
    }
}
