using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public record SectorColorDTO(
      byte DeviceAddress,
      byte StripNumber,
      byte Index,
      string Ip,
      int Port,
      byte Brightness
    );
}
