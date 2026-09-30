using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.Abstractions
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IComponentRepository Components { get; }
        IStockRepository Stocks { get; }
        IHistoryRepository History { get; }
        IOperatorRepository Operator { get; }


        Task<int> SaveChangesAsync(CancellationToken ct = default);

    }
}
