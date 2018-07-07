using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing
{
    public class BucketContext
    {
        public int NbRetry { get; private set; }
        public HostProvider Hosts { get; private set; }
        public SocketFactory Sockets { get; private set; }

        public BucketContext(int nbRetry)
        {
            NbRetry = nbRetry;
            Hosts = new HostProvider();
            Sockets = new SocketFactory(Hosts);
        }
    }
}
