using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace DivaModManager.UI
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class PathNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((string)value).Replace($@"{Global.assemblyLocation}{Global.s}Mods{Global.s}{Global.config.CurrentGame}", "...");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
