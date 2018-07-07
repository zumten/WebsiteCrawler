using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    /// <summary>
    /// Référence d'une resource vers une autre. (tel qu'un lien hypertexte, un import
    /// de css, un script javascript, un lien d'un sitemap, une redirection).
    /// </summary>
    [DebuggerDisplay(@"\{ResourceReference Type={Type}, Url={Url.OriginalString}\}")]
    public class ResourceReference
    {
        public ResourceReference(Resource source, ResourceReferenceTypes type, ReferenceSubType subType, Uri url, int count = 1)
        {
            Source = source;
            Type = type;
            SubType = subType;
            Url = url;
            Count = count;
        }

        private Resource _target;
        public Resource Target
        {
            get { return _target; }
            set
            {
                // Met automatiquement à jour la liste des liens référencant une resource.
                if (_target != null)
                    lock (((ICollection) _target.ReferencedBy).SyncRoot)
                        _target.ReferencedBy.Remove(this);
                _target = value;
                if (_target != null)
                    lock (((ICollection)_target.ReferencedBy).SyncRoot)
                        _target.ReferencedBy.Add(this);
            }
        }

        public ResourceReferenceTypes Type { get; private set; }
        public ReferenceSubType SubType { get; private set; }

        /// <summary>
        /// URL Originale (tel que trouvée dans le document source)
        /// </summary>
        public Uri Url { get; private set; }
        public Resource Source { get; set; }

        /// <summary>
        /// Nombre d'occurences de cette référence.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Collection de références de ressources. Cette classe contient certaines méthodes utilisataires pour créer
    /// des références et gère automatiquement l'assignation de la source.
    /// </summary>
    public class ResourceReferenceCollection : List<ResourceReference>
    {
        public Resource Source { get; private set; }

        public ResourceReferenceCollection(Resource source)
        {
            Source = source;
        }

        public ResourceReference Add(ResourceReferenceTypes type, Uri url, ReferenceSubType subType, int count = 1)
        {
            foreach (ResourceReference resourceLink in this)
            {
                if (resourceLink.Type == type && url.OriginalString == resourceLink.Url.OriginalString)
                {
                    resourceLink.Count += count;
                    return resourceLink;
                }
            }

            ResourceReference reference = new ResourceReference(Source, type, subType, url, count);
            Add(reference);
            return reference;
        }
    }
}