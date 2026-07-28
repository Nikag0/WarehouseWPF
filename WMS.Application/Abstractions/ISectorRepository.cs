using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.DTO;
using WMS.Domain.LedStrip;

namespace WMS.Application.Abstractions
{
    public interface ISectorRepository
    {
        Task<IReadOnlyList<SectorInitDTO>> GetAllForInitializationAsync(CancellationToken ct = default);
        Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default);
        Task<bool> SetColorAsync(Guid sectorId, byte r, byte g, byte b, CancellationToken ct = default);
    }
}
