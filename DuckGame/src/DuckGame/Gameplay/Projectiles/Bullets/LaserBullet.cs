using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class LaserBullet : Bullet
{
	protected Texture2D _beem;

	protected float _thickness;

	public LaserBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = false)
		: base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
	{
		_thickness = type.bulletThickness;
		_beem = Content.Load<Texture2D>("laserBeam");
	}

	public override void Draw()
	{
		if (_tracer || !(_bulletDistance > 0.1f))
		{
			return;
		}
		if (base.gravityAffected)
		{
			if (prev.Count < 1)
			{
				return;
			}
			int num = (int)Math.Ceiling((drawdist - startpoint) / 8f);
			Vec2 prevus = prev.Last();
			for (int i = 0; i < num; i++)
			{
				Vec2 cur = GetPointOnArc(i * 8);
				Graphics.DrawTexturedLine(_beem, cur, prevus, color * (1f - (float)i / (float)num) * base.alpha, ammo.bulletThickness, 0.9f);
				if (!(cur == prev.First()))
				{
					prevus = cur;
					if (i == 0 && ammo.sprite != null && !doneTravelling)
					{
						ammo.sprite.depth = 1f;
						ammo.sprite.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, travelDirNormalized);
						Graphics.Draw(ammo.sprite, prevus.x, prevus.y);
					}
					continue;
				}
				break;
			}
			return;
		}
		float length = (drawStart - drawEnd).length;
		float dist = 0f;
		float incs = 1f / (length / 8f);
		float alph = 0f;
		float drawLength = 8f;
		while (true)
		{
			bool doBreak = false;
			if (dist + drawLength > length)
			{
				drawLength = length - Maths.Clamp(dist, 0f, 99f);
				doBreak = true;
			}
			alph += incs;
			Graphics.DrawTexturedLine(_beem, drawStart + travelDirNormalized * dist, drawStart + travelDirNormalized * (dist + drawLength), Color.White * alph, _thickness, 0.6f);
			if (!doBreak)
			{
				dist += 8f;
				continue;
			}
			break;
		}
	}

	protected override void Rebound(Vec2 pos, float dir, float rng)
	{
		reboundBulletsCreated++;
		Bullet.isRebound = true;
		LaserBullet bullet = new LaserBullet(pos.x, pos.y, ammo, dir, null, rebound, rng);
		Bullet.isRebound = false;
		bullet._teleporter = _teleporter;
		bullet.firedFrom = base.firedFrom;
		bullet.timesRebounded = timesRebounded + 1;
		bullet.lastReboundSource = lastReboundSource;
		bullet.isLocal = isLocal;
		bullet.connection = base.connection;
		reboundCalled = true;
		Level.current.AddThing(bullet);
		Level.current.AddThing(new LaserRebound(pos.x, pos.y));
	}
}
