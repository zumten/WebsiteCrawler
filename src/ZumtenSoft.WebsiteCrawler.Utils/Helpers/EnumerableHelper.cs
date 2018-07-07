using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZumtenSoft.WebsiteCrawler.Utils.Helpers
{
	public static class EnumerableHelper
	{
		public static T Pop<T>(this IEnumerator<T> list)
		{
			return list.MoveNext() ? list.Current : default(T);
		}

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> list)
        {
            return list ?? Enumerable.Empty<T>();
        }

		public static void Each<T>(this IEnumerable<T> list, Action<T> handler)
		{
			foreach (T i in list)
				handler(i);
		}

		public static IEnumerable<Pair<T1, T2>> Pair<T1, T2>(this IEnumerable<T1> list1, IEnumerable<T2> list2)
		{
			return list1.Select(list2, (x, y) => new Pair<T1, T2>(x, y));
		}

		/// <summary>
		/// Pairs each item of the first enumerable with each item of the second one
		/// then calls the predicate for each of the results.
		/// </summary>
		public static IEnumerable<TResult> Select<T1, T2, TResult>(this IEnumerable<T1> list1, IEnumerable<T2> list2, Func<T1, T2, TResult> predicate)
		{
			var enum2 = list2.GetEnumerator();
			foreach (var item1 in list1)
			{
				if (!enum2.MoveNext())
					throw new Exception("First enumerable is larger");

				yield return predicate(item1, enum2.Current);
			}

			if (enum2.MoveNext())
				throw new Exception("Second enumerable is larger");

		}

		/// <summary>
		/// Pair the source's items 2 by 2, then calls the predicates
		/// (first with second, second with third, third with fourth, etc.)
		/// </summary>
		public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> predicate)
		{
			var e = source.GetEnumerator();
			if (e.MoveNext())
			{
				var previous = e.Current;
				while (e.MoveNext())
					yield return predicate(previous, previous = e.Current);
			}
		}

		public static bool SequenceEqual<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> predicate)
		{
			var enum2 = second.GetEnumerator();
			foreach (var item1 in first)
			{
				if (!enum2.MoveNext())
					return false;

				if (!predicate(item1, enum2.Current))
					return false;
			}

			return !enum2.MoveNext();
		}

		public static IEnumerable<Pair<T>> PairWise<T>(this IEnumerable<T> source)
		{
			var e = source.GetEnumerator();
			if (e.MoveNext())
			{
				var previous = e.Current;
				while (e.MoveNext())
					yield return new Pair<T>(previous, previous = e.Current);
			}
		}

		public static IEnumerable<T> SelectAll<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predicate)
		{
			var stack = new Stack<IEnumerator<T>>();
			stack.Push(list.GetEnumerator());

			while (stack.Count > 0)
			{
				var currEnum = stack.Peek();
				while (currEnum.MoveNext())
				{
					yield return currEnum.Current;
					stack.Push(currEnum = predicate(currEnum.Current).GetEnumerator());
				}

				stack.Pop();
			}
		}

		public static IEnumerable<TFilter> Where<TFilter>(this IEnumerable source)
		{
			foreach (var item in source)
				if (item is TFilter)
					yield return (TFilter)item;
		}
	}

	public class Pair<T>
	{
		public T Value1 { get; set; }
		public T Value2 { get; set; }

		public Pair(T value1, T value2)
		{
			Value1 = value1;
			Value2 = value2;
		}
	}

	public class Pair<T1, T2>
	{
		public T1 Value1 { get; set; }
		public T2 Value2 { get; set; }

		public Pair(T1 value1, T2 value2)
		{
			Value1 = value1;
			Value2 = value2;
		}
	}
}