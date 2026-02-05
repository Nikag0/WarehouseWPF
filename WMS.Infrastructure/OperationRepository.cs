using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class OperationRepository : IOperationRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public OperationRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(Operation op)
        {
            using var db = _factory.CreateDbContext();

            db.Operations.Add(op);
            await db.SaveChangesAsync();
        }
    }
}
