using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Domain.LedStrip;

namespace WMS.Application.Services
{
    public class LedStripService
    {
        private readonly ILedStripRepository _repository;
        private readonly ITcpPacketSender _packetSender;

        public LedStripService(ILedStripRepository repository, ITcpPacketSender packetSender)
        {
            _repository = repository;
            _packetSender = packetSender;
        }

        //public async Task<bool> InitializeSectorsAsync(CancellationToken ct = default)
        //{
        //    string ip = "172.20.4.50";
        //    int port = 7;

        //    //Регистрация сектора
        //    var packetReg = BuildCreateSectorPacket(
        //        (byte)0,
        //        (byte)0,
        //        (byte)0,
        //        (byte)0,
        //        29
        //    );

        //    var success = await _packetSender.SendAsync(
        //        ip,
        //        port,
        //        packetReg,
        //        ct
        //    );

        //    //Зажигание сектора
        //    var packetCol = BuildColorPacket(
        //      0,
        //      0,
        //      0,
        //      255, 255, 0
        //    );

        //    success = await _packetSender.SendAsync(
        //        ip,
        //        port,
        //        packetCol,
        //        ct
        //    );

        //    return true;
        //}

        public async Task<bool> InitializeSectorsAsync(CancellationToken ct = default)
        {
            var strips = await _repository.GetAllSectorInitAsync(ct);

            foreach (var sector in strips)
            {
                var packet = BuildCreateSectorPacket(
                    sector.DeviceAddress,
                    sector.StripNumber,
                    sector.Index,
                    sector.StartDiode,
                    sector.EndDiode
                );

                var success = await _packetSender.SendAsync(
                    sector.Ip,
                    sector.Port,
                    packet,
                    ct
                );

                if (!success)
                {
                    // Можно логировать: какой сектор не создался
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> SetCellColorAsync(Guid cellId, byte r, byte g, byte b, CancellationToken ct = default)
        {
            var sector = await _repository.GetSectorByCellIdAsync(cellId, ct);
            if (sector is null) return false;

            var packet = BuildColorPacket(
                sector.DeviceAddress,
                sector.StripNumber,
                sector.Index,
                r, g, b,
                sector.Brightness
            );

            var success = await _packetSender.SendAsync(
                sector.Ip,
                sector.Port,
                packet,
                ct
            );

            return success;
        }

        // ============ Пакеты ============

        private static byte[] BuildCreateSectorPacket(
            byte deviceAddress,
            byte stripNumber,
            byte sectorIndex,
            int startDiode,
            int endDiode)
        {
            return new byte[]
            {
            deviceAddress,
            0x02,
            0xF0,
            stripNumber,
            sectorIndex,
            LowByte(startDiode),
            HighByte(startDiode),
            LowByte(endDiode),
            HighByte(endDiode),
            0x00
            };
        }

        private static byte[] BuildColorPacket(
            byte deviceAddress,
            byte stripNumber,
            byte sectorIndex,
            byte r, byte g, byte b,
            byte brightness)
        {
            var (pwmR, pwmG, pwmB) = CalculatePwm(r, g, b, brightness);

            return new byte[]
            {
            deviceAddress,
            0x02,
            0xEF,
            stripNumber,
            sectorIndex,
            pwmR, pwmG, pwmB,
            0x00,
            0x00
            };
        }

        private static byte LowByte(int value) => (byte)(value & 0xFF);
        private static byte HighByte(int value) => (byte)((value >> 8) & 0xFF);

        private static readonly byte[] Gamma8 = new byte[]
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  1,  1,  1,
            1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,  2,
            2,  2,  2,  2,  2,  2,  2,  3,  3,  3,  3,  3,  3,  3,  4,  4,
            4,  4,  4,  5,  5,  5,  5,  6,  6,  6,  6,  7,  7,  7,  7,  8,
            8,  8,  9,  9,  9, 10, 10, 10, 11, 11, 11, 12, 12, 13, 13, 13,
           14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21,
           22, 22, 23, 24, 24, 25, 25, 26, 27, 27, 28, 29, 29, 30, 31, 32,
           32, 33, 34, 35, 35, 36, 37, 38, 39, 39, 40, 41, 42, 43, 44, 45,
           46, 47, 48, 49, 50, 50, 51, 52, 54, 55, 56, 57, 58, 59, 60, 61,
           62, 63, 64, 66, 67, 68, 69, 70, 72, 73, 74, 75, 77, 78, 79, 81,
           82, 83, 85, 86, 87, 89, 90, 92, 93, 95, 96, 98, 99,101,102,104,
          105,107,109,110,112,114,115,117,119,120,122,124,126,127,129,131,
          133,135,137,138,140,142,144,146,148,150,152,154,156,158,160,162,
          164,167,169,171,173,175,177,180,182,184,186,189,191,193,196,198,
          200,203,205,208,210,213,215,218,220,223,225,228,231,233,236,239,
          241,244,247,249,252,255
        };

        public static (byte R, byte G, byte B) CalculatePwm(
            byte r, byte g, byte b, byte masterBrightness)
        {
            byte rLin = (byte)((r * masterBrightness) >> 8);
            byte gLin = (byte)((g * masterBrightness) >> 8);
            byte bLin = (byte)((b * masterBrightness) >> 8);

            return (
                Gamma8[rLin],
                Gamma8[gLin],
                Gamma8[bLin]
            );
        }
    }
}
