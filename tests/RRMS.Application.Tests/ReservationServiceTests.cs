
using Microsoft.Extensions.Options;
using RRMS.Application.DTOs.Reservations;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Options;
using RRMS.Application.Services;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

using MsOptions = Microsoft.Extensions.Options.Options;

namespace RRMS.Application.Tests;

public class ReservationServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

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
            _unitOfWork,
            new FixedTimeProvider(Now),
            MsOptions.Create(new RestaurantOptions()));
    }

    private static CreateReservationRequest ValidRequest(
        long tableId = 1,
        int guestsCount = 2)
    {
        return new CreateReservationRequest(
            tableId,
            DateOnly.FromDateTime(Now.UtcDateTime.AddDays(7)),
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
    public async Task GetForUserAsync_ReturnsOnlyUsersReservations()
    {
        _reservations.Items.AddRange([
            new Reservation
            {
                Id = 10,
                UserId = _userId,
                TableId = 1,
                ReservationDate = new DateOnly(2026, 2, 10),
                ReservationTime = new TimeOnly(19, 30),
                GuestsCount = 3,
                Notes = "Window table",
                Status = ReservationStatus.Confirmed
            },
            new Reservation
            {
                Id = 11,
                UserId = Guid.NewGuid(),
                TableId = 1,
                ReservationDate = new DateOnly(2026, 2, 11),
                ReservationTime = new TimeOnly(18, 0),
                GuestsCount = 2,
                Status = ReservationStatus.Pending
            }
        ]);

        var result = await _service.GetForUserAsync(_userId);

        var reservation = Assert.Single(result);
        Assert.Equal(10, reservation.Id);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal("Window table", reservation.Notes);
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
            DateOnly.FromDateTime(Now.UtcDateTime.AddDays(-1)),
            new TimeOnly(18, 0),
            2,
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(_userId, request));
    }

    [Fact]
    public async Task CreateAsync_ComparesAgainstConfiguredRestaurantTimeZone()
    {
        // 22:30 UTC is already 00:30 of the next day in Kyiv,
        // so a slot at 23:00 of the previous local date is in the past.
        var service = new ReservationService(
            _reservations,
            _tables,
            _unitOfWork,
            new FixedTimeProvider(new DateTimeOffset(2026, 1, 15, 22, 30, 0, TimeSpan.Zero)),
            MsOptions.Create(new RestaurantOptions { TimeZone = "Europe/Kyiv" }));

        var request = new CreateReservationRequest(
            1,
            new DateOnly(2026, 1, 15),
            new TimeOnly(23, 0),
            2,
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(_userId, request));
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

    [Fact]
    public async Task GetAllAsync_MapsReservations()
    {
        _reservations.Items.Add(new Reservation
        {
            Id = 11,
            UserId = _userId,
            TableId = 1,
            ReservationDate = new DateOnly(2026, 2, 1),
            ReservationTime = new TimeOnly(18, 0),
            GuestsCount = 2,
            Status = ReservationStatus.Confirmed,
            Notes = "window seat"
        });

        var items = await _service.GetAllAsync();

        var item = Assert.Single(items);
        Assert.Equal(11, item.Id);
        Assert.Equal(_userId, item.UserId);
        Assert.Equal(1, item.TableId);
        Assert.Equal(new DateOnly(2026, 2, 1), item.ReservationDate);
        Assert.Equal(new TimeOnly(18, 0), item.ReservationTime);
        Assert.Equal(2, item.GuestsCount);
        Assert.Equal(ReservationStatus.Confirmed, item.Status);
        Assert.Equal("window seat", item.Notes);
    }

    [Fact]
    public async Task GetAllAsync_NoReservations_ReturnsEmptyList()
    {
        var items = await _service.GetAllAsync();

        Assert.Empty(items);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatusAndSaves()
    {
        _reservations.Items.Add(new Reservation
        {
            Id = 3,
            UserId = _userId,
            TableId = 1,
            ReservationDate = new DateOnly(2026, 2, 1),
            ReservationTime = new TimeOnly(18, 0),
            GuestsCount = 2,
            Status = ReservationStatus.Pending
        });

        var response = await _service.UpdateStatusAsync(
            3, ReservationStatus.Confirmed);

        Assert.Equal(ReservationStatus.Confirmed, response.Status);
        Assert.Equal(
            ReservationStatus.Confirmed,
            _reservations.Items.Single().Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateStatusAsync_MissingReservation_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateStatusAsync(999, ReservationStatus.Confirmed));
    }

    [Theory]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Completed)]
    public async Task UpdateStatusAsync_FromCancelled_ThrowsConflict(
        ReservationStatus next)
    {
        _reservations.Items.Add(new Reservation
        {
            Id = 4,
            UserId = _userId,
            TableId = 1,
            ReservationDate = new DateOnly(2026, 2, 1),
            ReservationTime = new TimeOnly(18, 0),
            GuestsCount = 2,
            Status = ReservationStatus.Cancelled
        });

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateStatusAsync(4, next));
    }

    [Fact]
    public async Task UpdateStatusAsync_FromCompleted_ThrowsConflict()
    {
        _reservations.Items.Add(new Reservation
        {
            Id = 5,
            UserId = _userId,
            TableId = 1,
            ReservationDate = new DateOnly(2026, 2, 1),
            ReservationTime = new TimeOnly(18, 0),
            GuestsCount = 2,
            Status = ReservationStatus.Completed
        });

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateStatusAsync(5, ReservationStatus.Confirmed));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
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
