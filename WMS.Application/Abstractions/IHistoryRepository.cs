using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Abstractions
{
    public interface IHistoryRepository
    {
        void Add(OperationHistory operation, CancellationToken ct = default);
        
        // Границы периода — моменты времени в UTC: fromUtc включительно, toUtc не включительно.
        // Перевод местных суток в UTC делает HistoryService.
        Task<IReadOnlyList<OperationHistory>> GetFilteredAsync(
            string? searchText,
            DateTime? fromUtc,
            DateTime? toUtc,
            OperationType? operationType,
            int maxCount,
            CancellationToken ct = default);

        IAsyncEnumerable<OperationHistory> GetForPeriodAsync(DateTime fromUtc, DateTime toUtc);

        Task DeleteHistoryAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default);
    }
}
