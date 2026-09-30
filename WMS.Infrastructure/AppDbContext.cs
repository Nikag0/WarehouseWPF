using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using WMS.Domain;
using WMS.Domain.LedStrip;
using Component = WMS.Domain.Component;

namespace WMS.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<Component> Components => Set<Component>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<History> History => Set<History>();
        public DbSet<HistoryItem> HistoryItems => Set<HistoryItem>();
        public DbSet<Cell> Cells => Set<Cell>();
        public DbSet<Rack> Racks => Set<Rack>();
        public DbSet<Operator> Operators => Set<Operator>();
        public DbSet<Microcontroller> Microcontrollers => Set<Microcontroller>();
        public DbSet<Strip> Strips => Set<Strip>();
        public DbSet<Sector> Sectors => Set<Sector>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
    }
}
