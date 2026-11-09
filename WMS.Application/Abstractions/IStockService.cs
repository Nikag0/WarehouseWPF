using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.DTO;

namespace WMS.Application.Abstractions
{
    public interface IStockService
    {
        Task IssueAsync(IReadOnlyCollection<IssueStockDto> items, string operatorName, string? comment = null, CancellationToken ct = default);
        Task ReceiveAsync(ReceiptItemDto item, string operatorName, string? comment = null, CancellationToken ct = default);
        Task<IReadOnlyList<StockViewDTO>> GetFilteredStockAsync(string searchText, int maxCount);
        Task<IReadOnlyList<StockViewDTO>> GetFilteredStockOrComponentsAsync(string searchText, int maxCount, CancellationToken token);
        Task<IReadOnlyList<StockViewDTO>> GetStocksInRackAsync(Guid rackId);
        Task<StockViewDTO?> GetStockByIdAsync(Guid stokId);
        Task ExportCsvAsync(string filepath, CancellationToken ct = default);
    }
}