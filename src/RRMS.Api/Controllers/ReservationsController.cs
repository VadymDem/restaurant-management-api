using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Exceptions;
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
        var reservation = await _reservationService.CreateAsync(
            userId, request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, reservation);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetMy(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var reservations = await _reservationService.GetForUserAsync(
            userId, cancellationToken);

        return Ok(reservations);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Cancel(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await _reservationService.CancelAsync(
            userId, id, cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedException(
                "The access token does not contain a valid user identifier.");
        }

        return userId;
    }
}
