
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Services;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Application.Tests;

public class ReservationServiceTests
{
    private readonly FakeReservationRepository _reservations = new();
    private readonly FakeRestaurantTableRepository _tables = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ReservationService _service;

    private readonly Guid _userId = Guid.NewGuid();

    public ReservationServiceTests()
    {
        _tables.Items.Add(new RestaurantTable
        {
            Id = 1,
            Number = 1,
            Capacity = 4,
            IsActive = true
        });

        _service = new ReservationService(
            _reservations,
            _tables,
            _unitOfWork);
    }

    private static CreateReservationRequest ValidRequest(
        long tableId = 1,
        int guestsCount = 2)
    {
        return new CreateReservationRequest(
            tableId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            new TimeOnly(18, 0),
            guestsCount,
            "Test reservation");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPendingReservation()
    {
        var response = await _service.CreateAsync(
            _userId, ValidRequest());

        Assert.Equal(1, response.TableId);
        Assert.Equal(_userId, response.UserId);
        Assert.Equal(2, response.GuestsCount);
        Assert.Equal(ReservationStatus.Pending, response.Status);
        Assert.Single(_reservations.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_NonexistentTable_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(
                _userId, ValidRequest(tableId: 999)));
    }

    [Fact]
    public async Task CreateAsync_InactiveTable_ThrowsConflict()
    {
        _tables.Items[0].IsActive = false;

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(
                _userId, ValidRequest()));
    }

    [Fact]
    public async Task CreateAsync_ExceedsCapacity_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(
                _userId, ValidRequest(guestsCount: 5)));
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlot_ThrowsConflict()
    {
        var request = ValidRequest();

        await _service.CreateAsync(_userId, request);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(_userId, request));

        Assert.Single(_reservations.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_CancelledSlot_AllowsNewReservation()
    {
        var request = ValidRequest();

        _reservations.Items.Add(new Reservation
        {
            Id = 10,
            UserId = _userId,
            TableId = request.TableId,
            ReservationDate = request.ReservationDate,
            ReservationTime = request.ReservationTime,
            GuestsCount = 2,
            Status = ReservationStatus.Cancelled
        });

        var response = await _service.CreateAsync(_userId, request);

        Assert.Equal(ReservationStatus.Pending, response.Status);
        Assert.Equal(2, _reservations.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_PastDate_ThrowsArgumentException()
    {
        var request = new CreateReservationRequest(
            1,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            new TimeOnly(18, 0),
            2,
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(_userId, request));
    }

    [Fact]
    public async Task CreateAsync_ZeroGuests_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(
                _userId, ValidRequest(guestsCount: 0)));
    }

    [Fact]
    public async Task CreateAsync_InvalidTableId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(
                _userId, ValidRequest(tableId: 0)));
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public List<Reservation> Items { get; } = new();

        private long _nextId = 1;

        public Task<bool> SlotExistsAsync(
            long tableId,
            DateOnly reservationDate,
            TimeOnly reservationTime,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.Any(r =>
                r.TableId == tableId &&
                r.ReservationDate == reservationDate &&
                r.ReservationTime == reservationTime &&
                r.Status != ReservationStatus.Cancelled));
        }

        public Task<Reservation?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(r => r.Id == id));

        public Task<IReadOnlyList<Reservation>> GetByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Reservation>>(
                Items.Where(r => r.UserId == userId).ToList());

        public Task<IReadOnlyList<Reservation>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Reservation>>(
                Items.ToList());

        public Task AddAsync(
            Reservation reservation,
            CancellationToken cancellationToken = default)
        {
            reservation.Id = _nextId++;
            Items.Add(reservation);
            return Task.CompletedTask;
        }

        public void Update(Reservation reservation)
        {
        }

        public void Delete(Reservation reservation)
        {
            Items.Remove(reservation);
        }
    }

    private sealed class FakeRestaurantTableRepository
        : IRestaurantTableRepository
    {
        public List<RestaurantTable> Items { get; } = new();

        public Task<IReadOnlyList<RestaurantTable>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RestaurantTable>>(
                Items.ToList());

        public Task<RestaurantTable?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

        public Task<IReadOnlyList<RestaurantTable>> GetAvailableAsync(
            DateOnly? reservationDate,
            TimeOnly? reservationTime,
            int? minCapacity,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RestaurantTable>>(
                Items.Where(t => t.IsActive).ToList());

        public Task<bool> NumberExistsAsync(
            int number,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Items.Any(t => t.Number == number));

        public Task AddAsync(
            RestaurantTable table,
            CancellationToken cancellationToken = default)
        {
            Items.Add(table);
            return Task.CompletedTask;
        }

        public void Update(RestaurantTable table)
        {
        }

        public void Delete(RestaurantTable table)
        {
            Items.Remove(table);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public IUserRepository Users =>
            throw new NotSupportedException();

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
