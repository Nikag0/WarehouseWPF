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
        Task IssueAsync(IReadOnlyCollection<IssueItemDto> items, string operatorName, string? comment = null, CancellationToken ct = default);
        Task ReceiveAsync(ReceiptItemDto item, string operatorName, string? comment = null, CancellationToken ct = default);
        Task<IReadOnlyList<ViewItemDTO>> GetFilteredStockAsync(string searchText, int maxCount);
        Task<IReadOnlyList<ViewItemDTO>> GetStocksInRackAsync(Guid rackId);
        Task<ViewItemDTO> GetStockByIdAsync(Guid stokId);
    }
}