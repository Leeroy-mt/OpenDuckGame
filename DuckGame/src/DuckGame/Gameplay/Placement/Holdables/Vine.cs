using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
[BaggedProperty("isOnlineCapable", false)]
public class Vine : Holdable, ISwing
{
    protected SpriteMap _sprite;

    protected Harpoon _harpoon;

    public Rope _rope;

    public int sectionIndex;

    public EditorProperty<int> length = new EditorProperty<int>(4, null, 1f, 16f, 1f);

    public Vine nextVine;

    public Vine prevVine;

    protected Sprite _vinePartSprite;

    public float initLength;

    public bool changeSpeed = true;

    protected Vector2 _wallPoint;

    protected Vector2 _grappleTravel;

    public List<VineSection> points
    {
        get
        {
            List<VineSection> p = new List<VineSection>();
            List<VineSection> curSection = new List<VineSection>();
            Rope cur = _rope;
            Vine curVine = this;
            while (cur != null)
            {
                VineSection v = new VineSection();
                v.pos2 = cur.attach1Point;
                v.pos1 = cur.attach2Point;
                v.length = (v.pos1 - v.pos2).Length();
                cur = cur.attach2 as Rope;
                curSection.Add(v);
                if (cur == null && curVine.nextVine != null)
                {
                    curSection.Reverse();
                    p.AddRange(curSection);
                    curSection.Clear();
                    curVine = curVine.nextVine;
                    cur = curVine._rope;
                }
            }
            if (curSection.Count > 0)
            {
                curSection.Reverse();
                p.AddRange(curSection);
            }
            float totalLength = 0f;
            foreach (VineSection section in p)
            {
                totalLength += section.length;
            }
            int prevSectionIndex = 0;
            foreach (VineSection section2 in p)
            {
                section2.lowestSection = prevSectionIndex + (int)Math.Round(section2.length / totalLength * (float)sectionIndex);
                prevSectionIndex = section2.lowestSection;
            }
            return p;
        }
    }

    public Vector2 wallPoint => _wallPoint;

    public Vector2 grappelTravel => _grappleTravel;

    public Vine(float xpos, float ypos, float init)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("vine", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        _vinePartSprite = new Sprite("vine");
        _vinePartSprite.Center = new Vector2(8f, 0f);
        collisionOffset = new Vector2(-5f, -4f);
        collisionSize = new Vector2(11f, 7f);
        weight = 0.1f;
        thickness = 0.1f;
        canPickUp = false;
        initLength = init;
        base.Depth = -0.5f;
    }

    public override void OnPressAction()
    {
    }

    public override void OnReleaseAction()
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        _harpoon = new Harpoon(this);
        Level.Add(_harpoon);
        if (Level.current is not Editor)
        {
            Vector2 pos = Position;
            Y += (int)length * 16 - 8;
            _harpoon.noisy = false;
            _harpoon.Fire(pos + new Vector2(0, -8), new Vector2(0, -1));
            _rope = new Rope(X, Y, null, _harpoon, duck, vine: true, _vinePartSprite);
            if (initLength != 0)
            {
                _rope.properLength = initLength;
            }
            Level.Add(_rope);
        }
    }

    public override void Terminate()
    {
        if (_rope != null)
        {
            _rope.RemoveRope();
            Level.Remove(_harpoon);
            Level.Remove(_rope);
        }
    }

    public Rope GetRopeParent(Thing child)
    {
        for (Rope t = _rope; t != null; t = t.attach2 as Rope)
        {
            if (t.attach2 == child)
            {
                return t;
            }
        }
        return null;
    }

    public void Degrapple()
    {
        if (nextVine != null && nextVine._rope != null)
        {
            nextVine._rope.attach2 = _rope.attach2;
            nextVine._rope.properLength = (nextVine._rope.attach1Point - _rope.attach2Point).Length();
            nextVine.prevVine = null;
            nextVine = null;
        }
        if (prevVine != null)
        {
            prevVine.nextVine = null;
        }
        _harpoon.Return();
        _harpoon.visible = false;
        if (_rope != null)
        {
            _rope.RemoveRope();
            _rope.visible = false;
            visible = false;
        }
        _rope = null;
        if (base.duck != null)
        {
            base.duck.frictionMult = 1f;
            base.duck.gravMultiplier = 1f;
        }
        owner = null;
        frictionMult = 1f;
        gravMultiplier = 1f;
        visible = false;
        Level.Remove(_harpoon);
        Level.Remove(this);
        Update();
    }

    public void UpdateRopeStuff()
    {
        _rope.Update();
        Update();
    }

    public void MoveDuck()
    {
        Vector2 travel = _rope.attach1.Position - _rope.attach2.Position;
        if (travel.Length() > _rope.properLength)
        {
            travel.Normalize(); // TODO: travel = Vector2.Normalize(travel)
            if (base.duck != null)
            {
                PhysicsObject attach = base.duck;
                _ = attach.Position;
                attach.Position = _rope.attach2.Position + travel * _rope.properLength;
                _ = attach.Position - attach.lastPosition;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (owner != null)
        {
            offDir = owner.offDir;
        }
        if (base.duck != null && (base.duck.ragdoll != null || base.duck._trapped != null || base.duck.dead))
        {
            owner = null;
            _rope.visible = false;
        }
        if (_rope == null)
        {
            return;
        }
        if (owner != null)
        {
            _rope.Position = owner.Position;
        }
        else
        {
            _rope.Position = Position;
            if (base.prevOwner != null)
            {
                PhysicsObject obj = base.prevOwner as PhysicsObject;
                obj.frictionMult = 1f;
                obj.gravMultiplier = 1f;
                _prevOwner = null;
                frictionMult = 1f;
                gravMultiplier = 1f;
                Level.Remove(this);
            }
        }
        if (!_harpoon.stuck)
        {
            return;
        }
        if (base.duck != null)
        {
            if (!base.duck.grounded)
            {
                base.duck.frictionMult = 0f;
            }
            else
            {
                base.duck.frictionMult = 1f;
                base.duck.gravMultiplier = 1f;
            }
        }
        else if (!base.grounded)
        {
            frictionMult = 0f;
        }
        else
        {
            frictionMult = 1f;
            gravMultiplier = 1f;
        }
        Vector2 travel = _rope.attach1.Position - _rope.attach2.Position;
        if (_rope.properLength < 0f)
        {
            _rope.properLength = travel.Length();
        }
        if (!(travel.Length() > _rope.properLength))
        {
            return;
        }
        travel.Normalize(); // TODO: travel = Vector2.Normalize(travel)
        if (base.duck != null)
        {
            PhysicsObject attach = base.duck;
            if (base.duck.ragdoll != null)
            {
                Degrapple();
                return;
            }
            _ = attach.Position;
            attach.Position = _rope.attach2.Position + travel * _rope.properLength;
            Vector2 dif = attach.Position - attach.lastPosition;
            if (changeSpeed)
            {
                attach.hSpeed = dif.X;
                attach.vSpeed = dif.Y;
            }
        }
        else
        {
            _ = Position;
            Position = _rope.attach2.Position + travel * _rope.properLength;
            Vector2 dif2 = Position - base.lastPosition;
            hSpeed = dif2.X;
            vSpeed = dif2.Y;
        }
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            graphic.Center = new Vector2(8f, 8f);
            graphic.Depth = base.Depth;
            for (int i = 0; i < (int)length; i++)
            {
                Graphics.Draw(graphic, base.X, base.Y + (float)(i * 16));
            }
        }
    }
}
