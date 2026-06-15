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
        Task<List<Stock>> GetRawStockDataAsync();
        Task<Stock?> GetAsync(Guid rackId, Guid componentId, Guid cellId);
        Task AddAsync(Stock stock);
        Task UpdateAsync(Stock stock);
        Task RemoveAsync(Stock stock);
        Task<bool> HasStockWithQuantityAsync(Guid componentId);
        Task<List<Stock>> GetFilteredStockAsync(string searchText, int maxCount);
    }
}
