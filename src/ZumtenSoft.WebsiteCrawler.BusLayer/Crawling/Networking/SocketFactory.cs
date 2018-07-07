using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public class SocketFactory : IDisposable
    {
        public HostProvider Hosts { get; private set; }
        private readonly ConcurrentDictionary<string, ConcurrentBag<SocketProxy>> _socketsByHost = new ConcurrentDictionary<string, ConcurrentBag<SocketProxy>>();

        public SocketFactory(HostProvider hosts)
        {
            Hosts = hosts;
        }

        public SocketReference GetSocket(Uri uri)
        {
            bool useSsl = uri.Scheme == "https";

            IPAddress ipAddress = Hosts[uri.Host];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, uri.Port);
            int port = uri.Port;
            string key = ipAddress + ":" + port + (useSsl ? ":" + uri.Host : "");

            ConcurrentBag<SocketProxy> socketCollection = _socketsByHost.GetOrAdd(key, x => new ConcurrentBag<SocketProxy>());

            SocketProxy socketProxy;

            do
            {
                socketCollection.TryTake(out socketProxy);
                if (socketProxy != null && !socketProxy.IsValid)
                    socketProxy.Dispose();
            } while (socketProxy != null && !socketProxy.IsValid);

            if (socketProxy == null || !socketProxy.IsValid)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 120000);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 120000);
                socket.Connect(endPoint);

                socketProxy = new SocketProxy(socket, useSsl, uri.Host);
            }

            socketProxy.Expire = DateTime.Now.Add(TimeSpan.FromMinutes(2));
            return new SocketReference(socketProxy, socketCollection);
        }

        public SocketReference GetSocket(IPEndPoint endPoint, string domain = "")
        {
            string key = endPoint.Address + ":" + endPoint.Port + (endPoint.Port == 443 ? ":" + domain : "");

            ConcurrentBag<SocketProxy> socketCollection = _socketsByHost.GetOrAdd(key, x => new ConcurrentBag<SocketProxy>());

            SocketProxy socketProxy;
            do
            {
                socketCollection.TryTake(out socketProxy);
                if (socketProxy != null && !socketProxy.Socket.Connected)
                    socketProxy.Dispose();
            } while (socketProxy != null && !socketProxy.Socket.Connected);

            if (socketProxy == null)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 120000);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 120000);
                socket.Connect(endPoint);

                socketProxy = new SocketProxy(socket, endPoint.Port == 443, domain);
            }

            return new SocketReference(socketProxy, socketCollection);
        }

        public void Dispose()
        {
            foreach (ConcurrentBag<SocketProxy> socketCollection in _socketsByHost.Values)
                foreach (SocketProxy socket in socketCollection)
                    socket.Dispose();
        }
    }
}