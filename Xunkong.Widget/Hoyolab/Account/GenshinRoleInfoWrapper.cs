using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xunkong.Widget.Hoyolab.Account
{
    internal class GenshinRoleInfoWrapper
    {
        [JsonPropertyName("list")]
        public List<GenshinRoleInfo> List { get; set; }
    }
}