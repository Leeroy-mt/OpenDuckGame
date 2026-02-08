using Microsoft.Xna.Framework;

namespace DuckGame;

public class RoomDefenceTurret : Gun
{
    public StateBinding _targetBinding = new StateBinding(nameof(_target));

    public StateBinding _friendlyBinding = new StateBinding(nameof(_friendly));

    private Duck _target;

    public Duck _friendly;

    private float _charge;

    private Sprite _base;

    public RoomDefenceTurret(Vector2 pPosition, Duck pOwner)
        : base(pPosition.X, pPosition.Y)
    {
        graphic = new Sprite("turretHead");
        Center = new Vector2(7f, 4f);
        collisionSize = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-4f, -4f);
        _friendly = pOwner;
        canPickUp = false;
        enablePhysics = false;
        _base = new Sprite("turretBase");
        _base.CenterOrigin();
        _barrelOffsetTL = new Vector2(12f, 5f);
        _ammoType = new ATDefenceLaser();
        _fireSound = "phaserSmall";
        _fireSoundPitch = 0.4f;
        weight = 10f;
        _kickForce = 0f;
        ammo = 99;
    }

    public override void Update()
    {
        if (_friendly != null)
        {
            ammo = 99;
            if (base.isServerForObject && _target == null)
            {
                Duck d = Duck.GetAssociatedDuck(base.level.NearestThingFilter<IAmADuck>(Position, (Thing x) => (_friendly == null || Duck.GetAssociatedDuck(x) != _friendly) && Level.CheckLine<Block>(Position, x.Position) == null && Level.CheckLine<TeamBeam>(Position, x.Position) == null) as PhysicsObject);
                if (d != null && !d.dead)
                {
                    _target = d;
                    _charge = 1f;
                    SFX.PlaySynchronized("chaingunSpinUp", 0.95f, 0.1f);
                }
            }
            if (_target != null)
            {
                if (base.isServerForObject && (Level.CheckLine<Block>(Position, _target.cameraPosition) != null || Level.CheckLine<TeamBeam>(Position, _target.cameraPosition) != null))
                {
                    LoseTarget();
                }
                else
                {
                    base.AngleDegrees = 0f - Maths.PointDirection(Position, _target.cameraPosition);
                    if (offDir < 0)
                    {
                        base.AngleDegrees += 180f;
                    }
                    if (base.isServerForObject)
                    {
                        _charge += Maths.IncFrameTimer();
                        if (_charge > 0.2f)
                        {
                            owner = _friendly;
                            _charge = 0f;
                            Fire();
                            owner = null;
                            enablePhysics = false;
                        }
                        Duck d2 = Duck.GetAssociatedDuck(_target);
                        if (d2 != null && d2.dead)
                        {
                            LoseTarget();
                        }
                    }
                }
            }
            else
            {
                base.AngleDegrees = 10 * offDir;
            }
        }
        base.Update();
    }

    private void LoseTarget()
    {
        _target = null;
        if (_charge > 0f)
        {
            SFX.PlaySynchronized("chaingunSpinDown", 0.95f, 0.1f);
        }
        _charge = 0f;
    }

    public override void Draw()
    {
        Vector2 kickVector = Position + -base.barrelVector * (kick * 4f);
        graphic.AngleDegrees = base.AngleDegrees;
        graphic.Center = Center;
        graphic.flipH = offDir < 0;
        Graphics.Draw(graphic, kickVector.X, kickVector.Y, base.Depth);
        _base.Depth = base.Depth - 10;
        Graphics.Draw(_base, Position.X, Position.Y - 6f);
    }
}
