using Microsoft.EntityFrameworkCore;
using System.Threading;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class HistoryRepository : IHistoryRepository
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

        public async Task<IReadOnlyList<OperationItem>> GetFilteredAsync(
             string? searchText,
             DateTime? dateFrom,
             DateTime? dateTo,
             OperationType? operationType,
             int maxCount,
             CancellationToken token)
        {
            using var db = _factory.CreateDbContext();

            IQueryable<OperationItem> query = db.OperationItems
                   .AsNoTracking()
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
                query = query.Where(x => x.Operation.OccurredAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(x => x.Operation.OccurredAt <= dateTo.Value);
            }

            if (operationType.HasValue)
            {
                query = query.Where(x => x.Operation.Type == operationType.Value);
            }

            return await query
                 .OrderByDescending(o => o.Operation.OccurredAt)
                 .Take(maxCount)
                 .ToListAsync(token);
        }

        //public async Task<IReadOnlyList<Operation>> GetFilteredAsync(
        //    string searchText, 
        //    DateTime? dateFrom,
        //    DateTime? dateTo,
        //    OperationType? operationType,
        //    int maxCount,
        //    CancellationToken token)
        //{
        //    using var db = _factory.CreateDbContext();

        //    IQueryable<Operation> query = db.Operations.AsNoTracking();

        //    if (operationType.HasValue)
        //    {
        //        query = query.Where(o => o.Type == operationType.Value);
        //    }

        //    if (dateFrom.HasValue)
        //    {
        //        query = query.Where(o => o.OccurredAt >= dateFrom.Value);
        //    }

        //    if (dateTo.HasValue)
        //    {
        //        var endDate = dateTo.Value.Date.AddDays(1);

        //        query = query.Where(o => o.OccurredAt < endDate);
        //    }

        //    if (!string.IsNullOrWhiteSpace(searchText))
        //    {
        //        var text = searchText.Trim();

        //        query = query.Where(o =>
        //            o.Items.Any(i =>
        //                EF.Functions.ILike(i.Component.Name, $"%{text}%") ||
        //                EF.Functions.ILike(i.Component.Article, $"%{text}%") ||
        //                EF.Functions.ILike(i.Component.Manufacturer, $"%{text}%")));
        //    }

        //    return await query
        //        .Include(o => o.Items)
        //        .ThenInclude(i => i.Component)
        //        .OrderByDescending(o => o.OccurredAt)
        //        .Take(maxCount)
        //        .ToListAsync(token);
        //}
    }
}
