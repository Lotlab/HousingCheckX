using Lotlab.PluginCommon.FFXIV.Parser;
using System.Runtime.InteropServices;
using System;

namespace HousingCheck
{
    public class LandSaleInfo : IPCPacketBase<FFXIVIpcLandSaleInfo>
    {
        public override string ToString()
        {
            return $"LandSaleInfo: {Value.purchase_type}, {Value.region_type}, {Value.status}. {Value.persons} participated, {Value.winner} win, ends at {DateTimeOffset.FromUnixTimeSeconds(Value.endTime).LocalDateTime}";
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

        public UInt32 endTime;
        public UInt32 padding2;
        public UInt32 persons;
        public UInt32 winner;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] unknown;
    }

    public class ClientTrigger : IPCPacketBase<FFXIVIpcClientTrigger>
    {
        public override string ToString()
        {
            return $"ClientTrigger. command: {Value.commandId}, data: {Value.data.ToHexString()}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FFXIVIpcClientTrigger
    {
        public IPCHeader ipc;

        public UInt16 commandId;
        public UInt16 padding1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClientTriggerLandSaleRequest
    {
        public UInt16 territoryTypeId;
        public UInt16 padding1;

        public byte landId;
        public byte wardNum;
        public UInt16 padding2;

        public UInt32 unknown1;

        public UInt32 unknown2;
        public UInt32 unknown3;
        public UInt32 unknown4;
        public UInt32 unknown5;

        public override string ToString()
        {
            return $"(?) Land Sale. territory: {territoryTypeId}, ward: {wardNum + 1}, land: {landId + 1}";
        }
    }
}
