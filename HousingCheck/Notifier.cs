using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advanced_Combat_Tracker;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;
using System.ComponentModel;

namespace HousingCheck
{
    public class Notifier
    {
        /// <summary>
        /// 定时任务Worker
        /// </summary>
        private BackgroundWorker TickWorker = new BackgroundWorker { WorkerSupportsCancellation = true };

        Config config { get; }

        public Notifier(Config config)
        {
            this.config = config;

            TickWorker.DoWork += TickDoWorker;
        }

        public void Start()
        {
            TickWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            TickWorker.CancelAsync();
        }


        private DateTime GetNextNotifyTime()
        {
            var nT = DateTime.Now.AddHours(1);
            return new DateTime(nT.Year, nT.Month, nT.Day, nT.Hour, 0, 0);
        }

        /// <summary>
        /// 弹出查房提示
        /// </summary>
        private void NotifyCheckHouse()
        {
            if (config.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS("查房提醒：该查房了");
            }
            if (config.EnableNotification)
            {
                new ToastContentBuilder()
                    .AddText("查房提醒")
                    .AddText("该查房了")
                    .Show();
            }
        }

        /// <summary>
        /// 弹出查房提示
        /// </summary>
        private void NotifyCheckHouseAsnyc()
        {
            new Action(NotifyCheckHouse).Invoke();
        }

        private void TickDoWorker(object sender, DoWorkEventArgs e)
        {
            DateTime lastNotify = DateTime.Now.AddSeconds(-1);
            while (!TickWorker.CancellationPending)
            {
                DateTime nextNotify = GetNextNotifyTime();
                if (DateTime.Now > nextNotify.AddSeconds(-config.CheckNotifyAheadTime))
                {
                    if (lastNotify != nextNotify && config.EnableNotifyCheck)
                    {
                        NotifyCheckHouseAsnyc();
                        lastNotify = nextNotify;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void NotifyEmptyHouseAsync(HousingOnSaleItem onSaleItem, bool exists)
        {
            new Action<HousingOnSaleItem, bool>((item, exist) => { NotifyEmptyHouse(item, exist); }).Invoke(onSaleItem, exists);
        }

        public void NotifyEmptyHouse(HousingOnSaleItem onSaleItem, bool exists)
        {
            if (onSaleItem.Size == HouseSize.S && !config.EnableNotifyHouseS)
                return;

            if ((onSaleItem.Size == HouseSize.M || onSaleItem.Size == HouseSize.L) && !config.EnableNotifyHouseML)
                return;

            if (onSaleItem.Area == HouseArea.穹顶皓天 && config.IgnoreEmpyreum)
                return;

            if (config.EnableNotification)
            {
                var title = string.Format("{0} 第{1}区 {2}号 {3}房",
                        onSaleItem.AreaStr,
                        onSaleItem.DisplaySlot,
                        onSaleItem.DisplayId,
                        onSaleItem.SizeStr
                    );
                new ToastContentBuilder()
                    .AddText("新空房")
                    .AddText(title)
                    .Show();
            }
            if (config.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS(
                    string.Format("{0}{1}区{2}号{3}房",
                        HousingItem.GetHouseAreaShortStr(onSaleItem.Area),
                        onSaleItem.DisplaySlot,
                        onSaleItem.DisplayId,
                        onSaleItem.SizeStr
                    )
                );
            }
        }
    }
}
