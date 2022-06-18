using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xunkong.Widget.Hoyolab.Wishlog
{
    /// <summary>
    /// 祈愿记录请求类
    /// </summary>
    public class WishlogClient
    {

        private const string CnUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";

        private const string SeaUrl = "https://hk4e-api-os.mihoyo.com/event/gacha_info/api/getGachaLog";

        private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

        private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");

        private readonly HttpClient _httpClient;

        private readonly Random Random;

        /// <summary>
        /// 获取记录的进度
        /// </summary>
        public event EventHandler<(WishType WishType, int Page)> ProgressChanged;


        public WishlogClient(HttpClient httpClient = null)
        {
            Random = new Random();
            if (httpClient is null)
            {
                _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
            }
            else
            {
                _httpClient = httpClient;
            }
        }



        /// <summary>
        /// 从原神的日志文件获取祈愿记录网址（仅 Windows 可用）
        /// </summary>
        /// <param name="isSea">是否外服优先</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">没有找到日志文件，或没有从日志文件中找到祈愿记录网址</exception>
        public static async Task<string> GetWishlogUrlFromLogFileAsync(bool isSea = false)
        {
            var file = isSea ? LogFile_Sea : LogFile_Cn;
            if (!File.Exists(file))
            {
                file = isSea ? LogFile_Cn : LogFile_Sea;
            }
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Cannot find log file of Genshin Impact.");
            }
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {

                using (var reader = new StreamReader(stream))
                {
                    var log = await reader.ReadToEndAsync();
                    var matches = Regex.Matches(log, @"OnGetWebViewPageFinish:(.+#/log)");
                    if (matches.Any())
                    {
                        return matches.Last().Value.Replace("OnGetWebViewPageFinish:", "");
                    }
                    else
                    {
                        throw new FileNotFoundException("Cannot find wishlog url from log file of Genshin Impact.");
                    }
                }
            }
        }


        /// <summary>
        /// 合并网址和身份验证相关的查询参数
        /// </summary>
        /// <param name="wishlogUrl"></param>
        /// <returns></returns>
        /// <exception cref="HoyolabException"></exception>
        private static string GetBaseAndAuthString(string wishlogUrl)
        {
            var match = Regex.Match(wishlogUrl, @"(https://webstatic.+#/log)");
            if (!match.Success)
            {
                throw new HoyolabException(-1, "Url does not fit the requirement.");
            }
            wishlogUrl = match.Groups[1].Value;
            var auth = wishlogUrl.Substring(wishlogUrl.IndexOf('?')).Replace("#/log", "");
            if (wishlogUrl.Contains("webstatic-sea"))
            {
                return SeaUrl + auth;
            }
            else
            {
                return CnUrl + auth;
            }
        }


        /// <summary>
        /// 获取一页祈愿记录
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="param"></param>
        /// <returns>没有数据时返回空集合</returns>
        /// <exception cref="HoyolabException"></exception>
        private async Task<List<WishlogItem>> GetWishlogByParamAsync(string baseString, QueryParam param)
        {
            var url = $"{baseString}&{param}";
            await Task.Delay(Random.Next(200, 300));
#if NativeAOT
        var response = await _httpClient.GetFromJsonAsync(url, WishlogJsonContext.Default.HoyolabBaseWrapperWishlogWrapper);
#else
            var response = await _httpClient.GetFromJsonAsync<HoyolabBaseWrapper<WishlogWrapper>>(url);
#endif
            if (response is null)
            {
                throw new HoyolabException(-1, "Cannot parse the return data.");
            }
            if (response.ReturnCode != 0)
            {
                throw new HoyolabException(response.ReturnCode, response.Message ?? "No return meesage.");
            }
            return response.Data?.List ?? new List<WishlogItem>(0);
        }


        /// <summary>
        /// 获取一种卡池类型的祈愿数据
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="lastId">获取的祈愿id小于最新id即停止</param>
        /// <param name="size">每次api请求获取几条数据，不超过20，默认6</param>
        /// <returns>没有数据返回空集合</returns>
        /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
        private async Task<List<WishlogItem>> GetWishlogByTypeAsync(string baseString, WishType queryType, long lastId = 0, int size = 20)
        {
            var param = new QueryParam(queryType, 1, size);
            var result = new List<WishlogItem>();
            while (true)
            {
                ProgressChanged?.Invoke(this, (queryType, param.Page));
                var list = await GetWishlogByParamAsync(baseString, param);
                result.AddRange(list);
                if (list.Count == size && list.Last().Id > lastId)
                {
                    param.Page++;
                    param.EndId = list.Last().Id;
                }
                else
                {
                    break;
                }
            }
            foreach (var item in result)
            {
                item.QueryType = queryType;
            }
            return result;
        }


        /// <summary>
        /// 获取所有的祈愿数据
        /// </summary>
        /// <param name="wishlogUrl"></param>
        /// <param name="lastId"></param>
        /// <param name="size">一次获取几条记录，[1,20]</param>
        /// <returns>以 id 顺序排列，没有数据返回空集合</returns>
        public async Task<List<WishlogItem>> GetAllWishlogAsync(string wishlogUrl, long lastId = 0, int size = 20)
        {
            var baseUrl = GetBaseAndAuthString(wishlogUrl);
            lastId = lastId < 0 ? 0 : lastId;
            size = Math.Clamp(size, 1, 20);
            var result = new List<WishlogItem>();
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Novice, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Permanent, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.CharacterEvent, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.WeaponEvent, lastId, size));
            return result.OrderBy(x => x.Id).ToList();
        }


        /// <summary>
        /// 获取祈愿记录网址关联的 uid
        /// </summary>
        /// <param name="wishlogUrl"></param>
        /// <returns>没有祈愿记录则返回 0</returns>
        public async Task<int> GetUidAsync(string wishlogUrl)
        {
            var baseUrl = GetBaseAndAuthString(wishlogUrl);
            var param = new QueryParam(WishType.CharacterEvent, 1);
            var list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.Permanent;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.WeaponEvent;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.Novice;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            return 0;
        }

    }
}