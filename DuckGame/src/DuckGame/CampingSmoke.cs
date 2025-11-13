namespace DuckGame;

public class CampingSmoke : Thing
{
	private float _angleInc;

	private float _scaleInc;

	private float _fade;

	public Vec2 move;

	public Vec2 fly;

	private float _fastGrow;

	private float _shrinkSpeed;

	private Sprite _backgroundSmoke;

	public CampingSmoke(float xpos, float ypos)
		: base(xpos, ypos)
	{
		base.xscale = 0.3f + Rando.Float(0.2f);
		base.yscale = base.xscale;
		angle = Maths.DegToRad(Rando.Float(360f));
		_fastGrow = 0.3f + Rando.Float(0.3f);
		_angleInc = Maths.DegToRad(-1f + Rando.Float(2f));
		_scaleInc = 0.001f + Rando.Float(0.001f);
		_fade = 0.0015f + Rando.Float(0.001f);
		move.x = -0.1f + Rando.Float(0.2f);
		move.y = -0.1f + Rando.Float(0.2f);
		GraphicList glist = new GraphicList();
		Sprite smoke = new Sprite("smoke");
		smoke.depth = 1f;
		smoke.CenterOrigin();
		glist.Add(smoke);
		Sprite backSmoke = new Sprite("smokeBack");
		backSmoke.depth = -0.1f;
		backSmoke.CenterOrigin();
		glist.Add(backSmoke);
		graphic = glist;
		center = new Vec2(0f, 0f);
		base.depth = 1f;
		_backgroundSmoke = new Sprite("smokeBack");
		_shrinkSpeed = 0.01f + Rando.Float(0.005f);
	}

	public override void Update()
	{
		angle += _angleInc;
		base.xscale += _scaleInc;
		if (_fastGrow > 0f)
		{
			_fastGrow -= 0.05f;
			base.xscale += 0.03f;
		}
		if (fly.x > 0.01f || fly.x < -0.01f)
		{
			base.x += fly.x;
			fly.x *= 0.9f;
		}
		if (fly.y > 0.01f || fly.y < -0.01f)
		{
			base.y += fly.y;
			fly.y *= 0.9f;
		}
		base.yscale = base.xscale;
		base.x += move.x;
		base.y += move.y;
		if (base.xscale < 0.25f)
		{
			base.alpha -= 0.01f;
		}
		base.xscale -= _shrinkSpeed;
		if (base.xscale < 0.05f)
		{
			Level.Remove(this);
		}
	}
}
