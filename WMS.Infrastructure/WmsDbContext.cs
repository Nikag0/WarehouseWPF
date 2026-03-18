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
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<Operation> Operations => Set<Operation>();
        public DbSet<OperationItem> OperationItems => Set<OperationItem>();
        public DbSet<Cell> Cells => Set<Cell>();
        public DbSet<Rack> Racks => Set<Rack>();
        public DbSet<Operator> Operators => Set<Operator>();

        public WmsDbContext(DbContextOptions<WmsDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(WmsDbContext).Assembly);
        }
    }
}
