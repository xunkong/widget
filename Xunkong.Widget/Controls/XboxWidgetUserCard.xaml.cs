using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using Xunkong.Widget.Models;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Xunkong.Widget.Controls
{
    public sealed partial class XboxWidgetUserCard : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


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


        public XboxWidgetUserCard()
        {
            this.InitializeComponent();
        }


    }
}
