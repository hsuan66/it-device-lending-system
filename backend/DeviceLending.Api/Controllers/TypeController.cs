using DeviceLending.Api.Models.Dtos;
using DeviceLending.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceLending.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TypeController(IEquipmentTypeService service) : ControllerBase
{
    // GET /api/Type/Equipment  取得所有設備類型
    [HttpGet("Equipment")]
    public async Task<ActionResult<IReadOnlyList<EquipmentTypeResponse>>> GetEquipmentType(
        CancellationToken cancellationToken)
    {
        var types = await service.GetAllAsync(cancellationToken);
        return Ok(types);
    }
}
