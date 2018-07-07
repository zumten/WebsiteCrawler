using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    /// <summary>
    /// Implémentation de IHttpDataProvider passant par l'ouverture de Socket à travers
    /// le SocketFactory.
    /// </summary>
    public class SocketHttpDataProvider : IHttpDataProvider, IDisposable
    {
        public int NbRetry { get; private set; }
        public SocketFactory Sockets { get; private set; }

        public SocketHttpDataProvider(int nbRetry)
        {
            NbRetry = nbRetry;
            Sockets = new SocketFactory(new HostProvider());
        }

        public IHttpRequest GetRequest(Uri uri)
        {
            return new SocketHttpRequest(Sockets, uri, NbRetry);
        }

        public void Dispose()
        {
            Sockets.Dispose();
        }
    }

    [DebuggerDisplay(@"\{SocketHttpRequest Method={Method}, Url={Url.AbsoluteUri}\}")]
    public class SocketHttpRequest : IHttpRequest
    {
        private readonly SocketFactory _socketFactory;
        private readonly int _nbRetry;

        public SocketHttpRequest(SocketFactory socketFactory, Uri url, int nbRetry)
        {
            Method = HttpMethod.GET;
            Url = url;
            _socketFactory = socketFactory;
            _nbRetry = nbRetry;
            Headers = new Dictionary<string, string>
            {
                {"Host", url.Host},
                {"User-Agent", "WebsiteCrawler/1.0"},
                {"Accept-Encoding", "gzip"},
            };
        }

        public HttpMethod Method { get; set; }
        public Uri Url { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; set; }

        public IHttpResponse GetResponse()
        {
            SocketHttpResponse response = new SocketHttpResponse(Url);
            int retry = _nbRetry;
            do
            {
                try
                {
                    ProcessUri(Url, response);
                    return response;
                }
                catch (Exception e)
                {
                    retry--;
                    if (retry == 0)
                    {
                        response.StatusCode = 0;
                        response.Headers["ErrorMessage"] = e.Message;
                        response.Headers["StackTrace"] = e.StackTrace;
                    }
                }
            } while (retry > 0);

            return response;
        }

        public IHttpResponse GetResponse(SocketProxy socket)
        {
            SocketHttpResponse response = new SocketHttpResponse(Url);
            ProcessUri(Url, response, socket.Stream);

            return response;
        }

        private void ProcessUri(Uri uri, SocketHttpResponse response)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (SocketReference socketReference = _socketFactory.GetSocket(uri))
            {
                watch.Stop();
                response.Timing.OpeningConnection = watch.Elapsed;
                ProcessUri(uri, response, socketReference.Socket.Stream);
                if (response.Headers.TryGetValue("Connection") == "close")
                    socketReference.Socket.Socket.Close();
            }
        }

        private void ProcessUri(Uri uri, SocketHttpResponse response, Stream stream)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            SocketWriter writer = new SocketWriter(stream);

            writer.WriteLine(Method + " " + uri.PathAndQuery + " HTTP/1.1");
            foreach (KeyValuePair<string, string> requestHeader in Headers)
                writer.WriteLine(requestHeader.Key + ": " + requestHeader.Value);

            writer.WriteLine("");
            if (Body != null)
                writer.WriteLine(Body);
            stream.Flush();
            //sb.AppendLine("Connection: keep-alive");

            response.Timing.RequestSent = watch.Elapsed;
            watch.Restart();

            string status = writer.ReadLine();
            response.StatusCode = (HttpStatusCode)Int32.Parse(status.Split(' ')[1]);

            response.Timing.Waiting = watch.Elapsed;
            watch.Restart();

            string responseHeader;
            while (!String.IsNullOrEmpty(responseHeader = writer.ReadLine()))
            {
                string[] parts = responseHeader.Split(new char[] { ':' }, 2);
                if (parts.Length > 0)
                    response.Headers[parts[0].Trim()] = parts[1].Trim();
            }

            string strContentLength = response.Headers.TryGetValue("Content-Length");
            string transferEncoding = response.Headers.TryGetValue("Transfer-Encoding");
            if (transferEncoding == "chunked")
            {
                response.CompressedContent = writer.ReadByChunks();
            }
            else if (response.StatusCode == HttpStatusCode.NotModified)
            {
                response.CompressedContent = writer.Read(0);
            }
            else if (String.IsNullOrEmpty(strContentLength))
            {
                response.CompressedContent = writer.ReadToEnd();
            }
            else
            {
                response.CompressedContent = writer.Read(Int32.Parse(strContentLength));
            }

            switch (response.Headers.TryGetValue("Content-Encoding"))
            {
                case "gzip":
                    response.Content = response.CompressedContent.DecodeGZip();
                    break;

                default:
                    response.Content = response.CompressedContent;
                    break;
            }

            response.Timing.Download = watch.Elapsed;
        }
    }

    [DebuggerDisplay(@"\{ResponseTiming Waiting={Waiting}, Download={Download}, Total={Total}\}")]
    public class ResponseTiming
    {
        public TimeSpan OpeningConnection { get; set; }
        public TimeSpan RequestSent { get; set; }
        public TimeSpan Waiting { get; set; }
        public TimeSpan Download { get; set; }

        public TimeSpan Total
        {
            get { return OpeningConnection + RequestSent + Waiting + Download; }
        }
    }

    [DebuggerDisplay(@"\{SocketHttpRequest StatusCode={StatusCode}, CompressedLength={CompressedContent.Length}, Length={Content.Length}\}")]
    public class SocketHttpResponse : IHttpResponse
    {
        public SocketHttpResponse(Uri requestUri)
        {
            RequestUri = requestUri;
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Timing = new ResponseTiming();
        }

        public ResponseTiming Timing { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Uri RequestUri { get; set; }
        public Dictionary<string, string> Headers { get; private set; }
        public byte[] CompressedContent { get; set; }
        public byte[] Content { get; set; }
        public string ReadContent()
        {
            return TryReadContent(Encoding.UTF8, 0xEF, 0xBB, 0xBF)
                   ?? TryReadContent(Encoding.UTF32, 0x00, 0x00, 0xFE, 0xFF)
                   ?? TryReadContent(Encoding.UTF32, 0xFF, 0xFE, 0x00, 0x00)
                   ?? TryReadContent(Encoding.Unicode, 0xFE, 0xFF)
                   ?? TryReadContent(Encoding.Unicode, 0xFF, 0xFE)
                   ?? TryReadContent(Encoding.UTF7, 0x2B, 0x2F, 0x76, 0x38)
                   ?? TryReadContent(Encoding.UTF7, 0x2B, 0x2F, 0x76, 0x39)
                   ?? TryReadContent(Encoding.UTF7, 0x2B, 0x2F, 0x76, 0x2B)
                   ?? TryReadContent(Encoding.UTF7, 0x2B, 0x2F, 0x76, 0x2F)
                   ?? TryReadContent(Encoding.UTF8);
        }

        /// <summary>
        /// Lit le contenu de la propriété Content en utilisant l'encoding passé en paramètre
        /// uniquement si les premiers bytes correspondent à la signature.
        /// </summary>
        /// <param name="encoding">Encoding à utiliser pour lire la chaine de caractères</param>
        /// <param name="signature">Signature de l'encoding</param>
        /// <returns>Chaîne de caractères décodée ou null si la signature ne correspond pas</returns>
        private string TryReadContent(Encoding encoding, params byte[] signature)
        {
            if (Content.Length >= signature.Length)
            {
                for (int i = 0; i < signature.Length; i++)
                    if (signature[i] != Content[i])
                        return null;

                return encoding.GetString(Content, signature.Length, Content.Length - signature.Length);
            }

            return null;
        }
    }
}