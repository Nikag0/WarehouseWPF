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
        void Add(History operation, CancellationToken ct = default);
        Task<IReadOnlyList<HistoryItem>> GetFilteredAsync(
            string searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType operationType,
            int maxCount,
            CancellationToken ct = default);
    }
}
