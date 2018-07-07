using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public class UrlRepeaterQuery
    {
        public UrlRepeaterQuery(Uri url)
        {
            Url = url;
            Start = DateTime.Now;
        }

        public Uri Url { get; set; }
        public DateTime Start { get; set; }
        public TimeSpan Duration { get; set; }
        public HttpStatusCode Status { get; set; }
    }
    
    public class UrlRepeater
    {
        private readonly IHttpDataProvider _provider;

        public UrlRepeater(IHttpDataProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<UrlRepeaterQuery> Run(Uri url, int nbTimes, TimeSpan delay)
        {
            DateTime start = DateTime.Now;
            DateTime nextQuery = start;

            for (int i = 0; i < nbTimes; i++)
            {
                UrlRepeaterQuery query = new UrlRepeaterQuery(url);
                HandleRequest(query);
                yield return query;

                if (i < nbTimes - 1)
                {
                    DateTime now = DateTime.Now;
                    while (now > nextQuery)
                        nextQuery = nextQuery + delay;
                    
                    Thread.Sleep(nextQuery - now);
                }
            }
        }

        private void HandleRequest(UrlRepeaterQuery query)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IHttpRequest request = _provider.GetRequest(query.Url);
            IHttpResponse response = request.GetResponse();
            query.Status = response.StatusCode;
            watch.Stop();
            query.Duration = watch.Elapsed;
        }
    }
}
