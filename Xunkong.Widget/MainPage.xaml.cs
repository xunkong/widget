using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using Xunkong.Widget.Models;
using Xunkong.Widget.Services;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Xunkong.Widget
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
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


        public MainPage()
        {
            this.InitializeComponent();
            InitializeTitleBar();
            _hoyolabService = new HoyolabService();
            _UserInfos.CollectionChanged += _UserInfos_CollectionChanged;
            this.Loaded += MainPage_Loaded;
        }

        private void _UserInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_UserInfos.Any())
            {
                Image_Splash.Visibility = Visibility.Collapsed;
                TextBlock_NoUserInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeTitleBar()
        {
            var view = ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Size(360, 480));
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            Window.Current.SetTitleBar(AppTitleBar);
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LaunchActivatedEventArgs args)
            {
                if (args.SplashScreen is SplashScreen splash)
                {
                    var rect = splash.ImageLocation;
                    Image_Splash.Width = rect.Width;
                    Image_Splash.Height = rect.Height;
                    Image_Splash.Margin = new Thickness(rect.Left, rect.Top, Window.Current.Bounds.Width - rect.Right, Window.Current.Bounds.Height - rect.Bottom);
                    TextBlock_NoUserInfo.Margin = new Thickness(0, rect.Height, 0, 0);
                    Window.Current.SizeChanged += Current_SizeChanged;
                }
            }
        }



        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Image_Splash.Margin = new Thickness(0);
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            GridView_UserInfo.Focus(FocusState.Pointer);
            try
            {
                bool cleared = false;
                if (!_hoyolabService.AnyUserInfo())
                {
                    TextBlock_NoUserInfo.Visibility = Visibility.Visible;
                    return;
                }
                await foreach (var item in _hoyolabService.GetAllUserInfosAsync())
                {
                    if (!cleared)
                    {
                        UserInfos.Clear();
                        cleared = true;
                    }
                    UserInfos.Add(item);
                }
                await _hoyolabService.UpdateUserInfosAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }


        private async void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri("https://github.com/xunkong/widget"));
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }


        private async void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = new StackPanel { Spacing = 8 };
            var hyperLink = new Hyperlink
            {
                NavigateUri = new Uri("https://xunkong.cc/help/desktop/account.html"),
                UnderlineStyle = UnderlineStyle.None,
            };
            hyperLink.Inlines.Add(new Run { Text = "如何获取 Cookie" });
            var text = new TextBlock();
            text.Inlines.Add(hyperLink);
            stackPanel.Children.Add(text);
            var input = new TextBox();
            stackPanel.Children.Add(input);
            var dialog = new ContentDialog
            {
                Title = "请输入 Cookie",
                Content = stackPanel,
                PrimaryButtonText = "确认",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var cookie = input.Text;
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    return;
                }
                try
                {
                    await _hoyolabService.AddUserInfosAsync(cookie);
                    bool cleared = false;
                    await foreach (var item in _hoyolabService.GetAllUserInfosAsync())
                    {
                        if (!cleared)
                        {
                            UserInfos.Clear();
                            cleared = true;
                        }
                        UserInfos.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private async void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _hoyolabService.UpdateUserInfosAsync();
                bool cleared = false;
                await foreach (var item in _hoyolabService.GetAllUserInfosAsync())
                {
                    if (!cleared)
                    {
                        UserInfos.Clear();
                        cleared = true;
                    }
                    UserInfos.Add(item);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }



        private void ShowError(string message)
        {
            var fly = new Flyout();
            fly.Content = new TextBlock { Text = message };
            fly.Placement = FlyoutPlacementMode.Bottom;
            fly.ShowAt(AppTitleBar);
        }


        private async void UserCard_Deleted(object sender, UserInfo e)
        {
            var dialog = new ContentDialog
            {
                Title = "删除账号",
                Content = $"删除 {e.GenshinRoleInfo.Nickname} 及相关账号？",
                PrimaryButtonText = "删除",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _hoyolabService.DeleteUserInfo(e);
                    if (UserInfos?.Contains(e) ?? false)
                    {
                        UserInfos.Remove(e);
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }


    }
}
