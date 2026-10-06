using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ComponentRepository> _logger;

        public ComponentRepository(IDbContextAccessor contextAccessor, ILogger<ComponentRepository> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        private AppDbContext _db => _contextAccessor.CurrentContext;

        public async Task<IReadOnlyList<ComponentViewDto>> GetViewFilterAsync(string searchText, int maxCount, CancellationToken ct)
        {
            var query = ApplyFilters(_db.Components.AsNoTracking(), searchText);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(maxCount)
                .Select(x => new ComponentViewDto(x.Id, x.Article, x.Name, x.Manufacturer))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ComponentEditDto>> GetEditFilterAsync(string searchText, int maxCount, CancellationToken ct)
        {
            var query = ApplyFilters(_db.Components.AsNoTracking(), searchText);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(maxCount)
                .Select(x => new ComponentEditDto(x.Id, x.Article, x.Name, x.Manufacturer, x.ExpirationDate, x.MinQuantity))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ComponentViewDto>> GetAllAsync()
        {
            return await _db.Components.
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
            var result = await _db.Components.FindAsync(id);

            if (result is null)
                _logger.LogWarning("Component for id {id} not found", id);

            return result;
        }

        public async Task<Component?> GetByArticleAsync(string article)
        {
            var result = await _db.Components.
                AsNoTracking().
                FirstOrDefaultAsync(c => c.Article.ToLower() == article.ToLower());

            if (result is null)
                _logger.LogWarning("Component for article {article} not found", article);

            return result;
        }

        public async Task AddAsync(Component component)
        {
            try
            {
                await _db.Components.AddAsync(component);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add component {Id}", component.Id);
                throw;
            }
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
