using RRMS.Application.DTOs.Menu;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Domain.Entities;

namespace RRMS.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MenuService(
        IMenuItemRepository menuItemRepository,
        IUnitOfWork unitOfWork)
    {
        _menuItemRepository = menuItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MenuItemResponse>> GetAllAsync(
        bool includeUnavailable = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _menuItemRepository.GetAllAsync(
            includeUnavailable, cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<MenuItemResponse> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var item = await _menuItemRepository.GetByIdAsync(
            id, cancellationToken)
            ?? throw new NotFoundException(
                $"Menu item with id '{id}' was not found.");

        return Map(item);
    }

    public async Task<MenuItemResponse> CreateAsync(
        MenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = new MenuItem
        {
            ItemName = request.ItemName.Trim(),
            Description = Normalize(request.Description),
            Price = request.Price,
            Category = Normalize(request.Category),
            ImageUrl = Normalize(request.ImageUrl),
            IsAvailable = request.IsAvailable
        };

        await _menuItemRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task<MenuItemResponse> UpdateAsync(
        long id,
        MenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _menuItemRepository.GetByIdAsync(
            id, cancellationToken)
            ?? throw new NotFoundException(
                $"Menu item with id '{id}' was not found.");

        item.ItemName = request.ItemName.Trim();
        item.Description = Normalize(request.Description);
        item.Price = request.Price;
        item.Category = Normalize(request.Category);
        item.ImageUrl = Normalize(request.ImageUrl);
        item.IsAvailable = request.IsAvailable;

        _menuItemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var item = await _menuItemRepository.GetByIdAsync(
            id, cancellationToken)
            ?? throw new NotFoundException(
                $"Menu item with id '{id}' was not found.");

        _menuItemRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();

    private static MenuItemResponse Map(MenuItem item)
        => new(
            item.Id,
            item.ItemName,
            item.Description,
            item.Price,
            item.Category,
            item.ImageUrl,
            item.IsAvailable);
}
