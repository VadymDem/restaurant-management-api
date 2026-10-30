using RRMS.Application.DTOs.Menu;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Services;
using RRMS.Domain.Entities;

namespace RRMS.Application.Tests;

public class MenuServiceTests
{
    private readonly FakeMenuItemRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly MenuService _service;

    public MenuServiceTests()
    {
        _service = new MenuService(_repository, _unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndMapsResponse()
    {
        var request = new MenuItemRequest { ItemName = "  Caesar Salad  ", Description = "Fresh", Price = 9.99m };

        var response = await _service.CreateAsync(request);

        Assert.Equal("Caesar Salad", response.ItemName);
        Assert.Equal("Fresh", response.Description);
        Assert.Equal(9.99m, response.Price);
        Assert.Single(_repository.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_NormalizesBlankDescriptionToNull(string? description)
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = description, Price = 5m };

        var response = await _service.CreateAsync(request);

        Assert.Null(response.Description);
        Assert.Null(_repository.Items.Single().Description);
    }

    [Fact]
    public async Task CreateAsync_TrimsDescription()
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = "  spicy  ", Price = 5m };

        var response = await _service.CreateAsync(request);

        Assert.Equal("spicy", response.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemMissing_ThrowsNotFound()
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = null, Price = 5m };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(999, request));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFields()
    {
        var existing = new MenuItem { Id = 1, ItemName = "Old", Price = 1m };
        _repository.Items.Add(existing);
        var request = new MenuItemRequest { ItemName = "New", Description = "  note  ", Price = 12.50m };

        var response = await _service.UpdateAsync(1, request);

        Assert.Equal("New", response.ItemName);
        Assert.Equal("note", response.Description);
        Assert.Equal(12.50m, response.Price);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemMissing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var existing = new MenuItem { Id = 7, ItemName = "Item", Price = 3m };
        _repository.Items.Add(existing);

        await _service.DeleteAsync(7);

        Assert.Empty(_repository.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsSortedByName()
    {
        _repository.Items.Add(new MenuItem { Id = 1, ItemName = "Banana", Price = 1m });
        _repository.Items.Add(new MenuItem { Id = 2, ItemName = "Apple", Price = 2m });

        var items = await _service.GetAllAsync();

        Assert.Equal(new[] { "Apple", "Banana" }, items.Select(i => i.ItemName));
    }

    [Fact]
    public async Task GetAllAsync_ExcludesUnavailableItemsByDefault()
    {
        _repository.Items.Add(new MenuItem { Id = 1, ItemName = "Visible", Price = 1m });
        _repository.Items.Add(new MenuItem { Id = 2, ItemName = "Hidden", Price = 2m, IsAvailable = false });

        var items = await _service.GetAllAsync();

        Assert.Equal(new[] { "Visible" }, items.Select(i => i.ItemName));
    }

    [Fact]
    public async Task GetAllAsync_IncludesUnavailableItemsWhenRequested()
    {
        _repository.Items.Add(new MenuItem { Id = 1, ItemName = "Visible", Price = 1m });
        _repository.Items.Add(new MenuItem { Id = 2, ItemName = "Hidden", Price = 2m, IsAvailable = false });

        var items = await _service.GetAllAsync(includeUnavailable: true);

        Assert.Equal(new[] { "Hidden", "Visible" }, items.Select(i => i.ItemName));
    }

    [Fact]
    public async Task CreateAsync_MapsCategoryImageUrlAndAvailability()
    {
        var request = new MenuItemRequest
        {
            ItemName = "Soup",
            Price = 4.5m,
            Category = "  Starters  ",
            ImageUrl = "  https://cdn.example.com/soup.png  "
        };

        var response = await _service.CreateAsync(request);

        Assert.Equal("Starters", response.Category);
        Assert.Equal("https://cdn.example.com/soup.png", response.ImageUrl);
        Assert.True(response.IsAvailable);
        Assert.Equal("Starters", _repository.Items.Single().Category);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateAsync_NormalizesBlankCategoryToNull(string? category)
    {
        var request = new MenuItemRequest
        {
            ItemName = "Soup",
            Price = 4.5m,
            Category = category
        };

        var response = await _service.CreateAsync(request);

        Assert.Null(response.Category);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAvailability()
    {
        _repository.Items.Add(new MenuItem { Id = 1, ItemName = "Soup", Price = 4.5m });
        var request = new MenuItemRequest
        {
            ItemName = "Soup",
            Price = 4.5m,
            IsAvailable = false
        };

        var response = await _service.UpdateAsync(1, request);

        Assert.False(response.IsAvailable);
        Assert.False(_repository.Items.Single().IsAvailable);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedItem()
    {
        _repository.Items.Add(new MenuItem
        {
            Id = 5,
            ItemName = "Soup",
            Price = 4.5m,
            Category = "Starters",
            IsAvailable = false
        });

        var response = await _service.GetByIdAsync(5);

        Assert.Equal(5, response.Id);
        Assert.Equal("Soup", response.ItemName);
        Assert.Equal("Starters", response.Category);
        Assert.False(response.IsAvailable);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemMissing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByIdAsync(999));
    }

    private sealed class FakeMenuItemRepository : IMenuItemRepository
    {
        public List<MenuItem> Items { get; } = new();

        private long _nextId = 1;

        public Task<IReadOnlyList<MenuItem>> GetAllAsync(
            bool includeUnavailable,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MenuItem>>(
                Items.Where(i => includeUnavailable || i.IsAvailable)
                    .OrderBy(i => i.ItemName)
                    .ToList());

        public Task<MenuItem?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(i => i.Id == id));

        public Task AddAsync(
            MenuItem menuItem,
            CancellationToken cancellationToken = default)
        {
            menuItem.Id = _nextId++;
            Items.Add(menuItem);
            return Task.CompletedTask;
        }

        public void Update(MenuItem menuItem)
        {
        }

        public void Delete(MenuItem menuItem)
        {
            Items.Remove(menuItem);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public IUserRepository Users => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
