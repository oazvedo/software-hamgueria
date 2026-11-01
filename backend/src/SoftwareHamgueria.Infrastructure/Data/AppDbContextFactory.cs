using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SoftwareHamgueria.Infrastructure.Data;

// Design-time factory used only by `dotnet ef` so migration commands don't have
// to boot the API host (Program.cs). Uses ConnectionStrings__Postgres when set,
// otherwise the local dev database (appsettings.Development.json, port 5433).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                       ?? "Host=localhost;Port=5433;Database=hamgueria;Username=hamgueria;Password=hamgueria123")
            .Options;
        return new AppDbContext(options);
    }
}
