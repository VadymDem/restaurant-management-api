using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var reservation = await _reservationService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMy), reservation);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetMy(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var reservations = await _reservationService.GetForUserAsync(userId, cancellationToken);
        return Ok(reservations);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _reservationService.CancelAsync(userId, id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(value!);
    }
}