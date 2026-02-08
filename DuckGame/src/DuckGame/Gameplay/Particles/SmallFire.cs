using Microsoft.Xna.Framework;

namespace DuckGame;

public class SmallFire : PhysicsParticle, ITeleport
{
    public static int kMaxObjects = 256;

    public float waitToHurt;

    public Duck whoWait;

    private SpriteMap _sprite;

    private SpriteMap _airFire;

    private float _airFireScale;

    private float _spinSpeed;

    private bool _multiplied;

    private byte _groundLife = 125;

    private Vector2 _stickOffset;

    private MaterialThing _stick;

    private Thing _firedFrom;

    private static bool kAlternate = false;

    private bool _alternate;

    private bool _alternateb;

    public bool doFloat;

    private int _fireID;

    private bool _canMultiply = true;

    private bool didRemove;

    public byte groundLife
    {
        get
        {
            return _groundLife;
        }
        set
        {
            _groundLife = value;
        }
    }

    public Vector2 stickOffset
    {
        get
        {
            return _stickOffset;
        }
        set
        {
            _stickOffset = value;
        }
    }

    public MaterialThing stick
    {
        get
        {
            return _stick;
        }
        set
        {
            _stick = value;
        }
    }

    public Thing firedFrom => _firedFrom;

    public int fireID => _fireID;

    public static SmallFire New(float xpos, float ypos, float hspeed, float vspeed, bool shortLife = false, MaterialThing stick = null, bool canMultiply = true, Thing firedFrom = null, bool network = false)
    {
        SmallFire obj = null;
        if (Network.isActive)
        {
            obj = new SmallFire();
        }
        else if (Level.core.firePool[Level.core.firePoolIndex] == null)
        {
            obj = new SmallFire();
            Level.core.firePool[Level.core.firePoolIndex] = obj;
        }
        else
        {
            obj = Level.core.firePool[Level.core.firePoolIndex];
        }
        Level.core.firePoolIndex = (Level.core.firePoolIndex + 1) % kMaxObjects;
        if (obj != null)
        {
            obj.ResetProperties();
            obj.Init(xpos, ypos, hspeed, vspeed, shortLife, stick, canMultiply);
            obj._sprite.globalIndex = Thing.GetGlobalIndex();
            obj._airFire.globalIndex = Thing.GetGlobalIndex();
            obj._firedFrom = firedFrom;
            obj.needsSynchronization = true;
            obj.isLocal = !network;
            if (Network.isActive && !network)
            {
                GhostManager.context.particleManager.AddLocalParticle(obj);
            }
            if (float.IsNaN(obj.Position.X) || float.IsNaN(obj.Position.Y))
            {
                if (obj.stick != null)
                {
                    obj.X = 0f;
                    obj.Y = 0f;
                }
                else
                {
                    obj.X = Vector2.NetMin.X;
                    obj.Y = Vector2.NetMin.Y;
                }
            }
        }
        return obj;
    }

    public override void NetSerialize(BitBuffer b)
    {
        if (stick != null && stick.ghostObject != null)
        {
            b.Write(val: true);
            b.Write((ushort)(int)stick.ghostObject.ghostObjectIndex);
            b.Write((sbyte)stickOffset.X);
            b.Write((sbyte)stickOffset.Y);
        }
        else
        {
            b.Write(val: false);
            b.Write((short)base.X);
            b.Write((short)base.Y);
        }
    }

    public override void NetDeserialize(BitBuffer d)
    {
        if (d.ReadBool())
        {
            GhostObject attach = GhostManager.context.GetGhost(d.ReadUShort());
            if (attach != null && attach.thing != null)
            {
                stick = attach.thing as MaterialThing;
            }
            int xOffset = d.ReadSByte();
            int yOffset = d.ReadSByte();
            stickOffset = new Vector2(xOffset, yOffset);
            UpdateStick();
            hSpeed = 0f;
            vSpeed = 0f;
        }
        else
        {
            float xpos = d.ReadShort();
            float ypos = d.ReadShort();
            netLerpPosition = new Vector2(xpos, ypos);
        }
    }

    private SmallFire()
        : base(0f, 0f)
    {
        _bounceEfficiency = 0.2f;
        _sprite = new SpriteMap("smallFire", 16, 16);
        _sprite.AddAnimation("burn", 0.2f + Rando.Float(0.2f), true, 0, 1, 2, 3, 4);
        graphic = _sprite;
        Center = new Vector2(8f, 14f);
        _airFire = new SpriteMap("airFire", 16, 16);
        _airFire.AddAnimation("burn", 0.2f + Rando.Float(0.2f), true, 0, 1, 2, 1);
        _airFire.Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(12f, 12f);
        _collisionOffset = new Vector2(-6f, -6f);
    }

    private void Init(float xpos, float ypos, float hspeed, float vspeed, bool shortLife = false, MaterialThing stick = null, bool canMultiply = true)
    {
        if (xpos == 0f && ypos == 0f && stick == null)
        {
            xpos = Vector2.NetMin.X;
            ypos = Vector2.NetMin.Y;
        }
        X = xpos;
        Y = ypos;
        _airFireScale = 0f;
        _multiplied = false;
        _groundLife = 125;
        doFloat = false;
        hSpeed = hspeed;
        vSpeed = vspeed;
        _sprite.SetAnimation("burn");
        _sprite.imageIndex = Rando.Int(4);
        float num = (base.ScaleY = 0.8f + Rando.Float(0.6f));
        base.ScaleX = num;
        base.AngleDegrees = -10f + Rando.Float(20f);
        _airFire.SetAnimation("burn");
        _airFire.imageIndex = Rando.Int(2);
        SpriteMap airFire = _airFire;
        num = (_airFire.ScaleY = 0f);
        airFire.ScaleX = num;
        _spinSpeed = 0.1f + Rando.Float(0.1f);
        _airFire.color = Color.Orange * (0.8f + Rando.Float(0.2f));
        _gravMult = 0.7f;
        _sticky = 0.6f;
        _life = 100f;
        if (Network.isActive)
        {
            _sticky = 0f;
        }
        _fireID = FireManager.GetFireID();
        needsSynchronization = true;
        if (shortLife)
        {
            _groundLife = 31;
        }
        base.Depth = 0.6f;
        _stick = stick;
        _stickOffset = new Vector2(xpos, ypos);
        UpdateStick();
        _alternate = kAlternate;
        kAlternate = !kAlternate;
        _canMultiply = canMultiply;
    }

    public void UpdateStick()
    {
        if (_stick != null)
        {
            Position = _stick.Offset(_stickOffset);
        }
    }

    public void SuckLife(float l)
    {
        _life -= l;
    }

    public override void Removed()
    {
        if (Network.isActive && !didRemove && isLocal && GhostManager.context != null)
        {
            didRemove = true;
            GhostManager.context.particleManager.RemoveParticle(this);
        }
        base.Removed();
    }

    public override void Update()
    {
        if (waitToHurt > 0f)
        {
            waitToHurt -= Maths.IncFrameTimer();
        }
        else
        {
            whoWait = null;
        }
        if (!isLocal)
        {
            if (_stick != null)
            {
                UpdateStick();
            }
            else
            {
                base.Update();
            }
            return;
        }
        if (_airFireScale < 1.2f)
        {
            _airFireScale += 0.15f;
        }
        if (_grounded && _stick == null)
        {
            _airFireScale -= 0.3f;
            if (_airFireScale < 0.9f)
            {
                _airFireScale = 0.9f;
            }
            _spinSpeed -= 0.01f;
            if (_spinSpeed < 0.05f)
            {
                _spinSpeed = 0.05f;
            }
        }
        if (_grounded)
        {
            if (_groundLife <= 0)
            {
                base.Alpha -= 0.04f;
                if (base.Alpha < 0f)
                {
                    Level.Remove(this);
                }
            }
            else
            {
                _groundLife--;
            }
        }
        if (base.Y > Level.current.bottomRight.Y + 200f)
        {
            Level.Remove(this);
        }
        SpriteMap airFire = _airFire;
        float num = (_airFire.ScaleY = _airFireScale);
        airFire.ScaleX = num;
        _airFire.Depth = base.Depth - 1;
        _airFire.Alpha = 0.5f;
        _airFire.Angle += hSpeed * _spinSpeed;
        if (isLocal && _canMultiply && !_multiplied && Rando.Float(310f) < 1f && base.Y > base.level.topLeft.Y - 500f)
        {
            Level.Add(New(base.X, base.Y, -0.5f + Rando.Float(1f), 0f - (0.5f + Rando.Float(0.5f))));
            _multiplied = true;
        }
        if (_stick == null)
        {
            if (base.level != null && base.Y < base.level.topLeft.Y - 1500f)
            {
                Level.Remove(this);
            }
            base.Update();
        }
        else
        {
            _grounded = true;
            if (_stick.destroyed)
            {
                _stick = null;
                _grounded = false;
            }
            else
            {
                UpdateStick();
                stick.UpdateFirePosition(this);
                if (!_stick.onFire || _stick.removeFromLevel || _stick.Alpha < 0.01f)
                {
                    Level.Add(SmallSmoke.New(base.X, base.Y));
                    Level.Remove(this);
                }
            }
        }
        _alternateb = !_alternateb;
        if (_alternateb)
        {
            _alternate = !_alternate;
        }
    }

    public override void Draw()
    {
        base.Draw();
    }
}
