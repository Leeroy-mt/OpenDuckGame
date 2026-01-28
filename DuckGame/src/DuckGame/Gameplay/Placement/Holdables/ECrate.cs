using System;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
public class ECrate : Holdable, IPlatform
{
    private float damageMultiplier = 1f;

    private SpriteMap _sprite;

    private Sprite _light;

    private SinWaveManualUpdate _colorFlux = 0.1f;

    public ECrate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _maxHealth = 15f;
        _hitPoints = 15f;
        _sprite = new SpriteMap("eCrate", 16, 16);
        graphic = _sprite;
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.Depth = -0.5f;
        _editorName = "E Crate";
        editorTooltip = "A mysterious unbreakable crate.";
        thickness = 2f;
        weight = 5f;
        flammable = 0.3f;
        _holdOffset = new Vec2(2f, 0f);
        _light = new Sprite("eCrateLight");
        _light.CenterOrigin();
        base.collideSounds.Add("crateHit");
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        _hitPoints = 0f;
        for (int i = 0; i < 6; i++)
        {
            Level.Add(new GlassParticle(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f), new Vec2(Rando.Float(-2f, 2f), Rando.Float(-2f, 2f))));
        }
        for (int j = 0; j < 5; j++)
        {
            SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-6f, 6f), base.Y + Rando.Float(-6f, 6f));
            smallSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
            smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
            Level.Add(smallSmoke);
        }
        SFX.Play("crateDestroy");
        Level.Remove(this);
        return true;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (_hitPoints <= 0f)
        {
            return false;
        }
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            Level.Add(new GlassParticle(hitPos.X, hitPos.Y, bullet.travelDirNormalized));
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
            Level.Add(new GlassParticle(exitPos.X, exitPos.Y, -bullet.travelDirNormalized));
        }
    }

    public override void Update()
    {
        _colorFlux.Update();
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
        if (_onFire && burnt < 0.9f)
        {
            float c = 1f - burnt;
            if (_hitPoints > c * _maxHealth)
            {
                _hitPoints = c * _maxHealth;
            }
            _sprite.color = new Color(c, c, c);
        }
    }

    public override void Draw()
    {
        base.Draw();
        _light.Depth = base.Depth + 1;
        float normHealth = _hitPoints / _maxHealth;
        float flash = normHealth * 0.7f + 0.3f;
        normHealth = 1f;
        flash = 1f;
        if (_hitPoints < _maxHealth / 2f)
        {
            normHealth = 0f;
            flash = 0.4f;
        }
        _light.color = new Color(1f - normHealth, normHealth, 0.2f) * Maths.Clamp(flash + _colorFlux.normalized * (1f - flash), 0f, 1f);
        Graphics.Draw(_light, base.X, base.Y);
    }
}
