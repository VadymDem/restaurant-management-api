using RRMS.Application.Interfaces.Repositories;
using RRMS.Domain.Entities;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return await _dbContext.Users.FindAsync(...)");

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: singleOrDefault by a case-insensitive email match.");

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: any() by email.");

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: await _dbContext.Users.AddAsync(user);");

    public void Update(User user)
        => throw new NotImplementedException("TODO: _dbContext.Users.Update(user);");

    public void Delete(User user)
        => throw new NotImplementedException("TODO: _dbContext.Users.Remove(user);");
}