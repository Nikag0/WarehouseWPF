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
        private readonly IComponentRepository _repo;

        public ComponentService(IComponentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Component>> GetAllAsycn()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddAsync(string article, string name)
        {
            if (await _repo.ExistsByArticle(article))
                throw new Exception("Компонент с таким артиклом уже существует");

            var component = Component.Create(
                article,
                name,
                "Unknow",
                DateOnly.FromDateTime(DateTime.Today),
                10
            );

            await _repo.AddAsync(component);
        }

    }

}
