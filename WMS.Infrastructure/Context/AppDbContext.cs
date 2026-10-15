using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using WMS.Domain;
using WMS.Domain.LedStrip;
using Component = WMS.Domain.Component;

namespace WMS.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Component> Component => Set<Component>();
        public DbSet<Stock> Stock => Set<Stock>();
        //public DbSet<History> History => Set<History>();
        //public DbSet<HistoryItem> HistoryItems => Set<HistoryItem>();
        public DbSet<OperationHistory> OperationHistory => Set<OperationHistory>();
        public DbSet<Cell> Cell => Set<Cell>();
        public DbSet<Rack> Rack => Set<Rack>();
        public DbSet<Operator> Operator => Set<Operator>();
        public DbSet<Microcontroller> Microcontroller => Set<Microcontroller>();
        public DbSet<Strip> Strip => Set<Strip>();
        public DbSet<Sector> Sector => Set<Sector>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
    }
}
