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
    public async Task<ActionResult<IReadOnlyList<MenuItemResponse>>> GetAll(
        [FromQuery] bool includeUnavailable,
        CancellationToken cancellationToken)
    {
        if (includeUnavailable && !User.IsInRole(Roles.Administrator))
        {
            return Forbid();
        }

        var items = await _menuService.GetAllAsync(
            includeUnavailable, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuItemResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var item = await _menuService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<MenuItemResponse>> Create(
        [FromBody] MenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _menuService.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<MenuItemResponse>> Update(
        long id,
        [FromBody] MenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _menuService.UpdateAsync(
            id, request, cancellationToken);

        return Ok(item);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        await _menuService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
