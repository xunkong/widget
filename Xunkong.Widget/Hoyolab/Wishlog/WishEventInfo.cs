﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunkong.Widget.Hoyolab.Core;

namespace Xunkong.Widget.Hoyolab.Wishlog
{
    public class WishEventInfo
    {

        public int Id { get; set; }


        public WishType WishType { get; set; }

        [JsonIgnore]
        public WishType QueryType => WishTypeToQueryType(WishType);

        public string Name { get; set; }

        public string Version { get; set; }


        [JsonIgnore]
        public DateTimeOffset StartTime => TimeStringToTimeOffset(_StartTimeString);


        [JsonIgnore]
        public DateTimeOffset EndTime => TimeStringToTimeOffset(_EndTimeString);


        [JsonPropertyName("StartTime"), JsonInclude]
#if NativeAOT
    public string _StartTimeString { get; private set; }
#else
        public string _StartTimeString { get; set; }
#endif

        [JsonPropertyName("EndTime"), JsonInclude]
#if NativeAOT
    public string _EndTimeString { get; set; }
#else
        public string _EndTimeString { get; private set; }
#endif

        /// <summary>
        /// Up5星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank5UpItems { get; set; }

        /// <summary>
        /// Up4星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank4UpItems { get; set; }



        private static WishType WishTypeToQueryType(WishType type)
        {
            switch (type)
            {

                case WishType.CharacterEvent_2:
                    return WishType.CharacterEvent;
                default:
                    return type;
            }
        }


        [JsonIgnore]
        public static RegionType RegionType { get; set; }


        private static DateTimeOffset TimeStringToTimeOffset(string str)
        {
            if (str.Contains("+"))
            {
                return DateTimeOffset.Parse(str);
            }
            else
            {
                string offset;
                switch (RegionType)
                {
                    case RegionType.os_usa:
                        offset = "-05:00";
                        break;
                    case RegionType.os_euro:
                        offset = "+01:00";
                        break;
                    default:
                        offset = "+08:00";
                        break;
                }
                return DateTimeOffset.Parse($"{str} {offset}");
            }
        }

    }
}