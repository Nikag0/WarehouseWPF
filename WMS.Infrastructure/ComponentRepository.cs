using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ComponentRepository> _logger;

        public ComponentRepository(IDbContextFactory<WmsDbContext> factory, ILogger<ComponentRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Component>> GetAllAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var result = await db.Components.ToListAsync(ct).ConfigureAwait(false);

            _logger.LogInformation("Loaded {Count} componetns", result.Count);
            return result;
        }

        public async Task<Component?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var result = await db.Components.FindAsync(new object[] { id }, ct).ConfigureAwait(false);

            if (result is null)
                _logger.LogWarning("Component for id {id} not found", id);

            return result;
        }

        public async Task AddAsync(Component component, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);
            
                db.Components.Add(component);
            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add component {Name}", component.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Component component, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);

            db.Components.Update(component);

            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update component {Id}", component.Id);
                throw;
            }
        }

        public async Task RemoveAsync(Component component, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);

            db.Components.Attach(component);
            component.Delete();

            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to remove component {Id}", component.Id);
                throw;
            }
        }

        public async Task<IReadOnlyList<Component>> GetFilteredComponentAsync(
            string searchText, 
            int maxCount, 
            CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);

            IQueryable<Component> query = db.Components.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var text = searchText.Trim();

                query = query.Where(x =>
                   EF.Functions.ILike(x.Name, $"%{text}%") ||
                   EF.Functions.ILike(x.Article, $"%{text}%") ||
                   EF.Functions.ILike(x.Manufacturer, $"%{text}%"));
            }

            var result = await query
                .OrderByDescending(s => s.CreatedAt)
                .Take(maxCount)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            _logger.LogDebug("Found {Count} components by filter", result.Count);
            return result;
        }
    }
}
