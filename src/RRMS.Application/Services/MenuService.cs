using RRMS.Application.DTOs.Menu;
using RRMS.Application.Interfaces.Services;

namespace RRMS.Application.Services;

public class MenuService : IMenuService
{
    // TODO: inject IMenuItemRepository and IUnitOfWork via constructor.

    public Task<IReadOnlyList<MenuItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return all menu items mapped to MenuItemResponse.");

    public Task<MenuItemResponse> CreateAsync(MenuItemRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: create a MenuItem, save and return the created item.");

    public Task<MenuItemResponse> UpdateAsync(Guid id, MenuItemRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: update an existing MenuItem or throw a not-found error.");

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: delete an existing MenuItem or throw a not-found error.");
}