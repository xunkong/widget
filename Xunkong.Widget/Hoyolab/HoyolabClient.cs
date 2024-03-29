﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Xunkong.Widget.Hoyolab.Account;
using Xunkong.Widget.Hoyolab.Avatar;
using Xunkong.Widget.Hoyolab.DailyNote;
using Xunkong.Widget.Hoyolab.GameRecord;
using Xunkong.Widget.Hoyolab.News;
using Xunkong.Widget.Hoyolab.SpiralAbyss;
using Xunkong.Widget.Hoyolab.TravelNotes;

namespace Xunkong.Widget.Hoyolab
{

    /// <summary>
    /// 米游社 API 请求类
    /// </summary>
    public class HoyolabClient
    {

        #region Constant

        private const string Accept = "Accept";
        private const string Cookie = "Cookie";
        private const string UserAgent = "User-Agent";
        private const string X_Reuqest_With = "X-Requested-With";
        private const string DS = "DS";
        private const string Referer = "Referer";
        private const string Application_Json = "application/json";
        private const string com_mihoyo_hyperion = "com.mihoyo.hyperion";
        private const string x_rpc_app_version = "x-rpc-app_version";
        private const string x_rpc_device_id = "x-rpc-device_id";
        private const string x_rpc_client_type = "x-rpc-client_type";
        private const string UA2251 = @"Mozilla/5.0 miHoYoBBS/2.25.1";
        private const string AppVersion = "2.25.1";
        private static readonly string DeviceId = Guid.NewGuid().ToString("D");

        #endregion


        private readonly HttpClient _httpClient;



        /// <summary>
        /// 传入参数为空时会自动构造新的 <see cref="HttpClient"/>
        /// </summary>
        /// <param name="httpClient"></param>
        public HoyolabClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
        }



        private async Task<T> CommonSendAsync<T>(HttpRequestMessage request, CancellationToken? cancellationToken = null) where T : class
        {
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2251);
            var response = await _httpClient.SendAsync(request, cancellationToken ?? CancellationToken.None);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
#if NativeAOT
        var responseData = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), typeof(HoyolabBaseWrapper<T>), HoyolabJsonContext.Default) as HoyolabBaseWrapper<T>;
#else
            var responseData = await JsonSerializer.DeserializeAsync<HoyolabBaseWrapper<T>>(await response.Content.ReadAsStreamAsync());
#endif
            if (responseData is null)
            {
                throw new HoyolabException(-1, "Can not parse the response body.");
            }
            if (responseData.ReturnCode != 0)
            {
                throw new HoyolabException(responseData.ReturnCode, responseData.Message);
            }
            if (responseData.Data is null)
            {
                throw new HoyolabException(-1, "Response data is null");
            }
            return responseData.Data;
        }


        /// <summary>
        /// 米游社账号信息
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">输入的 <c>cookie</c> 为空</exception>
        /// <exception cref="ArgumentException"><c>cookie</c> 中不包含 account_id 或 cookie_token</exception>
        public async Task<HoyolabUserInfo> GetHoyolabUserInfoAsync(string cookie, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            if (!(cookie.Contains("account_id") && cookie.Contains("cookie_token")))
            {
                throw new ArgumentException(@"Cookie does not contain ""account_id"" or ""cookie_token"".");
            }
            var request = new HttpRequestMessage(HttpMethod.Get, "https://bbs-api.mihoyo.com/user/wapi/getUserFullInfo?gids=2");
            request.Headers.Add(Cookie, cookie);
            request.Headers.Add(Referer, "https://bbs.mihoyo.com/");
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(x_rpc_client_type, "5");
            var data = await CommonSendAsync<HoyolabUserInfoWrapper>(request, cancellationToken);
            data.HoyolabUserInfo.Cookie = cookie;
            return data.HoyolabUserInfo;
        }


        /// <summary>
        /// 获取原神账号信息
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">输入的 <c>cookie</c> 为空</exception>
        /// <exception cref="ArgumentException"><c>cookie</c> 中不包含 account_id 或 cookie_token</exception>
        public async Task<List<GenshinRoleInfo>> GetGenshinRoleInfosAsync(string cookie, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            if (!(cookie.Contains("account_id") && cookie.Contains("cookie_token")))
            {
                throw new ArgumentException(@"Cookie does not contain ""account_id"" or ""cookie_token"".");
            }
            var url = "https://api-takumi.mihoyo.com/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url));
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?game_id=2&utm_source=bbs&utm_medium=mys&utm_campaign=box");
            var data = await CommonSendAsync<GenshinRoleInfoWrapper>(request, cancellationToken);
            data.List?.ForEach(x => x.Cookie = cookie);
            return data.List ?? new List<GenshinRoleInfo>();
        }


        /// <summary>
        /// 账号的签到信息
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<SignInInfo> GetSignInInfoAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-takumi.mihoyo.com/event/bbs_sign_reward/info?act_id=e202009291139501&region={role.Region}&uid={role.Uid}");
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html?bbs_auth_required=true&act_id=e202009291139501&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            return await CommonSendAsync<SignInInfo>(request, cancellationToken);
        }


        /// <summary>
        /// 进行签到
        /// </summary>
        /// <param name="role"></param>
        /// <returns>true 签到成功，false 无需签到</returns>
        public async Task<bool> SignInAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var signInfo = await GetSignInInfoAsync(role, cancellationToken);
            if (signInfo.IsSign)
            {
                return false;
            }
            var obj = new { act_id = "e202009291139501", region = role.Region.ToString(), uid = role.Uid.ToString() };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-takumi.mihoyo.com/event/bbs_sign_reward/sign");
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(x_rpc_app_version, "2.10.1");
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html?bbs_auth_required=true&act_id=e202009291139501&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            request.Content = JsonContent.Create(obj);
            await CommonSendAsync<JsonNode>(request, cancellationToken);
            return true;
        }


        /// <summary>
        /// 获取玩家原神战绩
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<GameRecordSummary> GetGameRecordSummaryAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var url = $"https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/index?server={role.Region}&role_id={role.Uid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url));
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?game_id=2&utm_source=bbs&utm_medium=mys&utm_campaign=box");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<GameRecordSummary>(request);
        }


        /// <summary>
        /// 获取角色详细信息
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<List<AvatarDetail>> GetAvatarDetailsAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var obj = new
            {
                role_id = role.Uid.ToString(),
                server = role.Region.ToString(),
            };
            var url = "https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/character";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url, obj));
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?bbs_presentation_style=fullscreen");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Content = JsonContent.Create(obj);
            var data = await CommonSendAsync<AvatarDetailWrapper>(request);
            return data.Avatars;
        }


        /// <summary>
        /// 获取角色技能
        /// </summary>
        /// <param name="role"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public async Task<List<AvatarSkill>> GetAvatarSkillLevelAsync(GenshinRoleInfo role, int characterId, CancellationToken? cancellationToken = null)
        {
            var url = $"https://api-takumi.mihoyo.com/event/e20200928calculate/v1/sync/avatar/detail?avatar_id={characterId}&uid={role.Uid}&region={role.Region}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/ys/event/e20200923adopt_calculator/index.html?bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            var data = await CommonSendAsync<AvatarCalculate>(request);
            return data.Skills;
        }


        /// <summary>
        /// 活动记录
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>没办法定义数据结构</returns>
        public async Task<JsonNode> GetActivitiesAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var url = $"https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/activities?server={role.Region}&role_id={role.Uid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url));
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?game_id=2&utm_source=bbs&utm_medium=mys&utm_campaign=box");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<JsonNode>(request);
        }


        /// <summary>
        /// 实时便笺
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DailyNoteInfo> GetDailyNoteAsync(GenshinRoleInfo role, CancellationToken? cancellationToken = null)
        {
            var url = $"https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/dailyNote?server={role.Region}&role_id={role.Uid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url));
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?game_id=2&utm_source=bbs&utm_medium=mys&utm_campaign=box");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<DailyNoteInfo>(request);
            data.Uid = role.Uid;
            data.Nickname = role.Nickname;
            return data;
        }


        /// <summary>
        /// 旅行札记总览
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month">0 当前月</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TravelNotesSummary> GetTravelNotesSummaryAsync(GenshinRoleInfo role, int month = 0, CancellationToken? cancellationToken = null)
        {
            var url = $"https://hk4e-api.mihoyo.com/event/ys_ledger/monthInfo?month=0&bind_uid={role.Uid}&bind_region={role.Region}&bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/ys/event/e20200709ysjournal/index.html?bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<TravelNotesSummary>(request);
        }


        /// <summary>
        /// 旅行札记收入详情
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month"></param>
        /// <param name="type"></param>
        /// <param name="page">从1开始</param>
        /// <param name="limit">最大100</param>
        /// <param name="cancellationToken"></param>
        /// <returns>返回一页收入记录</returns>
        private async Task<TravelNotesDetail> GetTravelNotesDetailByPageAsync(GenshinRoleInfo role, int month, TravelNotesAwardType type, int page, int limit = 100, CancellationToken? cancellationToken = null)
        {
            var url = $"https://hk4e-api.mihoyo.com/event/ys_ledger/monthDetail?page={page}&month={month}&limit={limit}&type={(int)type}&bind_uid={role.Uid}&bind_region={role.Region}&bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/ys/event/e20200709ysjournal/index.html?bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<TravelNotesDetail>(request);
            foreach (var item in data.List)
            {
                item.Type = type;
            }
            return data;
        }


        /// <summary>
        /// 旅行札记收入详情
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month"></param>
        /// <param name="type"></param>
        /// <param name="limit">最大100</param>
        /// <param name="cancellationToken"></param>
        /// <returns>返回该月所有收入记录</returns>
        public async Task<TravelNotesDetail> GetTravelNotesDetailAsync(GenshinRoleInfo role, int month, TravelNotesAwardType type, int limit = 100, CancellationToken? cancellationToken = null)
        {
            var data = await GetTravelNotesDetailByPageAsync(role, month, type, 1, limit);
            if (data.List.Count < limit)
            {
                return data;
            }
            for (int i = 2; ; i++)
            {
                var addData = await GetTravelNotesDetailByPageAsync(role, month, type, i, limit);
                data.List.AddRange(addData.List);
                if (addData.List.Count < limit)
                {
                    break;
                }
            }
            return data;
        }


        /// <summary>
        /// 深境螺旋
        /// </summary>
        /// <param name="role"></param>
        /// <param name="schedule">1当期，2上期</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SpiralAbyssInfo> GetSpiralAbyssInfoAsync(GenshinRoleInfo role, int schedule, CancellationToken? cancellationToken = null)
        {
            var url = $"https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/spiralAbyss?schedule_type={schedule}&server={role.Region}&role_id={role.Uid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecret2(url));
            request.Headers.Add(Referer, "https://webstatic.mihoyo.com/app/community-game-records/?game_id=2&utm_source=bbs&utm_medium=mys&utm_campaign=box");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<SpiralAbyssInfo>(request);
            data.Uid = role.Uid;
            return data;
        }


        /// <summary>
        /// 新闻列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="lastId">已获取的新闻数量（如果第一页获取了20条，获取第二页时这个值为20）</param>
        /// <param name="size">最大50</param>
        /// <returns></returns>
        public async Task<List<NewsPost>> GetNewsListAsync(NewsType type, int lastId = 0, int size = 20)
        {
            var url = $"https://bbs-api.mihoyo.com/post/api/getNewsList?gids=2&last_id={lastId}&page_size={size}&type={(int)type}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(Referer, "https://app.mihoyo.com");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            var data = await CommonSendAsync<NewsListWrapper>(request);
            return data.List.Select(x => x.Post).ToList();
        }


        /// <summary>
        /// 新闻正文
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<NewsPost> GetNewsDetailAsync(int postId)
        {
            var url = $"https://bbs-api.mihoyo.com/post/api/getPostFull?post_id={postId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(Referer, "https://app.mihoyo.com");
            request.Headers.Add(x_rpc_app_version, AppVersion);
            var data = await CommonSendAsync<NewsDetailWrapper>(request);
            return data.Post.Post;
        }


    }
}