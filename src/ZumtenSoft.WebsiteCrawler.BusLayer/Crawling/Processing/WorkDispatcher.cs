using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing
{
    public class WorkDispatcher<TItem, TContext> : NotifyObject where TItem : class
    {
        public Action<TItem, TContext> Action { get; private set; }
        public ObservableCollection<WorkBucket<TItem, TContext>> Buckets { get; private set; }

        public IEnumerable<Worker<TItem, TContext>> Workers
        {
            get { return Buckets.SelectMany(x => x.Workers); }
        }

        public WorkDispatcher(Action<TItem, TContext> action)
        {
            Action = action;
            Buckets = new ObservableCollection<WorkBucket<TItem, TContext>>();
        }

        public WorkBucket<TItem, TContext> AddBucket(string name, int nbWorker, TContext context)
        {
            WorkBucket<TItem, TContext> bucket = new WorkBucket<TItem, TContext>(context, name, Action, nbWorker);
            Buckets.Add(bucket);
            foreach (Worker<TItem, TContext> worker in bucket.Workers)
                worker.PropertyChanged += WorkerOnPropertyChanged;

            return bucket;
        }

        private readonly object _lock = new object();
        private int _nbWorking = 0;

        private void WorkerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                if (e.PropertyName == "IsWorking")
                {
                    Worker<TItem, TContext> worker = (Worker<TItem, TContext>)sender;
                    _nbWorking += worker.IsWorking ? 1 : -1;
                    IsWorking = _nbWorking > 0;
                }
            }
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get { return _isWorking; }
            private set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    Notify("IsWorking");
                }
            }
        }
    }
}