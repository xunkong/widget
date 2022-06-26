using Microsoft.Data.Sqlite;
using System.IO;
using Windows.Storage;
using static Xunkong.Widget.Services.DatabaseSql;

namespace Xunkong.Widget.Services
{
    public static class DatabaseService
    {

        private static readonly string _dbPath;

        private static readonly string _dbConnectionString;

        private static bool _initialized;

        private const int DATABASE_VERSION = 1;



        static DatabaseService()
        {
            _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "XunkongWidget.db");
            _dbConnectionString = $"Data Source={_dbPath};";
        }




        private static void Initialize()
        {
            using (var db = new SqliteConnection(_dbConnectionString))
            {
                db.Open();
                var command_init = new SqliteCommand(TableStructure_Initial, db);
                command_init.ExecuteNonQuery();
                var command_queryVersion = new SqliteCommand("SELECT Value FROM DatabaseVersion WHERE Key='DatabaseVersion' LIMIT 1;", db);
                int.TryParse(command_queryVersion.ExecuteScalar() as string, out int version);
                if (version < DATABASE_VERSION)
                {
                    var updatingSqls = GetUpdatingSqls(version);
                    foreach (var sql in updatingSqls)
                    {
                        var command_updateStructure = new SqliteCommand(sql, db);
                        command_updateStructure.ExecuteNonQuery();
                    }
                }
            }
            _initialized = true;
        }



        public static SqliteConnection CreateConnection()
        {
            if (!_initialized)
            {
                Initialize();
            }
            var db = new SqliteConnection(_dbConnectionString);
            db.Open();
            return db;
        }





    }
}
