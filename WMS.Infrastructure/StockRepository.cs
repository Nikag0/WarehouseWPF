using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
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
        private readonly AppDbContext _db;
        private readonly ILogger<StockRepository> _logger;

        public StockRepository(AppDbContext db, ILogger<StockRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Stock?> GetByIdAsync(Guid stockId, CancellationToken ct = default)
        {
            try
            {
                var stock = await _db.Stocks
                    .AsNoTracking()
                    .Include(s => s.Component)
                    .Include(s => s.Rack)
                    .Include(s => s.Cell)
                    .FirstOrDefaultAsync(x => x.Id == stockId, ct)
                    .ConfigureAwait(false);

                if (stock is null)
                    _logger.LogWarning("Stock {StockId} not found", stockId);

                return stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error fetching stock {StockId}", stockId);
                throw; 
            }
        }

        public async Task<Stock?> GetByLocationAsync(Guid componentId, Guid rackId, Guid cellId, CancellationToken ct = default)
        {
            try
            {
                return await _db.Stocks
                    .AsNoTracking()
                    .Include(s => s.Component)
                    .Include(s => s.Rack)
                    .Include(s => s.Cell)
                    .FirstOrDefaultAsync(s => s.ComponentId == componentId &&
                                                s.RackId == rackId &&
                                                s.CellId == cellId, ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Database error fetching stock by location. Component:{ComponentId}, Rack:{RackId}, Cell:{CellId}",
                    componentId, rackId, cellId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Stock>> GetByRackAsync(Guid rackId, CancellationToken ct = default)
        {
            return await _db.Stocks
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .Where(s => s.RackId == rackId)
                .ToListAsync();
        }

        public void Add(Stock stock, CancellationToken ct = default)
        {

            try
            {
                _db.Stocks.Add(stock);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add stock {Name}", stock.Id);
                throw;
            }
        }

        public void Update(Stock stock, CancellationToken ct = default)
        {
            _db.Stocks.Update(stock);
        }

        public void Delet(Stock stock, CancellationToken ct = default)
        {
            _db.Stocks.Remove(stock);
        }

        public async Task<IReadOnlyList<Stock>> SearchAsync(string searchText, int maxCount, CancellationToken ct = default)
        {
            var query = _db.Stocks
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .Where(s => s.Quantity > 0);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim().ToLower();

                query = query.Where(s =>
                    (s.Component.Article != null && s.Component.Article.ToLower().Contains(text)) ||
                    (s.Component.Name != null && s.Component.Name.ToLower().Contains(text)) ||
                    (s.Component.Manufacturer != null && s.Component.Manufacturer.ToLower().Contains(text))
                );
            }

            return await query
                .OrderBy(s => s.Component.Name)
                .Take(maxCount)
                .ToListAsync();
        }


        public async Task<IReadOnlyList<Stock>> GetMinQuantityAsync(CancellationToken ct = default)
        {
            // Экранируем кавычки для PostgreSQL, чтобы сохранить оригинальный регистр EF Core
            var query = @"
                SELECT s.""Id"", s.""ComponentId"", s.""RackId"", s.""CellId"", s.""Quantity""
                FROM (
                    SELECT st.*, 
                            SUM(st.""Quantity"") OVER(PARTITION BY st.""ComponentId"") as ""TotalComponentQuantity""
                    FROM ""stocks"" st
                ) s
                INNER JOIN ""components"" c ON s.""ComponentId"" = c.""Id""
                WHERE c.""IsDeleted"" = FALSE 
                    AND s.""TotalComponentQuantity"" <= c.""MinQuantity""";


            return await _db.Stocks
                .FromSqlRaw(query)
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .ToListAsync();
        }

        //Можно подумать над реалзацией.

        public async Task<bool> HasStockWithQuantityAsync(Guid componentId, CancellationToken ct = default)
        {
            return await _db.Stocks
                .AnyAsync(s => s.ComponentId == componentId && s.Quantity > 0);
        }

    }
}
