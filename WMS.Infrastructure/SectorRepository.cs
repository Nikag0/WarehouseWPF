using Microsoft.EntityFrameworkCore;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Infrastructure.Context;

namespace WMS.Infrastructure
{
    public class SectorRepository : ISectorRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SectorRepository(IDbContextFactory<AppDbContext> factory)
        {
            _contextFactory = factory;
        }

        public async Task<IReadOnlyList<SectorInitDTO>> GetAllSectorsForInitializationAsync(CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Sector
                .AsNoTracking()
                .Include(s => s.Strip)
                    .ThenInclude(strip => strip.Microcontroller)
                .Select(s => new SectorInitDTO(
                    s.Strip.Microcontroller.DeviceAddress,
                    s.Strip.StripNumber,
                    s.Index,
                    s.StartDiode,
                    s.EndDiode,
                    s.Strip.Microcontroller.Ip,
                    s.Strip.Microcontroller.Port))
                .ToListAsync(ct);
        }

        public async Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Sector
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
                .SingleOrDefaultAsync(ct);
        }

        // Возвращает false, если у ячейки нет сектора подсветки
        public async Task<bool> SetColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var sector = await db.Sector.SingleOrDefaultAsync(s => s.CellId == cellId, ct).ConfigureAwait(false);

            if (sector is null)
                return false;

            sector.SetColor(r, g, b);

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
