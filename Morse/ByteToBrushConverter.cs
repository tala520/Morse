using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Morse
{
    public class ByteToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null, nameof(value) + " != null");
            return parameter != null ? new SolidColorBrush(BlockColors.Covert(((ObservableCollection<byte?>) value)[(int)parameter])) 
                : new SolidColorBrush(BlockColors.Covert((byte?) (value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}