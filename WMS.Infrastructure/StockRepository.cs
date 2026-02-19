using Microsoft.EntityFrameworkCore;
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

        public async Task<Stock?> GetAsync(Guid componentId, Guid cellId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ComponentId == componentId &&
                    x.CellId == cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Add(stock);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Update(stock);
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Remove(stock);
            await db.SaveChangesAsync();
        }
    }
}
