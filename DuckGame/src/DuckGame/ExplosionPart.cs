namespace DuckGame;

public class ExplosionPart : Thing
{
	private bool _created;

	private SpriteMap _sprite;

	private float _wait;

	private int _smokeFrame;

	private bool _smoked;

	public ExplosionPart(float xpos, float ypos, bool doWait = true)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("explosion", 64, 64);
		switch (Rando.ChooseInt(0, 1, 2))
		{
		case 0:
			_sprite.AddAnimation("explode", 1f, false, 0, 0, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			break;
		case 1:
			_sprite.AddAnimation("explode", 1.2f, false, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
			break;
		case 2:
			_sprite.AddAnimation("explode", 0.9f, false, 3, 4, 5, 6, 7, 8, 9);
			break;
		}
		_sprite.SetAnimation("explode");
		graphic = _sprite;
		_sprite.speed = 0.4f + Rando.Float(0.2f);
		base.xscale = 0.5f + Rando.Float(0.5f);
		base.yscale = base.xscale;
		center = new Vec2(32f, 32f);
		_wait = Rando.Float(1f);
		_smokeFrame = Rando.Int(1, 3);
		base.depth = 1f;
		vSpeed = Rando.Float(-0.2f, -0.4f);
		if (!doWait)
		{
			_wait = 0f;
		}
	}

	public override void Initialize()
	{
	}

	public override void Update()
	{
		if (!_created)
		{
			_created = true;
		}
		if (_sprite.frame > _smokeFrame && !_smoked)
		{
			int num = ((Graphics.effectsLevel != 2) ? 1 : Rando.Int(1, 4));
			for (int i = 0; i < num; i++)
			{
				SmallSmoke smallSmoke = SmallSmoke.New(base.x + Rando.Float(-5f, 5f), base.y + Rando.Float(-5f, 5f));
				smallSmoke.vSpeed = Rando.Float(0f, -0.5f);
				float num2 = (smallSmoke.yscale = Rando.Float(0.2f, 0.7f));
				smallSmoke.xscale = num2;
				Level.Add(smallSmoke);
			}
			_smoked = true;
		}
		if (!(_wait > 0f))
		{
			base.y += vSpeed;
		}
		if (_sprite.finished)
		{
			Level.Remove(this);
		}
	}

	public override void Draw()
	{
		if (_wait > 0f)
		{
			_wait -= 0.2f;
		}
		else
		{
			base.Draw();
		}
	}
}
