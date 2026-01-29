//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using WMS.Infrastructure;

//var services = new ServiceCollection();

//var config = new ConfigurationBuilder()
//    .AddJsonFile("appsettings.json")
//    .Build();

//services.AddDbContext<WmsDbContext>(opt =>
//    opt.UseNpgsql(
//        config.GetConnectionString("Warehouse")));

//var sp = services.BuildServiceProvider();

//using var scope = sp.CreateScope();
//var db = scope.ServiceProvider.GetRequiredService<WmsDbContext>();

//await db.Database.MigrateAsync();

//Console.WriteLine("Database connected.");
