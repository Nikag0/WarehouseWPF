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
        Task<IReadOnlyList<Stock>> GetAllAsync();
        Task<Stock?> GetByIdAsync(Guid stockId);
        Task<Stock?> GetByLocationAsync(Guid componentId, Guid rackId, Guid cellId);
        Task<IReadOnlyList<Stock>> GetByRackAsync(Guid rackId);
        Task<IReadOnlyList<Stock>> SearchAsync(string searchText, int maxCount);
        Task AddAsync(Stock stock);
        Task UpdateAsync(Stock stock);
        Task DeletAsync(Stock stock);

        //Можно подумать над реалзацией.
        Task<List<Stock>> GetRawStockDataAsync();
        Task<bool> HasStockWithQuantityAsync(Guid componentId);
    }
}
