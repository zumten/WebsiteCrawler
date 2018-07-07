using System;
using System.Diagnostics;
using ZumtenSoft.WebsiteCrawler.Utils.Events;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingCondition fieldType={fieldType.Name}, IsRegex={IsRegex}, Value={Value}\}")]
    public class CrawlingCondition : NotifyObject
    {
        public Guid Guid { get; set; }

        private CrawlingConditionFieldType _fieldType;
        public CrawlingConditionFieldType FieldType
        {
            get { return _fieldType; }
            set { if (_fieldType != value) { _fieldType = value; Notify("FieldType"); } }
        }

        private CrawlingConditionComparisonType _comparisonType;
        public CrawlingConditionComparisonType ComparisonType
        {
            get { return _comparisonType; }
            set { if (_comparisonType != value) { _comparisonType = value; Notify("ComparisonType"); } }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { if (_value != value) { _value = value; Notify("Value"); } }
        }
    }
}