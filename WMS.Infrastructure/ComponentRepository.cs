using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Infrastructure
{
    public class ComponentRepository : IComponentRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public ComponentRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Component>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();

            return await db.Components.ToListAsync();
        }

        public async Task AddAsync(Component component)
        {
            using var db = _factory.CreateDbContext();

            db.Components.Add(component);
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Component component)
        {
            using var db = _factory.CreateDbContext();

            db.Components.Remove(component);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Component component)
        {
            using var db = _factory.CreateDbContext();

            db.Components.Update(component);
            await db.SaveChangesAsync();
        }

        public async Task<Component?> GetByIdAsync(Guid id)
        {
            using var db = _factory.CreateDbContext();

            return await db.Components.FindAsync(id);
        }
    }
}
