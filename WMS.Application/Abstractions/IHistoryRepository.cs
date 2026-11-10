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
        
        Task<IReadOnlyList<OperationHistory>> GetFilteredAsync(
            string searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType operationType,
            int maxCount,
            CancellationToken ct = default);

        IAsyncEnumerable<OperationHistory> GetForPeriodAsync(DateTime dateFrom, DateTime dateTo);

        Task DeleteHistoryAsync(
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken ct = default);
    }
}
