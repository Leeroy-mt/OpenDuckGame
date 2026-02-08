using Microsoft.Xna.Framework;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class BoxingGloves : Gun
{
    private float _swing;

    private float _hold;

    public override float Angle
    {
        get
        {
            return base.Angle + (_swing + _hold) * (float)offDir;
        }
        set
        {
            AngleValue = value;
        }
    }

    public BoxingGloves(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        graphic = new Sprite("boxingGlove");
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-4f, -4f);
        collisionSize = new Vector2(8f, 8f);
        _barrelOffsetTL = new Vector2(16f, 7f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 3f;
        _holdOffset = new Vector2(-4f, 4f);
        weight = 0.9f;
        physicsMaterial = PhysicsMaterial.Paper;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        SFX.Play("ting");
        return base.Hit(bullet, hitPos);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void OnPressAction()
    {
    }

    public override void Fire()
    {
    }
}
