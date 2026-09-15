using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RRMS.Application.DTOs.Tables;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Constants;

namespace RRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<RestaurantTableResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var tables = await _tableService.GetAllAsync(cancellationToken);
        return Ok(tables);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<RestaurantTableResponse>> Create(
        [FromBody] RestaurantTableRequest request,
        CancellationToken cancellationToken)
    {
        var table = await _tableService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), table);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<RestaurantTableResponse>> Update(
        Guid id,
        [FromBody] RestaurantTableRequest request,
        CancellationToken cancellationToken)
    {
        var table = await _tableService.UpdateAsync(id, request, cancellationToken);
        return Ok(table);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _tableService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}