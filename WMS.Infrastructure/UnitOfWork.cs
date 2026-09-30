using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;

namespace WMS.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IComponentRepository Components { get; }
        public IStockRepository Stocks { get; }
        public IHistoryRepository History { get; }
        public IOperatorRepository Operator { get; }

        public UnitOfWork(
            AppDbContext context,
            IComponentRepository components,
            IStockRepository stocks,
            IHistoryRepository history,
            IOperatorRepository @operator)
        {
            _context = context;
            Components = components;
            Stocks = stocks;
            History = history;
            Operator = @operator;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
