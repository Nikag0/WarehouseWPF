using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.LedStrip
{
    public class Strip
    {
        public Guid Id { get; private set; }
        public Guid MicrocontrollerId { get; private set; }
        public Microcontroller Microcontroller { get; private set; } = null!;

        public byte StripNumber { get; private set; }

        public ICollection<Sector> Sectors { get; private set; } = new List<Sector>();
    }
}
