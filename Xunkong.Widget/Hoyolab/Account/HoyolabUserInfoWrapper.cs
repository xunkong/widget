using System.Text.Json.Serialization;

namespace Xunkong.Widget.Hoyolab.Account
{
    internal class HoyolabUserInfoWrapper
    {
        [JsonPropertyName("user_info")]
        public HoyolabUserInfo HoyolabUserInfo { get; set; }
    }
}