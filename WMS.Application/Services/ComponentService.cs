using Microsoft.Extensions.Logging;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Application.Services
{
    public class ComponentService
    {
        private readonly IComponentRepository _componentRepo;
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<ComponentService> _logger;
        private readonly IUnitOfWorkFactory _uowFactory;

        public ComponentService(IComponentRepository componentRepo,
                                IStockRepository stockRepository,
                                IUnitOfWorkFactory uowFactory,
                                ILogger<ComponentService> logger)
        {
            _componentRepo = componentRepo;
            _stockRepository = stockRepository;
            _uowFactory = uowFactory;
            _logger = logger;
        }

        public async Task UpdateAsync(ComponentEditDto dto)
        {
            try
            {
                await using var uow = _uowFactory.Create();

                var component = await _componentRepo.GetByIdAsync(dto.Id);

                if (component is null)
                    throw new BusinessException("Компонент не найден.");

                // Сначала доменная проверка: она обрежет пробелы в артикуле. До CommitAsync изменения в БД не попадут
                component.Update(
                    dto.Article,
                    dto.Name,
                    dto.Manufacturer,
                    dto.ExpirationDate,
                    dto.MinQuantity);

                // Уникальность проверяем по уже очищенному артикулу; сам редактируемый компонент не считается дублем
                var existing = await _componentRepo.GetByArticleAsync(component.Article);
                if (existing is not null && existing.Id != component.Id)
                    throw new BusinessException($"Артикул '{component.Article}' уже используется другим компонентом.");

                await uow.CommitAsync();
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, "Критическая ошибка при обновлении компонента {ComponentId} в БД", dto.Id);
                throw;
            }
        }

        public async Task AddAsync(ComponentEditDto dto)
        {
            try
            {
                await using var uow = _uowFactory.Create();

                // Сначала доменная проверка: она обрежет пробелы в артикуле, и уникальность проверится по очищенному значению
                var component = Domain.Component.Create(dto.Article, dto.Name, dto.Manufacturer, dto.ExpirationDate, dto.MinQuantity);

                var existing = await _componentRepo.GetByArticleAsync(component.Article);
                if (existing is not null)
                    throw new BusinessException($"Артикул '{component.Article}' уже используется.");

                await _componentRepo.AddAsync(component);
                await uow.CommitAsync();
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, "Критическая ошибка при добавлении компонента в БД");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                await using var uow = _uowFactory.Create();

                var component = await _componentRepo.GetByIdAsync(id);
                if (component is null)
                    throw new BusinessException("Компонент не найден.");

                bool hasStock = await _stockRepository.HasAnyStockAsync(id);
                if (hasStock)
                    throw new BusinessException("Существуют остатки с этим компонентом");

                _componentRepo.Delete(component);

                await uow.CommitAsync();
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, "Ошибка при удалении компонента {ComponentId}", id);
                throw;
            }
        }

        public async Task<IReadOnlyList<ComponentViewDto>> GetViewFilterAsync(string searchText, int maxCount, CancellationToken token)
        {
            try
            {
                await using var uow = _uowFactory.Create();
                return await _componentRepo.GetViewFilterAsync(searchText, maxCount, token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Ошибка при получении отфильтрованного списка для отображения");
                throw;
            }
        }

        public async Task<IReadOnlyList<ComponentEditDto>> GetEditFilterAsync (string searchText,int maxCount, CancellationToken token)
        {
            try
            {
                await using var uow = _uowFactory.Create();
                return await _componentRepo.GetEditFilterAsync(searchText, maxCount, token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Ошибка при получении отфильтрованного списка для отображения");
                throw;
            }
        }
    }
}
