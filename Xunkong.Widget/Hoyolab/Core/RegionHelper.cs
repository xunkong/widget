using System.Linq;

namespace Xunkong.Widget.Hoyolab.Core
{
    public static class RegionHelper
    {

        public static RegionType UidToRegionType(int uid)
        {
            switch (uid.ToString().FirstOrDefault())
            {
                case '1': return RegionType.cn_gf01;
                case '2': return RegionType.cn_gf01;
                case '3': return RegionType.cn_gf01;
                case '4': return RegionType.cn_gf01;
                case '5': return RegionType.cn_qd01;
                case '6': return RegionType.os_usa;
                case '7': return RegionType.os_euro;
                case '8': return RegionType.os_asia;
                case '9': return RegionType.os_cht;
                default: return RegionType.None;
            }
        }

    }
}