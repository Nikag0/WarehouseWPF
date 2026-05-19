using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class ComponentService
    {
        private readonly IComponentRepository _componentRepo;

        public ComponentService(IComponentRepository componentRepo)
        {
            _componentRepo = componentRepo;
        }

        public async Task<List<ComponentDTO>> GetAllAsync()
        {
            var components =  await _componentRepo.GetAllAsync();

            return components
                .Where(c => !c.IsDelet)
                .Select(c => new ComponentDTO(
                    c.Id,
                    c.Article,
                    c.Name,
                    c.Manufacturer,
                    c.MinQuantity
            )).ToList();
        }

        public async Task AddAsync(string article, string name, string manufacturer)
        {
            var component = Component.Create(
                article,
                name,
                manufacturer,
                DateOnly.FromDateTime(DateTime.Today),
                10
            );

            await _componentRepo.AddAsync(component);
        }

        public async Task DeletAsync(Guid id)
        {
            var compoment = await _componentRepo.GetByIdAsync(id);

            if (compoment is null)
                throw new Exception("Компонент с таким артиклом не найден");

            compoment.IsDelet = true;
        }

        public async Task UpdateAsync(Component component)
        {
            await _componentRepo.UpdateAsync(component);
        }

        public async Task<Component> GetByIdAsync(Guid id)
        {
            // Обязательно добавляем return, чтобы вернуть сущность во ViewModel
            return await _componentRepo.GetByIdAsync(id);
        }
    }
}
