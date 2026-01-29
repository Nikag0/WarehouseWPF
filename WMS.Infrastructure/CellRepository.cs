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
        private readonly WmsDbContext _db;

        public CellRepository(WmsDbContext db) 
        {
            _db = db;
        }

        public async Task<List<Cell>> GetAllAsync()
        {
            return await _db.Cells.ToListAsync();
        }
    }
}
