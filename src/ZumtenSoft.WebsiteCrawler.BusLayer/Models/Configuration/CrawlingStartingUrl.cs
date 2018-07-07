using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ZumtenSoft.WebsiteCrawler.Utils.Events;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingStartingUrl Name={Name}, Value={Value}\}")]
    public class CrawlingStartingUrl : NotifyObject
    {
        public Guid Guid { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; Notify("Name"); } }
        }

        private Uri _value;
        public Uri Value
        {
            get { return _value; }
            set { if (_value != value) { _value = value; Notify("Value"); } }
        }
    }
}
