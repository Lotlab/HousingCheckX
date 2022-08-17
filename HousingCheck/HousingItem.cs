using Lotlab.PluginCommon.FFXIV.Parser;
using Lotlab.PluginCommon.FFXIV.Parser.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingCheck
{
    public enum HouseOwnerType
    {
        PERSON,
        GUILD,
        EMPTY,
    }

    public enum HouseAccess
    {
        PUBLIC,
        LOCKED,
    }

    public enum HouseArea
    {
        UNKNOW,
        海雾村,
        薰衣草苗圃,
        白银乡,
        高脚孤丘,
        穹顶皓天
    }

    public enum HouseSize
    {
        S,
        M,
        L
    }

    public enum HouseFlagsDefine : byte
    {
        IsEstateOwned = 1,
        IsPublicEstate = 2,
        HasEstateGreeting = 4,
        EstateFlagUnknown = 8,
        IsFreeCompanyEstate = 16,
    };

    public enum HousePurchaseType : byte
    {
        NotPurchasable, // 不可购买
        FCFS, // 先到先得
        Lottery // 抽选
    }

    public enum HouseRegionType : byte
    {
        FC = 1,
        Personal = 2
    }

    public struct HouseFlags
    {
        public bool IsForSale;
        public bool IsEstateOwned;
        public bool IsPublicEstate;
        public bool HasEstateGreeting;
        public bool EstateFlagUnknown;
        public bool IsFreeCompanyEstate;

        public HouseFlags(byte flags)
        {
            IsForSale = flags == 0;
            IsEstateOwned = (flags & (byte)HouseFlagsDefine.IsEstateOwned) > 0;
            IsPublicEstate = (flags & (byte)HouseFlagsDefine.IsPublicEstate) > 0;
            HasEstateGreeting = (flags & (byte)HouseFlagsDefine.HasEstateGreeting) > 0;
            EstateFlagUnknown = (flags & (byte)HouseFlagsDefine.EstateFlagUnknown) > 0;
            IsFreeCompanyEstate = (flags & (byte)HouseFlagsDefine.IsFreeCompanyEstate) > 0;
        }

        public override string ToString()
        {
            List<string> flags = new List<string>();
            if (IsForSale) flags.Add("IsForSale");
            if (IsEstateOwned) flags.Add("IsEstateOwned");
            if (IsPublicEstate) flags.Add("IsPublicEstate");
            if (HasEstateGreeting) flags.Add("HasEstateGreeting");
            if (EstateFlagUnknown) flags.Add("EstateFlagUnknown");
            if (IsFreeCompanyEstate) flags.Add("IsFreeCompanyEstate");

            return string.Join(", ", flags);
        }
    }

    public enum HouseTagsDefine : byte
    {
        None = 0,
        Emporium = 1,
        Boutique = 2,
        DesignerHome = 3,
        MessageBook = 4,
        Tavern = 5,
        Eatery = 6,
        ImmersiveExperience = 7,
        Cafe = 8,
        Aquarium = 9,
        Sanctum = 10,
        Venue = 11,
        Florist = 12,
        Library = 14,
        PhotoStudio = 15,
        HauntedHouse = 16,
        Atelier = 17,
        Bathhouse = 18,
        Garden = 19,
        FarEastern = 20,
    };

    public class HousingItemJSONObject
    {
        public int id;
        public string owner;
        public int price;
        public string size;
        public int[] tags;
        public bool isPersonal;
        public bool isEmpty;
        public bool isPublic;
        public bool hasGreeting;
    }

    public class HousingItem
    {
        public LandIdent LandIdent { get; }

        public HouseArea Area => LandIdent.Area;

        public int Slot => LandIdent.Slot;

        /// <summary>
        /// 房屋id
        /// </summary>
        public int Id => LandIdent.LandID;

        /// <summary>
        /// 所有者
        /// </summary>
        public string Owner;

        /// <summary>
        /// 售价
        /// </summary>
        public int Price;

        /// <summary>
        /// 房屋大小
        /// </summary>
        public HouseSize Size;

        /// <summary>
        /// 房屋信息
        /// </summary>
        public HouseFlags Flags;

        /// <summary>
        /// 所有者类型
        /// </summary>
        public HouseOwnerType OwnerType;

        public bool IsEmpty => OwnerType == HouseOwnerType.EMPTY;

        /// <summary>
        /// 访客权限
        /// </summary>
        public HouseAccess Access;

        /// <summary>
        /// 房屋展示信息
        /// </summary>
        public HouseTagsDefine[] Tags = new HouseTagsDefine[3] { 0, 0, 0 };

        public static string GetHouseAreaStr(HouseArea area)
        {
            switch (area)
            {
                case HouseArea.海雾村:
                    return "海雾村";
                case HouseArea.薰衣草苗圃:
                    return "薰衣草苗圃";
                case HouseArea.高脚孤丘:
                    return "高脚孤丘";
                case HouseArea.白银乡:
                    return "白银乡";
                case HouseArea.穹顶皓天:
                    return "穹顶皓天";
                default:
                    return "未知";
            }
        }
        public static string GetHouseAreaShortStr(HouseArea area)
        {
            switch (area)
            {
                case HouseArea.海雾村:
                    return "海";
                case HouseArea.薰衣草苗圃:
                    return "森";
                case HouseArea.高脚孤丘:
                    return "沙";
                case HouseArea.白银乡:
                    return "白";
                case HouseArea.穹顶皓天:
                    return "雪";
                default:
                    return "未知";
            }
        }
        public static int GetHouseAreaNum(HouseArea area)
        {
            switch (area)
            {
                case HouseArea.UNKNOW:
                    return -1;
                case HouseArea.海雾村:
                    return 0;
                case HouseArea.薰衣草苗圃:
                    return 1;
                case HouseArea.高脚孤丘:
                    return 2;
                case HouseArea.白银乡:
                    return 3;
                case HouseArea.穹顶皓天:
                    return 4;
                default:
                    return -1;
            }
        }

        public static string GetHouseSizeStr(HouseSize size)
        {
            switch (size)
            {
                case HouseSize.S:
                    return "S";
                case HouseSize.M:
                    return "M";
                case HouseSize.L:
                    return "L";
                default:
                    return "未知";
            }
        }

        public static string GetOwnerTypeStr(HouseOwnerType type)
        {
            switch (type)
            {
                case HouseOwnerType.EMPTY:
                    return "空房";
                case HouseOwnerType.GUILD:
                    return "部队房";
                case HouseOwnerType.PERSON:
                    return "个人房";
                default:
                    return "未知";
            }
        }

        public HousingItem() { }

        public HousingItem(LandIdent ident, FFXIVIpcHousingWardInfo.HouseInfoEntry entry)
        {
            LandIdent = ident;
            Price = (int)entry.housePrice;
            if (Price < 0 || Price > 100000000)
                throw new ArgumentException($"数据解析错误: 价格{Price}不正确");

            Size = (Price > 30000000) ? HouseSize.L : ((Price > 10000000) ? HouseSize.M : HouseSize.S);
            Flags = new HouseFlags(entry.infoFlags);
            //读取房屋展示信息
            for (var i = 0; i < 3; i++)
            {
                Tags[i] = (HouseTagsDefine)entry.houseAppeal[i];
            }
            Owner = entry.estateOwnerName.GetUTF8String();
            if (Flags.IsEstateOwned)
            {
                if (Flags.IsFreeCompanyEstate)
                {
                    OwnerType = HouseOwnerType.GUILD;
                }
                else
                {
                    OwnerType = HouseOwnerType.PERSON;
                }
            }
            else
            {
                OwnerType = HouseOwnerType.EMPTY;
            }
            //获取访问权限
            if (Flags.IsPublicEstate)
            {
                Access = HouseAccess.PUBLIC;
            }
            else
            {
                Access = HouseAccess.LOCKED;
            }
        }

        public string ToCsvLine(HousePurchaseType purchaseType, HouseRegionType regionType)
        {
            return string.Join(",", new string[] {
                GetHouseAreaStr(Area),
                (Slot + 1) + "区" + (Id + 1) + "号",
                GetOwnerTypeStr(OwnerType),
                Owner,
                Price.ToString(),
                GetHouseSizeStr(Size),
                (Access == HouseAccess.PUBLIC) ? "开放" : "封闭",
                HousingSlotSnapshot.GetPurchaseTypeName(purchaseType),
                HousingSlotSnapshot.GetRegionTypeName(regionType),
            });
        }

        public HousingItemJSONObject ToJsonObject()
        {
            HousingItemJSONObject ret = new HousingItemJSONObject();
            int[] tags = new int[3];
            for (var i = 0; i < 3; i++)
            {
                tags[i] = (int)Tags[i];
            }
            ret.id = Id + 1;
            ret.owner = Owner;
            ret.price = Price;
            ret.size = GetHouseSizeStr(Size);
            ret.tags = tags;
            ret.isPersonal = OwnerType == HouseOwnerType.PERSON;
            ret.isEmpty = IsEmpty;
            ret.isPublic = Access == HouseAccess.PUBLIC;
            ret.hasGreeting = Flags.HasEstateGreeting;
            return ret;
        }
    }
}
