using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class FireCrackers : Gun
{
    private SpriteMap _sprite;

    private int _ammoMax = 8;

    private int burnTime;

    public FireCrackers(float xval, float yval)
        : base(xval, yval)
    {
        ammo = _ammoMax;
        _type = "gun";
        _sprite = new SpriteMap("fireCrackers", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-4f, -4f);
        collisionSize = new Vector2(8f, 8f);
        _barrelOffsetTL = new Vector2(12f, 6f);
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        flammable = 1f;
        physicsMaterial = PhysicsMaterial.Paper;
        editorTooltip = "Warning: these are dangerous explosives, not a spicy snack treat.";
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        _sprite.frame = _ammoMax - ammo;
        if (ammo == 0 && owner != null && base.isServerForObject)
        {
            if (base.held && owner is Duck duckOwner)
            {
                duckOwner.ThrowItem();
            }
            base.level.RemoveThing(this);
        }
        if (base.onFire && infinite.value)
        {
            burnTime++;
            if (burnTime % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Level.Add(new Firecracker(base.barrelPosition.X, base.barrelPosition.Y)
                    {
                        hSpeed = Rando.Float(-4f, 4f),
                        vSpeed = Rando.Float(-1f, -6f)
                    });
                }
                SFX.PlaySynchronized("lightMatch", 0.5f, -0.4f + Rando.Float(0.2f));
            }
            if (burnTime > 120)
            {
                Level.Remove(this);
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void OnPressAction()
    {
        if (ammo <= 0 || !base.isServerForObject)
        {
            return;
        }
        ammo--;
        SFX.PlaySynchronized("lightMatch", 0.5f, -0.4f + Rando.Float(0.2f));
        if (owner is Duck duckOwner)
        {
            float addSpeedX = 0f;
            float addSpeedY = 0f;
            if (duckOwner.inputProfile.Down("LEFT"))
            {
                addSpeedX -= 2f;
            }
            if (duckOwner.inputProfile.Down("RIGHT"))
            {
                addSpeedX += 2f;
            }
            if (duckOwner.inputProfile.Down("UP"))
            {
                addSpeedY -= 2f;
            }
            if (duckOwner.inputProfile.Down("DOWN"))
            {
                addSpeedY += 2f;
            }
            Firecracker f = new Firecracker(base.barrelPosition.X, base.barrelPosition.Y);
            if (!duckOwner.crouch)
            {
                f.hSpeed = (float)offDir * Rando.Float(2f, 2.5f) + addSpeedX;
                f.vSpeed = -1f + addSpeedY + Rando.Float(-0.2f, 0.8f);
            }
            else
            {
                f.spinAngle = 90f;
            }
            Level.Add(f);
        }
    }

    protected override bool OnBurn(Vector2 firePosition, Thing litBy)
    {
        if (base.isServerForObject)
        {
            if (!infinite.value)
            {
                for (int i = 0; i < Math.Min(ammo, _ammoMax); i++)
                {
                    Level.Add(new Firecracker(base.barrelPosition.X, base.barrelPosition.Y)
                    {
                        hSpeed = Rando.Float(-4f, 4f),
                        vSpeed = Rando.Float(-1f, -6f)
                    });
                }
                Level.Remove(this);
            }
            if (owner is Duck duckOwner)
            {
                duckOwner.ThrowItem();
            }
        }
        return true;
    }

    public override void Fire()
    {
    }
}
