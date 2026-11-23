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
        Task<IReadOnlyList<StockAvailabilityDto>> GetAvailabilityAsync(CancellationToken token);
        Task<IReadOnlyList<StockViewDTO>> GetFilteredStockAsync(string searchText, int maxCount, CancellationToken token = default, Guid? rackId = null, Guid? cellId = null);
        // rackId / cellId — искать только в этом стеллаже / ячейке; пустой текст тогда вернёт всё их содержимое
        Task<IReadOnlyList<StockViewDTO>> GetFilteredStockOrComponentsAsync(string searchText, int maxCount, Guid? rackId, Guid? cellId, CancellationToken token);
        Task<StockViewDTO?> GetStockByIdAsync(Guid stokId);
        Task ExportCsvAsync(string filepath, CancellationToken ct = default);
    }
}
