namespace DuckGame;

[EditorGroup("Guns|Fire")]
public class RomanCandle : FlareGun
{
    public StateBinding _litBinding = new StateBinding(nameof(_lit));

    private SpriteMap _sprite;

    private bool _lit;

    private int _flip = 1;

    private ActionTimer _timer = 0.5f;

    private ActionTimer _litTimer;

    private ActionTimer _litStartTimer;

    private Sound _burnSound;

    public RomanCandle(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 9;
        _type = "gun";
        _sprite = new SpriteMap("romanCandle", 16, 16);
        graphic = _sprite;
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -4f);
        collisionSize = new Vec2(16f, 6f);
        _barrelOffsetTL = new Vec2(16f, 9f);
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        flammable = 1f;
        _bio = "FOOM";
        _editorName = "Roman Candle";
        editorTooltip = "Fireworks, but even more dangerous! Light the fuse and cross your fingers.";
        physicsMaterial = PhysicsMaterial.Paper;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        if (_sprite == null)
        {
            return;
        }
        Vec2 fusePos = Offset(new Vec2(-6f, -4f));
        if (_lit && (bool)_timer)
        {
            Level.Add(Spark.New(fusePos.X, fusePos.Y, new Vec2(Rando.Float(-1f, 1f), -0.5f), 0.1f));
        }
        if (_lit && _litTimer != null && (bool)_litTimer && _litStartTimer != null && (bool)_litStartTimer)
        {
            if (_sprite.frame == 0)
            {
                _sprite.frame = 1;
            }
            if (owner == null)
            {
                base.Y -= 6f;
            }
            ammo--;
            SFX.Play("netGunFire", 0.5f, -0.4f + Rando.Float(0.2f));
            kick = 1f;
            if (base.duck != null)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
            }
            if (base.isServerForObject)
            {
                Vec2 pos = Offset(base.barrelOffset);
                CandleBall d = new CandleBall(pos.X, pos.Y, this, 4);
                Fondle(d);
                Vec2 travelDir = Maths.AngleToVec(base.barrelAngle + Rando.Float(-0.1f, 0.1f));
                d.hSpeed = travelDir.X * 14f;
                d.vSpeed = travelDir.Y * 14f;
                Level.Add(d);
            }
            if (owner == null)
            {
                hSpeed -= (float)_flip * Rando.Float(1f, 5f);
                vSpeed -= Rando.Float(1f, 7f);
                if (_flip > 0)
                {
                    _flip = -1;
                }
                else
                {
                    _flip = 1;
                }
            }
            offDir = (sbyte)_flip;
        }
        if (ammo <= 0)
        {
            _lit = false;
            _sprite.frame = 2;
            if (_burnSound != null)
            {
                _burnSound.Stop();
                _burnSound = null;
            }
        }
        base.Update();
        if (owner != null)
        {
            _flip = owner.offDir;
        }
        else
        {
            graphic.flipH = _flip < 0;
        }
    }

    public override void Terminate()
    {
        if (_burnSound != null)
        {
            _burnSound.Kill();
        }
        base.Terminate();
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        Light();
        return true;
    }

    public override void Draw()
    {
        base.Draw();
    }

    public void Light()
    {
        if (!_lit && ammo > 0)
        {
            _lit = true;
            _litTimer = 0.03f;
            _litStartTimer = new ActionTimer(0.01f, 1f, reset: false);
            _burnSound = SFX.Play("fuseBurn", 0.5f, 0f, 0f, looped: true);
        }
    }

    public override void OnPressAction()
    {
        Light();
    }

    public override void Fire()
    {
    }
}
