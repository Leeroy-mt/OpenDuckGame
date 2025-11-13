using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class ParallaxBackground : Thing
{
	public class Definition
	{
		public struct Zone
		{
			public int index;

			public float distance;

			public float speed;

			public bool moving;

			public Sprite sprite;
		}

		public List<Zone> zones = new List<Zone>();

		public List<Zone> sprites = new List<Zone>();
	}

	public float FUCKINGYOFFSET;

	public Color color = Color.White;

	private Sprite _sprite;

	private Dictionary<int, ParallaxZone> _zones = new Dictionary<int, ParallaxZone>();

	private int _hRepeat = 1;

	public float xmove;

	public Rectangle scissor;

	public Definition definition;

	public bool restrictBottom = true;

	public ParallaxBackground(string image, float vx, float vdepth, int hRepeat = 1)
	{
		_sprite = new Sprite(image);
		graphic = _sprite;
		base.x = vx;
		base.depth = vdepth;
		base.layer = Layer.Parallax;
		_hRepeat = hRepeat;
		_opaque = true;
		definition = Content.LoadParallaxDefinition(image);
		if (definition == null)
		{
			return;
		}
		foreach (Definition.Zone z in definition.zones)
		{
			AddZone(z.index, z.distance, z.speed, z.moving);
		}
		foreach (Definition.Zone z2 in definition.sprites)
		{
			AddZoneSprite(z2.sprite.Clone(), z2.index, z2.distance, z2.speed, z2.moving);
		}
	}

	public ParallaxBackground(Texture2D t)
	{
		_sprite = new Sprite(t);
		graphic = _sprite;
		base.x = 0f;
		base.depth = 0f;
		base.layer = Layer.Parallax;
		_hRepeat = 3;
		_opaque = true;
	}

	public void AddZone(int yPos, float distance, float speed, bool moving = false, bool vis = true)
	{
		_zones[yPos] = new ParallaxZone(distance, speed, moving, vis);
	}

	public void AddZoneSprite(Sprite s, int yPos, float distance, float speed, bool moving = false, float wrapMul = 1f)
	{
		if (!_zones.ContainsKey(yPos))
		{
			_zones[yPos] = new ParallaxZone(distance, speed, moving, vis: false)
			{
				wrapMul = wrapMul
			};
		}
		_zones[yPos].AddSprite(s);
	}

	public void AddZoneThing(Thing s, int yPos, float distance, float speed, bool moving = false)
	{
		_zones[yPos] = new ParallaxZone(distance, speed, moving, vis: false);
		_zones[yPos].AddThing(s);
	}

	public override void Initialize()
	{
	}

	public override void Update()
	{
		foreach (KeyValuePair<int, ParallaxZone> zone2 in _zones)
		{
			zone2.Value.Update(xmove);
		}
	}

	public override void Draw()
	{
		if (scissor.width != 0f)
		{
			base.layer.scissor = scissor;
		}
		if (position.y > 0f)
		{
			position.y = 0f;
		}
		if (restrictBottom && position.y + (float)_sprite.texture.height < Layer.Parallax.camera.bottom)
		{
			position.y = Layer.Parallax.camera.bottom - (float)_sprite.texture.height;
		}
		for (int xpos = 0; xpos < _hRepeat; xpos++)
		{
			for (int i = 0; i < graphic.height / 8; i++)
			{
				if (_zones.ContainsKey(i))
				{
					ParallaxZone zone = _zones[i];
					if (xpos == 0)
					{
						zone.RenderSprites(position);
					}
					if (zone.visible)
					{
						float offset = zone.scroll % (float)graphic.width;
						Graphics.Draw(_sprite.texture, position + new Vec2(0f, FUCKINGYOFFSET) + new Vec2((offset - (float)graphic.width + (float)(xpos * graphic.width)) * base.scale.x, (float)(i * 8) * base.scale.y), new Rectangle(0f, i * 8, graphic.width, 8f), color, 0f, default(Vec2), new Vec2(base.scale.x, base.scale.y), SpriteEffects.None, base.depth);
					}
				}
			}
		}
	}
}
