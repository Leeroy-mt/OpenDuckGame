using Microsoft.Xna.Framework;

namespace DuckGame;

public class Dart : PhysicsObject, IPlatform
{
    public StateBinding _stickTimeBinding = new StateBinding(nameof(_stickTime));

    public StateBinding _stuckBinding = new StateBinding(nameof(_stuck));

    private SpriteMap _sprite;

    public bool _stuck;

    public float _stickTime = 1f;

    private new Duck _owner;

    public bool burning;

    public Dart(float xpos, float ypos, Duck owner, float fireAngle)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("dart", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-4f, -2f);
        collisionSize = new Vector2(9f, 4f);
        base.Depth = -0.5f;
        thickness = 1f;
        weight = 3f;
        _owner = owner;
        base.breakForce = 1f;
        _stickTime = 2f + Rando.Float(0.8f);
        if (Rando.Float(1f) > 0.95f)
        {
            _stickTime += Rando.Float(15f);
        }
        Angle = fireAngle;
        if (owner != null)
        {
            owner.clip.Add(this);
            base.clip.Add(owner);
        }
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (_stuck && _stickTime > 0.98f)
        {
            return false;
        }
        if (type is DTFade)
        {
            DartShell obj = new DartShell(base.X, base.Y, Rando.Float(0.1f) * (0f - _sprite.flipMultH), _sprite.flipH)
            {
                Angle = Angle
            };
            Level.Add(obj);
            obj.hSpeed = (0.5f + Rando.Float(0.3f)) * (0f - _sprite.flipMultH);
            Level.Remove(this);
            return true;
        }
        if (_stuck && _stickTime > 0.4f)
        {
            _stickTime = 0.4f;
        }
        return false;
    }

    public override void OnImpact(MaterialThing with, ImpactedFrom from)
    {
        if (_stuck || with is Gun || (with.weight < 5f && !(with is Dart) && !(with is RagdollPart)) || with is FeatherVolume || with is Teleporter || base.removeFromLevel || with is Spring || with is SpringUpLeft || with is SpringUpRight)
        {
            if (with is EnergyBlocker && with.solid)
            {
                LightOnFire();
            }
        }
        else
        {
            if (destroyed || _stuck)
            {
                return;
            }
            if (with is PhysicsObject)
            {
                Duck duck = with as Duck;
                if (duck != null && base.isServerForObject)
                {
                    if (duck.isServerForObject)
                    {
                        duck.hSpeed += hSpeed * 0.7f;
                        duck.vSpeed -= 1.5f;
                        Event.Log(new DartHitEvent(base.responsibleProfile, duck.profile));
                        if (duck.holdObject is Grenade)
                        {
                            duck.forceFire = true;
                        }
                        if (Rando.Float(1f) > 0.6f)
                        {
                            duck.Swear();
                        }
                        duck.Disarm(this);
                    }
                    else
                    {
                        Send.Message(new NMDartSmack(new Vector2(hSpeed * 0.7f, -1.5f), duck), duck.connection);
                    }
                }
                RagdollPart r = with as RagdollPart;
                if (r != null && base.isServerForObject && r.doll != null && r.doll.captureDuck != null)
                {
                    Duck d = r.doll.captureDuck;
                    if (r.isServerForObject)
                    {
                        r.hSpeed += hSpeed * 0.7f;
                        r.vSpeed -= 1.5f;
                        if (d.holdObject is Grenade)
                        {
                            d.forceFire = true;
                        }
                        if (Rando.Float(1f) > 0.6f)
                        {
                            d.Swear();
                        }
                        d.Disarm(this);
                    }
                    else
                    {
                        Send.Message(new NMDartSmack(new Vector2(hSpeed * 0.7f, -1.5f), r), r.connection);
                    }
                }
                if (with is IPlatform || duck != null || r != null)
                {
                    DartShell dartShell = new DartShell(base.X, base.Y, (0f - _sprite.flipMultH) * Rando.Float(0.6f), _sprite.flipH);
                    Level.Add(dartShell);
                    dartShell.hSpeed = (0f - hSpeed) / 3f * (0.3f + Rando.Float(0.8f));
                    dartShell.vSpeed = -2f + Rando.Float(4f);
                    Level.Remove(this);
                    if (burning)
                    {
                        with.Burn(Position, this);
                    }
                    return;
                }
            }
            float deg = (0f - base.AngleDegrees) % 360f;
            if (deg < 0f)
            {
                deg += 360f;
            }
            bool stick = false;
            if ((with is Block || with is Spikes || with is Saws) && from == ImpactedFrom.Right && (deg < 45f || deg > 315f))
            {
                stick = true;
                base.AngleDegrees = 0f;
            }
            else if ((with is Block || with is Spikes || with is Saws) && from == ImpactedFrom.Top && deg > 45f && deg < 135f)
            {
                stick = true;
                base.AngleDegrees = 270f;
            }
            else if ((with is Block || with is Spikes || with is Saws) && from == ImpactedFrom.Left && deg > 135f && deg < 225f)
            {
                stick = true;
                base.AngleDegrees = 180f;
            }
            else if (from == ImpactedFrom.Bottom && deg > 225f && deg < 315f)
            {
                stick = true;
                base.AngleDegrees = 90f;
            }
            if (stick)
            {
                _stuck = true;
                SFX.Play("dartStick", 0.8f, -0.1f + Rando.Float(0.2f));
                vSpeed = 0f;
                gravMultiplier = 0f;
                base.grounded = true;
                _sprite.frame = 1;
                _stickTime = 1f;
            }
        }
    }

    public void LightOnFire()
    {
        if (!burning)
        {
            burning = true;
            base.onFire = true;
            Level.Add(SmallFire.New(0f, 0f, 0f, 0f, shortLife: false, this, canMultiply: true, this));
            SFX.Play("ignite", Rando.Float(0.9f, 1f), Rando.Float(-0.2f, 0.2f));
        }
    }

    public override void Update()
    {
        base.Update();
        if (!destroyed && !_stuck)
        {
            if (!burning && Level.CheckCircle<SmallFire>(Position, 8f) != null)
            {
                LightOnFire();
            }
            _sprite.frame = 0;
            base.AngleDegrees = 0f - Maths.PointDirection(Vector2.Zero, new Vector2(hSpeed, vSpeed));
        }
        if (_stuck)
        {
            vSpeed = 0f;
            hSpeed = 0f;
            base.grounded = true;
            _sprite.frame = 1;
            _stickTime -= 0.005f;
            gravMultiplier = 0f;
        }
        if (_stickTime <= 0f && !destroyed)
        {
            Destroy(new DTFade());
        }
    }
}
