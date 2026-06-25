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

        public int Column { get; private set; }
        public int Row { get; private set; }

        private Cell() { }

        internal Cell(Guid rackId, int column, int row)
        {
            Id = Guid.NewGuid();
            RackId = rackId;
            Column = column;
            Row = row;
        }
    }
}
