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

        public Task<Stock?> GetAsync(Guid componentId, Guid cellId)
        {
            return _db.CellStocks
                .FirstOrDefaultAsync(x =>
                    x.ComponentId == componentId &&
                    x.CellId == cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            _db.CellStocks.Add(stock);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            _db.CellStocks.Update(stock);
            await _db.SaveChangesAsync();
        }
    }
}
