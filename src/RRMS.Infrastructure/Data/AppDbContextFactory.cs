using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RRMS.Infrastructure.Data;

/// <summary>
/// Design-time factory used by the EF Core CLI (dotnet ef migrations add/update).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
        => throw new NotImplementedException(
            "TODO: build a DbContextOptions<AppDbContext> pointing to the Supabase PostgreSQL connection string " +
            "(e.g. read it from the SUPABASE_CONNECTION_STRING environment variable or appsettings) and return a new AppDbContext.");
}