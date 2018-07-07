using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.WebsiteCrawler.Models
{
    [DebuggerDisplay(@"\{ResourceViewModel URL={URL}, Status={Status}\}")]
    public class ResourceViewModel : NotifyObject, IDisposable
    {
        private DispatcherQueue _dispatcherQueue;
        public Resource Model { get; private set; }

        public ResourceViewModel(Resource model, DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
            Model = model;
            Model.PropertyChanged += ModelOnPropertyChanged;

            URL = model.Url.AbsoluteUri;
            Host = model.Url.Host;
            _status = model.Status;
            _httpStatus = model.HttpStatus;
            _duration = model.TimeLoading;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch(args.PropertyName)
            {
                case "Status":
                    ResourceStatus newStatus = Model.Status;
                    _dispatcherQueue.Push(() => { Status = newStatus; });
                    break;

                case "HttpStatus":
                    HttpStatusCode newHttpStatus = Model.HttpStatus;
                    _dispatcherQueue.Push(() => { HttpStatus = newHttpStatus; });
                    break;

                case "TimeProcessing":
                    TimeSpan newTimeProcessing = Model.TimeProcessing;
                    _dispatcherQueue.Push(() => { Duration = newTimeProcessing; });
                    break;

                case "CurrentBucket":
                    string newCurrentbucket = Model.CurrentBucket;
                    _dispatcherQueue.Push(() => { CurrentBucket = newCurrentbucket; });
                    break;
            }
        }

        public string URL { get; private set; }
        public string Host { get; private set; }

        private ResourceStatus _status;
        public ResourceStatus Status
        {
            get { return _status; }
            private set { if (_status != value) { _status = value; Notify("Status"); } }
        }

        private HttpStatusCode _httpStatus;
        public HttpStatusCode HttpStatus
        {
            get { return _httpStatus; }
            private set { if (_httpStatus != value) { _httpStatus = value; Notify("HttpStatus"); } }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set { if (_duration != value) { _duration = value; Notify("Duration"); } }
        }

        private string _currentBucket;
        public string CurrentBucket
        {
            get { return _currentBucket; }
            set { if (_currentBucket != value) { _currentBucket = value; Notify("CurrentBucket"); } }
        }

        public void Dispose()
        {
            Model.PropertyChanged -= ModelOnPropertyChanged;

            _dispatcherQueue.Push(() =>
            {
                _dispatcherQueue = null;
                Model = null;
            });
        }
    }
}
