namespace DuckGame;

[EditorGroup("Guns|Fire")]
[BaggedProperty("isSuperWeapon", true)]
public class FlameThrower : Gun
{
    public StateBinding _firingBinding = new StateBinding(nameof(_firing));

    private SpriteMap _barrelFlame;

    public bool _firing;

    private new float _flameWait;

    private SpriteMap _can;

    private ConstantSound _sound = new ConstantSound("flameThrowing");

    private int _maxAmmo = 100;

    public FlameThrower(float xval, float yval)
        : base(xval, yval)
    {
        barrelInsertOffset = new Vec2(0f, -2f);
        wideBarrel = true;
        ammo = _maxAmmo;
        _ammoType = new AT9mm();
        _ammoType.combustable = true;
        _type = "gun";
        graphic = new Sprite("flamethrower");
        center = new Vec2(16f, 15f);
        collisionOffset = new Vec2(-8f, -3f);
        collisionSize = new Vec2(16f, 9f);
        _barrelOffsetTL = new Vec2(28f, 16f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        _barrelFlame = new SpriteMap("flameBurst", 20, 21);
        _barrelFlame.center = new Vec2(0f, 17f);
        _barrelFlame.AddAnimation("idle", 0.4f, true, 0, 1, 2, 3);
        _barrelFlame.AddAnimation("puff", 0.4f, false, 4, 5, 6, 7);
        _barrelFlame.AddAnimation("flame", 0.4f, true, 8, 9, 10, 11);
        _barrelFlame.AddAnimation("puffOut", 0.4f, false, 12, 13, 14, 15);
        _barrelFlame.SetAnimation("idle");
        _can = new SpriteMap("flamethrowerCan", 8, 8);
        _can.center = new Vec2(4f, 4f);
        _holdOffset = new Vec2(2f, 0f);
        _barrelAngleOffset = 8f;
        _editorName = "Flame Thrower";
        editorTooltip = "Some Ducks just want to watch the world burn.";
        _bio = "I have a problem. I want this flame here, to be over there. But I can't pick it up, it's too damn hot. If only there was some way I could throw it.";
    }

    public override void Update()
    {
        base.Update();
        if (ammo == 0)
        {
            _firing = false;
            _barrelFlame.speed = 0f;
        }
        if (_firing && _barrelFlame.currentAnimation == "idle")
        {
            _barrelFlame.SetAnimation("puff");
        }
        if (_firing && _barrelFlame.currentAnimation == "puff" && _barrelFlame.finished)
        {
            _barrelFlame.SetAnimation("flame");
        }
        if (!_firing && _barrelFlame.currentAnimation != "idle")
        {
            _barrelFlame.SetAnimation("puffOut");
        }
        if (_barrelFlame.currentAnimation == "puffOut" && _barrelFlame.finished)
        {
            _barrelFlame.SetAnimation("idle");
        }
        _sound.lerpVolume = (_firing ? 0.5f : 0f);
        if (base.isServerForObject && _firing && _barrelFlame.imageIndex > 5)
        {
            _flameWait -= 0.25f;
            if (_flameWait <= 0f)
            {
                Vec2 travelDir = Maths.AngleToVec(base.barrelAngle + Rando.Float(-0.5f, 0.5f));
                Vec2 moveSpeed = new Vec2(travelDir.x * Rando.Float(2f, 3.5f), travelDir.y * Rando.Float(2f, 3.5f));
                ammo -= 2;
                Level.Add(SmallFire.New(base.barrelPosition.x, base.barrelPosition.y, moveSpeed.x, moveSpeed.y, shortLife: false, null, canMultiply: true, this));
                _flameWait = 1f;
            }
        }
        else
        {
            _flameWait = 0f;
        }
    }

    public override void Draw()
    {
        base.Draw();
        Material obj = Graphics.material;
        Graphics.material = null;
        if (_barrelFlame.speed > 0f)
        {
            _barrelFlame.alpha = 0.9f;
            Draw(_barrelFlame, new Vec2(11f, 1f));
        }
        _can.frame = (int)((1f - (float)ammo / (float)_maxAmmo) * 15f);
        Draw(_can, new Vec2(base.barrelOffset.x - 11f, base.barrelOffset.y + 4f));
        Graphics.material = obj;
    }

    public override void OnPressAction()
    {
        if (heat > 1f)
        {
            for (int i = 0; i < ammo / 10 + 3; i++)
            {
                Level.Add(SmallFire.New(base.x - 6f + Rando.Float(12f), base.y - 8f + Rando.Float(4f), -3f + Rando.Float(6f), 1f - Rando.Float(4.5f), shortLife: false, null, canMultiply: true, this));
            }
            SFX.Play("explode", 1f, -0.3f + Rando.Float(0.3f));
            Level.Remove(this);
            _sound.Kill();
            Level.Add(new ExplosionPart(base.x, base.y));
        }
        _firing = true;
    }

    public override void OnReleaseAction()
    {
        _firing = false;
    }

    public override void Fire()
    {
    }
}
