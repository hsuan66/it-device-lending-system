using DeviceLending.Api.Models.Dtos;
using DeviceLending.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceLending.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController(IEquipmentService service) : ControllerBase
{
    // GET /api/Equipment?typeId=1&status=2  查詢設備清單（可依類型、狀態篩選）
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetEquipmentResponse>>> GetEquipmentList(
        [FromQuery] GetEquipmentRequest query, CancellationToken cancellationToken)
    {
        var equipment = await service.GetListAsync(query, cancellationToken);
        return Ok(equipment);
    }

    // POST /api/Equipment  新增設備
    [HttpPost]
    public async Task<IActionResult> CreateEquipment(
        PostEquipmentRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    // PUT /api/Equipment/{id}  修改設備
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEquipment(
        int id, PutEquipmentRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    // DELETE /api/Equipment/{id}  刪除設備（軟刪除）
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEquipment(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
