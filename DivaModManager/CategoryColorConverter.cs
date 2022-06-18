using System;
using System.Windows.Data;
using System.Windows.Media;

namespace DivaModManager
{
    public class CategoryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "BugFix":
                    return new SolidColorBrush(Color.FromRgb(255, 78, 78));
                case "Overhaul":
                    return new SolidColorBrush(Color.FromRgb(255, 78, 78));
                case "Addition":
                    return new SolidColorBrush(Color.FromRgb(108, 177, 255));
                case "Feature":
                    return new SolidColorBrush(Color.FromRgb(108, 177, 255));
                case "Tweak":
                    return new SolidColorBrush(Color.FromRgb(255, 94, 157));
                case "Improvement":
                    return new SolidColorBrush(Color.FromRgb(255, 94, 157));
                case "Optimization":
                    return new SolidColorBrush(Color.FromRgb(255, 94, 157));
                case "Adjustment":
                    return new SolidColorBrush(Color.FromRgb(110, 255, 108));
                case "Suggestion":
                    return new SolidColorBrush(Color.FromRgb(110, 255, 108));
                case "Ammendment":
                    return new SolidColorBrush(Color.FromRgb(110, 255, 108));
                case "Removal":
                    return new SolidColorBrush(Color.FromRgb(153, 153, 153));
                case "Refactor":
                    return new SolidColorBrush(Color.FromRgb(153, 153, 153));
                default:
                    return new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
