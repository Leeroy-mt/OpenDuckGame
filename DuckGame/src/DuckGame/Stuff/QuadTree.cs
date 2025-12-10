using System;
using System.Collections.Generic;

namespace DuckGame;

public class QuadTree
{
    private QuadTree _parent;

    private int _depth;

    private List<QuadTree> _children = new List<QuadTree>();

    private ObjectListImmediate _objects = new ObjectListImmediate();

    private List<Vec2> _corners = new List<Vec2>();

    private Stack<QuadTree> recurse = new Stack<QuadTree>();

    private Vec2 _position;

    private Vec2 _center;

    private float _width;

    private float _halfWidth;

    private int _max;

    private Rectangle _rectangle;

    private bool _split;

    private static int quadTreeIDX;

    private int _personalIDX;

    public Rectangle rectangle => _rectangle;

    public QuadTree(int depth, Vec2 position, float width, int max = 4, QuadTree parent = null)
    {
        _depth = depth;
        _position = position;
        _width = width;
        _halfWidth = _width / 2f;
        _max = max;
        _parent = parent;
        _center = _position + new Vec2(_halfWidth, _halfWidth);
        _rectangle = new Rectangle((int)position.x, (int)position.y, (int)width, (int)width);
        if (_depth != 0)
        {
            for (int i = 0; i < 4; i++)
            {
                _corners.Add(default(Vec2));
                Vec2 newPos = new Vec2(_position);
                if (i == 1 || i == 3)
                {
                    newPos.x += _halfWidth;
                }
                if (i == 2 || i == 3)
                {
                    newPos.y += _halfWidth;
                }
                _children.Add(new QuadTree(_depth - 1, newPos, _halfWidth, _max, this));
            }
        }
        _personalIDX = quadTreeIDX;
        quadTreeIDX++;
    }

    public Thing CheckPoint(Type pType, Vec2 pos, Thing ignore)
    {
        QuadTree current = this;
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[pType])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Point(pos, t))
                    {
                        return t;
                    }
                }
                return null;
            }
            if (pos.x > current._position.x + current._width)
            {
                return null;
            }
            if (pos.y > current._position.y + current._width)
            {
                return null;
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return null;
    }

    public T CheckPoint<T>(Vec2 pos, Thing ignore, Layer layer)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Point(pos, t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public T CheckPoint<T>(Vec2 pos, Thing ignore)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Point(pos, t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public T CheckPointFilter<T>(Vec2 pos, Func<Thing, bool> pFilter)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Point(pos, t) && pFilter(t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public T CheckPoint<T>(Vec2 pos)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Point(pos, t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public Thing CheckPoint(Vec2 pos, Type typer, Thing ignore, Layer layer)
    {
        QuadTree current = this;
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Point(pos, t))
                    {
                        return t;
                    }
                }
                return null;
            }
            if (pos.x > current._position.x + current._width)
            {
                return null;
            }
            if (pos.y > current._position.y + current._width)
            {
                return null;
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return null;
    }

    public Thing CheckPoint(Vec2 pos, Type typer, Thing ignore)
    {
        QuadTree current = this;
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Point(pos, t))
                    {
                        return t;
                    }
                }
                return null;
            }
            if (pos.x > current._position.x + current._width)
            {
                return null;
            }
            if (pos.y > current._position.y + current._width)
            {
                return null;
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return null;
    }

    public Thing CheckPoint(Vec2 pos, Type typer)
    {
        QuadTree current = this;
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Point(pos, t))
                    {
                        return t;
                    }
                }
                return null;
            }
            if (pos.x > current._position.x + current._width)
            {
                return null;
            }
            if (pos.y > current._position.y + current._width)
            {
                return null;
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return null;
    }

    public T CheckPointPlacementLayer<T>(Vec2 pos, Thing ignore = null, Layer layer = null)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.placementLayer) && Collision.Point(pos, t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public T CheckPointFilter<T>(Vec2 pos, Predicate<Thing> filter)
    {
        QuadTree current = this;
        Type typer = typeof(T);
        int index = 0;
        while (current != null)
        {
            if (!current._split)
            {
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && filter(t) && Collision.Point(pos, t))
                    {
                        return (T)(object)t;
                    }
                }
                return default(T);
            }
            if (pos.x > current._position.x + current._width)
            {
                return default(T);
            }
            if (pos.y > current._position.y + current._width)
            {
                return default(T);
            }
            index = 0;
            if (pos.x > current._position.x + current._halfWidth)
            {
                index = 1;
            }
            if (pos.y > current._position.y + current._halfWidth)
            {
                index += 2;
            }
            current = current._children[index];
        }
        return default(T);
    }

    public T CheckLine<T>(Vec2 p1, Vec2 p2, Thing ignore, Layer layer)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Line(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckLine<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Line(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckLine<T>(Vec2 p1, Vec2 p2)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Line(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public List<T> CheckLineAll<T>(Vec2 p1, Vec2 p2, Thing ignore, Layer layer)
    {
        recurse.Clear();
        recurse.Push(this);
        List<T> list = new List<T>();
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Line(p1, p2, t))
                    {
                        list.Add((T)(object)t);
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return list;
    }

    public List<T> CheckLineAll<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        recurse.Clear();
        recurse.Push(this);
        List<T> list = new List<T>();
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Line(p1, p2, t))
                    {
                        list.Add((T)(object)t);
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return list;
    }

    public List<T> CheckLineAll<T>(Vec2 p1, Vec2 p2)
    {
        recurse.Clear();
        recurse.Push(this);
        List<T> list = new List<T>();
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Line(p1, p2, t))
                    {
                        list.Add((T)(object)t);
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return list;
    }

    public T CheckLinePoint<T>(Vec2 p1, Vec2 p2, out Vec2 hit, Thing ignore, Layer layer)
    {
        hit = default(Vec2);
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer))
                    {
                        Vec2 p3 = Collision.LinePoint(p1, p2, t);
                        if (p3 != Vec2.Zero)
                        {
                            hit = p3;
                            return (T)(object)t;
                        }
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckLinePoint<T>(Vec2 p1, Vec2 p2, out Vec2 hit, Thing ignore)
    {
        hit = default(Vec2);
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore)
                    {
                        Vec2 p3 = Collision.LinePoint(p1, p2, t);
                        if (p3 != Vec2.Zero)
                        {
                            hit = p3;
                            return (T)(object)t;
                        }
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckLinePoint<T>(Vec2 p1, Vec2 p2, out Vec2 hit)
    {
        hit = default(Vec2);
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel)
                    {
                        Vec2 p3 = Collision.LinePoint(p1, p2, t);
                        if (p3 != Vec2.Zero)
                        {
                            hit = p3;
                            return (T)(object)t;
                        }
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Line(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckRectangleFilter<T>(Vec2 p1, Vec2 p2, Predicate<T> filter, Layer layer)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && (layer == null || layer == t.layer) && Collision.Rect(p1, p2, t) && filter((T)(object)t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckRectangleFilter<T>(Vec2 p1, Vec2 p2, Predicate<T> filter)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Rect(p1, p2, t) && filter((T)(object)t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckRectangle<T>(Vec2 p1, Vec2 p2, Thing ignore, Layer layer)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Rect(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckRectangle<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Rect(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckRectangle<T>(Vec2 p1, Vec2 p2)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Rect(p1, p2, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public void CheckRectangleAll<T>(Vec2 p1, Vec2 p2, List<T> outList)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Rect(p1, p2, t))
                    {
                        outList.Add((T)(object)t);
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                _ = current._children[i];
                if (Collision.Rect(p1, p2, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
    }

    public T CheckCircle<T>(Vec2 p1, float radius, Thing ignore, Layer layer)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && (layer == null || layer == t.layer) && Collision.Circle(p1, radius, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Circle(p1, radius, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckCircle<T>(Vec2 p1, float radius, Thing ignore)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && t != ignore && Collision.Circle(p1, radius, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Circle(p1, radius, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public T CheckCircle<T>(Vec2 p1, float radius)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Circle(p1, radius, t))
                    {
                        return (T)(object)t;
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Circle(p1, radius, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
        return default(T);
    }

    public void CheckCircleAll<T>(Vec2 p1, float radius, List<object> outList)
    {
        recurse.Clear();
        recurse.Push(this);
        while (recurse.Count > 0)
        {
            QuadTree current = recurse.Pop();
            if (!current._split)
            {
                Type typer = typeof(T);
                foreach (Thing t in current._objects[typer])
                {
                    if (!t.removeFromLevel && Collision.Circle(p1, radius, t))
                    {
                        outList.Add((T)(object)t);
                    }
                }
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Collision.Circle(p1, radius, current._children[i].rectangle))
                {
                    recurse.Push(current._children[i]);
                }
            }
        }
    }

    private void GetUniqueChildren(List<Thing> things)
    {
        foreach (Thing t in _objects)
        {
            if (!things.Contains(t))
            {
                things.Add(t);
            }
        }
        if (!_split)
        {
            return;
        }
        foreach (QuadTree child in _children)
        {
            child.GetUniqueChildren(things);
        }
    }

    private int Count()
    {
        return _objects.Count;
    }

    private void Divide()
    {
        if (_split || _depth == 0)
        {
            return;
        }
        _split = true;
        foreach (Thing t in _objects)
        {
            Add(t);
        }
    }

    private void Combine()
    {
        if (!_split)
        {
            return;
        }
        foreach (QuadTree child in _children)
        {
            child.Combine();
            _objects.AddRange(child._objects);
            child._objects.Clear();
        }
        _split = false;
    }

    public void Add(Thing t)
    {
        _objects.Add(t);
        if (!_split)
        {
            if (_objects.Count > _max && _depth > 0)
            {
                Divide();
            }
            return;
        }
        Rectangle tRect = t.rectangle;
        foreach (QuadTree child in _children)
        {
            if (Collision.Rect(child.rectangle, tRect))
            {
                child.Add(t);
            }
        }
    }

    public void Remove(Thing t)
    {
        _objects.Remove(t);
        if (!_split)
        {
            return;
        }
        Rectangle tRect = t.rectangle;
        foreach (QuadTree child in _children)
        {
            if (Collision.Rect(child.rectangle, tRect))
            {
                child.Remove(t);
            }
        }
        if (_objects.Count <= _max)
        {
            Combine();
        }
    }

    public void Draw()
    {
        Graphics.DrawRect(_position, _position + new Vec2(_width, _width), Color.Red, 1f, filled: false);
        if (!_split)
        {
            Graphics.DrawString(Change.ToString(_objects.Count), _position + new Vec2(2f, 2f), Color.White, 0.9f);
            Graphics.DrawString(Change.ToString(_personalIDX), _position + new Vec2(2f, 16f), Color.White, 0.9f, null, 0.5f);
            foreach (Thing t in _objects)
            {
                Graphics.DrawRect(t.rectangle, Color.Blue, 0f, filled: false);
                Graphics.DrawString(Change.ToString(_personalIDX), t.position, Color.Green, 0.9f, null, 0.5f);
            }
        }
        if (_depth == 0 || !_split)
        {
            return;
        }
        foreach (QuadTree child in _children)
        {
            child.Draw();
        }
    }

    public void Clear()
    {
        foreach (QuadTree child in _children)
        {
            child.Clear();
        }
        _objects.Clear();
    }
}
