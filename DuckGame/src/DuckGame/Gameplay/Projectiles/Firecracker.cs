using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class Firecracker : PhysicsParticle, ITeleport
{
    private ActionTimer _sparkTimer = 0.2f;

    private ActionTimer _explodeTimer = Rando.Float(0.01f, 0.012f);

    private bool didRemove;

    public Firecracker(float xpos, float ypos, float ang = 0f)
        : base(xpos, ypos)
    {
        graphic = new Sprite("fireCracker");
        Center = new Vector2(4f, 4f);
        _bounceSound = "plasticBounce";
        _airFriction = 0.02f;
        _bounceEfficiency = 0.65f;
        _spinAngle = ang;
        isLocal = true;
        if (Network.isActive)
        {
            GhostManager.context.particleManager.AddLocalParticle(this);
        }
    }

    public Firecracker(float xpos, float ypos, bool network)
        : this(xpos, ypos)
    {
        if (Network.isActive && !network)
        {
            GhostManager.context.particleManager.AddLocalParticle(this);
        }
        isLocal = !network;
    }

    public override void NetSerialize(BitBuffer b)
    {
        b.Write((short)base.X);
        b.Write((short)base.Y);
        b.Write(_spinAngle);
    }

    public override void NetDeserialize(BitBuffer d)
    {
        float xpos = d.ReadShort();
        float ypos = d.ReadShort();
        netLerpPosition = new Vector2(xpos, ypos);
        _spinAngle = d.ReadFloat();
    }

    public override void Removed()
    {
        if (Network.isActive && !didRemove)
        {
            didRemove = true;
            if (isLocal && GhostManager.context != null)
            {
                GhostManager.context.particleManager.RemoveParticle(this);
            }
            else
            {
                Position = netLerpPosition;
                Level.Add(SmallSmoke.New(base.X, base.Y));
            }
        }
        base.Removed();
    }

    public override void Update()
    {
        if ((bool)_sparkTimer)
        {
            Level.Add(Spark.New(base.X, base.Y - 2f, new Vector2(Rando.Float(-1f, 1f), -0.5f), 0.1f));
        }
        _life = 1f;
        base.AngleDegrees = _spinAngle;
        base.Update();
        if (isLocal && (bool)_explodeTimer)
        {
            SFX.PlaySynchronized("littleGun", Rando.Float(0.8f, 1f), Rando.Float(-0.5f, 0.5f));
            List<Bullet> firedBullets = new List<Bullet>();
            for (int i = 0; i < 8; i++)
            {
                float dir = (float)i * 45f - 5f + Rando.Float(10f);
                ATShrapnel shrap = new ATShrapnel();
                shrap.range = 8f + Rando.Float(3f);
                Bullet bullet = new Bullet(base.X + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), base.Y - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir);
                bullet.firedFrom = this;
                Level.Add(bullet);
                firedBullets.Add(bullet);
            }
            if (Network.isActive)
            {
                Send.Message(new NMFireGun(null, firedBullets, 0, rel: false, 4), NetMessagePriority.ReliableOrdered);
            }
            Level.Add(SmallSmoke.New(base.X, base.Y));
            if (Rando.Float(1f) < 0.1f)
            {
                Level.Add(SmallFire.New(base.X, base.Y, 0f, 0f, shortLife: false, null, canMultiply: true, this));
            }
            Level.Remove(this);
        }
    }
}
