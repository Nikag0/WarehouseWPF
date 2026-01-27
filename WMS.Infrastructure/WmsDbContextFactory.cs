using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WMS.Infrastructure;

public class WmsDbContextFactory
    : IDesignTimeDbContextFactory<WmsDbContext>
{
    public WmsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WmsDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=Warehouse;Username=postgres;Password=1234")
            .Options;

        return new WmsDbContext(options);
    }
}
