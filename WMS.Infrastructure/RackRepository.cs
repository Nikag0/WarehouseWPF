using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure
{
    public class RackRepository : IRackRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RackRepository(IDbContextFactory<AppDbContext> factory)
        {
            _contextFactory = factory;
        }

        public async Task<IReadOnlyList<Rack>> GetAllAsync()
        {
            using var db = _contextFactory.CreateDbContext();

            return await db.Rack
                .Include(r => r.Cells)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Rack?> GetRackAsync(Guid id)
        {
            using var db = _contextFactory.CreateDbContext();

            return await db.Rack
                 .Include(r => r.Cells)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Cell?> GetCellAsync(Guid id)
        {
            using var db = _contextFactory.CreateDbContext();

            return await db.Cell.FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
