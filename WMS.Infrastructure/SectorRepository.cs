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
                .Select(s => new SectorInitDTO(
                    s.Strip.MicrocontrollerId,
                    new ControllerEndpoint(
                        s.Strip.Microcontroller.Ip,
                        s.Strip.Microcontroller.Port,
                        s.Strip.Microcontroller.DeviceAddress),
                    s.Strip.StripNumber,
                    s.Index,
                    s.StartDiode,
                    s.EndDiode,
                    s.Strip.Microcontroller.SignalLampId))
                .ToListAsync(ct);
        }

        public async Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Sector
                .AsNoTracking()
                .Where(s => s.CellId == cellId)
                .Select(s => new SectorColorDTO(
                    new ControllerEndpoint(
                        s.Strip.Microcontroller.Ip,
                        s.Strip.Microcontroller.Port,
                        s.Strip.Microcontroller.DeviceAddress),
                    s.Strip.StripNumber,
                    s.Index,
                    s.R, s.G, s.B,
                    s.Bright,
                    // Лампа ряда может висеть на другом контроллере, поэтому берём её адрес отдельно
                    s.Strip.Microcontroller.SignalLamp == null
                        ? null
                        : new ControllerEndpoint(
                            s.Strip.Microcontroller.SignalLamp.Microcontroller.Ip,
                            s.Strip.Microcontroller.SignalLamp.Microcontroller.Port,
                            s.Strip.Microcontroller.SignalLamp.Microcontroller.DeviceAddress)))
                .SingleOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<SignalLampDto>> GetSignalLampsAsync(CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.SignalLamp
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .Select(l => new SignalLampDto(
                    l.Id,
                    l.Name,
                    new ControllerEndpoint(
                        l.Microcontroller.Ip,
                        l.Microcontroller.Port,
                        l.Microcontroller.DeviceAddress)))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SectorCheckDto>> GetAllSectorsForCheckAsync(CancellationToken ct = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            // У ячейки нет навигации на стеллаж, поэтому стеллаж присоединяется по RackId
            return await db.Sector
                .AsNoTracking()
                .Join(db.Rack, s => s.Cell.RackId, r => r.Id, (s, r) => new SectorCheckDto(
                    new ControllerEndpoint(
                        s.Strip.Microcontroller.Ip,
                        s.Strip.Microcontroller.Port,
                        s.Strip.Microcontroller.DeviceAddress),
                    s.Strip.StripNumber,
                    s.Index,
                    s.Strip.Microcontroller.SignalLampId,
                    r.Column,
                    r.Row,
                    s.Cell.Column,
                    s.Cell.Row))
                .ToListAsync(ct);
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
