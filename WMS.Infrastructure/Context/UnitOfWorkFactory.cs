using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;

namespace WMS.Infrastructure.Context
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDbContextFactory<AppDbContext> _efFactory;
        private readonly DbContextHolder _holder;

        public UnitOfWorkFactory(IDbContextFactory<AppDbContext> efFactory, DbContextHolder holder)
        {
            _efFactory = efFactory;
            _holder = holder;
        }

        public IUnitOfWork Create()
        {
            var context = _efFactory.CreateDbContext();

            return new UnitOfWork(context, _holder);
        }
    }
}
