using System;
using System.Net;
using System.Windows.Data;

namespace ZumtenSoft.WebsiteCrawler.Resources.Converters
{
    public class IPAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IPAddress input = value as IPAddress;
            return input == null ? String.Empty : input.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input = value as string;
            return String.IsNullOrEmpty(input) ? null : IPAddress.Parse(input);
        }
    }
}
