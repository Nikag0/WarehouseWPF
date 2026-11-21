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
        Task<IReadOnlyList<SectorInitDTO>> GetAllSectorsForInitializationAsync(CancellationToken ct = default);
        Task<SectorColorDTO?> GetByCellIdAsync(Guid cellId, CancellationToken ct = default);
        // Лампы рядов в порядке названия — в этом порядке они загораются при инициализации
        Task<IReadOnlyList<SignalLampDto>> GetSignalLampsAsync(CancellationToken ct = default);
        // Зоны с положением стеллажа и ячейки — для поочерёдной проверки подсветки
        Task<IReadOnlyList<SectorCheckDto>> GetAllSectorsForCheckAsync(CancellationToken ct = default);
        // Возвращает false, если у ячейки нет сектора подсветки
        Task<bool> SetColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default);
    }
}
