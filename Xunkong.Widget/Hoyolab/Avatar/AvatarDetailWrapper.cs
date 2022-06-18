using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xunkong.Widget.Hoyolab.Avatar
{
    internal class AvatarDetailWrapper
    {
        [JsonPropertyName("avatars")]
        public List<AvatarDetail> Avatars { get; set; }
    }
}