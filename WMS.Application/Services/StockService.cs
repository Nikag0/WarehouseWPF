using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

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

            return stocks.Select(MappingExtensions.ToViewItemDto).ToList();
        }

        public async Task<List<ViewItemDTO>> GetFilteredStockAsync(string searchText, int maxCount)
        {
            List<Stock> stocks = await _stockRepo.GetFilteredStockAsync(searchText, maxCount);

            return stocks.Select(MappingExtensions.ToViewItemDto).ToList();
        }

        public async Task<IEnumerable<ViewItemDTO>> GetStocksInRackAsync(Guid rackId)
        {
            var stocks = await _stockRepo.GetStocksInRackAsync(rackId);

            return EnumerateItems(stocks);
        }

        public async Task<ViewItemDTO> GetStockAsync(Guid stockId)
        {
            var item = await _stockRepo.GetStockAsync(stockId);

            if (item == null)
                throw new BusinessException($"Остаток {stockId} не найден");

            return MappingExtensions.ToViewItemDto(item);
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

        private IEnumerable<ViewItemDTO> EnumerateItems(IEnumerable<Stock> stocks)
        {
            foreach (var stock in stocks)
            {
                yield return MappingExtensions.ToViewItemDto(stock);
            }
        }
    }
}
