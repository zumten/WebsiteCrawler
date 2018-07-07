using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.Utils.Collections
{
    public class ExtendedList<T> : List<T>
    {
        public ExtendedList()
        {

        }

        public ExtendedList(IEnumerable<T> collection)
            : base(collection)
        {

        }

        private static Func<TClass, TField> FieldGetter<TClass, TField>(string name)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "arg");
            MemberExpression member = Expression.Field(param, name);
            LambdaExpression lambda = Expression.Lambda(typeof(Func<TClass, TField>), member, param);
            Func<TClass, TField> compiled = (Func<TClass, TField>)lambda.Compile();
            return compiled;
        }

        private static readonly Func<List<T>, T[]> ItemsGetter = FieldGetter<List<T>, T[]>("_items");

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                if (oldIndex == newIndex + 1 || oldIndex + 1 == newIndex)
                {
                    T item = this[oldIndex];
                    this[oldIndex] = this[newIndex];
                    this[newIndex] = item;
                }
                else if (newIndex > oldIndex)
                {
                    T[] items = ItemsGetter(this);
                    T item = items[oldIndex];
                    Array.Copy(items, oldIndex + 1, items, oldIndex, newIndex - oldIndex);
                    items[newIndex] = item;
                }
                else
                {
                    T[] items = ItemsGetter(this);
                    T item = items[oldIndex];
                    Array.Copy(items, newIndex, items, newIndex + 1, oldIndex - newIndex);
                    items[newIndex] = item;
                }
            }
        }
    }
}
