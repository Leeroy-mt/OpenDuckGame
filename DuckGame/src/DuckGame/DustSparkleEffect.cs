using System;
using System.Collections.Generic;

namespace DuckGame;

public class DustSparkleEffect : Thing
{
	private int _sparkleWait;

	private SpriteMap _light;

	private List<DustSparkle> _sparkles = new List<DustSparkle>();

	private bool _wide;

	private bool _lit;

	public float fade
	{
		get
		{
			return (base.material as MaterialDustSparkle).fade;
		}
		set
		{
			(base.material as MaterialDustSparkle).fade = value;
		}
	}

	public DustSparkleEffect(float xpos, float ypos, bool wide, bool lit)
		: base(xpos, ypos)
	{
		if (wide)
		{
			_light = new SpriteMap("arcade/prizeLights", 107, 55);
		}
		else
		{
			_light = new SpriteMap("arcade/lights", 56, 57);
		}
		_wide = wide;
		_lit = lit;
	}

	public override void Initialize()
	{
		base.material = new MaterialDustSparkle(position, new Vec2(_light.width, _light.height), _wide, _lit);
		base.Initialize();
	}

	public override void Update()
	{
		bool remove = false;
		for (int i = 0; i < _sparkles.Count; i++)
		{
			DustSparkle d = _sparkles[i];
			d.position += d.velocity;
			d.position.y += (float)Math.Sin(d.sin) * 0.01f;
			d.sin += 0.01f;
			if (d.alpha < 1f)
			{
				d.alpha += 0.01f;
			}
			remove = false;
			if (d.position.x > base.x + (float)_light.width + 2f || d.position.x < base.x - 2f || d.position.y < base.y + 1f || d.position.y > base.y + (float)_light.height)
			{
				remove = true;
			}
			if (remove)
			{
				_sparkles.RemoveAt(i);
				i--;
			}
		}
		_sparkleWait++;
		if (_sparkleWait > 10)
		{
			_sparkleWait = 0;
			int mul = 1;
			if (Rando.Float(1f) > 0.5f)
			{
				mul = -1;
			}
			_sparkles.Add(new DustSparkle(new Vec2(base.x + Rando.Float(_light.width), base.y + Rando.Float(_light.height)), new Vec2(Rando.Float(0.15f, 0.25f) * (float)mul, Rando.Float(-0.05f, 0.05f))));
		}
	}

	public override void Draw()
	{
		_light.depth = base.depth - 2;
		_light.frame = 1;
		_light.alpha = 0.7f;
		Graphics.Draw(_light, base.x, base.y);
		foreach (DustSparkle d in _sparkles)
		{
			Graphics.DrawRect(d.position + new Vec2(-0.5f, -0.5f), d.position + new Vec2(0.5f, 0.5f), Color.White * d.alpha, base.depth + 10);
		}
	}
}
