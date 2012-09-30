using System;
using System.Collections;
using System.Collections.Generic;

namespace Fsm
{
	public class Map<K, T>
		: IEnumerable<KeyValuePair<K, T>>
	{
		private Dictionary<K, HashSet<T>> _map;
		private HashSet<T> _null;
		private HashSet<T> _emptySet;

		public Map()
		{
			_map = new Dictionary<K, HashSet<T>>();
			_null = new HashSet<T>();
			_emptySet = new HashSet<T>();
		}

		public bool Add(K key, T value)
		{
			HashSet<T> set;

			if (key == null)
				set = _null;
			else
			{
				if (_map.TryGetValue(key, out set) == false)
				{
					set = new HashSet<T>();
					_map.Add(key, set);
				}
			}

			return set.Add(value);
		}

		public void AddAll(K key, IEnumerable<T> values)
		{
			foreach (var value in values)
				Add(key, value);
		}

		public void Remove(K key, T value)
		{
			HashSet<T> set = null;

			if (key == null)
				set = _null;
			else
				_map.TryGetValue(key, out set);

			if (set != null)
				set.Remove(value);
		}

		public T GetOne(K key)
		{
			var set = Get(key);

			foreach (var item in set)
				return item;

			return default(T);
		}

		public HashSet<T> Get(K key)
		{
			return this[key];
		}

		public HashSet<T> this[K key]
		{
			get
			{
				if (key == null)
					return _null;

				HashSet<T> set;
				if (_map.TryGetValue(key, out set))
					return set;

				return _emptySet;
			}
		}

		public bool IsEmpty(K key)
		{
			if (key == null)
				return _null.Count == 0;

			HashSet<T> set;
			if (_map.TryGetValue(key, out set))
				return set == null || set.Count == 0;

			return true;
		}

		public HashSet<NK> GetNotNullKeys<NK>(Func<K, NK> convert)
		{
			var keys = new HashSet<NK>();

			foreach (var key in _map.Keys)
				keys.Add(convert(key));

			return keys;
		}

		public void ForEachNotNullKeys(Action<K> action)
		{
			foreach (var key in _map.Keys)
				action(key);
		}

		#region IEnumerator<..>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new MapEnumerator(_map.GetEnumerator(), _null);
		}

		IEnumerator<KeyValuePair<K, T>> IEnumerable<KeyValuePair<K, T>>.GetEnumerator()
		{
			return new MapEnumerator(_map.GetEnumerator(), _null);
		}

		protected class MapEnumerator
			: IEnumerator<KeyValuePair<K, T>>
		{
			private IEnumerator<KeyValuePair<K, HashSet<T>>> _mapenumerator;
			private IEnumerator<T> _mapitemenumerator;
			private IEnumerator<T> _nullenumerator;
			private bool _mapnull;

			public MapEnumerator(IEnumerator<KeyValuePair<K, HashSet<T>>> mapenumerator, HashSet<T> nul1)
			{
				_mapnull = true;
				_mapenumerator = mapenumerator;
				_nullenumerator = nul1.GetEnumerator();
			}

			void IEnumerator.Reset()
			{
				_mapnull = true;
				_mapenumerator.Reset();
				_nullenumerator.Reset();
				_mapitemenumerator = null;
			}

			bool IEnumerator.MoveNext()
			{
				if (_mapnull)
				{
					if (_mapitemenumerator != null && _mapitemenumerator.MoveNext())
						return true;

					while (_mapenumerator.MoveNext())
					{
						if (_mapenumerator.Current.Value.Count > 0)
						{
							_mapitemenumerator = _mapenumerator.Current.Value.GetEnumerator();

							if (_mapitemenumerator.MoveNext())
								return true;
						}
					}

					_mapnull = false;
				}

				return _nullenumerator.MoveNext();
			}

			Object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public KeyValuePair<K, T> Current
			{
				get
				{
					if (_mapnull)
						return new KeyValuePair<K, T>(_mapenumerator.Current.Key, _mapitemenumerator.Current);
					return new KeyValuePair<K, T>(default(K), _nullenumerator.Current);
				}
			}

			void IDisposable.Dispose()
			{
				_mapenumerator.Dispose();
				_nullenumerator.Dispose();
			}
		}

		#endregion
	}
}
