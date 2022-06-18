using System.Text.Json.Serialization;

namespace Xunkong.Widget.Hoyolab.News
{
    internal class NewsDetailWrapper
    {

        [JsonPropertyName("post")]
        public NewsItem Post { get; set; }

    }
}