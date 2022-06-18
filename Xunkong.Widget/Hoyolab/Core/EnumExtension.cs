using System;
using System.ComponentModel;
using System.Linq;

namespace Xunkong.Widget.Hoyolab.Core
{
    public static class EnumExtension
    {
        public static string ToDescription(this Enum value)
        {
            var text = value.ToString();
            var fieldInfo = value.GetType().GetField(text);
            var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return (descriptionAttribute as DescriptionAttribute)?.Description ?? text;
        }

    }
}