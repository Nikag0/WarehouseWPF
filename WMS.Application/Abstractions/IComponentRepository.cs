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
        Task<IReadOnlyList<Component>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Component component, CancellationToken ct = default);
        Task UpdateAsync(Component component, CancellationToken ct = default);
        Task RemoveAsync(Component component, CancellationToken ct = default);
        Task<Component?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Component>> GetFilteredComponentAsync(string searchText, int maxCount, CancellationToken token = default);
    }
}
