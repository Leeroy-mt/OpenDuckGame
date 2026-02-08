using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Props", EditorItemType.Normal)]
public class DemoCrate : Holdable, IPlatform
{
    public float baseExplosionRange = 50f;

    private float damageMultiplier = 1f;

    private SpriteMap _sprite;

    public DemoCrate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _maxHealth = 15f;
        _hitPoints = 15f;
        _canFlip = false;
        _editorName = "Demo Crate";
        editorTooltip = "Makes a whole lotta mess.";
        base.collideSounds.Add("rockHitGround2");
        _sprite = new SpriteMap("demoCrate", 20, 20);
        graphic = _sprite;
        Center = new Vector2(10f, 10f);
        collisionOffset = new Vector2(-10f, -10f);
        collisionSize = new Vector2(20f, 19f);
        base.Depth = -0.5f;
        _editorName = "Demo Crate";
        thickness = 2f;
        weight = 10f;
        buoyancy = 1f;
        _holdOffset = new Vector2(2f, 0f);
        flammable = 0.3f;
        _placementCost += 15;
    }

    [NetworkAction]
    private void BlowUp(Vector2 pPosition, float pFlyX)
    {
        Level.Add(new ExplosionPart(pPosition.X, pPosition.Y));
        int num = 6;
        if (Graphics.effectsLevel < 2)
        {
            num = 3;
        }
        for (int i = 0; i < num; i++)
        {
            float dir = (float)i * 60f + Rando.Float(-10f, 10f);
            float dist = Rando.Float(12f, 20f);
            Level.Add(new ExplosionPart(pPosition.X + (float)(Math.Cos(Maths.DegToRad(dir)) * (double)dist), pPosition.Y - (float)(Math.Sin(Maths.DegToRad(dir)) * (double)dist)));
        }
        for (int j = 0; j < 5; j++)
        {
            SmallSmoke smallSmoke = SmallSmoke.New(pPosition.X + Rando.Float(-6f, 6f), pPosition.Y + Rando.Float(-6f, 6f));
            smallSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
            smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
            Level.Add(smallSmoke);
        }
        for (int k = 0; k < 3; k++)
        {
            Level.Add(new CampingSmoke(pPosition.X - 5f + Rando.Float(10f), pPosition.Y + 6f - 3f + Rando.Float(6f) - (float)k * 1f)
            {
                move =
                {
                    X = -0.3f + Rando.Float(0.6f),
                    Y = -0.5f + Rando.Float(1f)
                }
            });
        }
        for (int l = 0; l < 6; l++)
        {
            WoodDebris woodDebris = WoodDebris.New(pPosition.X - 8f + Rando.Float(16f), pPosition.Y - 8f + Rando.Float(16f));
            woodDebris.hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f) + (float)Math.Sign(pFlyX) * 0.5f;
            woodDebris.vSpeed = 0f - Rando.Float(1f);
            Level.Add(woodDebris);
        }
        foreach (Window w in Level.CheckCircleAll<Window>(pPosition, 40f))
        {
            if (Level.CheckLine<Block>(pPosition, w.Position, w) == null)
            {
                w.Destroy(new DTImpact(this));
            }
        }
        SFX.Play("explode", 1f, Rando.Float(0.1f, 0.3f));
        RumbleManager.AddRumbleEvent(pPosition, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (!base.isServerForObject)
        {
            return false;
        }
        if (base.removeFromLevel)
        {
            return true;
        }
        _hitPoints = 0f;
        Level.Remove(this);
        Vector2 flyDir = Vector2.Zero;
        if (type is DTShot)
        {
            flyDir = (type as DTShot).bullet.travelDirNormalized;
        }
        SyncNetworkAction(BlowUp, Position, flyDir.X);
        List<Bullet> firedBullets = new List<Bullet>();
        for (int i = 0; i < 20; i++)
        {
            float dir = (float)i * 18f - 5f + Rando.Float(10f);
            ATShrapnel shrap = new ATShrapnel();
            shrap.range = baseExplosionRange - 20f + Rando.Float(18f);
            Bullet bullet = new Bullet(base.X + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), base.Y - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir);
            bullet.firedFrom = this;
            firedBullets.Add(bullet);
            Level.Add(bullet);
        }
        DoBlockDestruction();
        if (Network.isActive)
        {
            Send.Message(new NMExplodingProp(firedBullets), NetMessagePriority.ReliableOrdered);
        }
        return true;
    }

    public virtual void DoBlockDestruction()
    {
        ATMissile.DestroyRadius(Position, baseExplosionRange, this);
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (_hitPoints <= 0f)
        {
            return base.Hit(bullet, hitPos);
        }
        Destroy(new DTShot(bullet));
        return base.Hit(bullet, hitPos);
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
}
