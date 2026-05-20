using Microsoft.EntityFrameworkCore;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure.Migrations
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public OperatorRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Operator>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Operators.ToListAsync();
        }

        public async Task<Operator?> GetByIdAsync(Guid id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Operators.FindAsync(id);
        }

        public async Task AddAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();
            db.Operators.Add(operatorr);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(operatorr).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();
            operatorr.Delete();
            db.Entry(operatorr).Property(o => o.IsDeleted).IsModified = true;
            await db.SaveChangesAsync();
        }
    }
}
