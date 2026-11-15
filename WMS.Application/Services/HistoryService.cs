using System.Collections.ObjectModel;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class HistoryService
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IUnitOfWorkFactory _uowFactory;

        // Часовой пояс склада. Операции хранятся в UTC, а пользователь видит и выбирает даты по местному времени.
        private readonly TimeZoneInfo _timeZone;

        public HistoryService(IHistoryRepository historyRepository, IUnitOfWorkFactory uowFactory, TimeZoneInfo? timeZone = null)
        {
            _historyRepository = historyRepository;
            _uowFactory = uowFactory;
            _timeZone = timeZone ?? TimeZoneInfo.Local;
        }

        public async Task<IReadOnlyList<OperationHystoryViewDto>> GetFilteredAsync(
            string? searchText,
            DateTime? dateFrom,
            DateTime? dateTo,
            OperationType? operationType,
            int maxCount,
            CancellationToken token)
        {
            await using var uow = _uowFactory.Create();

            var operationItem = await _historyRepository.GetFilteredAsync(
                searchText,
                dateFrom.HasValue ? DayStartUtc(dateFrom.Value) : null,
                dateTo.HasValue ? DayStartUtc(dateTo.Value.AddDays(1)) : null,
                operationType,
                maxCount,
                token);

            return operationItem.Select(o => o.OperationHystoryToViewDTO(_timeZone)).ToList();
        }

        public async Task DeleteHistoryAsync(
           DateTime dateFrom,
           DateTime dateTo,
           CancellationToken ct = default)
        {
            await using var uow = _uowFactory.Create();

            await _historyRepository.DeleteHistoryAsync(DayStartUtc(dateFrom), DayStartUtc(dateTo.AddDays(1)), ct);
        }

        public async Task ExportCsvAsync(
            string filePath,
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken ct = default)
        {
            await using var uow = _uowFactory.Create();

            var dtoStream = _historyRepository
                .GetForPeriodAsync(DayStartUtc(dateFrom), DayStartUtc(dateTo.AddDays(1)), ct)
                .Select(operation => operation.HistoryToCsv(_timeZone));

            await CsvExporter.WriteAsync(filePath, dtoStream, ct);
        }

        // Начало местных суток (00:00 на складе), переведённое в UTC
        private DateTime DayStartUtc(DateTime localDate)
        {
            var midnight = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(midnight, _timeZone);
        }
    }
}
