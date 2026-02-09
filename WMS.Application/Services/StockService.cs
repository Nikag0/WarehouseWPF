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
        private readonly IStockRepository _stockRepo;
        private readonly IComponentRepository _componentRepo;
        private readonly ICellRepository _cellRepo;

        public StockService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            ICellRepository cellRepo)
        {
            _stockRepo = stockRepo;
            _componentRepo = componentRepo;
            _cellRepo = cellRepo;
        }
            
        public async Task<List<StockDto>> GetAllAsync()
        {
            var stocks = await _stockRepo.GetAllAsync();
            var components = await _componentRepo.GetAllAsync();
            var cells = await _cellRepo.GetAllAsync();

            return stocks
                .Join(components, s => s.ComponentId, c => c.Id, (s, c) => new { s, c })
                .Join(cells, sc => sc.s.CellId, cell => cell.Id, (sc, cell) => new StockDto(
                    sc.c.Article,
                    sc.c.Name,
                    sc.c.Manufacturer,
                    cell.Code,
                    sc.s.Quantity)).ToList();
        }

        public async Task<Stock?> GetAsync(Guid componentId, Guid cellId)
        {
            return await _stockRepo.GetAsync(componentId, cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            await _stockRepo.AddAsync(stock);
        }

        public async Task UpdateAsync(Stock stock)
        {  
            await _stockRepo.UpdateAsync(stock);
        }
    }
}
