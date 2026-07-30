using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WMS.Infrastructure;

public class WmsDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=Warehouse;Username=postgres;Password=admin")
            .Options;

        return new AppDbContext(options);
    }
}
