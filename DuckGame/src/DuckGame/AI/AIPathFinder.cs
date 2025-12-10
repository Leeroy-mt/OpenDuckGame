using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class AIPathFinder
{
    private Thing _followObject;

    private PathNodeLink _revert;

    private List<PathNodeLink> _path;

    public Thing followObject
    {
        get
        {
            return _followObject;
        }
        set
        {
            _followObject = value;
        }
    }

    public List<PathNodeLink> path
    {
        get
        {
            if (_path != null)
            {
                if (_path.Count <= 0)
                {
                    return null;
                }
                return _path;
            }
            return null;
        }
    }

    public PathNodeLink target
    {
        get
        {
            if (_path == null || _path.Count == 0)
            {
                return null;
            }
            return _path[0];
        }
    }

    public PathNodeLink peek
    {
        get
        {
            if (_path == null || _path.Count == 0)
            {
                return null;
            }
            if (_path.Count > 1)
            {
                return _path[1];
            }
            return _path[0];
        }
    }

    public bool finished
    {
        get
        {
            if (_path != null)
            {
                return _path.Count == 0;
            }
            return true;
        }
    }

    public AIPathFinder(Thing t = null)
    {
        _followObject = t;
    }

    public void AtTarget()
    {
        if (_path != null && _path.Count > 0)
        {
            _revert = _path[0];
            _path.RemoveAt(0);
        }
    }

    public void Revert()
    {
        if (_path != null && _revert != null)
        {
            _path.Insert(0, _revert);
        }
    }

    public void Refresh()
    {
        if (_path != null)
        {
            PathNodeLink t = _path.Last();
            SetTarget(t);
        }
    }

    public void SetTarget(Vec2 target)
    {
        if (_followObject != null)
        {
            SetTarget(_followObject.position, target);
        }
        else
        {
            SetTarget(target, target);
        }
    }

    public void SetTarget(PathNodeLink target)
    {
        if (_followObject != null)
        {
            SetTarget(_followObject.position, target.owner.position);
        }
    }

    public void SetTarget(Vec2 position, Vec2 target)
    {
        _revert = null;
        _path = null;
        List<Thing> nodes = Level.current.things[typeof(PathNode)].ToList();
        nodes.Sort((Thing a, Thing b) => (!((a.position - position).lengthSq < (b.position - position).lengthSq)) ? 1 : (-1));
        PathNode startNode = null;
        foreach (Thing t in nodes)
        {
            if (PathNode.LineIsClear(position, t.position))
            {
                startNode = t as PathNode;
                break;
            }
        }
        if (startNode == null)
        {
            return;
        }
        nodes.Sort((Thing a, Thing b) => (!((a.position - target).lengthSq < (b.position - target).lengthSq)) ? 1 : (-1));
        PathNode endNode = null;
        foreach (Thing t2 in nodes)
        {
            if (PathNode.LineIsClear(target, t2.position))
            {
                endNode = t2 as PathNode;
                break;
            }
        }
        if (endNode == null)
        {
            return;
        }
        AIPath p = startNode.GetPath(endNode);
        if (p == null || p.nodes.Count <= 0)
        {
            return;
        }
        bool skipFirst = false;
        if (p.nodes.Count > 1 && PathNode.LineIsClear(position, p.nodes[1].position))
        {
            skipFirst = true;
        }
        _path = new List<PathNodeLink>();
        PathNode prevNode = null;
        foreach (PathNode node in p.nodes)
        {
            if (!skipFirst)
            {
                Thing t3 = null;
                PathNodeLink firstLink = new PathNodeLink();
                firstLink.owner = t3;
                firstLink.link = node;
                prevNode = node;
                skipFirst = true;
                _path.Add(firstLink);
            }
            else
            {
                if (prevNode != null)
                {
                    _path.Add(prevNode.GetLink(node));
                }
                prevNode = node;
            }
        }
        Thing newLinkThing = null;
        PathNodeLink link = new PathNodeLink();
        if (_path.Count > 0)
        {
            link.owner = _path.Last().link;
        }
        else
        {
            link.owner = newLinkThing;
        }
        link.link = newLinkThing;
        _path.Add(link);
    }
}
