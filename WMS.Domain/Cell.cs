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
        public int Row { get; private set; }
        public int Rack { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        private Cell() { } // для EF

        public Cell( int row, int rack, int position, int column)
        {
            Id = Guid.NewGuid();
            Row = row;
            Rack = rack;
            Line = position;
            Column = column;
        }

        public string Code => $"{Row}-{Rack}-{Line}-{Column}";
    }
}
