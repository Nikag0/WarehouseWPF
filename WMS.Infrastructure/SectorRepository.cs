using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;
using WMS.Application.Abstractions;
using WMS.Domain.LedStrip;
using WMS.Application.DTO;
using Microsoft.Extensions.Logging;


namespace WMS.Infrastructure
{
    public class SectorRepository : ISectorRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;
        private readonly ILogger<SectorRepository> _logger;

        public SectorRepository(IDbContextFactory<WmsDbContext> factory, ILogger<SectorRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }
        
        public async Task<IReadOnlyList<SectorInitDTO>> GetAllForInitializationAsync(CancellationToken ct = default)
        {
            await using var db =  await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var result = await db.Sectors
                .AsNoTracking()
                .Include(s => s.Strip)
                    .ThenInclude(strip =>  strip.Microcontroller)
                .Select(s => new SectorInitDTO(
                    s.Strip.Microcontroller.DeviceAddress,
                    s.Strip.StripNumber,
                    s.Index,
                    s.StartDiode,
                    s.EndDiode,
                    s.Strip.Microcontroller.Ip,
                    s.Strip.Microcontroller.Port))
                .ToListAsync(ct)
                .ConfigureAwait(false);

            _logger.LogInformation("Loaded {Count} sectors for initialization", result.Count);

            return result;
        }

        public async Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var dto = await db.Sectors
                .AsNoTracking()
                .Include(s => s.Strip)
                    .ThenInclude(strip => strip.Microcontroller)
                .Where(s => s.CellId == cellId)
                .Select(s => new SectorColorDTO(
                    s.Strip.Microcontroller.DeviceAddress,
                    s.Strip.StripNumber,
                    s.Index,
                    s.Strip.Microcontroller.Ip,
                    s.Strip.Microcontroller.Port,
                    s.R, s.G, s.B,
                    s.Bright
                ))
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (dto is null)
                _logger.LogWarning("Sector for CellId {CellId} not found", cellId);

            return dto;
        }

        public async Task<bool> SetColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var sector = await db.Sectors.SingleOrDefaultAsync(s => s.CellId == cellId, ct).ConfigureAwait(false);

            if (sector is null)
            {
                _logger.LogWarning("Cannot set color: sector for CellId {CellId} not found", cellId);
                return false;
            }

            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save color for CellId {CellId}", cellId);
                throw;
            }
        }
    }
}
