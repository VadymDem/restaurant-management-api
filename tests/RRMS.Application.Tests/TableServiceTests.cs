using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Services;
using RRMS.Domain.Entities;

namespace RRMS.Application.Tests;

public class TableServiceTests
{
    private readonly FakeRestaurantTableRepository _repository = new();
    private readonly TableService _service;

    public TableServiceTests()
    {
        _service = new TableService(_repository);
    }

    [Fact]
    public async Task GetAvailableAsync_MapsAllFields()
    {
        _repository.Items.Add(new RestaurantTable
        {
            Id = 7,
            Number = 3,
            Capacity = 4,
            Location = "Terrace",
            IsActive = true
        });

        var result = await _service.GetAvailableAsync(null, null, null);

        var table = Assert.Single(result);
        Assert.Equal(7, table.Id);
        Assert.Equal(3, table.Number);
        Assert.Equal(4, table.Capacity);
        Assert.Equal("Terrace", table.Location);
        Assert.True(table.IsActive);
    }

    [Fact]
    public async Task GetAvailableAsync_DateWithoutTime_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAvailableAsync(
                new DateOnly(2026, 11, 15), null, null));
    }

    [Fact]
    public async Task GetAvailableAsync_TimeWithoutDate_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAvailableAsync(
                null, new TimeOnly(19, 30), null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAvailableAsync_NonPositiveGuests_Throws(int guests)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAvailableAsync(null, null, guests));
    }

    [Fact]
    public async Task GetAvailableAsync_PassesFiltersToRepository()
    {
        var date = new DateOnly(2026, 11, 15);
        var time = new TimeOnly(19, 30);

        await _service.GetAvailableAsync(date, time, 4);

        Assert.Equal(date, _repository.LastDate);
        Assert.Equal(time, _repository.LastTime);
        Assert.Equal(4, _repository.LastMinCapacity);
    }

    [Fact]
    public async Task GetAvailableAsync_ExcludesInactiveAndTooSmallTables()
    {
        _repository.Items.AddRange(new[]
        {
            new RestaurantTable
            {
                Id = 1, Number = 1, Capacity = 2, IsActive = true
            },
            new RestaurantTable
            {
                Id = 2, Number = 2, Capacity = 8, IsActive = true
            },
            new RestaurantTable
            {
                Id = 3, Number = 3, Capacity = 8, IsActive = false
            }
        });

        var result = await _service.GetAvailableAsync(null, null, 4);

        Assert.Equal(new long[] { 2 }, result.Select(t => t.Id));
    }

    private sealed class FakeRestaurantTableRepository
        : IRestaurantTableRepository
    {
        public List<RestaurantTable> Items { get; } = new();

        public DateOnly? LastDate { get; private set; }

        public TimeOnly? LastTime { get; private set; }

        public int? LastMinCapacity { get; private set; }

        public Task<IReadOnlyList<RestaurantTable>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RestaurantTable>>(
                Items.OrderBy(t => t.Number).ToList());

        public Task<RestaurantTable?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Items.FirstOrDefault(t => t.Id == id));

        public Task<IReadOnlyList<RestaurantTable>> GetAvailableAsync(
            DateOnly? reservationDate,
            TimeOnly? reservationTime,
            int? minCapacity,
            CancellationToken cancellationToken = default)
        {
            LastDate = reservationDate;
            LastTime = reservationTime;
            LastMinCapacity = minCapacity;

            var query = Items.Where(t => t.IsActive);

            if (minCapacity is > 0)
            {
                query = query.Where(
                    t => t.Capacity >= minCapacity.Value);
            }

            return Task.FromResult<IReadOnlyList<RestaurantTable>>(
                query.OrderBy(t => t.Number).ToList());
        }

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
}
