using System;
using System.Collections.Generic;

namespace Fsm
{
	public interface IIndexed
	{
		Int32 Id { get; }
		void SetId(int id);
	}

	public class Indexer<T>
		where T : IIndexed
	{
		private static Int32 idCount;
		private static List<T> states;

		static Indexer()
		{
			idCount = 0;

			states = new List<T>(4000000);
			states.Add(default(T));
		}

		public static void Add(T item)
		{
			lock (states)
			{
				item.SetId(++idCount);
				states.Add(item);

#if DEBUG
				if (states[states.Count - 1].Id != states.Count - 1)
					throw new InvalidOperationException();
#endif
			}
		}

		public static int MaxId
		{
			get { return idCount; }
		}

		public static T Get(int id)
		{
			return states[id];
		}

		public class Array
		{
			private Int32[] ids;

			public Array(int length)
			{
				ids = new Int32[length];
			}

			public Array(Int32[] ids1)
			{
				ids = ids1;
			}

			public T this[int i]
			{
				get
				{
					if (ids[i] < 0)
						return default(T);
					return Indexer<T>.states[ids[i]];
				}
				set
				{
					ids[i] = value.Id;
				}
			}

			public IEnumerable<T> Items
			{
				get
				{
					int length = ids.Length;
					for (int i = 0; i < length; i++)
						yield return Indexer<T>.states[ids[i]];
				}
			}

			public Int32[] Ids
			{
				get { return ids; }
			}

			public int Length
			{
				get { return ids.Length; }
			}
		}
	}
}
