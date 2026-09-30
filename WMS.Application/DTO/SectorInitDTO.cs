using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public record SectorInitDTO(
      byte DeviceAddress,
      byte StripNumber,
      byte Index,
      int StartDiode,
      int EndDiode,
      string Ip,
      int Port
    );
}
