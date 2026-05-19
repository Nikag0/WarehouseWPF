using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure.Migrations
{
    public class UsersRepository : IOperatorRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public UsersRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Operator>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();

            return await db.Operators.ToListAsync();
        }

        public async Task AddAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();

            db.Operators.Add(operatorr);
            await db.SaveChangesAsync();
        }
        public async Task RemoveAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();

            db.Operators.Remove(operatorr);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operator operatorr)
        {
            using var db = _factory.CreateDbContext();

            db.Operators.Update(operatorr);
            await db.SaveChangesAsync();
        }
    }
}
