using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public class SocketProxy : IDisposable
    {
        public Socket Socket { get; private set; }
        public bool UseSsl { get; private set; }
        public Stream Stream { get; private set; }
        public DateTime Expire { get; set; }
        public bool IsValid
        {
            get { return Socket.Connected && Expire > DateTime.Now; }
        }

        public SocketProxy(Socket socket, bool useSsl, string domainToAuthenticate)
        {
            Socket = socket;
            UseSsl = useSsl;

            if (useSsl)
            {
                SslStream sslStream =new SslStream(new NetworkStream(socket, true), false, OnUserCertificateValidationCallback);
                sslStream.AuthenticateAsClient(domainToAuthenticate);
                Stream =  new BufferedStream(sslStream, 10240);
            }
            else
            {
                Stream = new BufferedStream(new NetworkStream(socket, true), 10240);
            }
        }

        private bool OnUserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public void Dispose()
        {
            try
            {
                Socket.Dispose();
                Stream.Dispose();
            }
            catch { }
        }
    }
}
