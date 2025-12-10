using System;

namespace DuckGame;

public class CampingBall : PhysicsObject
{
    private SpriteMap _sprite;

    private new Duck _owner;

    public CampingBall(float xpos, float ypos, Duck owner)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("camping_ball", 8, 9);
        graphic = _sprite;
        center = new Vec2(4f, 4f);
        collisionOffset = new Vec2(-4f, -4f);
        collisionSize = new Vec2(8f, 8f);
        base.depth = -0.5f;
        thickness = 2f;
        weight = 1f;
        base.bouncy = 0.5f;
        _owner = owner;
        _impactThreshold = 0.01f;
    }

    public override void Update()
    {
        if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 0.1f)
        {
            base.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, new Vec2(hSpeed, vSpeed));
        }
        if (base.isServerForObject)
        {
            if (base.grounded && Math.Abs(vSpeed) + Math.Abs(hSpeed) <= 0.2f)
            {
                base.alpha -= 0.2f;
            }
            if (base.alpha <= 0f)
            {
                Level.Remove(this);
            }
            if (!base.onFire && Level.CheckRect<SmallFire>(position + new Vec2(-6f, -6f), position + new Vec2(6f, 6f), this) != null)
            {
                LightOnFire();
            }
        }
        base.Update();
    }

    public void LightOnFire()
    {
        base.onFire = true;
        Level.Add(SmallFire.New(0f, 0f, 0f, 0f, shortLife: false, this, canMultiply: true, this));
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if ((Network.isActive && connection != DuckNetwork.localConnection) || base.removeFromLevel)
        {
            return;
        }
        if (with is Duck { ragdoll: null } d)
        {
            if (d._trapped != null)
            {
                return;
            }
            d.hSpeed = hSpeed * 0.75f;
            d.vSpeed = vSpeed * 0.75f;
            if (d.holdObject != null)
            {
                Thing t = d.holdObject;
                d.ThrowItem();
                Fondle(t);
            }
            Fondle(d);
            d.GoRagdoll();
            if (d.ragdoll != null && d.ragdoll.part1 != null && d.ragdoll.part2 != null && d.ragdoll.part3 != null)
            {
                Fondle(d.ragdoll);
                d.ragdoll.connection = connection;
                d.ragdoll.part1.connection = connection;
                d.ragdoll.part2.connection = connection;
                d.ragdoll.part3.connection = connection;
                d.ragdoll.inSleepingBag = true;
                d.ragdoll.sleepingBagHealth = 60;
                if (base.onFire)
                {
                    d.ragdoll.LightOnFire();
                }
                for (int i = 0; i < 4; i++)
                {
                    SmallSmoke smallSmoke = SmallSmoke.New(d.ragdoll.x + Rando.Float(-4f, 4f), d.ragdoll.y + Rando.Float(-4f, 4f));
                    smallSmoke.hSpeed += d.ragdoll.hSpeed * Rando.Float(0.3f, 0.5f);
                    smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
                    Level.Add(smallSmoke);
                }
            }
            if (Recorder.currentRecording != null)
            {
                Recorder.currentRecording.LogBonus();
            }
            Level.Remove(this);
        }
        else if (with is RagdollPart { doll: not null } p && !p.doll.inSleepingBag)
        {
            Fondle(p.doll);
            p.doll.inSleepingBag = true;
            p.doll.sleepingBagHealth = 60;
            if (base.onFire)
            {
                p.doll.LightOnFire();
            }
            if (p.doll.part2 != null && p.doll.part1 != null)
            {
                p.doll.part2.hSpeed = hSpeed * 0.75f;
                p.doll.part2.vSpeed = vSpeed * 0.75f;
                p.doll.part1.hSpeed = hSpeed * 0.75f;
                p.doll.part1.vSpeed = vSpeed * 0.75f;
            }
            for (int j = 0; j < 4; j++)
            {
                SmallSmoke smallSmoke2 = SmallSmoke.New(p.doll.x + Rando.Float(-4f, 4f), p.doll.y + Rando.Float(-4f, 4f));
                smallSmoke2.hSpeed += p.doll.hSpeed * Rando.Float(0.3f, 0.5f);
                smallSmoke2.vSpeed -= Rando.Float(0.1f, 0.2f);
                Level.Add(smallSmoke2);
            }
            if (Recorder.currentRecording != null)
            {
                Recorder.currentRecording.LogBonus();
            }
            Level.Remove(this);
        }
    }
}
