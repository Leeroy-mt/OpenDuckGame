using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckGame;

public class ObjectListImmediate : IEnumerable<Thing>, IEnumerable
{
    private HashSet<Thing> _emptyList = new HashSet<Thing>();

    private HashSet<Thing> _bigList = new HashSet<Thing>();

    private MultiMap<Type, Thing, HashSet<Thing>> _objectsByType = new MultiMap<Type, Thing, HashSet<Thing>>();

    public HashSet<Thing> this[Type key]
    {
        get
        {
            if (key == typeof(Thing))
            {
                return _bigList;
            }
            if (_objectsByType.ContainsKey(key))
            {
                return _objectsByType[key];
            }
            return _emptyList;
        }
    }

    public int Count => _bigList.Count;

    public List<Thing> ToList()
    {
        List<Thing> list = new List<Thing>();
        list.AddRange(_bigList);
        return list;
    }

    public void Add(Thing obj)
    {
        foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
        {
            _objectsByType.Add(t, obj);
        }
        _bigList.Add(obj);
    }

    public void AddRange(ObjectListImmediate list)
    {
        foreach (Thing t in list)
        {
            Add(t);
        }
    }

    public void Remove(Thing obj)
    {
        foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
        {
            _objectsByType.Remove(t, obj);
        }
        _bigList.Remove(obj);
    }

    public void Clear()
    {
        _bigList.Clear();
        _objectsByType.Clear();
    }

    public bool Contains(Thing obj)
    {
        return _bigList.Contains(obj);
    }

    public IEnumerator<Thing> GetEnumerator()
    {
        return _bigList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
