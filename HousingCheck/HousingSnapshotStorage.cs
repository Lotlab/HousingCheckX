using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingCheck
{
    public class HousingSnapshotStorage
    {
        readonly object snapshotLock = new Object();

        SortedDictionary<LandIdent, HousingSlotSnapshot> storage = new SortedDictionary<LandIdent, HousingSlotSnapshot>();

        DateTime timeAfter = DateTime.Now;

        public void Add(HousingSlotSnapshot snapshot)
        {
            lock (snapshotLock)
            {
                storage[snapshot.landIdent] = snapshot;
            }
        }

        public HousingSlotSnapshot Get(LandIdent ident)
        {
            lock (snapshotLock)
            {
                if (!storage.ContainsKey(ident)) return null;
                return storage[ident];
            }
        }

        public void MarkOutdated(DateTime date)
        {
            timeAfter = date;
        }

        public int UploadCount
        {
            get
            {
                lock (snapshotLock)
                {
                    return storage.Where(kv => kv.Value.Time > timeAfter).Count();
                }
            }
        }

        public void SaveCsv(StreamWriter writer)
        {
            lock (snapshotLock)
            {
                LandIdent lastIdent = new LandIdent(0, HouseArea.UNKNOW, 0, 0);
                foreach (var item in storage)
                {
                    if (item.Key.ServerID != lastIdent.ServerID)
                    {
                        writer.WriteLine("服务器：" + item.Key.ServerID);
                        writer.WriteLine("房区,房号,类型,所有者,售价,大小,访客权限,购买方式,购买限制");
                    }
                    var snapshot = item.Value;
                    writer.WriteLine(snapshot.ToCsv());

                    lastIdent = item.Key;
                }
            }
        }

        public IEnumerable<HousingSlotSnapshot> GetModifiedItems()
        {
            lock (snapshotLock)
            {
                return storage.Where(kv => kv.Value.Time > timeAfter).Select(kv => kv.Value);
            }
        }
    }
}
