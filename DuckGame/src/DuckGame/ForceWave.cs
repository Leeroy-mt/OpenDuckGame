using System.Collections.Generic;

namespace DuckGame;

public class ForceWave : Thing
{
	public StateBinding _positionBinding = new InterpolatedVec2Binding("netPosition");

	public StateBinding _offDirBinding = new StateBinding(GhostPriority.High, "_offDir");

	public StateBinding _alphaBinding = new StateBinding(GhostPriority.High, "alpha");

	public StateBinding _waveOwnerBinding = new StateBinding(GhostPriority.High, "_waveOwner");

	private Thing _waveOwner;

	private float _alphaSub;

	private float _speed;

	private float _speedv;

	private List<Thing> _hits = new List<Thing>();

	public ForceWave(float xpos, float ypos, int dir, float alphaSub, float speed, float speedv, Duck own)
		: base(xpos, ypos)
	{
		offDir = (sbyte)dir;
		graphic = new Sprite("sledgeForce");
		center = new Vec2(graphic.w, graphic.h);
		_alphaSub = alphaSub;
		_speed = speed;
		_speedv = speedv;
		_collisionSize = new Vec2(6f, 30f);
		_collisionOffset = new Vec2(-3f, -15f);
		graphic.flipH = offDir <= 0;
		_waveOwner = own;
		base.depth = -0.7f;
	}

	public override void Update()
	{
		graphic.flipH = offDir <= 0;
		if (base.alpha > 0.1f)
		{
			foreach (MaterialThing hit in Level.CheckRectAll<MaterialThing>(base.topLeft, base.bottomRight))
			{
				if ((!(hit is PhysicsObject) && !(hit is Icicles)) || _hits.Contains(hit) || hit == _waveOwner || hit.owner == _waveOwner || Duck.GetAssociatedDuck(hit) == _waveOwner)
				{
					continue;
				}
				if (hit.owner != null)
				{
					if (base.isServerForObject && !hit.isServerForObject)
					{
						continue;
					}
				}
				else if (!base.isServerForObject)
				{
					continue;
				}
				if (_waveOwner != null)
				{
					Thing.Fondle(hit, _waveOwner.connection);
				}
				if (hit is Grenade g)
				{
					g.PressAction();
				}
				if (hit is PhysicsObject)
				{
					hit.hSpeed = ((_speed - 3f) * (float)offDir * 1.5f + (float)offDir * 4f) * base.alpha;
					hit.vSpeed = (_speedv + -4.5f) * base.alpha;
					hit.clip.Add(_waveOwner as MaterialThing);
				}
				if (!hit.destroyed && !(hit is Equipment))
				{
					hit.Destroy(new DTImpact(this));
				}
				_hits.Add(hit);
			}
			if (base.isServerForObject)
			{
				foreach (Door hit2 in Level.CheckRectAll<Door>(base.topLeft, base.bottomRight))
				{
					if (_waveOwner != null)
					{
						Thing.Fondle(hit2, _waveOwner.connection);
					}
					if (!hit2.destroyed)
					{
						hit2.Destroy(new DTImpact(this));
					}
				}
				foreach (Window hit3 in Level.CheckRectAll<Window>(base.topLeft, base.bottomRight))
				{
					if (_waveOwner != null)
					{
						Thing.Fondle(hit3, _waveOwner.connection);
					}
					if (!hit3.destroyed)
					{
						hit3.Destroy(new DTImpact(this));
					}
				}
			}
		}
		if (base.isServerForObject)
		{
			base.x += (float)offDir * _speed;
			base.y += _speedv;
			base.alpha -= _alphaSub;
			if (base.alpha <= 0f)
			{
				Level.Remove(this);
			}
		}
	}
}
