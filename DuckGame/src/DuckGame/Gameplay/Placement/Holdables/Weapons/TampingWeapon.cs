using System;

namespace DuckGame;

public class TampingWeapon : Gun
{
    public StateBinding _tampedBinding = new TampingFlagBinding();

    public StateBinding _tampIncBinding = new StateBinding(nameof(_tampInc));

    public StateBinding _tampTimeBinding = new StateBinding(nameof(_tampTime));

    public StateBinding _offsetYBinding = new StateBinding(nameof(_offsetY));

    public StateBinding _rotAngleBinding = new StateBinding(nameof(_rotAngle));

    public bool _tamped = true;

    public float _tampInc;

    public float _tampTime;

    public bool _rotating;

    public float _offsetY;

    public float _rotAngle;

    public float _tampBoost = 1f;

    private Sprite _tampingHand;

    private bool _puffed;

    private Duck _prevDuckOwner;

    public override float angle
    {
        get
        {
            return base.angle + Maths.DegToRad(0f - _rotAngle);
        }
        set
        {
            _angle = value;
        }
    }

    public TampingWeapon(float xval, float yval)
        : base(xval, yval)
    {
        _tampingHand = new Sprite("tampingHand");
        _tampingHand.center = new Vec2(4f, 8f);
    }

    public override void Update()
    {
        base.Update();
        _tampBoost = Lerp.Float(_tampBoost, 1f, 0.01f);
        if (owner is Duck { inputProfile: not null } duckOwner && base.duck != null && base.duck.profile != null)
        {
            _prevDuckOwner = duckOwner;
            if (duckOwner.inputProfile.Pressed("SHOOT"))
            {
                _tampBoost += 0.14f;
            }
            if (base.duck.immobilized)
            {
                base.duck.profile.stats.timeSpentReloadingOldTimeyWeapons += Maths.IncFrameTimer();
            }
            if (_rotating)
            {
                if (offDir < 0)
                {
                    if (_rotAngle > -90f)
                    {
                        _rotAngle -= 3f;
                    }
                    if (_rotAngle <= -90f)
                    {
                        tamping = true;
                        _tampInc += 0.2f * _tampBoost;
                        tampPos = (float)Math.Sin(_tampInc) * 2f;
                        if (tampPos < -1f && !_puffed)
                        {
                            Vec2 pos = Offset(base.barrelOffset) - base.barrelVector * 8f;
                            Level.Add(SmallSmoke.New(pos.x, pos.y));
                            _puffed = true;
                        }
                        if (tampPos > -1f)
                        {
                            _puffed = false;
                        }
                        _tampTime += 0.005f * _tampBoost;
                    }
                    if ((double)_tampTime >= 1.0)
                    {
                        _rotAngle += 8f;
                        if (_offsetY > 0f)
                        {
                            _offsetY -= 2f;
                        }
                        tamping = false;
                        if (_rotAngle >= 0f)
                        {
                            _rotAngle = 0f;
                            _rotating = false;
                            _tamped = true;
                            _offsetY = 0f;
                            duckOwner.immobilized = false;
                        }
                    }
                }
                else
                {
                    if (_rotAngle < 90f)
                    {
                        _rotAngle += 3f;
                    }
                    if (_rotAngle >= 90f)
                    {
                        tamping = true;
                        _tampInc += 0.2f * _tampBoost;
                        tampPos = (float)Math.Sin(_tampInc) * 2f;
                        if (tampPos < -1f && !_puffed)
                        {
                            Vec2 pos2 = Offset(base.barrelOffset) - base.barrelVector * 8f;
                            Level.Add(SmallSmoke.New(pos2.x, pos2.y));
                            _puffed = true;
                        }
                        if (tampPos > -1f)
                        {
                            _puffed = false;
                        }
                        _tampTime += 0.005f * _tampBoost;
                    }
                    if ((double)_tampTime >= 1.0)
                    {
                        _rotAngle -= 8f;
                        if (_offsetY > 0f)
                        {
                            _offsetY -= 2f;
                        }
                        tamping = false;
                        if (_rotAngle <= 0f)
                        {
                            _rotAngle = 0f;
                            _rotating = false;
                            _tamped = true;
                            _offsetY = 0f;
                            duckOwner.immobilized = false;
                        }
                    }
                }
                if (_offsetY < 10f)
                {
                    _offsetY += 1f;
                }
            }
            else
            {
                _tampBoost = 1f;
            }
        }
        else if (_prevDuckOwner != null)
        {
            _prevDuckOwner.immobilized = false;
            tamping = false;
            _rotAngle = 0f;
            _rotating = false;
            _offsetY = 0f;
            _prevDuckOwner = null;
        }
    }

    public override void Draw()
    {
        base.y += _offsetY;
        base.Draw();
        if (base.duck != null && tamping)
        {
            if (offDir < 0)
            {
                _tampingHand.x = base.x + 3f;
                _tampingHand.y = base.y - 16f + tampPos;
                _tampingHand.flipH = true;
            }
            else
            {
                _tampingHand.x = base.x - 3f;
                _tampingHand.y = base.y - 16f + tampPos;
                _tampingHand.flipH = false;
            }
            _tampingHand.depth = base.depth - 1;
            float rot = base.duck._spriteArms.angle;
            Vec2 vec = Offset(base.barrelOffset);
            Vec2 end = vec + base.barrelVector * (tampPos * 2f + 3f);
            Graphics.DrawLine(vec - base.barrelVector * 6f, end, Color.Gray, 1f, base.depth - 2);
            base.duck._spriteArms.depth = base.depth - 1;
            Graphics.Draw(base.duck._spriteArms, end.x, end.y);
            base.duck._spriteArms.angle = rot;
        }
        position = new Vec2(position.x, position.y - _offsetY);
    }
}
