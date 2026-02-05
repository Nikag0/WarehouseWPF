using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<StockItemDto>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();

            var query = from s in db.Stocks
                        join c in db.Components on s.ComponentId equals c.Id
                        join cell in db.Cells on s.CellId equals cell.Id
                        where s.Quantity > 0
                        select new StockItemDto(
                            c.Article,
                            c.Name,
                            cell.Code,
                            s.Quantity
                        );

            return await query.ToListAsync();
        }

        public Task<Stock?> GetAsync(Guid componentId, Guid cellId)
        {

            using var db = _factory.CreateDbContext();

            return db.Stocks
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
    }
}
