using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.LedStrip
{
    public class Microcontroller
    {
        public Guid Id { get; private set; }
        public string Ip { get; private set; } = null!;
        public int Port { get; private set; }
        public byte DeviceAddress {  get; private set; }

        // Навигация
        public ICollection<Strip> Strips { get; private set; } = new List<Strip>();
    }
}
