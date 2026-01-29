using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class StockService
    {
        private readonly IStockRepository _repo;

        public StockService(IStockRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<StockItemDto>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Stock?> GetAsync(Guid componentId, Guid cellId)
        {
            return await _repo.GetAsync(componentId, cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            await _repo.AddAsync(stock);
        }

        public async Task UpdateAsync(Stock stock)
        {  
            await _repo.UpdateAsync(stock);
        }
    }
}
