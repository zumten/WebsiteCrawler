using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace ZumtenSoft.WebsiteCrawler.Utils.Events
{
    /// <summary>
    /// Classe de base permettant de créer un objet observable.
    /// Le patron WeakEvent est utilisé pour éviter les memory leaks.
    /// </summary>
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        protected void Notify(string name)
        {
            //if (PropertyChanged != null)
            //    PropertyChanged(this, new PropertyChangedEventArgs(name));
            _propertyChanged.Raise(this, new PropertyChangedEventArgs(name));
        }

        private readonly FastSmartWeakEvent<PropertyChangedEventHandler> _propertyChanged = new FastSmartWeakEvent<PropertyChangedEventHandler>();
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged.Add(value); }
            remove { _propertyChanged.Remove(value); }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ActionDeferrer
    {
        private readonly Action _action;
        private readonly TimeSpan _delayBetweenActions;
        private DateTime _waitUntil = DateTime.MinValue;
        private readonly DispatcherTimer _timer;
        private bool _timerActive = false;

        public ActionDeferrer(Action action, TimeSpan delayBetweenActions, Dispatcher dispatcher)
        {
            _action = action;
            _delayBetweenActions = delayBetweenActions;
            _timer = new DispatcherTimer(delayBetweenActions, DispatcherPriority.Normal, Fire, dispatcher);
        }

        public void Raise()
        {
            if (!_timerActive)
            {
                DateTime now = DateTime.Now;
                if (now >= _waitUntil)
                {
                    _waitUntil = now.Add(_delayBetweenActions);
                    _action();
                }
                else
                {
                    _timerActive = true;
                    _timer.Start();
                }
            }
        }

        private void Fire(object sender, EventArgs e)
        {
            _action();
            _timer.Stop();
            _timerActive = false;
            _waitUntil = DateTime.Now.Add(_delayBetweenActions);
        }
    }
}