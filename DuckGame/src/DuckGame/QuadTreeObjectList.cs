using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class QuadTreeObjectList : IEnumerable<Thing>, IEnumerable
{
	private HashSet<Thing> _emptyList = new HashSet<Thing>();

	private HashSet<Thing> _bigList = new HashSet<Thing>();

	private HashSet<Thing> _addThings = new HashSet<Thing>();

	private HashSet<Thing> _removeThings = new HashSet<Thing>();

	private MultiMap<Type, Thing, HashSet<Thing>> _objectsByType = new MultiMap<Type, Thing, HashSet<Thing>>();

	private MultiMap<Type, Thing, HashSet<Thing>> _staticObjectsByType = new MultiMap<Type, Thing, HashSet<Thing>>();

	private MultiMap<Type, Thing, HashSet<Thing>> _allObjectsByType = new MultiMap<Type, Thing, HashSet<Thing>>();

	private QuadTree _quadTree = new QuadTree(4, new Vec2(-2304f, -2304f), 4608f, 64);

	private List<CollisionIsland> _islands = new List<CollisionIsland>();

	private bool _autoRefresh;

	private bool _useTree;

	public bool objectsDirty;

	public HashSet<Thing> updateList => _bigList;

	public QuadTree quadTree => _quadTree;

	public bool useTree
	{
		get
		{
			return _useTree;
		}
		set
		{
			_useTree = value;
		}
	}

	public IEnumerable<Thing> this[Type key]
	{
		get
		{
			if (key == typeof(Thing))
			{
				return _bigList;
			}
			HashSet<Thing> tryStuff = null;
			if (_allObjectsByType.TryGetValue(key, out tryStuff))
			{
				return tryStuff;
			}
			return _emptyList;
		}
	}

	public int Count => _bigList.Count;

	public void RandomizeObjectOrder()
	{
		_bigList = new HashSet<Thing>(_bigList.OrderBy((Thing x) => Rando.Int(999999)).ToList());
	}

	public List<CollisionIsland> GetIslands(Vec2 point)
	{
		List<CollisionIsland> isle = new List<CollisionIsland>();
		foreach (CollisionIsland i in _islands)
		{
			if (!i.willDie && (point - i.owner.position).lengthSq < i.radiusSquared)
			{
				isle.Add(i);
			}
		}
		return isle;
	}

	public List<CollisionIsland> GetIslandsForCollisionCheck(Vec2 point)
	{
		List<CollisionIsland> isle = new List<CollisionIsland>();
		foreach (CollisionIsland i in _islands)
		{
			if (!i.willDie && (point - i.owner.position).lengthSq < i.radiusCheckSquared)
			{
				isle.Add(i);
			}
		}
		return isle;
	}

	public CollisionIsland GetIsland(Vec2 point, CollisionIsland ignore = null)
	{
		foreach (CollisionIsland i in _islands)
		{
			if (!i.willDie && i != ignore && (point - i.owner.position).lengthSq < i.radiusSquared)
			{
				return i;
			}
		}
		return null;
	}

	public void AddIsland(MaterialThing t)
	{
		_islands.Add(new CollisionIsland(t, this));
	}

	public void RemoveIsland(CollisionIsland i)
	{
		if (i.things.Count != 0)
		{
			i.owner = i.things.First();
			for (int j = 0; j < i.things.Count; j++)
			{
				MaterialThing t = i.things.ElementAt(j);
				if (t != i.owner)
				{
					int prevCount = i.things.Count;
					t.UpdateIsland();
					if (i.things.Count != prevCount)
					{
						j--;
					}
				}
			}
		}
		else
		{
			i.willDie = true;
		}
	}

	public void UpdateIslands()
	{
	}

	public QuadTreeObjectList(bool automatic = false, bool tree = true)
	{
		_autoRefresh = automatic;
		_useTree = tree;
	}

	public List<Thing> ToList()
	{
		List<Thing> list = new List<Thing>();
		list.AddRange(_bigList);
		return list;
	}

	public int CountType<T>()
	{
		HashSet<Thing> tryStuff = null;
		if (_allObjectsByType.TryGetValue(typeof(T), out tryStuff))
		{
			return tryStuff.Count;
		}
		return 0;
	}

	public HashSet<Thing> GetDynamicObjects(Type key)
	{
		if (key == typeof(Thing))
		{
			return _bigList;
		}
		HashSet<Thing> tryStuff = null;
		if (_objectsByType.TryGetValue(key, out tryStuff))
		{
			return tryStuff;
		}
		return _emptyList;
	}

	public HashSet<Thing> GetStaticObjects(Type key)
	{
		if (key == typeof(Thing))
		{
			return _bigList;
		}
		HashSet<Thing> tryStuff = null;
		if (_staticObjectsByType.TryGetValue(key, out tryStuff))
		{
			return tryStuff;
		}
		return _emptyList;
	}

	private IEnumerable<Thing> GetIslandObjects(Type t, Vec2 pos, float radiusSq)
	{
		IEnumerable<Thing> things = new List<Thing>();
		foreach (CollisionIsland i in _islands)
		{
			if ((i.owner.position - pos).lengthSq - radiusSq < i.radiusCheckSquared)
			{
				things = things.Concat(i.things);
			}
		}
		return things;
	}

	public bool HasStaticObjects(Type key)
	{
		if (!(key == typeof(Thing)))
		{
			return _staticObjectsByType.ContainsKey(key);
		}
		return true;
	}

	public void Add(Thing obj)
	{
		_addThings.Add(obj);
		if (_autoRefresh)
		{
			RefreshState();
		}
	}

	public void AddRange(ObjectList list)
	{
		foreach (Thing t in list)
		{
			Add(t);
		}
	}

	public void Remove(Thing obj)
	{
		_removeThings.Add(obj);
		if (_autoRefresh)
		{
			RefreshState();
		}
	}

	public void Clear()
	{
		_bigList.Clear();
		_addThings.Clear();
		_objectsByType.Clear();
		_staticObjectsByType.Clear();
		_quadTree.Clear();
		_allObjectsByType.Clear();
	}

	public bool Contains(Thing obj)
	{
		return _bigList.Contains(obj);
	}

	public void CleanAddList()
	{
		foreach (Thing t in _removeThings)
		{
			_addThings.Remove(t);
		}
	}

	public void RefreshState()
	{
		foreach (Thing t in _removeThings)
		{
			t.level = null;
			if (t is IDontMove && _useTree)
			{
				removeItem(_staticObjectsByType, t);
				_quadTree.Remove(t);
			}
			else
			{
				removeItem(_objectsByType, t);
			}
			_bigList.Remove(t);
			objectsDirty = true;
		}
		_removeThings.Clear();
		foreach (Thing t2 in _addThings)
		{
			_bigList.Add(t2);
			t2.level = Level.current;
			if (t2 is IDontMove && _useTree)
			{
				addItem(_staticObjectsByType, t2);
				_quadTree.Add(t2);
			}
			else
			{
				addItem(_objectsByType, t2);
			}
			objectsDirty = true;
		}
		_addThings.Clear();
	}

	private void addItem(MultiMap<Type, Thing, HashSet<Thing>> list, Thing obj)
	{
		foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
		{
			list.Add(t, obj);
			_allObjectsByType.Add(t, obj);
		}
	}

	private void removeItem(MultiMap<Type, Thing, HashSet<Thing>> list, Thing obj)
	{
		foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
		{
			list.Remove(t, obj);
			_allObjectsByType.Remove(t, obj);
		}
	}

	public IEnumerator<Thing> GetEnumerator()
	{
		return _bigList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _bigList.GetEnumerator();
	}

	public void Draw()
	{
		if (!DevConsole.showIslands)
		{
			return;
		}
		int num = 0;
		foreach (CollisionIsland i in _islands)
		{
			Graphics.DrawCircle(i.owner.position, i.radiusCheck, Color.Red * 0.7f, 1f, 0.9f, 64);
			Graphics.DrawCircle(i.owner.position, i.radius, Color.Blue * 0.3f, 1f, 0.9f, 64);
			Graphics.DrawString(Convert.ToString(num), i.owner.position, Color.Red, 1f);
			foreach (MaterialThing t in i.things)
			{
				if (t != i.owner)
				{
					Graphics.DrawString(Convert.ToString(num), t.position, Color.White, 1f);
				}
			}
			num++;
		}
	}
}
