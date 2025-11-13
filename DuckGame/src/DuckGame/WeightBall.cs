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
			center = new Vec2(9f, 9f);
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
			center = new Vec2(8f, 8f);
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
			ChainLink link = new ChainLink(base.x, base.y);
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
		hSpeed += bullet.travelDirNormalized.x;
		vSpeed += bullet.travelDirNormalized.y;
		SFX.Play("ricochetSmall", Rando.Float(0.6f, 0.7f), Rando.Float(-0.2f, 0.2f));
		return base.Hit(bullet, hitPos);
	}

	public float Solve(PhysicsObject b1, PhysicsObject b2, float dist)
	{
		Thing body1 = ((b1.owner != null) ? b1.owner : b1);
		Thing body2 = ((b2.owner != null) ? b2.owner : b2);
		Vec2 axis = b2.position - b1.position;
		float currentDistance = axis.length;
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
		body1.hSpeed = vel1.x;
		body1.vSpeed = vel1.y;
		body2.hSpeed = vel2.x;
		body2.vSpeed = vel2.y;
		if (body1 is ChainLink && (body2.position - body1.position).length > dist * 12f)
		{
			body1.position = position;
		}
		if (body2 is ChainLink && (body2.position - body1.position).length > dist * 12f)
		{
			body2.position = position;
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
			l.depth = _attach.depth - 8 - index;
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
			Level.Add(Spark.New(base.x + (float)((hSpeed > 0f) ? (-2) : 2), base.y + 7f, new Vec2(0f, 0.5f)));
		}
	}
}
