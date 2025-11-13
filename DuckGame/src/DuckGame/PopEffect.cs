using System.Collections.Generic;

namespace DuckGame;

public class PopEffect : Thing
{
	public class PopEffectPart
	{
		public float scale;

		public float rot;
	}

	private List<PopEffectPart> parts = new List<PopEffectPart>();

	private SpriteMap _sprite;

	public PopEffect(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("popLine", 16, 16);
		center = new Vec2(_sprite.w / 2, _sprite.h / 2);
		graphic = _sprite;
		int num = 8;
		for (int i = 0; i < num; i++)
		{
			float rot = 360f / (float)num * (float)i;
			parts.Add(new PopEffectPart
			{
				scale = Rando.Float(0.6f, 1f),
				rot = Maths.DegToRad(rot + Rando.Float(-10f, 10f))
			});
		}
		base.scale = new Vec2(1.5f, 1.5f);
		base.depth = 0.85f;
	}

	public override void Update()
	{
		base.xscale -= 0.24f;
		base.yscale = base.xscale;
		if (base.xscale < 0.01f)
		{
			Level.Remove(this);
		}
	}

	public override void Draw()
	{
		foreach (PopEffectPart part in parts)
		{
			_sprite.angle = part.rot;
			SpriteMap sprite = _sprite;
			float num = (_sprite.yscale = base.xscale * part.scale);
			sprite.xscale = num;
			_sprite.center = new Vec2(_sprite.w / 2, _sprite.h / 2);
			_sprite.alpha = 0.8f;
			Graphics.Draw(_sprite, base.x, base.y);
		}
		base.Draw();
	}
}
