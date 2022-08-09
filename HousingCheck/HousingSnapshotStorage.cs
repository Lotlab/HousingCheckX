using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingCheck
{
    public class AreaHousingStorage : SortedList<int, HousingSlotSnapshot> { }
    public class ServerHousingStorage : SortedList<HouseArea, AreaHousingStorage> { }
    public class HousingStorage : SortedList<int, ServerHousingStorage> { }
    public class HousingSnapshotStorage
    {
        public HousingStorage Snapshots = new HousingStorage();

        public void Insert(HousingSlotSnapshot snapshot)
        {
            lock (this) 
            {
                if (!Snapshots.ContainsKey(snapshot.ServerId))
                {
                    Snapshots.Add(snapshot.ServerId, new ServerHousingStorage());
                }
                var serverStorage = Snapshots[snapshot.ServerId];

                if (!serverStorage.ContainsKey(snapshot.Area))
                {
                    serverStorage.Add(snapshot.Area, new AreaHousingStorage());
                }
                var areaStorage = serverStorage[snapshot.Area];

                areaStorage[snapshot.Slot] = snapshot;
            }
        }

        public void SaveCsv(StreamWriter writer)
        {
            foreach(KeyValuePair<int, ServerHousingStorage> housingStorage in Snapshots)
            {
                writer.WriteLine("服务器：" + housingStorage.Key);
                writer.WriteLine("房区,小区,非扩购买方式,扩展购买方式,非扩购买限制,扩展购买限制");
                foreach (AreaHousingStorage serverHousingStorage in housingStorage.Value.Values)
                {
                    foreach (HousingSlotSnapshot slotSnapshot in serverHousingStorage.Values)
                    {
                        writer.WriteLine(slotSnapshot.SlotStateCsv());
                    }
                }

                writer.WriteLine("房区,房号,类型,所有者,售价,大小,访客权限");
                foreach(AreaHousingStorage serverHousingStorage in housingStorage.Value.Values)
                {
                    foreach(HousingSlotSnapshot slotSnapshot in serverHousingStorage.Values)
                    {
                        writer.WriteLine(slotSnapshot.ToCsv());
                    }
                }
            }
        }
    }
}
