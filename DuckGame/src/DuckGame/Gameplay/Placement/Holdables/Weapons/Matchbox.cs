namespace DuckGame;

[EditorGroup("Guns|Fire")]
[BaggedProperty("isOnlineCapable", true)]
[BaggedProperty("isFatal", false)]
public class Matchbox : Gun
{
    public Matchbox(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 20;
        _type = "gun";
        graphic = new Sprite("matchbox");
        Center = new Vec2(8f, 14f);
        collisionOffset = new Vec2(-6f, -3f);
        collisionSize = new Vec2(12f, 5f);
        _barrelOffsetTL = new Vec2(15f, 6f);
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        flammable = 1f;
        editorTooltip = "A box full of fire sticks. Keep away from children.";
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        base.Update();
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        if (base.isServerForObject && ammo > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                Level.Add(SmallFire.New(base.X - 6f + Rando.Float(12f), base.Y - 8f + Rando.Float(4f), -2f + Rando.Float(4f), 0f - (1f + Rando.Float(2f)), shortLife: false, null, canMultiply: true, this));
            }
            SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
            if (owner is Duck duckOwner)
            {
                duckOwner.ThrowItem();
            }
            ammo = 0;
        }
        return true;
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void OnPressAction()
    {
        if (ammo > 0)
        {
            if (owner is Duck duckOwner)
            {
                ammo--;
                SFX.Play("lightMatch", 0.5f, -0.4f + Rando.Float(0.2f));
                float addSpeedX = 0f;
                float addSpeedY = 0f;
                if (duckOwner.inputProfile.Down("LEFT"))
                {
                    addSpeedX -= 1f;
                }
                if (duckOwner.inputProfile.Down("RIGHT"))
                {
                    addSpeedX += 1f;
                }
                if (duckOwner.inputProfile.Down("UP"))
                {
                    addSpeedY -= 1f;
                }
                if (duckOwner.inputProfile.Down("DOWN"))
                {
                    addSpeedY += 1f;
                }
                if (!receivingPress && base.isServerForObject)
                {
                    if (duckOwner.crouch)
                    {
                        Level.Add(SmallFire.New(base.X + (float)offDir * 11f, base.Y, 0f, 0f, shortLife: false, null, canMultiply: true, this));
                    }
                    else
                    {
                        Level.Add(SmallFire.New(base.X + (float)offDir * 11f, base.Y, (float)offDir * (1f + Rando.Float(0.3f)) + addSpeedX, -0.6f - Rando.Float(0.5f) + addSpeedY, shortLife: false, null, canMultiply: true, this));
                    }
                }
            }
            else
            {
                OnBurn(Position, this);
            }
        }
        else
        {
            DoAmmoClick();
        }
    }

    public override void Fire()
    {
    }
}
