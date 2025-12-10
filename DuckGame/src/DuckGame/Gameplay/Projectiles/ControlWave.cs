using System;

namespace DuckGame;

public class ControlWave : Thing, ITeleport
{
    private new MindControlRay _owner;

    private float _fade = 1f;

    private bool _isNotControlRay;

    private bool _isLocalWave = true;

    public ControlWave(float xpos, float ypos, float dir, MindControlRay owner, bool local = true)
        : base(xpos, ypos)
    {
        _owner = owner;
        graphic = new Sprite("controlWave");
        graphic.flipH = offDir < 0;
        center = new Vec2(8f, 8f);
        base.xscale = (base.yscale = 0.2f);
        angle = dir;
        _isLocalWave = local;
    }

    public override void Update()
    {
        if (base.isServerForObject)
        {
            float num = (base.yscale = Maths.CountUp(base.yscale, 0.05f));
            base.xscale = num;
            _fade -= 0.05f;
            if (_fade < 0f)
            {
                Level.Remove(this);
            }
            base.alpha = Maths.NormalizeSection(_fade, 0.2f, 0.3f);
            Vec2 dir = Vec2.Zero;
            if (_owner.controlledDuck == null && !_isNotControlRay)
            {
                dir = new Vec2((float)Math.Cos(angle), (float)(0.0 - Math.Sin(angle)));
                if (_isLocalWave)
                {
                    foreach (IAmADuck d in Level.CheckCircleAll<IAmADuck>(position, 3f))
                    {
                        Duck trueDuck = d as Duck;
                        if (!(d is Duck))
                        {
                            if (d is RagdollPart && (d as RagdollPart).doll.captureDuck != null)
                            {
                                trueDuck = (d as RagdollPart).doll.captureDuck;
                            }
                            else if (d is TrappedDuck && (d as TrappedDuck).captureDuck != null)
                            {
                                trueDuck = (d as TrappedDuck).captureDuck;
                            }
                        }
                        if (trueDuck != null && trueDuck.mindControl == null && !trueDuck.HasEquipment(typeof(TinfoilHat)) && !(trueDuck.holdObject is MindControlRay))
                        {
                            _owner.ControlDuck(trueDuck);
                        }
                    }
                }
            }
            else
            {
                if (_owner.controlledDuck != null)
                {
                    dir = _owner.controlledDuck.cameraPosition - position;
                    dir.Normalize();
                    base.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, dir);
                }
                _isNotControlRay = true;
            }
            position += dir * 2.6f;
        }
        else
        {
            float num = (base.yscale = 1f);
            base.xscale = num;
            Vec2 dir2 = new Vec2((float)Math.Cos(angle), (float)(0.0 - Math.Sin(angle)));
            position += dir2 * 2.6f;
        }
    }
}
