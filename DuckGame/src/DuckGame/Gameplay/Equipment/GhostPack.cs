namespace DuckGame;

public class GhostPack : Jetpack
{
	public GhostPack(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("jetpack", 16, 16);
		graphic = _sprite;
		center = new Vec2(8f, 8f);
		collisionOffset = new Vec2(-5f, -5f);
		collisionSize = new Vec2(11f, 12f);
		_offset = new Vec2(-3f, 3f);
		thickness = 0.1f;
	}

	public override void Draw()
	{
		_heat = 0.01f;
		if (_equippedDuck != null)
		{
			base.depth = -0.5f;
			Vec2 off = _offset;
			if (base.duck.offDir < 0)
			{
				off.x *= -1f;
			}
			position = base.duck.position + off;
		}
		else
		{
			base.depth = 0f;
		}
	}
}
