using Microsoft.EntityFrameworkCore;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure.Migrations
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public OperatorRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IReadOnlyList<Operator>> GetAllAsync()
        {
            using var db = _contextFactory.CreateDbContext();
            return await db.Operator.ToListAsync();
        }

        public async Task<Operator?> GetByIdAsync(Guid id)
        {
            using var db = _contextFactory.CreateDbContext();
            return await db.Operator.FindAsync(id);
        }

        public async Task<bool> IsOperatorExistsAsync(string surname, string name, string patronymic, Guid? excludeId = null)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Operator
                .AnyAsync(o =>
                    (excludeId == null || o.Id != excludeId) &&
                    o.Surname.ToLower() == surname.ToLower() &&
                    o.Name.ToLower() == name.ToLower() &&
                    o.Patronymic.ToLower() == patronymic.ToLower());
        }

        public async Task AddAsync(Operator operatorr)
        {
            using var db = _contextFactory.CreateDbContext();

            await db.Operator.AddAsync(operatorr);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operator operatorr)
        {
            using var db = _contextFactory.CreateDbContext();

            db.Operator.Update(operatorr);

            await db.SaveChangesAsync();
        }
    }
}
