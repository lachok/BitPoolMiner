using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BitPoolMiner.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class WorkerTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (Application.Current.Properties["WorkerName"] == null)
                return "unknown";

            if (Application.Current.Properties["WorkerName"].ToString() == (string)value)
                return "local";

            return "remote";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //not implemented for now
            throw new NotImplementedException();
        }
    }
}
