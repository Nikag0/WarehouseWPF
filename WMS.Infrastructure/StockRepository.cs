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
        private readonly WmsDbContext _db;

        public StockRepository(WmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<StockItemDto>> GetAllAsync()
        {
            var query = from s in _db.Stocks
                        join c in _db.Components on s.ComponentId equals c.Id
                        join cell in _db.Cells on s.CellId equals cell.Id
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
            return _db.Stocks
                .FirstOrDefaultAsync(x =>
                    x.ComponentId == componentId &&
                    x.CellId == cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            _db.Stocks.Add(stock);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            _db.Stocks.Update(stock);
            await _db.SaveChangesAsync();
        }
    }
}
