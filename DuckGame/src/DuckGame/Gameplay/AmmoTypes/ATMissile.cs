using System;
using System.Collections.Generic;

namespace DuckGame;

public class ATMissile : AmmoType
{
    public ATMissile()
    {
        accuracy = 1f;
        range = 850f;
        penetration = 0.4f;
        bulletSpeed = 7f;
        bulletThickness = 2f;
        sprite = new Sprite("missile");
        sprite.CenterOrigin();
        speedVariation = 0f;
        flawlessPipeTravel = true;
    }

    public override void PopShell(float x, float y, int dir)
    {
        Level.Add(new PistolShell(x, y)
        {
            hSpeed = (float)dir * (1.5f + Rando.Float(1f))
        });
    }

    public override void OnHit(bool destroyed, Bullet b)
    {
        if (!b.isLocal)
        {
            return;
        }
        if (destroyed)
        {
            RumbleManager.AddRumbleEvent(b.Position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
            ATMissileShrapnel shrap = new ATMissileShrapnel();
            shrap.MakeNetEffect(b.Position);
            Random rand = null;
            if (Network.isActive && b.isLocal)
            {
                rand = Rando.generator;
                Rando.generator = new Random(NetRand.currentSeed);
            }
            List<Bullet> firedBullets = new List<Bullet>();
            for (int i = 0; i < 12; i++)
            {
                float dir = (float)i * 30f - 10f + Rando.Float(20f);
                shrap = new ATMissileShrapnel();
                shrap.range = 15f + Rando.Float(5f);
                Vec2 shrapDir = new Vec2((float)Math.Cos(Maths.DegToRad(dir)), (float)Math.Sin(Maths.DegToRad(dir)));
                Bullet bullet = new Bullet(b.X + shrapDir.X * 8f, b.Y - shrapDir.Y * 8f, shrap, dir);
                bullet.firedFrom = b;
                firedBullets.Add(bullet);
                Level.Add(bullet);
                Level.Add(Spark.New(b.X + Rando.Float(-8f, 8f), b.Y + Rando.Float(-8f, 8f), shrapDir + new Vec2(Rando.Float(-0.1f, 0.1f), Rando.Float(-0.1f, 0.1f))));
                Level.Add(SmallSmoke.New(b.X + shrapDir.X * 8f + Rando.Float(-8f, 8f), b.Y + shrapDir.Y * 8f + Rando.Float(-8f, 8f)));
            }
            if (Network.isActive && b.isLocal)
            {
                Send.Message(new NMFireGun(null, firedBullets, 0, rel: false, 4), NetMessagePriority.ReliableOrdered);
                firedBullets.Clear();
            }
            if (Network.isActive && b.isLocal)
            {
                Rando.generator = rand;
            }
            DestroyRadius(b.Position, 50f, b);
        }
        base.OnHit(destroyed, b);
    }

    public static int DestroyRadius(Vec2 pPosition, float pRadius, Thing pBullet, bool pExplode = false)
    {
        foreach (Window w in Level.CheckCircleAll<Window>(pPosition, pRadius - 20f))
        {
            Thing.Fondle(w, DuckNetwork.localConnection);
            if (Level.CheckLine<Block>(pPosition, w.Position, w) == null)
            {
                w.Destroy(new DTImpact(pBullet));
            }
        }
        foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(pPosition, pRadius + 30f))
        {
            if (pBullet.isLocal && pBullet.owner == null)
            {
                Thing.Fondle(p, DuckNetwork.localConnection);
            }
            if ((p.Position - pPosition).Length() < 30f)
            {
                p.Destroy(new DTImpact(pBullet));
            }
            p.sleeping = false;
            p.vSpeed = -2f;
        }
        int idd = 0;
        HashSet<ushort> idx = new HashSet<ushort>();
        foreach (BlockGroup block in Level.CheckCircleAll<BlockGroup>(pPosition, pRadius))
        {
            if (block == null)
            {
                continue;
            }
            BlockGroup group = block;
            new List<Block>();
            foreach (Block bl in group.blocks)
            {
                if (!Collision.Circle(pPosition, pRadius - 22f, bl.rectangle))
                {
                    continue;
                }
                bl.shouldWreck = true;
                if (bl is AutoBlock && !(bl as AutoBlock).indestructable)
                {
                    idx.Add((bl as AutoBlock).blockIndex);
                    if (pExplode && idd % 10 == 0)
                    {
                        Level.Add(new ExplosionPart(bl.X, bl.Y));
                        Level.Add(SmallFire.New(bl.X, bl.Y, Rando.Float(-2f, 2f), Rando.Float(-2f, 2f)));
                    }
                    idd++;
                }
            }
            group.Wreck();
        }
        foreach (Block block2 in Level.CheckCircleAll<Block>(pPosition, pRadius - 22f))
        {
            if (block2 is AutoBlock && !(block2 as AutoBlock).indestructable)
            {
                block2.skipWreck = true;
                block2.shouldWreck = true;
                idx.Add((block2 as AutoBlock).blockIndex);
                if (pExplode)
                {
                    if (idd % 10 == 0)
                    {
                        Level.Add(new ExplosionPart(block2.X, block2.Y));
                        Level.Add(SmallFire.New(block2.X, block2.Y, Rando.Float(-2f, 2f), Rando.Float(-2f, 2f)));
                    }
                    idd++;
                }
            }
            else if (block2 is Door || block2 is VerticalDoor)
            {
                Level.Remove(block2);
                block2.Destroy(new DTRocketExplosion(null));
            }
        }
        if (Network.isActive && (pBullet.isLocal || pBullet.isServerForObject) && idx.Count > 0)
        {
            Send.Message(new NMDestroyBlocks(idx));
        }
        foreach (ILight item in Level.current.things[typeof(ILight)])
        {
            item.Refresh();
        }
        return idx.Count;
    }
}
