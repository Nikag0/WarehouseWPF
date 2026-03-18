using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class CellRepository : ICellRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public CellRepository(IDbContextFactory<WmsDbContext> factory) 
        {
            _factory = factory;
        }

        public async Task<List<Cell>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();

            return await db.Cells
                .Include(c => c.Rack)
                .ToListAsync();
        }
    }
}
