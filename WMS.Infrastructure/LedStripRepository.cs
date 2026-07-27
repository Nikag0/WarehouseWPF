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
        
        public async Task<IReadOnlyList<SectorInitDTO>> GetAllInitAsync(CancellationToken ct = default)
        {
            await using var db =  await _factory.CreateDbContextAsync(ct);
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

        public async Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Sectors
                .AsNoTracking()
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
                .FirstOrDefaultAsync(ct);
        }

        public async Task SetColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var sector = await db.Sectors.FindAsync(new object[] { cellId }, ct);
            if (sector is null) return;

            sector.SetColor(r, g, b);
            db.Sectors.Update(sector);
            await db.SaveChangesAsync(ct);
        }
    }
}
