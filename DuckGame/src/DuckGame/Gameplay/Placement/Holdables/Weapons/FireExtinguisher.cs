namespace DuckGame;

[EditorGroup("Guns|Fire")]
[BaggedProperty("isFatal", false)]
public class FireExtinguisher : Gun
{
    public StateBinding _firingBinding = new StateBinding(nameof(_firing));

    private SpriteMap _guage;

    public bool _firing;

    private bool _smoke = true;

    private ConstantSound _sound = new ConstantSound("flameThrowing");

    private int _maxAmmo = 200;

    public FireExtinguisher(float xval, float yval)
        : base(xval, yval)
    {
        ammo = _maxAmmo;
        _type = "gun";
        graphic = new Sprite("extinguisher");
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-3f, -8f);
        collisionSize = new Vec2(6f, 16f);
        _barrelOffsetTL = new Vec2(15f, 2f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        _guage = new SpriteMap("netGunGuage", 8, 8);
        _holdOffset = new Vec2(0f, 2f);
        if (Network.isActive)
        {
            ammo = 120;
        }
        isFatal = false;
        editorTooltip = "Safety first! Extinguishes fires but also just makes everything fun and foamy.";
    }

    public override void Update()
    {
        base.Update();
        if (base.isServerForObject && _firing && ammo > 0)
        {
            if (_smoke)
            {
                Vec2 travelDir = Maths.AngleToVec(base.barrelAngle + Rando.Float(-0.5f, 0.5f));
                Vec2 moveSpeed = new Vec2(travelDir.x * Rando.Float(0.9f, 3f), travelDir.y * Rando.Float(0.9f, 3f));
                ExtinguisherSmoke thing = new ExtinguisherSmoke(base.barrelPosition.x, base.barrelPosition.y)
                {
                    hSpeed = moveSpeed.x,
                    vSpeed = moveSpeed.y
                };
                ammo--;
                _guage.frame = 3 - (int)((float)ammo / (float)_maxAmmo * 4f);
                Level.Add(thing);
            }
            _smoke = !_smoke;
        }
        else
        {
            _smoke = true;
        }
        _sound.lerpVolume = (_firing ? 0.5f : 0f);
    }

    public override void Draw()
    {
        base.Draw();
        _guage.flipH = graphic.flipH;
        _guage.alpha = graphic.alpha;
        _guage.depth = base.depth + 1;
        Draw(_guage, new Vec2(-6f, -8f));
    }

    public override void OnPressAction()
    {
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
