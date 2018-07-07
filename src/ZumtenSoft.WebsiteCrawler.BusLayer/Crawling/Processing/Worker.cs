using System.Threading;
using ZumtenSoft.WebsiteCrawler.Utils.Events;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing
{
    public class Worker<TItem, TContext> : NotifyObject where TItem : class
    {
        public string Name { get; private set; }
        public WorkBucket<TItem, TContext> Bucket { get; private set; }

        private bool _isWorking;
        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    Notify("IsWorking");
                }
            }
        }

        private WorkerStatus _status;
        public WorkerStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    Notify("Status");
                }
            }
        }

        private TItem _currentItem;
        public TItem CurrentItem
        {
            get { return _currentItem; }
            set { if (_currentItem != value) { _currentItem = value; Notify("CurrentItem"); } }
        }

        private readonly Semaphore _isRunning = new Semaphore(1, 1);

        public Worker(string name, WorkBucket<TItem, TContext> bucket)
        {
            Name = name;
            Bucket = bucket;
        }

        public bool TryStart()
        {
            if (_isRunning.WaitOne(0))
            {
                bool handlesRelease = false;
                try
                {
                    Status = WorkerStatus.Processing;
                    IsWorking = true;
                    ThreadPool.QueueUserWorkItem(Run);
                    handlesRelease = true;
                }
                finally
                {
                    if (!handlesRelease)
                        _isRunning.Release();
                }
            }

            return false;
        }

        private void Run(object obj)
        {
            do
            {
                Status = WorkerStatus.Processing;
                IsWorking = true;

                try
                {
                    TItem item;
                    while (Bucket.Items.TryDequeue(out item))
                    {
                        CurrentItem = item;
                        Bucket.Action(item, Bucket.Context);
                    }
                }
                finally
                {
                    Status = WorkerStatus.Waiting;
                    CurrentItem = null;
                    IsWorking = false;
                    _isRunning.Release();
                }
            } while (Bucket.Items.Count > 0 && _isRunning.WaitOne());
        }
    }
}
