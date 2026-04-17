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
            var stocks = await _stockRepo.GetAllAsync();
            var components = await _componentRepo.GetAllAsync();
            var racks = await _rackRepo.GetAllAsync();
            var cells = await _cellRepo.GetAllAsync();

            var result =
                 from s in stocks
                 join c in components on s.ComponentId equals c.Id
                 join rack in racks on s.RackId equals rack.Id
                 join cell in cells on s.CellId equals cell.Id
                 select new ViewItemDTO(
                     s.ComponentId,
                     c.Article,
                     c.Name,
                     c.Manufacturer,
                     s.RackId,
                     rack.RackCode,
                     LocationFormatter.CodeToDisplay(rack.Column, rack.Row),
                     s.CellId,
                     cell.CellCode,
                     LocationFormatter.CodeToDisplay(cell.Column, cell.Row),
                     s.Quantity,
                     0);

            return result.ToList();
        }

        public async Task RemoveStock(Stock stock)
        {
            await _stockRepo.RemoveAsync(stock);
        }

        public async Task<Stock?> GetAsync(Guid  rackId, Guid componentId, Guid cellId)
        {
            return await _stockRepo.GetAsync(rackId, componentId, cellId);
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
