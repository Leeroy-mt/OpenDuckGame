using System;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class Crate : Holdable, IPlatform
{
    public StateBinding _destroyedBinding = new StateBinding(nameof(_destroyed));

    public StateBinding _hitPointsBinding = new StateBinding(nameof(_hitPoints));

    public StateBinding _damageMultiplierBinding = new StateBinding(nameof(damageMultiplier));

    public float damageMultiplier = 1f;

    private SpriteMap _sprite;

    private float _burnt;

    public Crate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _maxHealth = 15f;
        _hitPoints = 15f;
        _sprite = new SpriteMap("crate", 16, 16);
        graphic = _sprite;
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.Depth = -0.5f;
        _editorName = "Crate";
        thickness = 2f;
        weight = 5f;
        buoyancy = 1f;
        _holdOffset = new Vec2(2f, 0f);
        flammable = 0.3f;
        base.collideSounds.Add("crateHit");
        editorTooltip = "It's made of wood. That's...pretty much it.";
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        _hitPoints = 0f;
        Level.Remove(this);
        SFX.Play("crateDestroy");
        Vec2 flyDir = Vec2.Zero;
        if (type is DTShot)
        {
            flyDir = (type as DTShot).bullet.travelDirNormalized;
        }
        for (int i = 0; i < 6; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f));
            woodDebris.hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f) + (float)Math.Sign(flyDir.X) * 0.5f;
            woodDebris.vSpeed = 0f - Rando.Float(1f);
            Level.Add(woodDebris);
        }
        for (int j = 0; j < 5; j++)
        {
            SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-6f, 6f), base.Y + Rando.Float(-6f, 6f));
            smallSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
            smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
            Level.Add(smallSmoke);
        }
        return true;
    }

    private bool CheckForPhysicalBullet(MaterialThing with)
    {
        if (with is PhysicalBullet)
        {
            Bullet b = (with as PhysicalBullet).bullet;
            if (b != null && b.ammo is ATGrenade)
            {
                return true;
            }
        }
        return false;
    }

    public override void SolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (CheckForPhysicalBullet(with))
        {
            Destroy(new DTShot((with as PhysicalBullet).bullet));
        }
        else
        {
            base.SolidImpact(with, from);
        }
    }

    public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
    {
        if (CheckForPhysicalBullet(with))
        {
            Destroy(new DTShot((with as PhysicalBullet).bullet));
        }
        else
        {
            base.Impact(with, from, solidImpact);
        }
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (_hitPoints <= 0f)
        {
            return base.Hit(bullet, hitPos);
        }
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(hitPos.X, hitPos.Y);
            woodDebris.hSpeed = (0f - bullet.travelDirNormalized.X) * 2f * (Rando.Float(1f) + 0.3f);
            woodDebris.vSpeed = (0f - bullet.travelDirNormalized.Y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
            Level.Add(woodDebris);
        }
        SFX.Play("woodHit");
        if (base.isServerForObject && TeamSelect2.Enabled("EXPLODEYCRATES"))
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
            if (base.duck != null)
            {
                base.duck.ThrowItem();
            }
            Destroy(new DTShot(bullet));
            Level.Add(new GrenadeExplosion(base.X, base.Y));
        }
        _hitPoints -= damageMultiplier;
        damageMultiplier += 2f;
        if (_hitPoints <= 0f)
        {
            if (bullet.isLocal)
            {
                Thing.SuperFondle(this, DuckNetwork.localConnection);
            }
            Destroy(new DTShot(bullet));
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(exitPos.X, exitPos.Y);
            woodDebris.hSpeed = bullet.travelDirNormalized.X * 3f * (Rando.Float(1f) + 0.3f);
            woodDebris.vSpeed = bullet.travelDirNormalized.Y * 3f * (Rando.Float(1f) + 0.3f) - (-1f + Rando.Float(2f));
            Level.Add(woodDebris);
        }
    }

    public override void Update()
    {
        base.Update();
        if (damageMultiplier > 1f)
        {
            damageMultiplier -= 0.2f;
        }
        else
        {
            damageMultiplier = 1f;
        }
        _sprite.frame = (int)Math.Floor((1f - _hitPoints / _maxHealth) * 4f);
        if (_hitPoints <= 0f && !base._destroyed)
        {
            Destroy(new DTImpact(this));
        }
        if (_onFire && _burnt < 0.9f)
        {
            float c = 1f - burnt;
            if (_hitPoints > c * _maxHealth)
            {
                _hitPoints = c * _maxHealth;
            }
            _sprite.color = new Color(c, c, c);
        }
    }
}
