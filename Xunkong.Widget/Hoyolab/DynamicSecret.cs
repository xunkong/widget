using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Xunkong.Widget.Hoyolab
{
    internal abstract class DynamicSecret
    {

        private static readonly string ApiSalt = "4a8knnbk5pbjqsrudp3dq484m9axoc5g";

        private static readonly string ApiSalt2 = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs";

        private static readonly Random Random = new Random();

        private static readonly MD5 MD5 = MD5.Create();

        public static string CreateSecret()
        {
            var t = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string r = GetRandomString(t);
            var bytes = MD5.ComputeHash(Encoding.UTF8.GetBytes($"salt={ApiSalt}&t={t}&r={r}"));
            var check = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            return $"{t},{r},{check}";
        }


        public static string CreateSecret2(string url, object postBody = null)
        {
            int t = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string r = Random.Next(100000, 200000).ToString();
            string b = postBody is null ? "" : JsonSerializer.Serialize(postBody);
            string q = "";
            string[] urls = url.Split('?');
            if (urls.Length == 2)
            {
                string[] queryParams = urls[1].Split('&').OrderBy(x => x).ToArray();
                q = string.Join("&", queryParams);
            }
            var bytes = MD5.ComputeHash(Encoding.UTF8.GetBytes($"salt={ApiSalt2}&t={t}&r={r}&b={b}&q={q}"));
            var check = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            string result = $"{t},{r},{check}";
            return result;
        }


        private static string GetRandomString(int timestamp)
        {
            var sb = new StringBuilder(6);
            var random = new Random(timestamp);
            for (int i = 0; i < 6; i++)
            {
                int v8 = random.Next(0, 32768) % 26;
                int v9 = 87;
                if (v8 < 10)
                {
                    v9 = 48;
                }
                _ = sb.Append((char)(v8 + v9));
            }
            return sb.ToString();
        }

    }
}