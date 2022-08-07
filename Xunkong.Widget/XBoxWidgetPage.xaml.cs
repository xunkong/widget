using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xunkong.Widget.Models;
using Xunkong.Widget.Services;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Xunkong.Widget
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class XBoxWidgetPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private readonly HoyolabService _hoyolabService;


        private ObservableCollection<UserInfo> _UserInfos = new();
        public ObservableCollection<UserInfo> UserInfos
        {
            get { return _UserInfos; }
            set
            {
                _UserInfos = value;
                OnPropertyChanged();
            }
        }


        private bool _ShowErrorEmotion;
        public bool ShowErrorEmotion
        {
            get { return _ShowErrorEmotion; }
            set
            {
                _ShowErrorEmotion = value;
                OnPropertyChanged();
            }
        }



        private string _ErrorMessage;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged();
            }
        }



        private DateTimeOffset lastUpdateTime;



        public XBoxWidgetPage()
        {
            this.InitializeComponent();
            _hoyolabService = new HoyolabService();
            this.Loaded += XBoxWidgetPage_Loaded;
        }



        private async void XBoxWidgetPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                bool cleared = false;
                ShowErrorEmotion = false;
                if (!_hoyolabService.AnyUserInfo())
                {
                    ShowErrorEmotion = true;
                    ErrorMessage = "没有账号";
                    return;
                }
                await foreach (var item in _hoyolabService.GetAllUserInfosAsync())
                {
                    if (item.Error)
                    {
                        ShowErrorEmotion = true;
                        ErrorMessage = item.ErrorMessage;
                        UserInfos.Clear();
                        return;
                    }
                    else
                    {
                        if (!cleared)
                        {
                            UserInfos.Clear();
                        }
                        UserInfos.Add(item);
                    }
                }
                lastUpdateTime = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                ShowErrorEmotion = true;
                ErrorMessage = ex.Message;
            }
        }



        public async Task RefreshAsync()
        {
            if (Window.Current.Visible)
            {
                // 判断条件没有错
                return;
            }
            if (DateTimeOffset.Now - lastUpdateTime < TimeSpan.FromMinutes(1))
            {
                return;
            }
            try
            {
                bool cleared = false;
                ShowErrorEmotion = false;
                if (!_hoyolabService.AnyUserInfo())
                {
                    ShowErrorEmotion = true;
                    ErrorMessage = "没有账号";
                    return;
                }
                await foreach (var item in _hoyolabService.GetAllUserInfosAsync())
                {
                    if (item.Error)
                    {
                        ShowErrorEmotion = true;
                        ErrorMessage = item.ErrorMessage;
                        UserInfos.Clear();
                        return;
                    }
                    else
                    {
                        if (!cleared)
                        {
                            UserInfos.Clear();
                        }
                        UserInfos.Add(item);
                    }
                }
                lastUpdateTime = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                ShowErrorEmotion = true;
                ErrorMessage = ex.Message;
            }
        }


    }
}
