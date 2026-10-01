using Microsoft.EntityFrameworkCore;
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

        public ComponentService(IComponentRepository componentRepo,
                                IStockRepository stockRepository,
                                ILogger<ComponentService> logger)
        {
            _componentRepo = componentRepo;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<Result> UpdateAsync(ComponentEditDto dto)
        {
            try
            {
                var component = await _componentRepo.GetByIdAsync(dto.Id);

                if (component is null)
                    return Result.Failure("Компонент не найден.");

                if (!string.Equals(component.Article, dto.Article, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await _componentRepo.GetByArticleAsync(dto.Article);
                    if (existing is not null)
                        return Result.Failure($"Артикул '{dto.Article}' уже используется другим компонентом.");
                }

                component.Update(
                    dto.Article,
                    dto.Name,
                    dto.Manufacturer,
                    dto.ExpirationDate,
                    dto.MinQuantity);

                await _componentRepo.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Критическая ошибка при обновлении компонента {dto.Id} в БД");
                throw;
            }
        }

        public async Task<Result> AddAsync(ComponentEditDto dto)
        {
            try
            {
                var existing = await _componentRepo.GetByArticleAsync(dto.Article);
                if (existing is not null)
                    return Result.Failure($"Артикул '{dto.Article}' уже используется.");

                var component = Domain.Component.Create(dto.Article, dto.Name, dto.Manufacturer, dto.ExpirationDate, dto.MinQuantity);
                await _componentRepo.AddAsync(component);
                await _componentRepo.SaveChangesAsync();
                return Result.Success();
            }
            catch (BusinessException domainEx)
            {
                return Result.Failure(domainEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при добавлении компонента в БД");
                throw;
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var component = await _componentRepo.GetByIdAsync(id);
                if (component is null)
                {
                    return Result.Failure("Компонент не найден.");
                }

                bool hasActiveStock = await _stockRepository.HasStockWithQuantityAsync(id);
                if (hasActiveStock)
                {
                    return Result.Failure("Существуют остатки с этим компонентом");
                }

                component.Delete();

                await _componentRepo.SaveChangesAsync();

                _logger.LogInformation($"Компонент с ID {id} успешно удален.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении компонента {id}");
                throw;
            }
        }

        public async Task<IReadOnlyList<ComponentViewDto>> GetViewFilterAsync(string searchText, int maxCount, CancellationToken token)
        {
            try
            {
                return await _componentRepo.GetViewFilterAsync(searchText, maxCount, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отфильтрованного списка для отображения");
                throw;
            }
        }

        public async Task<IReadOnlyList<ComponentEditDto>> GetEditFilterAsync (string searchText,int maxCount, CancellationToken token)
        {
            try
            {
                return await _componentRepo.GetEditFilterAsync(searchText, maxCount, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отфильтрованного списка для отображения");
                throw;
            }
        }
    }
}
