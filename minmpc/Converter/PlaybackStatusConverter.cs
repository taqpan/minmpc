using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using minmpc.Core;

namespace minmpc.Converter {
    internal class PlaybackStatusConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var status = (PlaybackStatus) value;
            switch (status) {
                case PlaybackStatus.Play:
                    return "Playing";
                case PlaybackStatus.Pause:
                    return "Paused";
                case PlaybackStatus.Stop:
                    return "Stopped";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
