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
        private readonly WmsDbContext _db;

        public ComponentRepository(WmsDbContext db)
        {
            _db = db;
        }

        public async Task<List<Component>> GetAllAsync()
        {
            return await _db.Components.ToListAsync();
        }

        public async Task AddAsync(Component component)
        {
            _db.Components.Add(component);
            await _db.SaveChangesAsync();
        }

        public async Task<Component?> GetByIdAsync(Guid id)
        {
            return await _db.Components.FindAsync(id);
        }

        public async Task<bool> ExistsByArticle(string article)
        {
            return await _db.Components.AnyAsync(x => x.Article == article);
        }
    }
}
