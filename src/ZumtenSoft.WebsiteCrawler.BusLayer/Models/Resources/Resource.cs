using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    [DebuggerDisplay(@"\{Resource Url={Url.AbsoluteUri}, HttpStatus={HttpStatus}, References={References.Count}\}")]
    public class Resource : BaseModel
    {
        public Resource(Uri url, ResourceBehavior behavior)
        {
            Url = url;
            Behavior = behavior;
            References = new ResourceReferenceCollection(this);
            ReferencedBy = new List<ResourceReference>();
            Headers = new ResourceHeaders();
            Content = new List<ResourceContent>();
            Status = ResourceStatus.ReadyToProcess;
            Errors = new List<ResourceError>();
        }

        /// <summary>
        /// Quel comportement adopter lors du chargement de la ressource.
        /// </summary>
        public ResourceBehavior Behavior { get; set; }

        /// <summary>
        /// Temps nécessaire pour télécharger la ressource.
        /// </summary>
        public TimeSpan TimeLoading { get; set; }
        public DateTime TimeStart { get; set; }

        /// <summary>
        /// Temps nécessaire pour que le crawler traite la ressource.
        /// </summary>
        private TimeSpan _timeProcessing;
        public TimeSpan TimeProcessing
        {
            get { return _timeProcessing; }
            set { if (_timeProcessing != value) { _timeProcessing = value; Notify("TimeProcessing"); } }
        }

        private ResourceStatus _status;
        public ResourceStatus Status
        {
            get { return _status; }
            set { if (_status != value) { _status = value; Notify("Status"); } }
        }

        private HttpStatusCode _httpStatus;
        public HttpStatusCode HttpStatus
        {
            get { return _httpStatus; }
            set { if (_httpStatus != value) { _httpStatus = value; Notify("HttpStatus"); } }
        }

        private string _currentBucket;
        public string CurrentBucket
        {
            get { return _currentBucket; }
            set { if (_currentBucket != value) { _currentBucket = value; Notify("CurrentBucket"); } }
        }

        public ResourceContentType ContentType { get; set; }
        public int Size { get; set; }
        public int CompressedSize { get; set; }
        public int? ViewStateSize { get; set; }
        public Uri Url { get; private set; }
        public ResourceHeaders Headers { get; private set; }
        public List<ResourceContent> Content { get; private set; }

        public ResourceReferenceCollection References { get; private set; }
        public List<ResourceReference> ReferencedBy { get; private set; }
        public List<ResourceError> Errors { get; private set; }

        public override string ToString()
        {
            return Url.AbsoluteUri;
        }
    }

}
