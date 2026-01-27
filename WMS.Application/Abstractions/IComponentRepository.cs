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
    }
}
