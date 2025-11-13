namespace DuckGame;

public class BigTitleShrink : Thing
{
	private static float _dept = 0.5f;

	private float _size;

	private bool _black;

	private Sprite _sprite;

	public BigTitleShrink(float vx, float vy, float vscale, Color vfade)
	{
		base.alpha = (float)(int)vfade.a / 255f;
		vfade.a = byte.MaxValue;
		_sprite = new Sprite("duckGameTitleOutline");
		graphic = _sprite;
		base.x = vx;
		base.y = vy;
		base.scale = new Vec2(vscale, vscale);
		base.depth = _dept;
		base.layer = Layer.HUD;
		base.centerx = _sprite.w / 2;
		base.centery = _sprite.h;
		graphic.color = vfade;
		_black = vfade == Color.Black;
		_dept -= 0.0001f;
		_size = vscale;
	}

	public override void Initialize()
	{
	}

	public override void Update()
	{
		_size += (0.98f - _size) * 0.08f;
		base.xscale = _size;
		base.yscale = _size;
		if (base.xscale < 1.1f)
		{
			base.alpha *= 0.8f;
		}
		if (base.xscale < 1.05f && _black)
		{
			Level.Remove(this);
		}
		if (base.alpha <= 0f)
		{
			Level.Remove(this);
		}
	}
}
