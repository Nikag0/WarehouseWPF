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

        public async Task<Stock?> GetByIdAsync(Guid stockId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .FirstOrDefaultAsync(x => x.Id == stockId);
        }

        public async Task<Stock?> GetByLocationAsync(Guid componentId, Guid rackId, Guid cellId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .FirstOrDefaultAsync(s => s.ComponentId == componentId &&
                                          s.RackId == rackId &&
                                          s.CellId == cellId);
        }

        public async Task<IReadOnlyList<Stock>> GetByRackAsync(Guid rackId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Stocks
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .Where(s => s.RackId == rackId)
                .ToListAsync();
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

        public async Task DeletAsync(Stock stock)
        {
            using var db = _factory.CreateDbContext();

            db.Stocks.Remove(stock);
            await db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Stock>> SearchAsync(string searchText, int maxCount)
        {
            using var db = _factory.CreateDbContext();

            var query = db.Stocks
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


        public async Task<IReadOnlyList<Stock>> GetMinQuantityAsync()
        {
            using var db = _factory.CreateDbContext();

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


            return await db.Stocks
                .FromSqlRaw(query)
                .AsNoTracking()
                .Include(s => s.Component)
                .Include(s => s.Rack)
                .Include(s => s.Cell)
                .ToListAsync();
        }

        //Можно подумать над реалзацией.
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

        public async Task<bool> HasStockWithQuantityAsync(Guid componentId)
        {
            using var context = _factory.CreateDbContext();
            return await context.Stocks
                .AnyAsync(s => s.ComponentId == componentId && s.Quantity > 0);
        }

    }
}
