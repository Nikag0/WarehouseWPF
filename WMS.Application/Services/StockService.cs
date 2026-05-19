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
        private readonly IRackRepository _rackRepo;
        private readonly ICellRepository _cellRepo;

        public StockService(
            IStockRepository stockRepo,
            IComponentRepository componentRepo,
            IRackRepository rackRepo,
            ICellRepository cellRepo)
        {
            _stockRepo = stockRepo;
            _componentRepo = componentRepo;
            _rackRepo = rackRepo;
            _cellRepo = cellRepo;
        }

        public async Task<List<ViewItemDTO>> GetAllAsync()
        {
            List<Stock> stocks = await _stockRepo.GetRawStockDataAsync();

            return stocks.Select(s => new ViewItemDTO(
                s.ComponentId,
                s.Component.Article,       // Данные из связанной таблицы Component
                s.Component.Name,          // Данные из связанной таблицы Component
                s.Component.Manufacturer,  // Данные из связанной таблицы Component
                s.RackId,
                s.Rack.RackCode,           // Данные из связанной таблицы Rack
                LocationFormatter.CodeToDisplay(s.Rack.Column, s.Rack.Row),
                s.CellId,
                s.Cell.CellCode,           // Данные из связанной таблицы Cell
                LocationFormatter.CodeToDisplay(s.Cell.Column, s.Cell.Row),
                s.Quantity,
                0
            )).ToList();
        }


        public async Task<Stock?> GetAsync(Guid  rackId, Guid componentId, Guid cellId)
        {
            return await _stockRepo.GetAsync(rackId, componentId, cellId);
        }

        public async Task AddAsync(Stock stock)
        {
            await _stockRepo.AddAsync(stock);
        }
        public async Task RemoveStock(Stock stock)
        {
            await _stockRepo.RemoveAsync(stock);
        }

        public async Task UpdateAsync(Stock stock)
        {  
            await _stockRepo.UpdateAsync(stock);
        }
    }
}
