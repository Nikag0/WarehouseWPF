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
        Task<IReadOnlyList<SectorInitDTO>> GetAllInitAsync(CancellationToken ct = default);
        Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default);
        Task SetColorAsync(Guid sectorId, byte r, byte g, byte b, CancellationToken ct = default);
    }
}
