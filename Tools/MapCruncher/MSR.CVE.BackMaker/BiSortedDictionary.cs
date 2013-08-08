using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	internal class BiSortedDictionary<TKey, TValue>
	{
		private class BackwardsComparer : IComparer<TKey>
		{
			private IComparer<TKey> comparer;
			public BackwardsComparer(IComparer<TKey> comparer)
			{
				this.comparer = comparer;
			}
			public int Compare(TKey x, TKey y)
			{
				return this.comparer.Compare(y, x);
			}
		}
		private SortedDictionary<TKey, TValue> forwardDict;
		private SortedDictionary<TKey, TValue> backwardDict;
		public int Count
		{
			get
			{
				return this.forwardDict.Count;
			}
		}
		public TKey FirstKey
		{
			get
			{
				SortedDictionary<TKey, TValue>.Enumerator enumerator = this.forwardDict.GetEnumerator();
				if (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> current = enumerator.Current;
					return current.Key;
				}
				return default(TKey);
			}
		}
		public TKey LastKey
		{
			get
			{
				SortedDictionary<TKey, TValue>.Enumerator enumerator = this.backwardDict.GetEnumerator();
				if (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> current = enumerator.Current;
					return current.Key;
				}
				return default(TKey);
			}
		}
		public SortedDictionary<TKey, TValue>.KeyCollection Keys
		{
			get
			{
				return this.forwardDict.Keys;
			}
		}
		public BiSortedDictionary()
		{
			this.forwardDict = new SortedDictionary<TKey, TValue>();
			this.backwardDict = new SortedDictionary<TKey, TValue>(new BiSortedDictionary<TKey, TValue>.BackwardsComparer(this.forwardDict.Comparer));
		}
		public void Add(TKey key, TValue value)
		{
			Monitor.Enter(this);
			try
			{
				this.forwardDict.Add(key, value);
				this.backwardDict.Add(key, value);
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void Remove(TKey key)
		{
			Monitor.Enter(this);
			try
			{
				this.forwardDict.Remove(key);
				this.backwardDict.Remove(key);
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public SortedDictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
		{
			return this.forwardDict.Keys.GetEnumerator();
		}
	}
}
