using Lotlab.PluginCommon.FFXIV.Parser;
using System.Runtime.InteropServices;
using System;

namespace HousingCheck
{
    public class LandSaleInfo : IPCPacketBase<FFXIVIpcLandSaleInfo>
    {
        public override string ToString()
        {
            return $"LandSaleInfo: {Value.purchase_type}, {Value.region_type}, {Value.status}. { Value.persons } participated, ends at {DateTimeOffset.FromUnixTimeSeconds(Value.end_time).LocalDateTime}";
        }
    }

    public enum LandStatus : byte
    {
        Available = 1,
        InResultsPeriod = 2,
        Unavailable = 3
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FFXIVIpcLandSaleInfo
    {
        public IPCHeader ipc;

        public HousePurchaseType purchase_type;
        public HouseRegionType region_type;
        public LandStatus status;
        public byte padding1;

        public UInt32 end_time;
        public UInt32 padding2;
        public UInt32 persons;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] unknown;
    }
}
