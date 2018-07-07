using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction
{
    /// <summary>
    /// Crawler permet de charger toutes les ressources d'un site afin de connaître
    /// le comportement de chacune d'elle et ainsi découvrir les liens brisés et les pages en erreur.
    /// </summary>
    public class Crawler : IDisposable
    {
        private readonly object _lock = new object();
        private int? _limitResources = null;

        public WorkDispatcher<Resource, BucketContext> WorkDispatcher { get; private set; }
        private BehaviorRuleCollection _behaviorRules = new BehaviorRuleCollection();
        public Dictionary<string, Resource> Resources { get; private set; }
        public CrawlingContext Context { get; private set; }

        public Crawler(CrawlingContext context, int? limitResources = null)
        {
            Context = context;
            Resources = Context.Resources.ToDictionary(x => x.Url.AbsoluteUri);
            _limitResources = limitResources;
            WorkDispatcher = new WorkDispatcher<Resource, BucketContext>(ProcessResource);
            WorkDispatcher.PropertyChanged += TrackerOnPropertyChanged;
        }

        private void TrackerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsWorking")
            {
                if (!WorkDispatcher.IsWorking && OnCompleted != null)
                    OnCompleted(this, new EventArgs());
            }
        }

        /// <summary>
        /// Ajouter une URL de base à traiter (tel qu'un sitemap ou une page principale)
        /// </summary>
        /// <param name="url">URL à traiter</param>
        public void AddUrlToProcess(Uri url)
        {
            GetResource(url, ResourceContentType.Unknown);
        }

        public WorkBucket<Resource, BucketContext> AddBucket(string name, int nbThreads, BucketContext context)
        {
            return WorkDispatcher.AddBucket(name, nbThreads, context);
        }

        public void AddBehaviorRule(string name, ResourceBehavior behavior, WorkBucket<Resource, BucketContext> targetBucket, ICollection<CrawlingCondition> conditions)
        {
            _behaviorRules.AddUrlFilterRule(name, behavior, targetBucket, conditions);
        }

        /// <summary>
        /// Attend que les ressources soit toutes terminées de traiter.
        /// </summary>
        public void Wait()
        {
            AutoResetEvent mre = new AutoResetEvent(false);
            OnCompleted += (sender, args) => mre.Set();
            mre.WaitOne();
        }

        public event EventHandler OnCompleted;
        public event EventHandler OnResourceStart;
        public event EventHandler OnResourceCompleted;

        /// <summary>
        /// Débuter le traitement de toutes les ressources.
        /// </summary>
        public void Reprocess()
        {
            lock (_lock)
            {
                foreach (Resource resource in Resources.Values.Where(x => x.Status == ResourceStatus.Ignored || x.Status == ResourceStatus.ReadyToProcess))
                {
                    IBehaviorRule rule = _behaviorRules.GetBehaviorRule(resource);
                    resource.Behavior = rule == null ? ResourceBehavior.Ignore : rule.Behavior;

                    // En fonction du behavior et de la limitation de ressource, on décide
                    // si une ressource devra être traitée ou non.
                    if (resource.Behavior != ResourceBehavior.Ignore && (!_limitResources.HasValue || _limitResources > 0))
                    {
                        if (_limitResources.HasValue)
                            _limitResources--;
                        resource.Status = ResourceStatus.ReadyToProcess;
                        rule.TargetBucket.QueueItem(resource);
                        resource.CurrentBucket = rule.TargetBucket.Name;
                    }
                    else
                    {
                        resource.Status = ResourceStatus.Ignored;
                        resource.CurrentBucket = null;
                    }
                }
            }
        }

        public void Stop()
        {
            _behaviorRules = new BehaviorRuleCollection();
            foreach (WorkBucket<Resource, BucketContext> bucket in WorkDispatcher.Buckets)
            {
                Resource resource;
                while (bucket.Items.TryDequeue(out resource))
                {
                    resource.Status = ResourceStatus.Ignored;
                    resource.CurrentBucket = null;
                }
            }
        }

        /// <summary>
        /// Traiter une ressource particulière.
        /// </summary>
        /// <param name="resource"></param>
        private void ProcessResource(Resource resource, BucketContext context)
        {
            resource.TimeStart = DateTime.Now;

            if (OnResourceStart != null)
                OnResourceStart(resource, new EventArgs());

            Stopwatch watch = new Stopwatch();
            watch.Start();

            SocketHttpResponse response = (SocketHttpResponse)SendRequest(resource, context);

            watch.Stop();
            resource.TimeLoading = watch.Elapsed;
            resource.Size = response.Content == null ? 0 : response.Content.Length;
            resource.CompressedSize = response.CompressedContent == null ? resource.Size : response.CompressedContent.Length;
            watch.Reset();

            if (response != null)
            {
                watch.Start();

                ExtractResponseData(resource, response);

                watch.Stop();
                resource.TimeProcessing = watch.Elapsed;
            }

            resource.CurrentBucket = null;

            if (OnResourceCompleted != null)
                OnResourceCompleted(resource, new EventArgs());
        }

        private void ExtractResponseData(Resource resource, IHttpResponse response)
        {
            foreach (KeyValuePair<string, string> pair in response.Headers)
                resource.Headers.Add(pair.Key, pair.Value);

            if (String.Equals(resource.Url.AbsolutePath, "/robots.txt", StringComparison.OrdinalIgnoreCase))
            {
                resource.ContentType = ResourceContentType.Robots;
            }
            else if (resource.ContentType == ResourceContentType.Unknown)
            {
                // Mettre à jour le type de contenu en fonction du header Content-Type
                string contentType = resource.Headers.TryGetValue("Content-Type") ?? String.Empty;
                if (contentType.Contains("text/html", StringComparison.InvariantCultureIgnoreCase))
                {
                    resource.ContentType = ResourceContentType.Html;
                }
                else if (contentType.Contains("text/css", StringComparison.InvariantCultureIgnoreCase))
                {
                    resource.ContentType = ResourceContentType.Css;
                }
                else if (contentType.Contains("javascript", StringComparison.InvariantCultureIgnoreCase))
                {
                    resource.ContentType = ResourceContentType.JavaScript;
                }
                else if (contentType.Contains("image", StringComparison.InvariantCultureIgnoreCase))
                {
                    resource.ContentType = ResourceContentType.Image;
                }
                else if (contentType.Contains("application", StringComparison.InvariantCultureIgnoreCase))
                {
                    resource.ContentType = ResourceContentType.Other;
                }
            }

            switch (resource.HttpStatus)
            {
                case HttpStatusCode.TemporaryRedirect:
                case HttpStatusCode.SeeOther:
                case HttpStatusCode.MovedPermanently:
                case HttpStatusCode.Redirect:
                    if (resource.Behavior >= ResourceBehavior.FollowRedirect)
                    {
                        Uri targetUri = new Uri(resource.Headers["Location"], UriKind.RelativeOrAbsolute);
                        resource.References.Add(ResourceReferenceTypes.Redirection, targetUri, (ReferenceSubType)(int)resource.HttpStatus);
                        resource.ContentType = ResourceContentType.Redirect;
                    }
                    break;
                case HttpStatusCode.OK:
                    UrlExtractors.ExtractLinks(resource, response);
                    break;
            }

            // Trouver les ressources correspondant à toutes les références trouvées
            // dans la ressource courante.
            foreach (ResourceReference link in resource.References)
            {
                Uri absoluteUri = link.Url.ToAbsolute(resource.Url);
                if (absoluteUri != null)
                {
                    Resource targetResource = GetResource(absoluteUri, resource.ContentType == ResourceContentType.Robots ? ResourceContentType.SiteMap : ResourceContentType.Unknown);
                    link.Target = targetResource;
                    if (resource.ContentType == ResourceContentType.Robots)
                        targetResource.ContentType = ResourceContentType.SiteMap;
                }
            }
        }

        /// <summary>
        /// Récupérer le contenu d'une requête (ou null si celle-ci est en erreur)
        /// </summary>
        /// <param name="resource">Ressource à traiter</param>
        /// <returns>Réponse du serveur</returns>
        private IHttpResponse SendRequest(Resource resource, BucketContext context)
        {
            try
            {
                resource.Status = ResourceStatus.Processing;
                SocketHttpRequest request = new SocketHttpRequest(context.Sockets, resource.Url, context.NbRetry);

                var firstReference = resource.ReferencedBy.FirstOrDefault();
                if (firstReference != null)
                    request.Headers["referrer"] = firstReference.Url.ToString();

                IHttpResponse response = request.GetResponse();
                resource.HttpStatus = response.StatusCode;
                resource.Status = ResourceStatus.Processed;
                return response;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                    resource.Status = ResourceStatus.Timeout;
                else
                    resource.Status = ResourceStatus.Error;

                resource.CurrentBucket = null;
                return null;
            }
        }

        /// <summary>
        /// Récupère une ressource à partir d'une URL absolue. Utilise un caching afin qu'une
        /// même ressource ne soit pas traitée deux fois.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Resource GetResource(Uri url, ResourceContentType initialContentType)
        {
            Resource resource;
            Uri urlWithoutSessionId = url.WithoutSession();
            if (!Resources.TryGetValue(urlWithoutSessionId.AbsoluteUri, out resource))
            {
                lock (_lock)
                {
                    if (!Resources.TryGetValue(urlWithoutSessionId.AbsoluteUri, out resource))
                    {
                        resource = new Resource(urlWithoutSessionId, ResourceBehavior.Ignore);
                        resource.ContentType = initialContentType;
                        IBehaviorRule behaviorRule = _behaviorRules.GetBehaviorRule(resource);
                        resource.Behavior = behaviorRule == null ? ResourceBehavior.Ignore : behaviorRule.Behavior;
                        Resources.Add(urlWithoutSessionId.AbsoluteUri, resource);
                        Context.Resources.Add(resource);

                        // En fonction du behavior et de la limitation de ressource, on décide
                        // si une ressource devra être traitée ou non.
                        if (resource.Behavior != ResourceBehavior.Ignore && (!_limitResources.HasValue || _limitResources > 0))
                        {
                            if (_limitResources.HasValue)
                                _limitResources--;

                            behaviorRule.TargetBucket.QueueItem(resource);
                            resource.CurrentBucket = behaviorRule.TargetBucket.Name;
                        }
                        else
                        {
                            resource.Status = ResourceStatus.Ignored;
                            resource.CurrentBucket = null;
                        }
                    }
                }
            }
            return resource;
        }

        public void Dispose()
        {
            foreach (WorkBucket<Resource, BucketContext> bucket in WorkDispatcher.Buckets)
                bucket.Context.Sockets.Dispose();
        }
    }
}