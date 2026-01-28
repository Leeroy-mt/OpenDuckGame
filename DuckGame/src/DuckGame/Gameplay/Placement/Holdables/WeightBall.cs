using System;
using System.Collections.Generic;

namespace DuckGame;

public class WeightBall : Holdable, IPlatform
{
    private PhysicsObject _attach;

    public ChokeCollar collar;

    private List<ChainLink> _links = new List<ChainLink>();

    private bool _isMace;

    private float _sparkWait;

    public WeightBall(float xpos, float ypos, PhysicsObject d, ChokeCollar c, bool isMace)
        : base(xpos, ypos)
    {
        _attach = d;
        if (isMace)
        {
            graphic = new Sprite("maceBall");
            Center = new Vec2(9f, 9f);
            _collisionOffset = new Vec2(-8f, -8f);
            _collisionSize = new Vec2(14f, 14f);
            _impactThreshold = 4f;
            canPickUp = false;
            onlyCrush = true;
            _isMace = true;
        }
        else
        {
            graphic = new Sprite("weightBall");
            Center = new Vec2(8f, 8f);
            _collisionOffset = new Vec2(-7f, -7f);
            _collisionSize = new Vec2(14f, 14f);
            _impactThreshold = 2f;
        }
        weight = 9f;
        thickness = 6f;
        physicsMaterial = PhysicsMaterial.Metal;
        base.collideSounds.Add("rockHitGround2");
        collar = c;
        tapeable = false;
    }

    public void SetAttach(PhysicsObject a)
    {
        _attach = a;
    }

    public override void Initialize()
    {
        for (int i = 0; i < 8; i++)
        {
            ChainLink link = new ChainLink(base.X, base.Y);
            Level.Add(link);
            _links.Add(link);
        }
        base.Initialize();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with is Duck && collar != null && with != null)
        {
            if (with != collar.owner && base.totalImpactPower > 8f)
            {
                if (collar.duck != null)
                {
                    RumbleManager.AddRumbleEvent(collar.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
                }
                if (_isMace)
                {
                    with.Destroy(new DTCrush(this));
                }
            }
        }
        else
        {
            base.OnSoftImpact(with, from);
        }
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (collar != null && collar.duck != null && collar.duck.profile != null && base.totalImpactPower > 4f)
        {
            RumbleManager.AddRumbleEvent(collar.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
        }
        base.OnSolidImpact(with, from);
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        hSpeed += bullet.travelDirNormalized.X;
        vSpeed += bullet.travelDirNormalized.Y;
        SFX.Play("ricochetSmall", Rando.Float(0.6f, 0.7f), Rando.Float(-0.2f, 0.2f));
        return base.Hit(bullet, hitPos);
    }

    public float Solve(PhysicsObject b1, PhysicsObject b2, float dist)
    {
        Thing body1 = ((b1.owner != null) ? b1.owner : b1);
        Thing body2 = ((b2.owner != null) ? b2.owner : b2);
        Vec2 axis = b2.Position - b1.Position;
        float currentDistance = axis.Length();
        if (currentDistance < 0.0001f)
        {
            currentDistance = 0.0001f;
        }
        if (currentDistance < dist)
        {
            return 0f;
        }
        Vec2 unitAxis = axis * (1f / currentDistance);
        Vec2 vel1 = new Vec2(body1.hSpeed, body1.vSpeed);
        Vec2 vel2 = new Vec2(body2.hSpeed, body2.vSpeed);
        float num = Vec2.Dot(vel2 - vel1, unitAxis);
        float relDist = currentDistance - dist;
        float invMass1 = 2.5f;
        float invMass2 = 2.1f;
        if (body1 is ChainLink && !(body2 is ChainLink))
        {
            invMass1 = 10f;
            invMass2 = 0f;
        }
        else if (body2 is ChainLink && !(body1 is ChainLink))
        {
            invMass1 = 0f;
            invMass2 = 10f;
        }
        else if (body1 is ChainLink && body2 is ChainLink)
        {
            invMass1 = 10f;
            invMass2 = 10f;
        }
        if (body1 is ChokeCollar)
        {
            invMass1 = 10f;
        }
        else if (body2 is ChokeCollar)
        {
            invMass2 = 10f;
        }
        float impulse = (num + relDist) / (invMass1 + invMass2);
        Vec2 impulseVector = unitAxis * impulse;
        vel1 += impulseVector * invMass1;
        vel2 -= impulseVector * invMass2;
        body1.hSpeed = vel1.X;
        body1.vSpeed = vel1.Y;
        body2.hSpeed = vel2.X;
        body2.vSpeed = vel2.Y;
        if (body1 is ChainLink && (body2.Position - body1.Position).Length() > dist * 12f)
        {
            body1.Position = Position;
        }
        if (body2 is ChainLink && (body2.Position - body1.Position).Length() > dist * 12f)
        {
            body2.Position = Position;
        }
        return impulse;
    }

    public override void Update()
    {
        PhysicsObject attach = _attach;
        if (_attach is Duck)
        {
            Duck d = _attach as Duck;
            attach = ((d.ragdoll != null) ? ((PhysicsObject)d.ragdoll.part1) : ((PhysicsObject)d));
        }
        if (attach == null)
        {
            return;
        }
        Solve(this, attach, 30f);
        int index = 0;
        PhysicsObject prev = this;
        foreach (ChainLink l in _links)
        {
            Solve(l, prev, 2f);
            prev = l;
            l.Depth = _attach.Depth - 8 - index;
            index++;
        }
        Solve(attach, prev, 2f);
        base.Update();
        if (_sparkWait > 0f)
        {
            _sparkWait -= 0.1f;
        }
        else
        {
            _sparkWait = 0f;
        }
        if (_sparkWait == 0f && base.grounded && Math.Abs(hSpeed) > 1f)
        {
            _sparkWait = 0.25f;
            Level.Add(Spark.New(base.X + (float)((hSpeed > 0f) ? (-2) : 2), base.Y + 7f, new Vec2(0f, 0.5f)));
        }
    }
}
