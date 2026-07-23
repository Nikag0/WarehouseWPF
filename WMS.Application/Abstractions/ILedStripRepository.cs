using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.DTO;
using WMS.Domain.LedStrip;

namespace WMS.Application.Abstractions
{
    public interface ILedStripRepository
    {
        Task<IReadOnlyList<Strip>> GetAllStripsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Microcontroller>> GetAllMicrocontrollersAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SectorInitDTO>> GetAllSectorInitAsync(CancellationToken ct = default);
        Task<SectorColorDTO?> GetSectorByCellIdAsync(Guid cellId, CancellationToken ct = default);
        Task<IReadOnlyList<Sector>> GetSectorsByStripAsync(Guid stripId, CancellationToken ct = default);
        Task UpdateSectorColorAsync(Guid sectorId, byte r, byte g, byte b, CancellationToken ct = default);
    }
}
