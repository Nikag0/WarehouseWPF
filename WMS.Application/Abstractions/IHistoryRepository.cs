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
        Task AddAsync(Operation operation);
        Task<IReadOnlyList<OperationItem>> GetFilteredAsync(
            string searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType? operationType,
            int maxCount,
            CancellationToken token);
    }
}
