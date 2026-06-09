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

        public async Task<Result<List<ComponentDTO>>> GetAllAsync()
        {
            try
            {
                var components = await _componentRepo.GetAllAsync();
                var dtos = components.Select(c => new ComponentDTO(
                    c.Id, c.Article, c.Name, c.Manufacturer, c.MinQuantity
                )).ToList();

                return Result<List<ComponentDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при получении списка компонентов из базы данных.");
                throw;
            }
        }

        public async Task<Result> AddAsync(string article, string name, string manufacturer)
        {
            try
            {
                var component = Domain.Component.Create(article, name, manufacturer, null, 10);
                await _componentRepo.AddAsync(component);

                _logger.LogInformation($"Успешно добавлен компонент: {name}");

                return Result.Success();
            }
            catch (OverallDomainException domainEx)
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

        public async Task<Result> UpdateAsync(Domain.Component component)
        {
            try
            {
                await _componentRepo.UpdateAsync(component);

                _logger.LogInformation($"Данные компонента с ID {component.Id} успешно обновлены.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Критическая ошибка при обновлении компонента {component.Id} в БД");
                throw;
            }
        }

        public async Task<Domain.Component?> GetByIdAsync(Guid id)
        {
            return await _componentRepo.GetByIdAsync(id);
        }
    }
}
