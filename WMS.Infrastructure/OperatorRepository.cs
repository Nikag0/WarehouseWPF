using Microsoft.EntityFrameworkCore;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure.Migrations
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly AppDbContext _db;

        public OperatorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Operator>> GetAllAsync()
        {
            return await _db.Operators.ToListAsync();
        }

        public async Task<Operator?> GetByIdAsync(Guid id)
        {
            return await _db.Operators.FindAsync(id);
        }

        public async Task AddAsync(Operator operatorr)
        {
            _db.Operators.Add(operatorr);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operator operatorr)
        {
            _db.Entry(operatorr).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Operator operatorr)
        {
            operatorr.Delete();
            _db.Entry(operatorr).Property(o => o.IsDeleted).IsModified = true;
            await _db.SaveChangesAsync();
        }
    }
}
