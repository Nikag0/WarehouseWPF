using System;
using System.Collections.Generic;
using WMS.Domain;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.Abstractions
{
    public interface IComponentRepository
    {
        Task<List<Component>> GetAllAsync();
        Task AddAsync(Component component);
        Task<Component?> GetByIdAsync(Guid id);
        Task<bool> ExistsByArticle(string article);
    }
}
