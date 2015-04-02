using System;
using System.Windows.Data;

namespace minmpc.Converter {
    internal class TimeFormatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var timespan = (TimeSpan) value;
            if (timespan.TotalHours < 1) {
                return timespan.ToString("m':'ss");
            } else if (timespan.TotalDays < 1) {
                return timespan.ToString("h':'mm':'ss");
            } else {
                return timespan.ToString("d'.'hh':'mm':'ss");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
