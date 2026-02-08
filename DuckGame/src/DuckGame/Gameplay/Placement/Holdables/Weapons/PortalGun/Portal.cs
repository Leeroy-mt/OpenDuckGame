using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Portal : Thing
{
    private PortalGun _gun;

    private List<PortalDoor> _doors = new List<PortalDoor>();

    private Layer _fakeLayer = new Layer("FAKE");

    private HashSet<PortalDrawTransformer> _inPortal = new HashSet<PortalDrawTransformer>();

    public PortalGun gun => _gun;

    public Portal(PortalGun Gun)
    {
        _gun = Gun;
    }

    public IEnumerable<MaterialThing> CheckRectAll(Vector2 TopLeft, Vector2 BottomRight)
    {
        List<MaterialThing> things = new List<MaterialThing>();
        foreach (PortalDoor p in _doors)
        {
            if (!Collision.Rect(TopLeft, BottomRight, p.rect))
            {
                continue;
            }
            foreach (Block b in p.collision)
            {
                if (Collision.Rect(TopLeft, BottomRight, b))
                {
                    things.Add(b);
                }
            }
        }
        return things.AsEnumerable();
    }

    public List<PortalDoor> GetDoors()
    {
        return _doors;
    }

    public PortalDoor GetOtherDoor(PortalDoor door)
    {
        if (door == _doors[0])
        {
            return _doors[1];
        }
        return _doors[0];
    }

    public void AddPortalDoor(PortalDoor door)
    {
        if (_doors.Count > 1)
        {
            PortalDoor portalDoor = _doors[0];
            portalDoor.point1 = door.point1;
            portalDoor.point2 = door.point2;
            portalDoor.center = door.center;
            portalDoor.horizontal = door.horizontal;
            door = portalDoor;
            _doors.Reverse();
        }
        else
        {
            _doors.Add(door);
        }
        door.collision.Clear();
        if (door.horizontal)
        {
            return;
        }
        AutoBlock lower = Level.CheckLine<AutoBlock>(door.point2 + new Vector2(-8f, 0f), door.point2 + new Vector2(8f, 0f));
        if (lower != null)
        {
            Vector2 newTopLeft = lower.topLeft;
            if (newTopLeft.Y < door.bottom)
            {
                newTopLeft.Y = door.bottom;
            }
            float high = lower.bottom - newTopLeft.Y;
            if (high < 8f)
            {
                high = 8f;
            }
            door.collision.Add(new Block(newTopLeft.X, newTopLeft.Y, lower.width, high));
        }
        AutoBlock upper = Level.CheckLine<AutoBlock>(door.point1 + new Vector2(-8f, 0f), door.point1 + new Vector2(8f, 0f));
        if (upper != null)
        {
            Vector2 newBottomLeft = lower.bottomLeft;
            if (newBottomLeft.Y > door.top)
            {
                newBottomLeft.Y = door.top;
            }
            float high2 = newBottomLeft.Y - upper.top;
            if (high2 < 8f)
            {
                high2 = 8f;
            }
            door.collision.Add(new Block(newBottomLeft.X, newBottomLeft.Y - high2, upper.width, high2));
        }
        if (door.layer == null)
        {
            door.layer = new Layer("PORTAL", Layer.Game.depth, Layer.Game.camera);
            door.layer.scissor = Graphics.viewport.Bounds;
            Layer.Add(door.layer);
        }
        if (Level.CheckPoint<AutoBlock>(door.center + new Vector2(-8f, 0f)) != null)
        {
            door.isLeft = true;
        }
        else
        {
            door.isLeft = false;
        }
        door.rect = new Rectangle((int)door.point1.X - 8, (int)door.point1.Y, 16f, (int)door.point2.Y - (int)door.point1.Y);
    }

    public override void Initialize()
    {
    }

    public override void Terminate()
    {
        foreach (PortalDoor door in _doors)
        {
            Layer.Remove(door.layer);
        }
    }

    public override void Update()
    {
        if (_doors.Count != 2)
        {
            return;
        }
        IEnumerable<ITeleport> things = null;
        foreach (PortalDoor d in _doors)
        {
            IEnumerable<ITeleport> moreThings = null;
            moreThings = (d.horizontal ? Level.CheckRectAll<ITeleport>(d.point1 + new Vector2(0f, -8f), d.point2 + new Vector2(0f, 8f)) : Level.CheckRectAll<ITeleport>(d.point1 + new Vector2(-8f, 0f), d.point2 + new Vector2(8f, 0f)));
            things = ((things != null) ? things.Concat(moreThings) : moreThings);
        }
        List<PortalDrawTransformer> removeList = new List<PortalDrawTransformer>();
        foreach (PortalDrawTransformer item in _inPortal)
        {
            if (!things.Contains(item.thing as ITeleport))
            {
                removeList.Add(item);
            }
        }
        foreach (PortalDrawTransformer item2 in removeList)
        {
            _inPortal.Remove(item2);
            item2.thing.portal = null;
            item2.thing.layer = Layer.Game;
            foreach (PortalDoor door in _doors)
            {
                door.layer.Remove(item2);
            }
        }
        foreach (ITeleport t in things)
        {
            if (_inPortal.FirstOrDefault((PortalDrawTransformer v) => v.thing == t) != null)
            {
                continue;
            }
            PortalDrawTransformer item3 = new PortalDrawTransformer(t as Thing, this);
            _inPortal.Add(item3);
            (t as Thing).portal = this;
            (t as Thing).layer = _fakeLayer;
            foreach (PortalDoor door2 in _doors)
            {
                door2.layer.Add(item3);
            }
        }
        foreach (PortalDoor p in _doors)
        {
            p.Update();
            foreach (PortalDrawTransformer t2 in _inPortal)
            {
                if (p.isLeft && t2.thing.X < p.center.X)
                {
                    t2.thing.Position += GetOtherDoor(p).center - p.center;
                }
                else if (!p.isLeft && t2.thing.X > p.center.X)
                {
                    t2.thing.Position += GetOtherDoor(p).center - p.center;
                }
            }
        }
    }

    public override void Draw()
    {
        foreach (PortalDoor door in _doors)
        {
            door.Draw();
        }
    }
}
