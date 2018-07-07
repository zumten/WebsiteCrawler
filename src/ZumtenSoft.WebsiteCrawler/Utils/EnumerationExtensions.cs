using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Markup;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.Utils
{
    // http://stackoverflow.com/questions/58743/databinding-an-enum-property-to-a-combobox-in-wpf
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;
        private static Dictionary<Type, EnumerationMember[]> _cache = new Dictionary<Type, EnumerationMember[]>();

        public EnumerationExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            EnumerationMember[] values;
            if (!_cache.TryGetValue(EnumType, out values))
            {
                values = (
                  from object enumValue in Enum.GetValues(EnumType)
                  select new EnumerationMember
                  {
                      Value = enumValue,
                      Description = DescriptionExtractor.GetDescription(EnumType.GetField(enumValue.ToString()))
                  }).ToArray();

                _cache.Add(EnumType, values);
            }

            return values;
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }
}
