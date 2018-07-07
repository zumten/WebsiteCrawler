using System;
using System.Collections.Concurrent;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking
{
    public class SocketReference : IDisposable
    {
        public SocketProxy Socket { get; private set; }
        private readonly ConcurrentBag<SocketProxy> _graveyard;

        public SocketReference(SocketProxy socket, ConcurrentBag<SocketProxy> graveyard)
        {
            Socket = socket;
            _graveyard = graveyard;
        }

        public void Dispose()
        {
            if (_graveyard != null)
                _graveyard.Add(Socket);
        }
    }
}