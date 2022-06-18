using System;
using Windows.UI.Xaml.Data;

namespace Xunkong.Widget.Converters
{
    public class ObjToBoolStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is null ? "False" : "True";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
