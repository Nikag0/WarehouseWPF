using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Abstractions
{
    public interface IRackRepository
    {
        Task<IReadOnlyList<Rack>> GetAllAsync();
        Task<Rack> GetRackAsync(Guid id);
    }
}
