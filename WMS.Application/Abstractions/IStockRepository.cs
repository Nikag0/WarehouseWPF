using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Abstractions
{
    public interface IStockRepository
    {
        Task<Stock?> GetAsync(Guid componentId, Guid cellId);
        Task AddAsync(Stock stock);
        Task UpdateAsync(Stock stock);
    }
}
