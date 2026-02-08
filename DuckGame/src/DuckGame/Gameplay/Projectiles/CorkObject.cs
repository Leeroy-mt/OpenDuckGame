using Microsoft.Xna.Framework;

namespace DuckGame;

public class CorkObject : PhysicsObject, ISwing, IPullBack
{
    private Thing _gun;

    public Sprite _ropeSprite;

    private Rope _rope;

    private Harpoon _sticker;

    public CorkObject(float pX, float pY, Thing pOwner)
        : base(pX, pY)
    {
        graphic = new Sprite("cork");
        _collisionSize = new Vector2(4f, 4f);
        _collisionOffset = new Vector2(-2f, -3f);
        Center = new Vector2(3f, 3f);
        _gun = pOwner;
        weight = 0.1f;
        base.bouncy = 0.5f;
        airFrictionMult = 0f;
        _ropeSprite = new Sprite("grappleWire");
        _ropeSprite.Center = new Vector2(8f, 0f);
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

    public override void Initialize()
    {
        if (_gun != null)
        {
            _sticker = new Harpoon(this);
            base.level.AddThing(_sticker);
            _sticker.SetStuckPoint(_gun.Position);
            _rope = new Rope(base.X, base.Y, null, _sticker, this, vine: false, _ropeSprite, this);
            Level.Add(_rope);
        }
        base.Initialize();
    }

    public override void Terminate()
    {
        if (_sticker != null)
        {
            Level.Remove(_sticker);
        }
        if (_rope != null)
        {
            _rope.RemoveRope();
        }
        base.Terminate();
    }

    public float WindUp(float pAmount)
    {
        if (pAmount > 0f && _rope.startLength > 0f)
        {
            _rope.Pull(0f - pAmount);
            _rope.startLength -= pAmount;
            return _rope.startLength;
        }
        return 100f;
    }

    public override void Update()
    {
        if (_rope != null)
        {
            if (!base.grounded)
            {
                specialFrictionMod = 0f;
            }
            else
            {
                specialFrictionMod = 1f;
            }
            _rope.Position = Position;
            _rope.SetServer(base.isServerForObject);
            Vector2 travel = _rope.attach1.Position - _rope.attach2.Position;
            bool physics = true;
            if (_rope.properLength < 0f)
            {
                Rope rope = _rope;
                float startLength = (_rope.properLength = 100f);
                rope.startLength = startLength;
                physics = false;
            }
            if (travel.Length() > _rope.properLength)
            {
                travel.Normalize(); // TODO: travel = Vector2.Normalize(travel)
                _ = Position;
                Vector2 start = Position;
                Vector2 p2 = _rope.attach2.Position + travel * _rope.properLength;
                Level.CheckRay<Block>(start, p2, out var _);
                if (physics)
                {
                    hSpeed = p2.X - Position.X;
                    vSpeed = p2.Y - Position.Y;
                    gravMultiplier = 0f;
                    float prevSpec = specialFrictionMod;
                    specialFrictionMod = 0f;
                    airFrictionMult = 0f;
                    Vector2 lastPos = base.lastPosition;
                    UpdatePhysics();
                    gravMultiplier = 1f;
                    specialFrictionMod = prevSpec;
                    Vector2 dif = p2 - lastPos;
                    if (dif.Length() > 32f)
                    {
                        Position = p2;
                    }
                    else if (dif.Length() > 6f)
                    {
                        hSpeed = Rando.Float(-2f, 2f);
                        vSpeed = Rando.Float(-2f, 2f);
                    }
                    else
                    {
                        hSpeed = dif.X;
                        vSpeed = dif.Y;
                    }
                }
                else
                {
                    Position = p2;
                }
            }
            _sticker.SetStuckPoint((_gun as Gun).barrelPosition);
        }
        base.Update();
    }
}
