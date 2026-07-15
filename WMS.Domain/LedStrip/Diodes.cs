using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.LedStrip
{
    public class Diodes
    {
        public Guid Id { get; private set; }
        public Guid CellId { get; private set; }
        public Cell Cell { get; private set; } = null!;
        public Guid StripId { get; private set; }
        public Strip Strip { get; private set; } = null!;
        public int StartNum { get; private set; }
        public int EndNum { get; private set; }
        public int ColorNum { get; private set; }
        public int KeyNum { get; private set; }
    }
}
