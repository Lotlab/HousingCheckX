using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HousingCheck
{
    public class HousingLandInfoSign
    {
        public LandIdent LandIdent { get; }
        public int ServerId => LandIdent.ServerID;
        public HouseArea Area => LandIdent.Area;
        public int Slot => LandIdent.Slot;
        public int LandID => LandIdent.LandID;

        public DateTime Time { get; }
        public UInt64 OwnerID { get; }
        public byte HouseIconAdd { get; }
        public HouseSize HouseSize { get; }
        public HouseOwnerType HouseType { get; }
        public string EstateName { get; }
        public string EstateGreeting { get; }
        public string OwnerName { get; }
        public string FcTag { get; }
        public byte[] Tag { get; }
 
        public HousingLandInfoSign(byte[] message)
        {
            /**
             *   struct FFXIVIpcLandInfoSign : FFXIVIpcBasePacket< LandInfoSign >
             *   {
             *      Common::LandIdent landIdent;
             *      uint64_t ownerId; // ither contentId or fcId
             *      uint32_t unknow1;
             *      uint8_t houseIconAdd;
             *      uint8_t houseSize;
             *      uint8_t houseType;
             *      char estateName[23];
             *      char estateGreeting[193];
             *      char ownerName[31];
             *      char fcTag[7];
             *      uint8_t tag[3];
             *   };
             */

            var time = message.SubArray(24, 4);
            Time = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(time, 0)).LocalDateTime;
            var msgBody = message.SubArray(32, message.Length - 32);
            var dataHeader = msgBody.SubArray(0, 8);
            LandIdent = new LandIdent(dataHeader);

            var offset = 8;
            OwnerID = BitConverter.ToUInt64(msgBody, offset);
            offset += sizeof(UInt64);

            offset += sizeof(UInt32); // Unknown1

            HouseIconAdd = msgBody[offset++]; // 01: ?
            byte houseSize = msgBody[offset++]; // 00: S, 01: M, 02: L
            HouseSize = (HouseSize)houseSize;
            byte houseType = msgBody[offset++]; // 00: FC, 02: Personal
            HouseType = houseType == 2 ? HouseOwnerType.PERSON : HouseOwnerType.GUILD;

            EstateName = HousingItem.DecodeOwnerName(msgBody.SubArray(offset, 23));
            offset += 23;
            EstateGreeting = HousingItem.DecodeOwnerName(msgBody.SubArray(offset, 193));
            offset += 193;
            OwnerName = HousingItem.DecodeOwnerName(msgBody.SubArray(offset, 31));
            offset += 31;
            FcTag = HousingItem.DecodeOwnerName(msgBody.SubArray(offset, 7));
            offset += 7;
            Tag = msgBody.SubArray(offset, 3);
        }
    }

    public class LandInfoSignBrief
    {
        public int ServerId { get; }
        public int Area { get; }
        /// <summary>
        /// 从0开始
        /// </summary>
        public int Slot { get; }
        /// <summary>
        /// 从1开始
        /// </summary>
        public int LandID { get; }
        public Int64 Time { get; }
        public UInt64 OwnerID { get; }
        public string OwnerName { get; }
        public string FcTag { get; }
        public bool IsPersonal { get; }

        public LandInfoSignBrief(HousingLandInfoSign sign)
        {
            ServerId = sign.ServerId;
            Area = HousingItem.GetHouseAreaNum(sign.Area);
            Slot = sign.Slot;
            LandID = sign.LandID + 1;
            Time = new DateTimeOffset(sign.Time).ToUnixTimeSeconds();
            OwnerID = sign.OwnerID;
            OwnerName = sign.OwnerName;
            FcTag = sign.FcTag;
            IsPersonal = sign.HouseType == HouseOwnerType.PERSON;
        }
    }

    public class LandInfoSignStorage
    {
        Dictionary<LandIdent, HousingLandInfoSign> storage = new Dictionary<LandIdent, HousingLandInfoSign>();

        DateTime timeAfter = DateTime.Now;

        public void Add(HousingLandInfoSign info)
        {
            lock (this)
            {
                storage[info.LandIdent] = info;
            }
        }

        public void Clear()
        {
            storage.Clear();
        }

        public void MarkOutdated(DateTime date)
        {
            timeAfter = date;
        }

        public int Count => storage.Count;

        public int UploadCount => storage.Where(i => i.Value.Time >= timeAfter).Count();

        public void WriteCSV(StreamWriter writer)
        {
            var values = storage.OrderBy(a => a.Key).Select(a => a.Value);
            writer.WriteLine("服务器,地区,房区,房号,大小,类型,所有者ID,所有者名称,部队简称,房屋名称,房屋问候语,更新时间");
            foreach (var item in values)
            {
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                    item.ServerId, 
                    item.Area, 
                    item.Slot + 1, 
                    item.LandID + 1,
                    item.HouseSize,
                    HousingItem.GetOwnerTypeStr(item.HouseType), 
                    item.OwnerID, 
                    item.OwnerName,
                    item.FcTag,
                    item.EstateName, 
                    item.EstateGreeting.Replace("\r", "\\n"), // 描述内换行替换
                    item.Time)
                );
            }
        }

        public LandInfoSignBrief[] ToJsonObj()
        {
            List<LandInfoSignBrief> array = new List<LandInfoSignBrief>();
            foreach (var item in storage)
            {
                if (item.Value.Time >= timeAfter)
                {
                    array.Add(new LandInfoSignBrief(item.Value));
                }
            }
            return array.ToArray();
        }
    }
}
