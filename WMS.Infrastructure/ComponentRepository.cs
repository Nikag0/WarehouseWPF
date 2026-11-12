using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure
{
    public class ComponentRepository : IComponentRepository
    {
        private readonly IDbContextAccessor _contextAccessor;
        public ComponentRepository(IDbContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        private AppDbContext _db => _contextAccessor.CurrentContext;

        public async Task<IReadOnlyList<ComponentViewDto>> GetViewFilterAsync(string searchText, int maxCount, CancellationToken ct)
        {
            var query = ApplyFilters(_db.Component.AsNoTracking(), searchText);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(maxCount)
                .Select(x => new ComponentViewDto(x.Id, x.Article, x.Name, x.Manufacturer))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ComponentEditDto>> GetEditFilterAsync(string searchText, int maxCount, CancellationToken ct)
        {
            var query = ApplyFilters(_db.Component.AsNoTracking(), searchText);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(maxCount)
                .Select(x => new ComponentEditDto(x.Id, x.Article, x.Name, x.Manufacturer, x.ExpirationDate, x.MinQuantity))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ComponentViewDto>> GetAllAsync()
        {
            return await _db.Component.
                AsNoTracking()
                .Select(c => new ComponentViewDto(
                    c.Id,
                    c.Article,
                    c.Name,
                    c.Manufacturer
                ))
                .ToListAsync();
        }

        public async Task<Component?> GetByIdAsync(Guid id)
        {
            return await _db.Component.FindAsync(id);
        }

        public async Task<Component?> GetByArticleAsync(string article)
        {
            return await _db.Component.
                AsNoTracking().
                FirstOrDefaultAsync(c => c.Article.ToLower() == article.ToLower());
        }

        public async Task AddAsync(Component component)
        {
            await _db.Component.AddAsync(component);
        }

        public void Delete(Component component)
        {
            _db.Component.Remove(component);
        }

        private IQueryable<Component> ApplyFilters(IQueryable<Component> query, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return query;
            var text = searchText.Trim();
            return query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{text}%") ||
                EF.Functions.ILike(x.Article, $"%{text}%") ||
                EF.Functions.ILike(x.Manufacturer, $"%{text}%"));
        }
    }
}
