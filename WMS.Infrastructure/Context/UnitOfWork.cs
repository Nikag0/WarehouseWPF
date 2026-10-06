using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;

namespace WMS.Infrastructure.Context
{
    public class UnitOfWork : IUnitOfWork
    {
        //private readonly AppDbContext _context;
        //public IComponentRepository Components { get; }
        //public IStockRepository Stocks { get; }
        //public IHistoryRepository History { get; }
        //public IOperatorRepository Operator { get; }

        //public UnitOfWork(
        //    AppDbContext context,
        //    IComponentRepository components,
        //    IStockRepository stocks,
        //    IHistoryRepository history,
        //    IOperatorRepository @operator)
        //{
        //    _context = context;
        //    Components = components;
        //    Stocks = stocks;
        //    History = history;
        //    Operator = @operator;
        //}

        //public UnitOfWork(IDbContextFactory<AppDbContext> contextFactory)
        //{
        //    _context = contextFactory.CreateDbContext();
        //}

        //public Task<int> SaveChangesAsync(CancellationToken ct = default)
        //=> _context.SaveChangesAsync(ct);

        //public async ValueTask DisposeAsync()
        //{
        //    await _context.DisposeAsync();
        //}

        private readonly AppDbContext _context;
        private readonly DbContextHolder _holder;
        private bool _disposed;

        public UnitOfWork(AppDbContext context, DbContextHolder holder)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _holder = holder ?? throw new ArgumentNullException(nameof(holder));

            // Как только UnitOfWork создан, мы записываем его контекст в хранитель текущего потока
            _holder.CurrentContext = _context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // Атомарно сохраняем все изменения из всех репозиториев
            await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _context.Dispose();
            _holder.CurrentContext = null!; // Очищаем поток после завершения работы
            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await _context.DisposeAsync();
            _holder.CurrentContext = null!; // Очищаем поток после завершения работы
            _disposed = true;
        }
    }
}
