using RRMS.Application.DTOs.Menu;

namespace RRMS.Application.Interfaces.Services;

public interface IMenuService
{
    Task<IReadOnlyList<MenuItemResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MenuItemResponse> CreateAsync(MenuItemRequest request, CancellationToken cancellationToken = default);

    Task<MenuItemResponse> UpdateAsync(Guid id, MenuItemRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}