
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
        CancellationToken cancellationToken = default)
    {
        var items = await _menuItemRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<MenuItemResponse> CreateAsync(
        MenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = new MenuItem
        {
            ItemName = request.ItemName.Trim(),
            Description = NormalizeDescription(request.Description),
            Price = request.Price
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
        item.Description = NormalizeDescription(request.Description);
        item.Price = request.Price;

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

    private static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

    private static MenuItemResponse Map(MenuItem item)
        => new(item.Id, item.ItemName, item.Description, item.Price);
}
