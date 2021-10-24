using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;

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

        public LandIdent(byte[] message)
        {
            LandID = BitConverter.ToUInt16(message, 0);
            Slot = BitConverter.ToUInt16(message, 2);
            var areaID = BitConverter.ToUInt16(message, 4);
            ServerID = BitConverter.ToUInt16(message, 6);

            switch (areaID)
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
                case 0x376: // ID 暂定
                    Area = HouseArea.天穹街;
                    break;
                default:
                    break;
            }
        }
    
        public LandIdent(int server, HouseArea area, int slot, int land = 0)
        {
            ServerID = server;
            Area = area;
            Slot = slot;
            LandID = land;
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

        /// <summary>
        /// 从buffer中解析房区信息
        /// </summary>
        /// <param name="message"></param>
        public HousingSlotSnapshot(byte[] message)
        {
            var time = message.SubArray(24, 4);
            Time = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(time, 0)).LocalDateTime;
            var dataList = message.SubArray(32, message.Length - 32);
            var dataHeader = dataList.SubArray(0, 8);
            landIdent = new LandIdent(dataHeader);

            for (int i = 8; i < dataList.Length; i += 40)
            {
                int houseId = (i - 8) / 40;
                HousingItem house = new HousingItem(this, houseId, dataList.SubArray(i, 40));
                HouseList.Add(houseId, house);
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
