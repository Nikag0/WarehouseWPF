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

        public async Task<IReadOnlyList<Operator>> GetAllAsync()
        {
            return await _db.Operators.ToListAsync();
        }

        public async Task<Operator?> GetByIdAsync(Guid id)
        {
            return await _db.Operators.FindAsync(id);
        }

        public async Task AddAsync(Operator operatorr)
        {
            await _db.Operators.AddAsync(operatorr);
        }

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
