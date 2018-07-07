using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ZumtenSoft.WebsiteCrawler.Utils
{
    public static class DependencyObjectHelper
    {
        /// <summary>
        /// Récupère l'ancètre immédiat d'un objet ou null s'il est non-trouvable.
        /// </summary>
        public static DependencyObject Ancestor(this DependencyObject current)
        {
            if (current != null)
            {
                return VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        /// <summary>
        /// Récupère le premier ancètre de type T ou null s'il est non-trouvable.
        /// </summary>
        public static T Ancestor<T>(this DependencyObject current) where T : DependencyObject
        {
            if (current != null)
            {
                while ((current = VisualTreeHelper.GetParent(current)) != null)
                {
                    T castedCurrent = current as T;
                    if (castedCurrent != null)
                        return castedCurrent;
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère la liste de tous les ancètres.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject current)
        {
            if (current != null)
            {
                while ((current = VisualTreeHelper.GetParent(current)) != null)
                    yield return current;
            }
        }

        /// <summary>
        /// Récupère la liste de tous les ancètres de type T.
        /// </summary>
        public static IEnumerable<T> Ancestors<T>(this DependencyObject current) where T : DependencyObject
        {
            if (current != null)
            {
                while ((current = VisualTreeHelper.GetParent(current)) != null)
                {
                    T castedCurrent = current as T;
                    if (castedCurrent != null)
                        yield return castedCurrent;
                }
            }
        }
    }
}