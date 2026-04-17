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

        private readonly List<Cell> _cells = new();
        public IReadOnlyCollection<Cell> Cells => _cells;

        private Rack() { } // EF

        public Rack(int column, int row)
        {
            Id = Guid.NewGuid();
            Column = column;
            Row = row;
        }

        public string RackCode => $"{Column}-{Row}";
    }
}
