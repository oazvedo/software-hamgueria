using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SoftwareHamgueria.Infrastructure.Data;

// Design-time factory used only by `dotnet ef` so migration commands don't have
// to boot the API host (Program.cs). Connection string mirrors appsettings.json.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=hamgueria;Username=hamgueria;Password=hamgueria123")
            .Options;
        return new AppDbContext(options);
    }
}
