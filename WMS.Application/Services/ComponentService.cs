using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
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

        public async Task<Result<IReadOnlyList<ComponentDTO>>> GetAllAsync()
        {
            try
            {
                var components = await _componentRepo.GetAllAsync();
                var dtos = components.Select(c => new ComponentDTO(
                    c.Id, c.Article, c.Name, c.Manufacturer,c.ExpirationDate, c.MinQuantity
                )).ToList();

                return Result<IReadOnlyList<ComponentDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при получении списка компонентов из базы данных.");
                throw;
            }
        }

        public async Task<ComponentDTO?> GetByIdAsync(Guid id)
        {
            var result = await _componentRepo.GetByIdAsync(id);

            return MappingExtensions.ToComponentDTO(result);
        }

        public async Task<Result> AddAsync(string article, string name, string manufacturer, DateOnly expirationDate, int minQuantity)
        {
            try
            {
                var component = Domain.Component.Create(article, name, manufacturer, expirationDate, minQuantity);
                await _componentRepo.AddAsync(component);

                _logger.LogInformation($"Успешно добавлен компонент: {name}");

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

        public async Task<Result> UpdateAsync(ComponentDTO dto)
        {
            try
            {
                var component = await _componentRepo.GetByIdAsync(dto.Id);

                if (component is null)
                    return Result.Failure("Компонент не найден.");

                component.Update(
                    dto.Article,
                    dto.Name,
                    dto.Manufacturer,
                    dto.ExpirationDate,
                    dto.MinQuantity);

                await _componentRepo.UpdateAsync(component);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Критическая ошибка при обновлении компонента {dto.Id} в БД");
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

                await _componentRepo.RemoveAsync(component);

                _logger.LogInformation($"Компонент с ID {id} успешно удален.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении компонента {id}");
                throw;
            }
        }

        public async Task<IReadOnlyList<ComponentDTO>> GetFilteredAsync (string searchText,int maxCount, CancellationToken token)
        {
            var components = await _componentRepo.GetFilteredComponentAsync(searchText, maxCount, token);

            return components.Select(MappingExtensions.ToComponentDTO).ToList();
        }
    }
}
