using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BitPoolMiner.Converter
{
    [ValueConversion(typeof(Decimal), typeof(string))]
    public class FiatCurrencyDisplayConverter : IValueConverter
    {
        private string FiatCurrencySymbol
        {
            get
            {
                if (Application.Current.Properties["Currency"] == null)
                    return "";
                else
                    return Application.Current.Properties["Currency"].ToString();
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //needs more sanity checks to make sure the value is really int
            //and that targetType is string
            Debug.Assert(value != null, nameof(value) + " != null");
            var fiatAmount = (decimal) value;
            return $"{Math.Round(fiatAmount, 2)} {FiatCurrencySymbol}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //not implemented for now
            throw new NotImplementedException();
        }
    }
}
