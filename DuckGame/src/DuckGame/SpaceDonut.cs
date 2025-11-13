using System;
using System.Collections.Generic;

namespace DuckGame;

public class SpaceDonut : Thing
{
	private float sinInc;

	private SpriteMap _donuroid;

	private List<Donuroid> _roids = new List<Donuroid>();

	public SpaceDonut(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("background/donut")
		{
			depth = -0.9f
		};
		_donuroid = new SpriteMap("background/donuroids", 32, 32);
		_donuroid.CenterOrigin();
		Random generator = new Random(4562280);
		Random old = Rando.generator;
		Rando.generator = generator;
		Vec2 launch = new Vec2(-22f, -14f);
		Vec2 start = new Vec2(130f, 120f);
		for (int i = 0; i < 20; i++)
		{
			_roids.Add(new Donuroid(start.x + Rando.Float(-6f, 6f), start.y + Rando.Float(-18f, 18f), _donuroid, Rando.Int(0, 7), 1f, 1f));
			_roids.Add(new Donuroid(start.x + Rando.Float(-6f, -1f), start.y + Rando.Float(-10f, 0f) - 10f, _donuroid, Rando.Int(0, 7), base.depth - 20, 0.5f));
			_roids.Add(new Donuroid(start.x + Rando.Float(6f, 1f), start.y + Rando.Float(10f, 0f) - 10f, _donuroid, Rando.Int(0, 7), base.depth - 20, 0.5f));
			_roids.Add(new Donuroid(start.x + Rando.Float(-6f, -1f), start.y + Rando.Float(-10f, 0f) - 20f, _donuroid, Rando.Int(0, 7), base.depth - 30, 0.25f));
			_roids.Add(new Donuroid(start.x + Rando.Float(6f, 1f), start.y + Rando.Float(10f, 0f) - 20f, _donuroid, Rando.Int(0, 7), base.depth - 30, 0.25f));
			start += launch;
			launch.y += 1.4f;
		}
		Rando.generator = old;
	}

	public override void Draw()
	{
		sinInc += 0.02f;
		Graphics.Draw(graphic, base.x, base.y + (float)Math.Sin(sinInc) * 2f, 0.9f);
		foreach (Donuroid roid in _roids)
		{
			roid.Draw(position);
		}
	}
}
