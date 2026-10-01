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
        Task<Component?> GetByIdAsync(Guid id);
        Task<Component?> GetByArticleAsync(string article);
        Task<IReadOnlyList<ComponentViewDto>> GetViewFilterAsync(string searchText, int maxCount, CancellationToken token);
        Task<IReadOnlyList<ComponentEditDto>> GetEditFilterAsync(string searchText, int maxCount, CancellationToken token);
        Task AddAsync(Component component);
        Task SaveChangesAsync();
    }
}
