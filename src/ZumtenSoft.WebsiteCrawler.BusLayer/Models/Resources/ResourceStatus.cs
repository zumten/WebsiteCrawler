using System.ComponentModel;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    /// <summary>
    /// Status d'une ressource.
    /// </summary>
    public enum ResourceStatus
    {
        /// <summary>
        /// La ressource est en attente d'être traitée.
        /// </summary>
        [Description("Ready to process")]
        ReadyToProcess,

        /// <summary>
        /// La ressource est en train d'être traitée
        /// </summary>
        Processing,

        /// <summary>
        /// La ressource est terminée d'être processée.
        /// </summary>
        Processed,

        /// <summary>
        /// La ressource est en erreur. Il n'y a présentement pas moyen de connaître l'erreur exacte.
        /// </summary>
        Error,

        /// <summary>
        /// Le ressource a pris trop de temps à traiter.
        /// </summary>
        Timeout,

        /// <summary>
        /// La ressource est ignorée soit à cause d'un filtre par URL ou parce que le nombre
        /// de requêtes à traiter est limitée.
        /// </summary>
        Ignored,
}
}