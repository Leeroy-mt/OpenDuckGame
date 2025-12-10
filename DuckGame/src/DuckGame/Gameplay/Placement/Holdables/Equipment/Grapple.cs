using System;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("previewPriority", true)]
public class Grapple : Equipment, ISwing
{
    public StateBinding _ropeDataBinding = new DataBinding("ropeData");

    public BitBuffer ropeData = new BitBuffer();

    protected SpriteMap _sprite;

    public Harpoon _harpoon;

    public Rope _rope;

    protected Vec2 _barrelOffsetTL;

    private float _grappleLength = 200f;

    private Tex2D _laserTex;

    public Sprite _ropeSprite;

    protected Vec2 _wallPoint;

    private Vec2 _lastHit = Vec2.Zero;

    protected Vec2 _grappleTravel;

    protected Sprite _sightHit;

    private float _grappleDist;

    private bool _canGrab;

    private int _lagFrames;

    public Vec2 barrelPosition => Offset(barrelOffset);

    public Vec2 barrelOffset => _barrelOffsetTL - center;

    public bool hookInGun => _harpoon.inGun;

    public Vec2 wallPoint => _wallPoint;

    public Vec2 grappelTravel => _grappleTravel;

    public Grapple(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("grappleArm", 16, 16);
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-5f, -4f);
        collisionSize = new Vec2(11f, 7f);
        _offset = new Vec2(0f, 7f);
        _equippedDepth = 12;
        _barrelOffsetTL = new Vec2(10f, 4f);
        _jumpMod = true;
        thickness = 0.1f;
        _laserTex = Content.Load<Tex2D>("pointerLaser");
        editorTooltip = "Allows you to swing from platforms like some kind of loon.";
    }

    public override void OnTeleport()
    {
        Degrapple();
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
        _sightHit = new Sprite("laserSightHit");
        _sightHit.CenterOrigin();
        _ropeSprite = new Sprite("grappleWire");
        _ropeSprite.center = new Vec2(8f, 0f);
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

    public void SerializeRope(Rope r)
    {
        if (r != null)
        {
            ropeData.Write(val: true);
            ropeData.Write(CompressedVec2Binding.GetCompressedVec2(r.attach2Point));
            SerializeRope(r.attach2 as Rope);
        }
        else
        {
            ropeData.Write(val: false);
        }
    }

    public void DeserializeRope(Rope r)
    {
        if (ropeData.ReadBool())
        {
            if (r == null)
            {
                _rope = new Rope(0f, 0f, r, null, null, vine: false, _ropeSprite, this);
                r = _rope;
            }
            r.attach1 = r;
            r._thing = null;
            Level.Add(r);
            Vec2 pos = CompressedVec2Binding.GetUncompressedVec2(ropeData.ReadInt());
            if (r == _rope)
            {
                r.attach1 = r;
                if (base.duck != null)
                {
                    r.position = base.duck.position;
                }
                else
                {
                    r.position = position;
                }
                r._thing = base.duck;
            }
            if (r.attach2 == null || !(r.attach2 is Rope) || r.attach2 == r)
            {
                Rope nextRope = new Rope(pos.x, pos.y, r, null, null, vine: false, _ropeSprite, this);
                r.attach2 = nextRope;
            }
            if (r.attach2 != null)
            {
                r.attach2.position = pos;
                (r.attach2 as Rope).attach1 = r;
            }
            DeserializeRope(r.attach2 as Rope);
        }
        else if (r == _rope)
        {
            Degrapple();
        }
        else if (r != null)
        {
            Rope obj = r.attach1 as Rope;
            obj.TerminateLaterRopes();
            _harpoon.Latch(r.position);
            obj.attach2 = _harpoon;
        }
    }

    public void Degrapple()
    {
        _harpoon.Return();
        if (_rope != null)
        {
            _rope.RemoveRope();
        }
        if (_rope != null && base.duck != null)
        {
            base.duck.frictionMult = 1f;
            base.duck.gravMultiplier = 1f;
            base.duck._double = false;
            if (base.duck.vSpeed < 0f && base.duck.framesSinceJump > 3)
            {
                base.duck.vSpeed *= 1.75f;
            }
            if (base.duck.vSpeed >= base.duck.jumpSpeed * 0.95f && Math.Abs(base.duck.vSpeed) + Math.Abs(base.duck.hSpeed) < 2f)
            {
                SFX.Play("jump", 0.5f);
                base.duck.vSpeed = base.duck.jumpSpeed;
            }
        }
        _rope = null;
        frictionMult = 1f;
        gravMultiplier = 1f;
    }

    public override void Update()
    {
        if (_harpoon == null)
        {
            return;
        }
        if (base.isServerForObject)
        {
            ropeData.Clear();
            SerializeRope(_rope);
        }
        else
        {
            ropeData.SeekToStart();
            DeserializeRope(_rope);
        }
        if (_rope != null)
        {
            _rope.SetServer(base.isServerForObject);
        }
        if (base.isServerForObject && _equippedDuck != null && base.duck != null)
        {
            if (base.duck._trapped != null)
            {
                Degrapple();
            }
            ATTracer tracer = new ATTracer();
            float dist = (tracer.range = _grappleLength);
            tracer.penetration = 1f;
            float a = 45f;
            if (offDir < 0)
            {
                a = 135f;
            }
            if (_harpoon.inGun)
            {
                Vec2 pos = Offset(barrelOffset);
                if (_lagFrames > 0)
                {
                    _lagFrames--;
                    if (_lagFrames == 0)
                    {
                        _canGrab = false;
                    }
                    else
                    {
                        a = Maths.PointDirection(pos, _lastHit);
                    }
                }
                tracer.penetration = 9f;
                Bullet b = new Bullet(pos.x, pos.y, tracer, a, owner, rbound: false, -1f, tracer: true);
                _wallPoint = b.end;
                _grappleTravel = b.travelDirNormalized;
                dist = (pos - _wallPoint).length;
            }
            if (dist < _grappleLength - 2f && !(dist > _grappleDist + 16f))
            {
                _lastHit = _wallPoint;
                _canGrab = true;
            }
            else if (_canGrab && _lagFrames == 0)
            {
                _lagFrames = 6;
                _wallPoint = _lastHit;
            }
            else
            {
                _canGrab = false;
            }
            _grappleDist = dist;
            if (base.duck.inputProfile.Pressed("JUMP") && base.duck._trapped == null)
            {
                if (_harpoon.inGun)
                {
                    if (!base.duck.grounded && base.duck.framesSinceJump > 6 && _canGrab && (!(base.duck.holdObject is TV) || (base.duck.holdObject as TV)._ruined || !(base.duck.holdObject as TV).channel || !base.duck._double || base.duck._groundValid <= 0))
                    {
                        RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
                        _harpoon.Fire(wallPoint, grappelTravel);
                        _rope = new Rope(barrelPosition.x, barrelPosition.y, null, _harpoon, base.duck, vine: false, _ropeSprite, this);
                        Level.Add(_rope);
                    }
                }
                else
                {
                    Degrapple();
                    _lagFrames = 0;
                    _canGrab = false;
                }
            }
        }
        base.Update();
        if (owner != null)
        {
            offDir = owner.offDir;
        }
        if (base.duck != null)
        {
            base.duck.grappleMul = false;
        }
        if (!base.isServerForObject || _rope == null)
        {
            return;
        }
        if (owner != null)
        {
            _rope.position = owner.position;
        }
        else
        {
            _rope.position = position;
            if (base.prevOwner != null)
            {
                PhysicsObject obj = base.prevOwner as PhysicsObject;
                obj.frictionMult = 1f;
                obj.gravMultiplier = 1f;
                _prevOwner = null;
                frictionMult = 1f;
                gravMultiplier = 1f;
                if (base.prevOwner is Duck)
                {
                    (base.prevOwner as Duck).grappleMul = false;
                }
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
            if (_rope.properLength > 0f)
            {
                if (base.duck.inputProfile.Down("UP") && _rope.properLength >= 16f)
                {
                    _rope.properLength -= 2f;
                }
                if (base.duck.inputProfile.Down("DOWN") && _rope.properLength <= 256f)
                {
                    _rope.properLength += 2f;
                }
                _rope.properLength = Maths.Clamp(_rope.properLength, 16f, 256f);
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
        Vec2 travel = _rope.attach1.position - _rope.attach2.position;
        if (_rope.properLength < 0f)
        {
            _rope.properLength = (_rope.startLength = travel.length);
        }
        if (!(travel.length > _rope.properLength))
        {
            return;
        }
        travel = travel.normalized;
        if (base.duck != null)
        {
            base.duck.grappleMul = true;
            PhysicsObject attach = base.duck;
            if (base.duck.ragdoll != null)
            {
                Degrapple();
                return;
            }
            _ = attach.position;
            attach.position = _rope.attach2.position + travel * _rope.properLength;
            Vec2 dif = attach.position - attach.lastPosition;
            attach.hSpeed = dif.x;
            attach.vSpeed = dif.y;
        }
        else
        {
            _ = position;
            position = _rope.attach2.position + travel * _rope.properLength;
            Vec2 dif2 = position - base.lastPosition;
            hSpeed = dif2.x;
            vSpeed = dif2.y;
        }
    }

    public override void Draw()
    {
        if (_equippedDuck == null)
        {
            base.Draw();
        }
        else if (_autoOffset)
        {
            Vec2 off = _offset;
            if (_equippedDuck.offDir < 0)
            {
                off.x *= -1f;
            }
            Vec2 handOffset = Vec2.Zero;
            if (_equippedDuck.holdObject != null)
            {
                handOffset = _equippedDuck.holdObject.handOffset;
                handOffset.x *= _equippedDuck.offDir;
            }
            Vec2 pos = _equippedDuck.armPosition + handOffset;
            position = pos;
        }
        if (!Options.Data.fireGlow)
        {
            DrawGlow();
        }
    }

    public override void DrawGlow()
    {
        if (_equippedDuck != null && _harpoon != null && _harpoon.inGun && _canGrab && _equippedDuck._trapped == null)
        {
            Graphics.DrawTexturedLine(_laserTex, Offset(barrelOffset), _wallPoint, Color.Red, 0.5f, base.depth - 1);
            if (_sightHit != null)
            {
                _sightHit.color = Color.Red;
                Graphics.Draw(_sightHit, _wallPoint.x, _wallPoint.y);
            }
        }
        base.DrawGlow();
    }
}
