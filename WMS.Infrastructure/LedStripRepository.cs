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


namespace WMS.Infrastructure
{
    public class LedStripRepository : ILedStripRepository
    {
        private readonly IDbContextFactory<WmsDbContext> _factory;

        public LedStripRepository(IDbContextFactory<WmsDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<Strip>> GetAllStripsAsync(CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            return await db.Strips
                .AsNoTracking()
                .Include(s => s.Microcontroller)
                .Include(s => s.Sectors)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Microcontroller>> GetAllMicrocontrollersAsync(CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            return await db.Microcontrollers
                .AsNoTracking()
                .Include(m => m.Strips)
                    .ThenInclude(s => s.Sectors)
                .ToListAsync(ct);
        }     
        
        public async Task<IReadOnlyList<SectorInitDTO>> GetAllSectorInitAsync(CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            return await db.Sectors
                .AsNoTracking()
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

        public async Task<SectorColorDTO?> GetSectorByCellIdAsync(Guid cellId, CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            return await db.Sectors
                .AsNoTracking()
                .Where(s => s.CellId == cellId)
                .Select(s => new SectorColorDTO(
                    s.Strip.Microcontroller.DeviceAddress,
                    s.Strip.StripNumber,
                    s.Index,
                    s.Strip.Microcontroller.Ip,
                    s.Strip.Microcontroller.Port,
                    s.Bright
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<Sector>> GetSectorsByStripAsync(Guid stripId, CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            return await db.Sectors
                .AsNoTracking()
                .Where(s => s.StripId == stripId)
                .ToListAsync(ct);
        }

        public async Task UpdateSectorColorAsync(Guid sectorId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            using var db = _factory.CreateDbContext();
            var sector = await db.Sectors.FindAsync(new object[] { sectorId }, ct);
            if (sector is null) return;

            sector.SetColor(r, g, b);
            db.Sectors.Update(sector);
            await db.SaveChangesAsync(ct);
        }
    }
}
