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

        public async Task<Component?> GetByIdAsync(Guid id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Components.FindAsync(id);
        }

        public async Task AddAsync(Component component)
        {
            using var db = _factory.CreateDbContext();
            db.Components.Add(component);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Component component)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(component).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Component component)
        {
            using var db = _factory.CreateDbContext();
            component.Delete();
            db.Entry(component).Property(c => c.IsDeleted).IsModified = true;
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Component>> GetFilteredComponentAsync(string searchText, int maxCount)
        {
            using var db = _factory.CreateDbContext();

            IQueryable<Component> query = db.Components;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim();

                query = query.Where(s =>
                    (s.Article != null && s.Article.ToLower().Contains(text)) ||
                    (s.Name != null && s.Name.ToLower().Contains(text)) ||
                    (s.Manufacturer != null && s.Manufacturer.ToLower().Contains(text))
                );
            }

            var result = await query
                .OrderBy(s => s.Name)
                .Take(maxCount)
                .ToListAsync();

            return result;
        }
    }
}
