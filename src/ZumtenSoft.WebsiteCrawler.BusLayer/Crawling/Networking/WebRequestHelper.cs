using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public static class WebRequestHelper
    {
        public static SocketHttpResponse GetRequest(string url)
        {
            using (SocketHttpDataProvider provider = new SocketHttpDataProvider(2))
            {
                SocketHttpRequest request = (SocketHttpRequest)provider.GetRequest(new Uri(url));
                return (SocketHttpResponse)request.GetResponse();
            }
        }

        public static SocketHttpResponse GetRequest(string url, string ipAddress)
        {
            using (SocketHttpDataProvider provider = new SocketHttpDataProvider(2))
            using (SocketReference socket = provider.Sockets.GetSocket(new IPEndPoint(IPAddress.Parse(ipAddress), 80)))
            {
                SocketHttpRequest request = (SocketHttpRequest)provider.GetRequest(new Uri(url));
                return (SocketHttpResponse) request.GetResponse(socket.Socket);
            }
        }

        //public static HttpWebResponse GetResponse(Uri url)
        //{
        //    int nbRetry = Int32.Parse(ConfigurationManager.AppSettings["NbRetry"]);
        //    for (int i = 0; i <= nbRetry; i++)
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.AbsoluteUri);
        //        request.AllowAutoRedirect = false;
        //        request.Timeout = Int32.Parse(ConfigurationManager.AppSettings["Timeout"]);
        //        request.KeepAlive = true;

        //        HttpWebResponse response;
        //        try
        //        {
        //            response = (HttpWebResponse) request.GetResponse();
        //        }
        //        catch (WebException e)
        //        {
        //            if (e.Response == null && i == nbRetry)
        //                throw;

        //            response = (HttpWebResponse) e.Response;
        //        }

        //        if (response != null)
        //            return response;
        //    }
        //    return null;
        //}

        //public static string GetContent(this HttpWebResponse response)
        //{
        //    using (Stream stream = response.GetResponseStream())
        //    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}
    }
}
