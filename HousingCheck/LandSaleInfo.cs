using Lotlab.PluginCommon.FFXIV.Parser;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace HousingCheck
{
    public class LandSaleInfo : IPCPacketBase<FFXIVIpcLandSaleInfo>
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("LandSaleInfo: ");
            sb.AppendFormat("{0}, ", Value.purchase_type);
            sb.AppendFormat("{0}, ", Value.region_type);
            sb.AppendFormat("{0}, ", Value.status);
            sb.AppendFormat("Unk: {0} {1}, ", Value.unknown0, Value.unknown1);
            sb.AppendFormat("Pad: {0} {1} {2}. ", Value.padding0, Value.padding1, Value.padding2);
            sb.AppendFormat("{0} participated, ", Value.persons);
            sb.AppendFormat("your number is {0}, {1} win, ", Value.player_number, Value.winner);
            sb.AppendFormat("ends at {0}, ", DateTimeOffset.FromUnixTimeSeconds(Value.endTime).LocalDateTime);
            sb.AppendFormat("refund {0} until {1}.", Value.refund_amount, DateTimeOffset.FromUnixTimeSeconds(Value.refund_expiry_time).LocalDateTime);
            return sb.ToString();
        }
    }

    public enum LandStatus : byte
    {
        FCFS = 0,
        Available = 1,
        InResultsPeriod = 2,
        Unavailable = 3
    }

    public enum LotteryPlayerResult : byte
    {
        NoEntry,
        Entered,
        Winner,
        WinnerForfeit,
        Loser,
        RefundExpired,
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FFXIVIpcLandSaleInfo
    {
        public IPCHeader ipc;

        public HousePurchaseType purchase_type;
        public HouseRegionType region_type;
        public LandStatus status;
        public byte unknown0;
        public byte unknown1;

        public byte padding0;
        public byte padding1;
        public byte padding2;

        public UInt32 endTime;
        public UInt32 refund_expiry_time;
        public UInt32 persons;
        public UInt32 winner;
        public UInt32 player_number;
        public UInt32 refund_amount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] unknown4;
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

        public bool IsValid()
        {
            var area = LandIdent.GetHouseArea(territoryTypeId);
            if (area == HouseArea.UNKNOW) return false;
            if (landId >= 60) return false;
            if (wardNum >= 24) return false;
            return true;
        }
    }
}
