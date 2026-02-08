using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DuckGame;

public class PortalBullet : Bullet
{
    private Texture2D _beem;

    private float _thickness;

    public PortalBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, float thick = 0.3f)
        : base(xval, yval, type, ang, owner, rbound, distance)
    {
        _thickness = thick;
        _beem = Content.Load<Texture2D>("laserBeam");
    }

    public override void OnCollide(Vector2 pos, Thing t, bool willBeStopped)
    {
        if (!(t is Block && willBeStopped) || !(owner is PortalGun gun))
        {
            return;
        }
        Portal portal = Level.current.things[typeof(Portal)].FirstOrDefault((Thing p) => (p as Portal).gun == owner) as Portal;
        if (portal == null)
        {
            portal = new Portal(gun);
            Level.Add(portal);
        }
        Vector2 backPos = pos - travelDirNormalized;
        PortalDoor door = new PortalDoor();
        door.center = pos;
        if (Math.Abs(travelDirNormalized.Y) < 0.5f)
        {
            door.horizontal = false;
            door.point1 = pos + new Vector2(0f, -16f);
            door.point2 = pos + new Vector2(0f, 16f);
            AutoBlock b = Level.CheckLine<AutoBlock>(backPos, backPos + new Vector2(0f, 16f));
            if (b != null && b.top < door.point2.Y)
            {
                door.point2.Y = b.top;
            }
            b = Level.CheckLine<AutoBlock>(backPos, backPos + new Vector2(0f, -16f));
            if (b != null && b.bottom > door.point1.Y)
            {
                door.point1.Y = b.bottom;
            }
        }
        else
        {
            door.horizontal = true;
            door.point1 = pos + new Vector2(-16f, 0f);
            door.point2 = pos + new Vector2(16f, 0f);
            AutoBlock b2 = Level.CheckLine<AutoBlock>(backPos, backPos + new Vector2(16f, 0f));
            if (b2 != null && b2.left < door.point2.X)
            {
                door.point2.X = b2.left;
            }
            b2 = Level.CheckLine<AutoBlock>(backPos, backPos + new Vector2(-16f, 0f));
            if (b2 != null && b2.right > door.point1.X)
            {
                door.point1.X = b2.right;
            }
        }
        portal.AddPortalDoor(door);
    }

    public override void Draw()
    {
        if (_tracer || !(_bulletDistance > 0.1f))
        {
            return;
        }
        float length = (drawStart - drawEnd).Length();
        float dist = 0f;
        float incs = 1f / (length / 8f);
        float alph = 0f;
        float drawLength = 8f;
        while (true)
        {
            bool doBreak = false;
            if (dist + drawLength > length)
            {
                drawLength = length - Maths.Clamp(dist, 0f, 99f);
                doBreak = true;
            }
            alph += incs;
            Graphics.DrawTexturedLine(_beem, drawStart + travelDirNormalized * dist, drawStart + travelDirNormalized * (dist + drawLength), Color.White * alph, _thickness, 0.6f);
            if (!doBreak)
            {
                dist += 8f;
                continue;
            }
            break;
        }
    }
}
