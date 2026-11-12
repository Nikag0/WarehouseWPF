using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WMS.Application;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure
{
    public class StockRepository : IStockRepository
    {
        private readonly IDbContextAccessor _contextAccessor;
        public StockRepository(IDbContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        private AppDbContext _db => _contextAccessor.CurrentContext;

        public async Task<Stock?> GetByIdAsync(Guid stockId, CancellationToken ct = default)
        {
            return await _db.Stock
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .FirstOrDefaultAsync(x => x.Id == stockId, ct);
        }

        public async Task<Stock?> GetByLocationAsync(Guid componentId, Guid rackId, Guid cellId, CancellationToken ct = default)
        {
            return await _db.Stock
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .FirstOrDefaultAsync(s => s.ComponentId == componentId &&
                                            s.RackId == rackId &&
                                            s.CellId == cellId, ct);
        }

        public async Task<IReadOnlyList<Stock>> GetByRackAsync(Guid rackId, CancellationToken ct = default)
        {
            return await _db.Stock
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .Where(s => s.RackId == rackId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Stock>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            return await _db.Stock
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .Where(s => ids.Contains(s.Id))
                .ToListAsync(ct);
        }

        public void Add(Stock stock, CancellationToken ct = default)
        {
            _db.Stock.Add(stock);
        }

        public void Update(Stock stock, CancellationToken ct = default)
        {
            _db.Stock.Update(stock);
        }

        public void Delete(Stock stock, CancellationToken ct = default)
        {
            _db.Stock.Remove(stock);
        }

        public async Task<IReadOnlyList<Stock>> SearchAsync(string searchText, int maxCount, CancellationToken ct = default)
        {
            var query = _db.Stock
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
                    FROM ""stock"" st
                ) s
                INNER JOIN ""component"" c ON s.""ComponentId"" = c.""Id""
                WHERE s.""TotalComponentQuantity"" <= c.""MinQuantity""";


            return await _db.Stock
                .FromSqlRaw(query)
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .ToListAsync();
        }

        public IAsyncEnumerable<Stock> GetAllForExportAsync(CancellationToken ct = default)
        {
            return _db.Stock
                .AsNoTracking() // Отключаем отслеживание сущностей ради экономии памяти
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .AsAsyncEnumerable(); // Переводим в асинхронный поток (стриминг)
        }

        public async Task<bool> HasAnyStockAsync(Guid componentId, CancellationToken ct = default)
        {
            return await _db.Stock
                .AsNoTracking()
                .AnyAsync(s => s.ComponentId == componentId, ct);
        }
    }
}
