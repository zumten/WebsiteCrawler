using System;
using System.Collections.Generic;
using System.Net;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public enum HttpMethod
    {
        OPTIONS,
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
        TRACE,
        CONNECT
    }

    /// <summary>
    /// Élément de base permettant de récupérer le contenu d'une requête HTTP.
    /// </summary>
    public interface IHttpDataProvider
    {
        IHttpRequest GetRequest(Uri uri);
    }

    public interface IHttpRequest
    {
        HttpMethod Method { get; }
        Uri Url { get; }
        Dictionary<string, string> Headers { get; }
        IHttpResponse GetResponse();
    }

    public interface IHttpResponse
    {
        HttpStatusCode StatusCode { get; }
        Uri RequestUri { get; }
        Dictionary<string, string> Headers { get; }
        byte[] Content { get; }
        string ReadContent();
    }
}
