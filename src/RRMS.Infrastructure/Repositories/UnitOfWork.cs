using RRMS.Application.Interfaces.Repositories;
using RRMS.Infrastructure.Data;

namespace RRMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IUserRepository Users => throw new NotImplementedException("TODO: lazy-initialize UserRepository");

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("TODO: return await _dbContext.SaveChangesAsync(cancellationToken);");
}