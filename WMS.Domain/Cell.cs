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
        public string Row { get; private set; }
        public int Rack { get; private set; }
        public int Position { get; private set; }

        private Cell() { } // для EF

        public Cell(string row, int rack, int position)
        {
            Id = Guid.NewGuid();
            Row = row;
            Rack = rack;
            Position = position;
        }

        public string Code => $"{Row}-{Rack}-{Position}";
    }
}
