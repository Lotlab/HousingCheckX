using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;
using Lotlab.PluginCommon.FFXIV.Parser.Packets;

namespace HousingCheck
{
    public class HousingSlotSnapshotJSONObject
    {
        public long time;
        public int server;
        public string area;
        public int slot;
        public HousingItemJSONObject[] houses;
    }

    public class LandIdent : IEquatable<LandIdent>, IComparable<LandIdent>
    {
        /// <summary>
        /// 服务器ID
        /// worldId
        /// </summary>
        public int ServerID { get; }
        /// <summary>
        /// 房屋区域
        /// territoryTypeId
        /// </summary>
        public HouseArea Area { get; } = HouseArea.UNKNOW;
        /// <summary>
        /// 小分区
        /// wardNum，从0开始
        /// </summary>
        public int Slot { get; }
        /// <summary>
        /// 房屋ID
        /// landID，从0开始
        /// </summary>
        public int LandID { get; } = 0;

        public LandIdent(Lotlab.PluginCommon.FFXIV.Parser.Packets.LandIdent ident)
        {
            ServerID = ident.worldId;
            switch (ident.territoryTypeId)
            {
                case 0x153:
                    Area = HouseArea.海雾村;
                    break;
                case 0x154:
                    Area = HouseArea.薰衣草苗圃;
                    break;
                case 0x155:
                    Area = HouseArea.高脚孤丘;
                    break;
                case 0x281:
                    Area = HouseArea.白银乡;
                    break;
                case 0x3D3:
                    Area = HouseArea.穹顶皓天;
                    break;
                default:
                    break;
            }
            Slot = ident.wardNum;
            LandID = ident.landId;
        }

        public LandIdent(int server, HouseArea area, int slot, int land = 0)
        {
            ServerID = server;
            Area = area;
            Slot = slot;
            LandID = land;
        }

        public LandIdent(LandIdent ident, int landID = 0)
        {
            ServerID = ident.ServerID;
            Area = ident.Area;
            Slot = ident.Slot;
            LandID = landID;
        }

        bool IEquatable<LandIdent>.Equals(LandIdent other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return ServerID == other.ServerID && Area == other.Area && Slot == other.Slot && LandID == other.LandID;
        }

        public override int GetHashCode()
        {
            return (ServerID << 6) + ((int)Area << 4) + (Slot << 2) + LandID;
        }

        int IComparable<LandIdent>.CompareTo(LandIdent other)
        {
            int tmp = ServerID - other.ServerID;
            if (tmp != 0) return Math.Sign(tmp);

            tmp = Area - other.Area;
            if (tmp != 0) return Math.Sign(tmp);

            tmp = Slot - other.Slot;
            if (tmp != 0) return Math.Sign(tmp);

            return Math.Sign(LandID - other.LandID);
        }
    }

    public class HousingSlotSnapshot
    {
        public DateTime Time { get; }
        public int ServerId => landIdent.ServerID;
        public HouseArea Area => landIdent.Area;
        public int Slot => landIdent.Slot;
        public LandIdent landIdent { get; }
        public SortedList<int, HousingItem> HouseList = new SortedList<int, HousingItem>();

        public HousingSlotSnapshot(HousingWardInfo info)
        {
            Time = DateTimeOffset.FromUnixTimeSeconds(info.Value.ipc.timestamp).LocalDateTime;
            landIdent = new LandIdent(info.Value.landIdent);

            for (int i = 0; i < info.Value.houseInfoEntry.Length; i++)
            {
                HousingItem house = new HousingItem(new LandIdent(landIdent, i), info.Value.houseInfoEntry[i]);
                HouseList.Add(i, house);
            }
        }

        public HousingItem[] GetOnSale()
        {
            List<HousingItem> onSaleList = new List<HousingItem>();
            foreach(HousingItem house in HouseList.Values)
            {
                if (house.IsEmpty)
                {
                    onSaleList.Add(house);
                }
            }
            return onSaleList.ToArray();
        }

        public string ToCsv()
        {
            StringBuilder csv = new StringBuilder();
            foreach(var house in HouseList.Values)
            {
                csv.AppendLine(house.ToCsvLine());
            }
            csv.AppendLine();
            return csv.ToString();
        }

        public HousingSlotSnapshotJSONObject ToJsonObject()
        {
            HousingSlotSnapshotJSONObject ret = new HousingSlotSnapshotJSONObject();
            ret.area = HousingItem.GetHouseAreaStr(Area);
            ret.server = ServerId;
            ret.slot = Slot;
            ret.time = new DateTimeOffset(Time).ToUnixTimeSeconds();

            List<HousingItemJSONObject> houseListJson = new List<HousingItemJSONObject>();
            foreach(var house in HouseList.Values)
            {
                houseListJson.Add(house.ToJsonObject());
            }

            ret.houses = houseListJson.ToArray();

            return ret;
        }
    }
}
