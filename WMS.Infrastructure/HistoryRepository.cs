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

        public void Add(History op, CancellationToken ct = default)
        {
            _db.History.Add(op);
        }

        public async Task<IReadOnlyList<HistoryItem>> GetFilteredAsync(
             string? searchText,
             DateTime? dateFrom,
             DateTime? dateTo,
             OperationType operationType,
             int maxCount,
             CancellationToken ct = default)
        {
            IQueryable<HistoryItem> query = _db.HistoryItems
                   .AsNoTracking()
                   .IgnoreQueryFilters()
                   .Include(x => x.Component)
                   .Include(x => x.Operation);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x =>
                    x.Component.Name.Contains(searchText) ||
                    x.Operation.Operator.Contains(searchText));
            }

            if (dateFrom.HasValue)
            {
                var utcFrom = DateTime.SpecifyKind(dateFrom.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.Operation.OccurredAt >= utcFrom);
            }

            if (dateTo.HasValue)
            {
                var utcTo = DateTime.SpecifyKind(dateTo.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.Operation.OccurredAt < utcTo);
            }

            if (operationType != OperationType.All)
            {
                query = query.Where(x => x.Operation.Type == operationType);
            }

            return await query
                 .OrderByDescending(o => o.Operation.OccurredAt)
                 .Take(maxCount)
                 .ToListAsync(ct);
        }
    }
}
