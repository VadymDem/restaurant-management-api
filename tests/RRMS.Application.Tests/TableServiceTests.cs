using RRMS.Application.DTOs.Tables;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Services;
using RRMS.Domain.Entities;
using RRMS.Application.Exceptions;

namespace RRMS.Application.Tests;

public class TableServiceTests
{
    private readonly FakeRestaurantTableRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly TableService _service;

    public TableServiceTests()
    {
        _service = new TableService(_repository, _unitOfWork);
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
        Assert.Equal(7L, table.Id);
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

    [Fact]
    public async Task GetAllAsync_ReturnsAllTables()
    {
        _repository.Items.AddRange(new[]
        {
            new RestaurantTable
            {
                Id = 1, Number = 2, Capacity = 4, IsActive = true
            },
            new RestaurantTable
            {
                Id = 2, Number = 1, Capacity = 6, IsActive = false
            }
        });

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Number);
        Assert.Equal(2, result[1].Number);
        Assert.False(result[0].IsActive);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesTable()
    {
        var request = new RestaurantTableRequest(5, 4);

        var result = await _service.CreateAsync(request);

        Assert.Equal(5, result.Number);
        Assert.Equal(4, result.Capacity);
        Assert.True(result.IsActive);
        Assert.Single(_repository.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumber_Throws()
    {
        _repository.Items.Add(new RestaurantTable
        {
            Id = 1,
            Number = 5,
            Capacity = 4
        });

        var request = new RestaurantTableRequest(5, 6);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request));

        Assert.Single(_repository.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(10001, 4)]
    [InlineData(1, 0)]
    [InlineData(1, 1001)]
    public async Task CreateAsync_InvalidRequest_Throws(
        int number, int capacity)
    {
        var request = new RestaurantTableRequest(number, capacity);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(request));

        Assert.Empty(_repository.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTable_UpdatesFields()
    {
        _repository.Items.Add(new RestaurantTable
        {
            Id = 1,
            Number = 2,
            Capacity = 4,
            IsActive = true
        });

        var request = new RestaurantTableRequest(3, 8);

        var result = await _service.UpdateAsync(1, request);

        Assert.Equal(3, result.Number);
        Assert.Equal(8, result.Capacity);
        Assert.Equal(3, _repository.Items[0].Number);
        Assert.Equal(8, _repository.Items[0].Capacity);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_SameNumber_Succeeds()
    {
        _repository.Items.Add(new RestaurantTable
        {
            Id = 1,
            Number = 2,
            Capacity = 4
        });

        var result = await _service.UpdateAsync(
            1, new RestaurantTableRequest(2, 6));

        Assert.Equal(2, result.Number);
        Assert.Equal(6, result.Capacity);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateNumber_Throws()
    {
        _repository.Items.AddRange(new[]
        {
            new RestaurantTable
            {
                Id = 1, Number = 1, Capacity = 4
            },
            new RestaurantTable
            {
                Id = 2, Number = 2, Capacity = 6
            }
        });

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(
                1, new RestaurantTableRequest(2, 8)));

        Assert.Equal(1, _repository.Items[0].Number);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(
                999, new RestaurantTableRequest(1, 4)));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTable_Deactivates()
    {
        _repository.Items.Add(new RestaurantTable
        {
            Id = 1,
            Number = 3,
            Capacity = 4,
            IsActive = true
        });

        await _service.DeleteAsync(1);

        var table = Assert.Single(_repository.Items);

        Assert.False(table.IsActive);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteAsync(999));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public IUserRepository Users => null!;

        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
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
