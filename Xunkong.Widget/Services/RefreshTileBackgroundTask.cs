using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Xunkong.Widget.Hoyolab;

namespace Xunkong.Widget.Services
{
    public sealed class RefreshTileBackgroundTask : IBackgroundTask
    {



        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            await RefreshTileAsync();
            deferral.Complete();
        }




        public static async Task RefreshTileAsync()
        {
            bool noTile = true;
            try
            {
                var service = new HoyolabService();
                var client = new HoyolabClient();
                var tiles = await TileService.FindAllAsync();
                foreach (var tile in tiles)
                {
                    noTile = false;
                    if (int.TryParse(tile.Replace("DailyNote_", ""), out int uid))
                    {
                        var info = service.GetGenshinRoleInfos(uid).FirstOrDefault();
                        if (info != null)
                        {
                            var dailyNote = await client.GetDailyNoteAsync(info);
                            TileService.UpdatePinnedTile(dailyNote);
                        }
                    }
                }
            }
            catch { }
            if (noTile)
            {
                UnregisterTask();
            }
        }



        public static async Task<bool> RegisterTaskAsync()
        {
            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (requestStatus == BackgroundAccessStatus.AlwaysAllowed || requestStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                UnregisterTask();
                var builder = new BackgroundTaskBuilder();
                builder.Name = "RefreshTileBackgroundTask";
                builder.SetTrigger(new TimeTrigger(16, false));
                BackgroundTaskRegistration task = builder.Register();
                return true;
            }
            else
            {
                return false;
            }
        }




        private static void UnregisterTask()
        {
            var allTasks = BackgroundTaskRegistration.AllTasks;
            foreach (var item in allTasks)
            {
                if (item.Value.Name == "RefreshTileBackgroundTask")
                {
                    item.Value.Unregister(true);
                }
            }
        }


    }
}
