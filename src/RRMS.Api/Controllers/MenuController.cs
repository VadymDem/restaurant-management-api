using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RRMS.Application.DTOs.Menu;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Constants;

namespace RRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MenuItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _menuService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<MenuItemResponse>> Create(
        [FromBody] MenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _menuService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), item);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<MenuItemResponse>> Update(
        Guid id,
        [FromBody] MenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _menuService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _menuService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}