using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    public class Cell
    {
        public Guid Id { get; private set; }

        public Guid RackId { get; private set; }
        public Rack Rack { get; private set; }

        public int Line { get; private set; }
        public int Column { get; private set; }

        private Cell() { }

        internal Cell(Guid rackId, int line, int column)
        {
            Id = Guid.NewGuid();
            RackId = rackId;
            Line = line;
            Column = column;
        }

        public string Code => $"{Rack.Row}-{Rack.RackNum}-{Line}-{Column}";
    }
}
