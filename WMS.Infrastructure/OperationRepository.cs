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
        private readonly WmsDbContext _db;

        public OperationRepository(WmsDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Operation op)
        {
            _db.Operations.Add(op);
            await _db.SaveChangesAsync();
        }
    }
}
