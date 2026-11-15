using Microsoft.EntityFrameworkCore;
using System.Threading;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure
{
    public class HistoryRepository : IHistoryRepository
    {

        private readonly IDbContextAccessor _contextAccessor;

        public HistoryRepository(IDbContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        private AppDbContext _db => _contextAccessor.CurrentContext;

        public void Add(OperationHistory op, CancellationToken ct = default)
        {
            _db.OperationHistory.Add(op);
        }

        public async Task<IReadOnlyList<OperationHistory>> GetFilteredAsync(
            string? searchText,
            DateTime? fromUtc,
            DateTime? toUtc,
            OperationType? operationType,
            int maxCount,
            CancellationToken ct = default)
        {
            IQueryable<OperationHistory> query = _db.OperationHistory
                   .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // ILike — поиск без учёта регистра, как в поиске компонентов: «иванов» найдёт «Иванов И. И.»
                var pattern = $"%{searchText.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.ComponentName, pattern) ||
                    EF.Functions.ILike(x.Operator, pattern) ||
                    EF.Functions.ILike(x.CellName, pattern) ||
                    EF.Functions.ILike(x.RackName, pattern));
            }

            if (fromUtc.HasValue)
            {
                var from = fromUtc.Value;
                query = query.Where(x => x.OccurredAt >= from);
            }

            if (toUtc.HasValue)
            {
                var to = toUtc.Value;
                query = query.Where(x => x.OccurredAt < to);
            }

            if (operationType.HasValue)
            {
                query = query.Where(x => x.Type == operationType.Value);
            }

            return await query
                 .OrderByDescending(o => o.OccurredAt)
                 .Take(maxCount)
                 .ToListAsync(ct);
        }

        public IAsyncEnumerable<OperationHistory> GetForPeriodAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            return QueryByPeriod(fromUtc, toUtc)
                .AsNoTracking()
                .OrderBy(h => h.OccurredAt)
                .StreamAsync(ct);
        }

        public async Task DeleteHistoryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default)
        {
            await QueryByPeriod(fromUtc, toUtc).ExecuteDeleteAsync(ct);
        }

        // Общий фильтр периода для выгрузки и удаления: в CSV попадают ровно те записи, которые будут удалены
        private IQueryable<OperationHistory> QueryByPeriod(DateTime fromUtc, DateTime toUtc)
        {
            return _db.OperationHistory.Where(h => h.OccurredAt >= fromUtc && h.OccurredAt < toUtc);
        }
    }
}
