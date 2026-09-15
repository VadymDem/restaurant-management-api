using Microsoft.EntityFrameworkCore;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Domain.Entities;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _dbContext;

    public ReservationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: _dbContext.Reservations.Include(r => r.Table).FirstOrDefaultAsync(...)");

    public Task<IReadOnlyList<Reservation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: filter by UserId and order by ReservationDateTime.");

    public Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: all reservations with their table loaded, ordered by ReservationDateTime.");

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: await _dbContext.Reservations.AddAsync(reservation);");

    public void Update(Reservation reservation)
        => throw new NotImplementedException("TODO: _dbContext.Reservations.Update(reservation);");

    public void Delete(Reservation reservation)
        => throw new NotImplementedException("TODO: _dbContext.Reservations.Remove(reservation);");
}