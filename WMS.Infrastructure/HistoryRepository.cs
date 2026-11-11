using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType operationType,
            int maxCount,
            CancellationToken ct = default)
        {
            IQueryable<OperationHistory> query = _db.OperationHistory
                   .AsNoTracking()
                   .IgnoreQueryFilters();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x =>
                    x.ComponentName.Contains(searchText) ||
                    x.Operator.Contains(searchText) ||
                    x.CellName.Contains(searchText) ||
                    x.RackName.Contains(searchText));
            }

            if (dateFrom.HasValue)
            {
                var utcFrom = DateTime.SpecifyKind(dateFrom.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.OccurredAt >= utcFrom);
            }

            if (dateTo.HasValue)
            {
                var utcTo = DateTime.SpecifyKind(dateTo.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.OccurredAt < utcTo);
            }

            if (operationType != OperationType.All)
            {
                query = query.Where(x => x.Type == operationType);
            }

            return await query
                 .OrderByDescending(o => o.OccurredAt)
                 .Take(maxCount)
                 .ToListAsync(ct);
        }

        public IAsyncEnumerable<OperationHistory> GetForPeriodAsync(DateTime dateFrom, DateTime dateTo)
        {
            return QueryByPeriod(dateFrom, dateTo)
                .AsNoTracking()
                .OrderBy(h => h.OccurredAt)
                .AsAsyncEnumerable();
        }

        public async Task DeleteHistoryAsync(
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken ct = default)
        {
            await QueryByPeriod(dateFrom, dateTo).ExecuteDeleteAsync(ct);
        }

        // Общий фильтр периода для выгрузки и удаления: в CSV попадают ровно те записи, которые будут удалены
        private IQueryable<OperationHistory> QueryByPeriod(DateTime dateFrom, DateTime dateTo)
        {
            var utcFrom = DateTime.SpecifyKind(dateFrom.Date, DateTimeKind.Utc);
            var utcTo = DateTime.SpecifyKind(dateTo.Date.AddDays(1), DateTimeKind.Utc);

            return _db.OperationHistory.Where(h => h.OccurredAt >= utcFrom && h.OccurredAt < utcTo);
        }
    }
}
