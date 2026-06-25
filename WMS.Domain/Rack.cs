using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    public class Rack
    {
        public Guid Id { get; private set; }
        public int Column { get; private set; }
        public int Row { get; private set; }
        public RackType Type { get; private set; }

        private readonly List<Cell> _cells = new();
        public IReadOnlyCollection<Cell> Cells => _cells;

        private Rack() { }

        public Rack(int column, int row, RackType type)
        {
            Id = Guid.NewGuid();
            Column = column;
            Row = row;
            Type = type;
        }
    }
}
