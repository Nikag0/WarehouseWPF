using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Abstractions
{
    public interface IStockRepository
    {
        Task<Stock?> GetByIdAsync(Guid stockId, CancellationToken ct = default);
        Task<Stock?> GetByLocationAsync(Guid componentId, Guid rackId, Guid cellId, CancellationToken ct = default);
        Task<IReadOnlyList<Stock>> GetByRackAsync(Guid rackId, CancellationToken ct = default);
        Task<IReadOnlyList<Stock>> SearchAsync(string searchText, int maxCount, CancellationToken ct = default);
        Task<IReadOnlyList<Stock>> GetMinQuantityAsync(CancellationToken ct = default);
        Task AddAsync(Stock stock, CancellationToken ct = default);
        Task UpdateAsync(Stock stock, CancellationToken ct = default);
        Task DeletAsync(Stock stock, CancellationToken ct = default);

        //Можно подумать над реалзацией.
        Task<bool> HasStockWithQuantityAsync(Guid componentId, CancellationToken ct = default);
    }
}
