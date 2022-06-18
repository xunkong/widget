using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunkong.Widget.Hoyolab;
using Xunkong.Widget.Hoyolab.Account;
using Xunkong.Widget.Hoyolab.Core;
using Xunkong.Widget.Models;

namespace Xunkong.Widget.Services
{
    public class HoyolabService
    {


        private readonly HoyolabClient _client;


        public HoyolabService()
        {
            _client = new HoyolabClient();
        }




        public bool AnyUserInfo()
        {
            using (var db = DatabaseService.CreateConnection())
            {
                var command_query = new SqliteCommand("SELECT Count(*) FROM HoyolabUserInfo;", db);
                var count = (long)command_query.ExecuteScalar();
                return count > 0;
            }
        }



        public List<HoyolabUserInfo> GetHoyolabUserInfos()
        {
            using (var db = DatabaseService.CreateConnection())
            {
                var command_query = new SqliteCommand("SELECT * FROM HoyolabUserInfo;", db);
                var reader = command_query.ExecuteReader();
                var hoyolabUsers = new List<HoyolabUserInfo>();
                while (reader.Read())
                {
                    var hoyolabUser = new HoyolabUserInfo
                    {
                        Uid = reader.GetInt32(0),
                        Nickname = reader.GetString(1),
                        Introduce = reader.GetString(2),
                        AvatarUrl = reader.GetString(3),
                        Pendant = reader.GetString(4),
                        Cookie = reader.GetString(5),
                    };
                    hoyolabUsers.Add(hoyolabUser);
                }
                return hoyolabUsers;
            }
        }


        public List<GenshinRoleInfo> GetGenshinRoleInfos(string cookie = null)
        {
            using (var db = DatabaseService.CreateConnection())
            {
                var command_query = db.CreateCommand();
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    command_query.CommandText = "SELECT * FROM GenshinRoleInfo;";
                }
                else
                {
                    command_query.CommandText = "SELECT * FROM GenshinRoleInfo WHERE Cookie=@Cookie;";
                    command_query.Parameters.AddWithValue("@Cookie", cookie);
                }
                var reader = command_query.ExecuteReader();
                var genshinRoles = new List<GenshinRoleInfo>();
                while (reader.Read())
                {
                    var genshinRole = new GenshinRoleInfo
                    {
                        Uid = reader.GetInt32(0),
                        Region = reader.GetFieldValue<RegionType>(1),
                        Nickname = reader.GetString(2),
                        Level = reader.GetInt32(3),
                        RegionName = reader.GetString(4),
                        Cookie = reader.GetString(5),
                    };
                    genshinRoles.Add(genshinRole);
                }
                return genshinRoles;
            }
        }


        public List<GenshinRoleInfo> GetGenshinRoleInfos(int uid)
        {
            using (var db = DatabaseService.CreateConnection())
            {
                var command_query = new SqliteCommand("SELECT * FROM GenshinRoleInfo WHERE Uid=@Uid;", db);
                command_query.Parameters.AddWithValue("@Uid", uid);
                var reader = command_query.ExecuteReader();
                var genshinRoles = new List<GenshinRoleInfo>();
                while (reader.Read())
                {
                    var genshinRole = new GenshinRoleInfo
                    {
                        Uid = reader.GetInt32(0),
                        Region = reader.GetFieldValue<RegionType>(1),
                        Nickname = reader.GetString(2),
                        Level = reader.GetInt32(3),
                        RegionName = reader.GetString(4),
                        Cookie = reader.GetString(5),
                    };
                    genshinRoles.Add(genshinRole);
                }
                return genshinRoles;
            }
        }



        public async Task UpdateUserInfosAsync()
        {
            try
            {
                using (var db = DatabaseService.CreateConnection())
                {
                    var hoyolabUsers = GetHoyolabUserInfos();
                    foreach (var hoyolabUser in hoyolabUsers)
                    {
                        var newHoyolabUser = await _client.GetHoyolabUserInfoAsync(hoyolabUser.Cookie);
                        var newGenshinRoles = await _client.GetGenshinRoleInfosAsync(hoyolabUser.Cookie);
                        using (var t = db.BeginTransaction())
                        {
                            new SqliteCommand(DatabaseSql.InsertHoyolabUserInfo(newHoyolabUser), db, t).ExecuteNonQuery();
                            foreach (var item in newGenshinRoles)
                            {
                                new SqliteCommand(DatabaseSql.InsertGenshinRoleInfo(item), db, t).ExecuteNonQuery();
                            }
                            t.Commit();
                        }
                    }
                }
            }
            catch { }
        }



        public async Task AddUserInfosAsync(string cookie)
        {
            var newHoyolabUser = await _client.GetHoyolabUserInfoAsync(cookie);
            var newGenshinRoles = await _client.GetGenshinRoleInfosAsync(cookie);
            using (var db = DatabaseService.CreateConnection())
            {
                using (var t = db.BeginTransaction())
                {
                    new SqliteCommand(DatabaseSql.InsertHoyolabUserInfo(newHoyolabUser), db, t).ExecuteNonQuery();
                    foreach (var item in newGenshinRoles)
                    {
                        new SqliteCommand(DatabaseSql.InsertGenshinRoleInfo(item), db, t).ExecuteNonQuery();
                    }
                    t.Commit();
                }
            }
        }



        public void DeleteUserInfo(UserInfo info)
        {
            using (var db = DatabaseService.CreateConnection())
            {
                using (var t = db.BeginTransaction())
                {
                    new SqliteCommand($"DELETE FROM HoyolabUserInfo WHERE Uid={info?.HoyolabUserInfo?.Uid};", db, t).ExecuteNonQuery();
                    new SqliteCommand($"DELETE FROM GenshinRoleInfo WHERE Uid={info?.GenshinRoleInfo?.Uid};", db, t).ExecuteNonQuery();
                    t.Commit();
                }
            }
        }




        public async Task<List<UserInfo>> GetAllUserInfosAsync()
        {
            var userInfos = new List<UserInfo>();
            var hoyolabUsers = GetHoyolabUserInfos();
            foreach (var hoyolabUser in hoyolabUsers)
            {
                var genshinRoles = GetGenshinRoleInfos(hoyolabUser.Cookie);
                userInfos.AddRange(genshinRoles.Select(x => new UserInfo { HoyolabUserInfo = hoyolabUser, GenshinRoleInfo = x }));
            }
            foreach (var userInfo in userInfos)
            {
                try
                {
                    userInfo.DailyNoteInfo = await _client.GetDailyNoteAsync(userInfo.GenshinRoleInfo);
                    var notes = await _client.GetTravelNotesSummaryAsync(userInfo.GenshinRoleInfo);
                    userInfo.TravelNotesDayData = notes.DayData;
                }
                catch (Exception ex)
                {
                    userInfo.Error = true;
                    userInfo.ErrorMessage = ex.Message;
                }
            }
            return userInfos;
        }






    }




}

