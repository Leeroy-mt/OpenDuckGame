using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Guns|Rifles")]
public class PelletGun : Gun
{
    public StateBinding _loadStateBinding = new StateBinding(nameof(_loadState));

    public StateBinding _firesTillFailBinding = new StateBinding(nameof(firesTillFail));

    public StateBinding _aimAngleBinding = new StateBinding(nameof(_aimAngle));

    private SpriteMap _sprite;

    private Sprite _spring;

    public int _loadState = -1;

    public float _angleOffset;

    public float _angleOffset2;

    public float _aimAngle;

    public int _aimWait;

    private Vector2 _posOffset;

    public int firesTillFail = 8;

    private Vector2 springPos = Vector2.Zero;

    private Vector2 springVel = Vector2.Zero;

    private bool _rising;

    public override float Angle
    {
        get
        {
            return base.Angle - Math.Max(_aimAngle, -0.2f) * (float)offDir;
        }
        set
        {
            base.Angle = value;
        }
    }

    public PelletGun(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 2;
        _ammoType = new ATPellet();
        _type = "gun";
        _sprite = new SpriteMap("pelletGun", 31, 7);
        graphic = _sprite;
        Center = new Vector2(15f, 2f);
        collisionOffset = new Vector2(-8f, -2f);
        collisionSize = new Vector2(16f, 5f);
        _spring = new Sprite("dandiSpring");
        _barrelOffsetTL = new Vector2(30f, 2f);
        _fireSound = "pelletgun";
        _kickForce = 0f;
        _manualLoad = true;
        _holdOffset = new Vector2(-2f, -1f);
        editorTooltip = "Careful with that thing, you'll lose an eye!";
        _editorName = "Dandylion";
    }

    public override Vector2 Offset(Vector2 pos)
    {
        return Position + _posOffset + OffsetLocal(pos);
    }

    public override void Update()
    {
        if ((bool)infinite)
        {
            firesTillFail = 100;
        }
        if (firesTillFail == 1)
        {
            _fireSound = "pelletgunFail";
        }
        if (firesTillFail <= 0)
        {
            _fireSound = "pelletgunBad";
            if (!(base.ammoType is ATFailedPellet))
            {
                _ammoType = new ATFailedPellet();
            }
            Vector2 dif = Offset(new Vector2(0f, -8f)) - springPos;
            springVel += dif * 0.15f;
            springVel *= 0.9f;
            springPos += springVel;
        }
        else
        {
            springPos = Position;
        }
        _aimWait++;
        if (_aimWait > 0)
        {
            _aimAngle = Lerp.Float(_aimAngle, _rising ? 0.4f : 0f, 0.05f);
            _aimWait = 0;
        }
        if (_rising && _aimAngle > 0.345f)
        {
            OnReleaseAction();
        }
        if (base.held)
        {
            Center = new Vector2(11f, 2f);
        }
        else
        {
            Center = new Vector2(15f, 2f);
        }
        if (_loadState > -1)
        {
            if (owner == null)
            {
                if (_loadState == 3)
                {
                    loaded = true;
                }
                _loadState = -1;
                _angleOffset = 0f;
                _posOffset = Vector2.Zero;
                handOffset = Vector2.Zero;
                _aimAngle = 0f;
                _angleOffset2 = 0f;
            }
            if (_loadState > 0 && _loadState < 4)
            {
                _posOffset = Lerp.Vector2(_posOffset, new Vector2(2f, 2f), 0.2f);
            }
            else
            {
                _posOffset = Lerp.Vector2(_posOffset, new Vector2(0f, 0f), 0.24f);
            }
            if (_loadState >= 2 && _loadState < 3)
            {
                _angleOffset2 = Lerp.Float(_angleOffset2, -0.17f, 0.04f);
            }
            else
            {
                _angleOffset2 = Lerp.Float(_angleOffset2, 0f, 0.02f);
            }
            if (_loadState == 0)
            {
                if (Network.isActive)
                {
                    if (base.isServerForObject)
                    {
                        NetSoundEffect.Play("pelletGunSwipe");
                    }
                }
                else
                {
                    SFX.Play("swipe", 0.4f, 0.3f);
                }
                _loadState++;
            }
            else if (_loadState == 1)
            {
                if (_angleOffset < 0.16f)
                {
                    _angleOffset = MathHelper.Lerp(_angleOffset, 0.2f, 0.11f);
                }
                else
                {
                    _loadState++;
                }
            }
            else if (_loadState == 2)
            {
                handOffset.X += 0.31f;
                if (handOffset.X > 4f)
                {
                    _loadState++;
                    ammo = 2;
                    loaded = false;
                    if (Network.isActive)
                    {
                        if (base.isServerForObject)
                        {
                            NetSoundEffect.Play("pelletGunLoad");
                        }
                    }
                    else
                    {
                        SFX.Play("loadLow", 0.7f, Rando.Float(-0.05f, 0.05f));
                    }
                }
            }
            else if (_loadState == 3)
            {
                handOffset.X -= 0.2f;
                if (handOffset.X <= 0f)
                {
                    _loadState++;
                    handOffset.X = 0f;
                    if (Network.isActive)
                    {
                        if (base.isServerForObject)
                        {
                            NetSoundEffect.Play("pelletGunSwipe2");
                        }
                    }
                    else
                    {
                        SFX.Play("swipe", 0.5f, 0.4f);
                    }
                }
            }
            else if (_loadState == 4)
            {
                if (_angleOffset > 0.03f)
                {
                    _angleOffset = MathHelper.Lerp(_angleOffset, 0f, 0.09f);
                }
                else
                {
                    _loadState = -1;
                    loaded = true;
                    _angleOffset = 0f;
                    if (Network.isActive)
                    {
                        if (base.isServerForObject)
                        {
                            NetSoundEffect.Play("pelletGunClick");
                        }
                    }
                    else
                    {
                        SFX.Play("click", 1f, 0.5f);
                    }
                }
            }
        }
        base.Update();
    }

    public override void OnPressAction()
    {
        if (base.isServerForObject)
        {
            if (loaded && ammo > 1)
            {
                _rising = true;
                _aimAngle = -0.3f;
                _aimWait = 0;
            }
            else if (_loadState == -1)
            {
                _loadState = 0;
            }
        }
        else
        {
            base.OnPressAction();
        }
    }

    private void RunFireCode()
    {
        base.OnPressAction();
        for (int i = 0; i < 4; i++)
        {
            Level.Add(SmallSmoke.New(base.barrelPosition.X + (float)offDir * 4f, base.barrelPosition.Y));
        }
        Level.Add(SmallSmoke.New(Position.X, Position.Y));
    }

    public override void OnReleaseAction()
    {
        if (receivingPress)
        {
            RunFireCode();
        }
        else if (_rising)
        {
            if (loaded && ammo > 1)
            {
                RunFireCode();
                firesTillFail--;
                ammo = 1;
            }
            _rising = false;
        }
    }

    public override void Draw()
    {
        _sprite.Center = Center;
        _sprite.Depth = base.Depth;
        _sprite.Angle = Angle;
        _sprite.frame = 0;
        _sprite.Alpha = base.Alpha;
        if (owner != null && owner.graphic != null && (base.duck == null || !(base.duck.holdObject is TapedGun)))
        {
            _sprite.flipH = owner.graphic.flipH;
        }
        else
        {
            _sprite.flipH = offDir <= 0;
        }
        if (offDir > 0)
        {
            _sprite.Angle = Angle - _angleOffset - _angleOffset2;
        }
        else
        {
            _sprite.Angle = Angle + _angleOffset + _angleOffset2;
        }
        Vector2 p = Offset(_posOffset);
        Graphics.Draw(_sprite, p.X, p.Y);
        _sprite.frame = 1;
        if (offDir > 0)
        {
            _sprite.Angle = Angle + _angleOffset * 3f - _angleOffset2;
        }
        else
        {
            _sprite.Angle = Angle - _angleOffset * 3f + _angleOffset2;
        }
        Graphics.Draw(_sprite, p.X, p.Y);
        if (firesTillFail <= 0)
        {
            _spring.Depth = base.Depth - 5;
            _spring.Center = new Vector2(4f, 7f);
            _spring.AngleDegrees = Maths.PointDirection(Position + _posOffset, springPos) - 90f;
            _spring.ScaleY = (Position.Y + _posOffset.Y - springPos.Y) / 8f;
            _spring.flipH = offDir < 0;
            if (_spring.ScaleY > 1.2f)
            {
                _spring.ScaleY = 1.2f;
            }
            if (_spring.ScaleY < -1.2f)
            {
                _spring.ScaleY = -1.2f;
            }
            _spring.Alpha = base.Alpha;
            Graphics.Draw(_spring, p.X, p.Y);
        }
    }
}
