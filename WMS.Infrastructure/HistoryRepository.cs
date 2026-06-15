using Microsoft.EntityFrameworkCore;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class HistoryRepository : IOperationRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public HistoryRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(Operation op)
        {
            using var db = _factory.CreateDbContext();

            db.Operations.Add(op);
            await db.SaveChangesAsync();
        }

        public async Task<List<OperationHistoryDto>> GetFilteredHistoryAsync(string searchText, int maxCount = 100)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Operations
                .SelectMany(o => o.Items, (o, item) => new { o, item });

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchPattern = $"%{searchText}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.o.Operator, searchPattern) ||

                    db.Components.Any(c => c.Id == x.item.ComponentId && EF.Functions.ILike(c.Name, searchPattern))
                );
            }

            // 3. Сортируем по дате (сначала новые), ограничиваем до 100 записей и трансформируем в DTO
            var result = await query
                .OrderByDescending(x => x.o.OccurredAt)
                .Take(maxCount)
                .Select(x => new OperationHistoryDto
                {
                    OperationId = x.o.Id,
                    OccurredAt = x.o.OccurredAt,
                    Operator = x.o.Operator,
                    OperationType = LocationFormatter.NumToOperation(((int)x.o.Type)),
                    Comment = x.o.Comment,
                    QuantityBefore = x.item.QuantityBefore,
                    QuantityAfter = x.item.QuantityAfter,

                    // Подтягиваем название компонента из таблицы компонентов по его ID
                    ComponentName = db.Components
                        .Where(c => c.Id == x.item.ComponentId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "Неизвестный компонент"
                })
                .ToListAsync();

            return result;
        }
    }
}
