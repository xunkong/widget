using System.Collections.Generic;
using Xunkong.Widget.Hoyolab.Account;

namespace Xunkong.Widget.Services
{
    public abstract class DatabaseSql
    {


        public static List<string> GetUpdatingSqls(int currentVersion)
        {
            if (currentVersion < 1)
            {
                return new List<string> { TableStructure_1 };
            }
            return new List<string>(0);
        }




        public static string InsertHoyolabUserInfo(HoyolabUserInfo info)
        {
            var h = info;
            return "INSERT OR REPLACE INTO HoyolabUserInfo (Uid, Nickname, Introduce, AvatarUrl, Pendant, Cookie) VALUES" + " " +
                $"({h.Uid}, '{h.Nickname}', '{h.Introduce}', '{h.AvatarUrl}', '{h.Pendant}', '{h.Cookie}');";
        }



        public static string InsertGenshinRoleInfo(GenshinRoleInfo info)
        {
            var h = info;
            return "INSERT OR REPLACE INTO GenshinRoleInfo (Uid, Region, Nickname, Level, RegionName, Cookie) VALUES" + " " +
                $"({h.Uid}, {(int)h.Region}, '{h.Nickname}', {h.Level}, '{h.RegionName}', '{h.Cookie}');";
        }




        public const string TableStructure_Initial = @"
CREATE TABLE IF NOT EXISTS DatabaseVersion
(
    Key   TEXT NOT NULL PRIMARY KEY,
    Value TEXT
);";


        private const string TableStructure_1 = @"
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS Setting
(
    Key   TEXT NOT NULL PRIMARY KEY,
    Value TEXT
);
CREATE TABLE IF NOT EXISTS HoyolabUserInfo
(
    Uid       INTEGER NOT NULL PRIMARY KEY,
    Nickname  TEXT,
    Introduce TEXT,
    AvatarUrl TEXT,
    Pendant   TEXT,
    Cookie    TEXT
);
CREATE TABLE IF NOT EXISTS GenshinRoleInfo
(
    Uid        INTEGER NOT NULL PRIMARY KEY,
    Region     INTEGER NOT NULL,
    Nickname   TEXT,
    Level      INTEGER NOT NULL,
    RegionName TEXT,
    Cookie     TEXT
);
INSERT OR REPLACE INTO DatabaseVersion (Key, Value) VALUES ('DatabaseVersion', 1);
COMMIT;";




    }
}
