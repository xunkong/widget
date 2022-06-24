using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xunkong.Widget.Models;
using Xunkong.Widget.Services;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Xunkong.Widget.Controls
{
    public sealed partial class UserCard : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public Visibility IsPinButtonVisible => Environment.OSVersion.Version < new Version("10.0.22000.0") ? Visibility.Visible : Visibility.Collapsed;



        private UserInfo _UserInfo;
        public UserInfo UserInfo
        {
            get { return _UserInfo; }
            set
            {
                _UserInfo = value;
                OnPropertyChanged();
            }
        }


        public UserCard()
        {
            this.InitializeComponent();
        }



        public event EventHandler<UserInfo> Deleted;

        private async void MenuItem_Pin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Environment.OSVersion.Version < new Version("10.0.22000.0"))
                {
                    await TileService.RequestPinTileAsync(UserInfo.DailyNoteInfo);
                    await RefreshTileBackgroundTask.RegisterTaskAsync();
                }
            }
            catch { }
        }



        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            Deleted?.Invoke(this, UserInfo);
        }


    }
}
