using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Constants;

namespace RRMS.Api.Controllers;

[ApiController]
[Route("api/admin/reservations")]
[Authorize(Roles = Roles.Administrator)]
public class AdminReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public AdminReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // GET api/admin/reservations
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetAllAsync(cancellationToken);
        return Ok(reservations);
    }

    // PUT api/admin/reservations/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ReservationResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateReservationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return Ok(reservation);
    }
}