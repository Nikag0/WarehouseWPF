using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using WMS.Domain;
using Component = WMS.Domain.Component;

namespace WMS.Infrastructure
{
    public class WmsDbContext : DbContext
    {
        public DbSet<Component> Components => Set<Component>();
        public DbSet<Stock> CellStocks => Set<Stock>();
        public DbSet<Operation> Operations => Set<Operation>();
        public DbSet<OperationItem> OperationItems => Set<OperationItem>();

        public WmsDbContext(DbContextOptions<WmsDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(WmsDbContext).Assembly);
        }
    }
}
