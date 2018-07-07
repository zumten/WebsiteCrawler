using System;
using System.Collections.Concurrent;
using System.Net;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public class HostProvider
    {
        private ConcurrentDictionary<string, IPAddress> _hosts = new ConcurrentDictionary<string, IPAddress>(StringComparer.InvariantCultureIgnoreCase);

        public HostProvider()
        {
            _hosts.TryAdd("localhost", IPAddress.Parse("127.0.0.1"));
        }

        public IPAddress this[string host]
        {
            get { return _hosts.GetOrAdd(host, x => Dns.GetHostEntry(x).AddressList[0]); }
            set { _hosts[host] = value; }
        }
    }
}
