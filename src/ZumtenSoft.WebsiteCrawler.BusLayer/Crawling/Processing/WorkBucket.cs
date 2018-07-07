using System;
using System.Collections.Concurrent;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing
{
    public class WorkBucket<TItem, TContext> where TItem : class
    {
        public Worker<TItem, TContext>[] Workers { get; private set; }
        public ConcurrentQueue<TItem> Items { get; private set; }
        public TContext Context { get; private set; }
        public string Name { get; private set; }
        public Action<TItem, TContext> Action { get; private set; }

        public WorkBucket(TContext context, string name, Action<TItem, TContext> action, int nbWorkers)
        {
            Context = context;
            Name = name;
            Action = action;
            Items = new ConcurrentQueue<TItem>();
            Workers = new Worker<TItem, TContext>[nbWorkers];
            for (int i = 0; i < nbWorkers; i++)
            {
                string workerName = nbWorkers > 1 ? name + " (" + (i + 1) + ")" : name;
                Workers[i] = new Worker<TItem, TContext>(workerName, this);
            }
        }

        public void QueueItem(TItem item)
        {
            Items.Enqueue(item);
            foreach (Worker<TItem, TContext> worker in Workers)
                if (worker.TryStart())
                    break;
        }
    }
}