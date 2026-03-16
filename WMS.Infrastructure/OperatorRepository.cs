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

            return await db.Users.ToListAsync();
        }
    }
}
