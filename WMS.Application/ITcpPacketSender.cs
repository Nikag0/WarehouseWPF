using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application
{
    public interface ITcpPacketSender
    {
        Task<bool> SendAsync(string ip, int port, byte[] packet, CancellationToken ct = default);
    }
}
