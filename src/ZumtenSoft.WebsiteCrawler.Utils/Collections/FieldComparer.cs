using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.Utils.Collections
{
    public class FieldComparer<T, TField> : IComparer<T>
    {
        private readonly Func<T, TField> _getValue;
        private readonly bool _descending;
        private readonly IComparer<TField> _fieldComparer;
        private readonly IComparer<T> _nextComparer;

        public FieldComparer(Func<T, TField> getValue, IComparer<TField> fieldComparer, bool descending, IComparer<T> nextComparer)
        {
            _getValue = getValue;
            _fieldComparer = fieldComparer;
            _descending = descending;
            _nextComparer = nextComparer;
        }

        public int Compare(T x, T y)
        {
            int result = _fieldComparer.Compare(_getValue(x), _getValue(y));
            if (result == 0 && _nextComparer != null)
                return _nextComparer.Compare(x, y);
            return _descending ? -result : result;
        }

        public FieldComparer<T, TOtherField> After<TOtherField>(Func<T, TOtherField> getValue, bool descending = false)
        {
            return new FieldComparer<T, TOtherField>(getValue, Comparer<TOtherField>.Default, descending, this);
        }
    }
}
