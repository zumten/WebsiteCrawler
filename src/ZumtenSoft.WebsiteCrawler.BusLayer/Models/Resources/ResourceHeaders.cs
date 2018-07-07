using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    [DebuggerDisplay(@"\{ResourceHeaders Count={Count}\}")]
    public class ResourceHeaders : Dictionary<string, string>
    {
        public ResourceHeaders() : base(StringComparer.InvariantCultureIgnoreCase)
        {
            
        }
    }
}
