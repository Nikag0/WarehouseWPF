using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class StockRepository : IStockRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public StockRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<Stock>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks.ToListAsync();
        }

        public async Task<List<Stock>> GetRawStockDataAsync()
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking() // Отключаем кэш отслеживания для скорости чтения
                .Include(s => s.Component) // SQL INNER JOIN к таблице Components
                .Include(s => s.Rack)      // SQL INNER JOIN к таблице Racks
                .Include(s => s.Cell)      // SQL INNER JOIN к таблице Cells
                .ToListAsync();
        }

        public async Task<Stock?> GetAsync(Guid rackId ,Guid componentId, Guid cellId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ComponentId == componentId &&
                    x.RackId == rackId &&
                    x.CellId == cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Add(stock);
            await db.SaveChangesAsync();
        }
        public async Task RemoveAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Remove(stock);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Update(stock);
            await db.SaveChangesAsync();
        }

        public async Task<bool> HasStockWithQuantityAsync(Guid componentId)
        {
            using var context = _factory.CreateDbContext();
            return await context.Stocks
                .AnyAsync(s => s.ComponentId == componentId && s.Quantity > 0);
        }
    }
}
