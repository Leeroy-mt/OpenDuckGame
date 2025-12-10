using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ObjectList : IEnumerable<Thing>, IEnumerable
{
    private List<Thing> _bigList = new List<Thing>();

    private MultiMap<string, Thing> _objects = new MultiMap<string, Thing>();

    private MultiMap<string, Thing> _addThings = new MultiMap<string, Thing>();

    private MultiMap<string, Thing> _removeThings = new MultiMap<string, Thing>();

    private MultiMap<Type, Thing> _objectsByType = new MultiMap<Type, Thing>();

    private bool _autoRefresh;

    public IEnumerable<Thing> this[Type key]
    {
        get
        {
            if (_objectsByType.ContainsKey(key))
            {
                return _objectsByType[key];
            }
            return Enumerable.Empty<Thing>();
        }
    }

    public IEnumerable<Thing> this[string key]
    {
        get
        {
            if (_objects.ContainsKey(key))
            {
                return _objects[key];
            }
            return Enumerable.Empty<Thing>();
        }
    }

    public Thing this[int key]
    {
        get
        {
            if (key < _bigList.Count)
            {
                return _bigList[key];
            }
            return null;
        }
    }

    public int Count => _bigList.Count;

    public ObjectList(bool automatic = false)
    {
        _autoRefresh = automatic;
    }

    public List<Thing> ToList()
    {
        List<Thing> things = new List<Thing>();
        foreach (List<Thing> p in _objects.Values)
        {
            things.AddRange(p);
        }
        return things;
    }

    public void Add(Thing obj)
    {
        removeItem(_removeThings, obj);
        addItem(_addThings, obj);
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
        addItem(_removeThings, obj);
        removeItem(_addThings, obj);
        if (_autoRefresh)
        {
            RefreshState();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Count; i++)
        {
            Remove(this[i]);
        }
    }

    public bool Contains(Thing obj)
    {
        return _bigList.Contains(obj);
    }

    public void RefreshState()
    {
        foreach (KeyValuePair<string, List<Thing>> removeThing in _removeThings)
        {
            foreach (Thing t in removeThing.Value)
            {
                _bigList.Remove(t);
                removeItem(_objects, t);
                removeItem(_objectsByType, t);
            }
        }
        _removeThings.Clear();
        foreach (KeyValuePair<string, List<Thing>> addThing in _addThings)
        {
            foreach (Thing t2 in addThing.Value)
            {
                _bigList.Add(t2);
                addItem(_objects, t2);
                addItem(_objectsByType, t2);
            }
        }
        _addThings.Clear();
    }

    private void addItem(MultiMap<string, Thing> list, Thing obj)
    {
        if (!list.Contains(obj.type, obj))
        {
            list.Add(obj.type, obj);
        }
    }

    private void removeItem(MultiMap<string, Thing> list, Thing obj)
    {
        list.Remove(obj.type, obj);
    }

    private void addItem(MultiMap<Type, Thing> list, Thing obj)
    {
        foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
        {
            if (list.Contains(t, obj))
            {
                break;
            }
            list.Add(t, obj);
        }
    }

    private void removeItem(MultiMap<Type, Thing> list, Thing obj)
    {
        foreach (Type t in Editor.AllBaseTypes[obj.GetType()])
        {
            list.Remove(t, obj);
        }
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
