using Microsoft.EntityFrameworkCore;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Domain.Entities;
using RRMS.Infrastructure.Data;
using RRMS.Domain.Enums;

namespace RRMS.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _dbContext;

    public ReservationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Reservation?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .Include(r => r.Table)
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<bool> SlotExistsAsync(
        long tableId,
        DateOnly reservationDate,
        TimeOnly reservationTime,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(r =>
                r.TableId == tableId &&
                r.ReservationDate == reservationDate &&
                r.ReservationTime == reservationTime &&
                r.Status != ReservationStatus.Cancelled,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.Table)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.Table)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Reservations.AddAsync(
            reservation,
            cancellationToken);
    }

    public void Update(Reservation reservation)
    {
        _dbContext.Reservations.Update(reservation);
    }

    public void Delete(Reservation reservation)
    {
        _dbContext.Reservations.Remove(reservation);
    }
}
