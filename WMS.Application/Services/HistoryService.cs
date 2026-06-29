using System.Collections.ObjectModel;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class HistoryService
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<IReadOnlyList<HistoryDto>> GetFilteredAsync(
            string? searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType? operationType,
            int maxCount,
            CancellationToken token)
        {
            var operationItem = await _historyRepository.GetFilteredAsync(
                searchText,
                dateFrom,
                dateTo,
                operationType,
                maxCount,
                token);

            return operationItem.Select(MappingExtensions.ToHistoryDTO).ToList();
        }
    }
}
