using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Controls;

namespace ZumtenSoft.WebsiteCrawler.Resources.Validators
{
    public class IPAddressValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string input = value as string;
            if (String.IsNullOrEmpty(input)) // Valid input, converts to null.
            {
                return new ValidationResult(false, "Field cannot be empty");
            }
            IPAddress ipAddress;
            if (IPAddress.TryParse(input, out ipAddress))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "String is not a valid URI");
        }
    }
}
