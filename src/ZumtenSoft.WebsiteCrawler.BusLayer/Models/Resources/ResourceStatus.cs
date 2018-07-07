using System.ComponentModel;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    /// <summary>
    /// Status d'une ressource.
    /// </summary>
    public enum ResourceStatus
    {
        /// <summary>
        /// La ressource est en attente d'�tre trait�e.
        /// </summary>
        [Description("Ready to process")]
        ReadyToProcess,

        /// <summary>
        /// La ressource est en train d'�tre trait�e
        /// </summary>
        Processing,

        /// <summary>
        /// La ressource est termin�e d'�tre process�e.
        /// </summary>
        Processed,

        /// <summary>
        /// La ressource est en erreur. Il n'y a pr�sentement pas moyen de conna�tre l'erreur exacte.
        /// </summary>
        Error,

        /// <summary>
        /// Le ressource a pris trop de temps � traiter.
        /// </summary>
        Timeout,

        /// <summary>
        /// La ressource est ignor�e soit � cause d'un filtre par URL ou parce que le nombre
        /// de requ�tes � traiter est limit�e.
        /// </summary>
        Ignored,
}
}