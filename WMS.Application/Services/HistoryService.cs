using System.Collections.ObjectModel;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class HistoryService
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IUnitOfWorkFactory _uowFactory;

        public HistoryService(IHistoryRepository historyRepository, IUnitOfWorkFactory uowFactory)
        {
            _historyRepository = historyRepository;
            _uowFactory = uowFactory;
        }

        public async Task<IReadOnlyList<HistoryDto>> GetFilteredAsync(
            string? searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType operationType,
            int maxCount,
            CancellationToken token)
        {
            await using var uow = _uowFactory.Create();

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
