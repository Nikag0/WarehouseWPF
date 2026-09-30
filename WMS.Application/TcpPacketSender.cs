using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application
{
    public class TcpPacketSender : ITcpPacketSender
    {
        private readonly TimeSpan _timeout;

        public TcpPacketSender(TimeSpan? timeout = null)
        {
            _timeout = timeout ?? TimeSpan.FromSeconds(3);
        }

        public async Task<bool> SendAsync(string ip, int port, byte[] packet, CancellationToken ct = default)
        {
            using var client = new TcpClient();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts.CancelAfter(_timeout);

            try
            {
                await client.ConnectAsync(ip, port, linkedCts.Token);
                await using var stream = client.GetStream();
                await stream.WriteAsync(packet, linkedCts.Token);

                var buffer = new byte[64];
                var read = await stream.ReadAsync(buffer.AsMemory(0, 64), linkedCts.Token);

                if (read < 4) return false;

                // byte[3] == 0x00 → OK
                return buffer[3] == 0x00;
            }
            catch
            {
                return false;
            }
        }
    }
}
