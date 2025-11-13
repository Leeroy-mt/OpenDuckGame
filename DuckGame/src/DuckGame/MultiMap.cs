using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// A map of key -&gt; collection&lt;element&gt;
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TElement">The type of the element.</typeparam>
/// <typeparam name="TList">The type collection to use as backing storage.</typeparam>
public class MultiMap<TKey, TElement, TList> : IEnumerable<KeyValuePair<TKey, ICollection<TElement>>>, IEnumerable where TList : ICollection<TElement>, new()
{
	private Dictionary<TKey, TList> _map = new Dictionary<TKey, TList>();

	public int Count => _map.Count;

	public int CountValues
	{
		get
		{
			int v = 0;
			foreach (KeyValuePair<TKey, TList> item in _map)
			{
				v += item.Value.Count;
			}
			return v;
		}
	}

	public TList this[TKey key] => _map[key];

	public IEnumerable<TKey> Keys => _map.Keys;

	public IEnumerable<TList> Values => _map.Values;

	public void Add(TKey key, TElement element)
	{
		if (!_map.TryGetValue(key, out var list))
		{
			_map.Add(key, list = new TList());
		}
		list.Add(element);
	}

	public void Insert(TKey key, int index, TElement value)
	{
		if (!_map.TryGetValue(key, out var list))
		{
			_map.Add(key, list = new TList());
		}
		(list as IList<TElement>).Insert(index, value);
	}

	public void AddRange(TKey key, ICollection<TElement> value)
	{
		if (!_map.TryGetValue(key, out var list))
		{
			try
			{
				_map.Add(key, (TList)Activator.CreateInstance(typeof(TList), value));
				return;
			}
			catch
			{
				_map.Add(key, list = new TList());
				foreach (TElement v in value)
				{
					list.Add(v);
				}
				return;
			}
		}
		foreach (TElement v2 in value)
		{
			list.Add(v2);
		}
	}

	public bool Remove(TKey key, TElement element)
	{
		if (!_map.TryGetValue(key, out var list))
		{
			return false;
		}
		bool result = list.Remove(element);
		if (list.Count == 0)
		{
			_map.Remove(key);
		}
		return result;
	}

	public void Remove(TKey key)
	{
		_map.Remove(key);
	}

	public bool Contains(TKey key, TElement value)
	{
		if (_map.TryGetValue(key, out var list))
		{
			return list.Contains(value);
		}
		return false;
	}

	public bool ContainsKey(TKey key)
	{
		return _map.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TList list)
	{
		return _map.TryGetValue(key, out list);
	}

	public void Clear()
	{
		_map.Clear();
	}

	public IEnumerator<KeyValuePair<TKey, TList>> GetEnumerator()
	{
		return _map.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<KeyValuePair<TKey, ICollection<TElement>>> IEnumerable<KeyValuePair<TKey, ICollection<TElement>>>.GetEnumerator()
	{
		foreach (KeyValuePair<TKey, TList> kvp in _map)
		{
			yield return new KeyValuePair<TKey, ICollection<TElement>>(kvp.Key, kvp.Value);
		}
	}
}
/// <summary>
/// Type alias for MultiMap
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TElement">The type of the element.</typeparam>
public class MultiMap<TKey, TElement> : MultiMap<TKey, TElement, List<TElement>>
{
}
