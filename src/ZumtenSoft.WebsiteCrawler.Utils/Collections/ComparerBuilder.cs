using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.Utils.Collections
{
    public static class Comparison<T>
    {
        public static FieldComparer<T, TField> Build<TField>(Func<T, TField> getValue, IComparer<TField> fieldComparer, bool descending = false, IComparer<T> nextComparer = null)
        {
            return new FieldComparer<T, TField>(getValue, fieldComparer, descending, nextComparer);
        }

        public static FieldComparer<T, TField> Build<TField>(Func<T, TField> getValue, bool descending = false, IComparer<T> nextComparer = null)
        {
            return new FieldComparer<T, TField>(getValue, Comparer<TField>.Default, descending, nextComparer);
        }
    }
}
