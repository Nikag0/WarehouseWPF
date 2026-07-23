using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.LedStrip
{
    public class Sector
    {
        public Guid Id { get; private set; }
        public Guid StripId { get; private set; }
        public Strip Strip { get; private set; } = null!;
        public Guid CellId { get; private set; }
        public Cell Cell { get; private set; } = null!;

        public byte Index { get; private set; }
        public int StartDiode { get; private set; }
        public int EndDiode { get; private set; }

        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public byte Bright { get; private set; }

        // EF Core требует пустой конструктор
        private Sector() { }

        public Sector(
            Guid id,
            Guid stripId,
            Guid cellId,
            byte index,
            int startDiode,
            int endDiode)
        {
            if (startDiode > endDiode)
                throw new ArgumentException("StartDiode не может быть больше EndDiode");

            Id = id;
            StripId = stripId;
            CellId = cellId;
            Index = index;
            StartDiode = startDiode;
            EndDiode = endDiode;
            R = 0;
            G = 0;
            B = 0;
        }

        public void SetColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
