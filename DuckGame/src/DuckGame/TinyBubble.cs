namespace DuckGame;

public class TinyBubble : PhysicsParticle
{
	private SinWaveManualUpdate _wave = new SinWaveManualUpdate(0.1f + Rando.Float(0.1f), Rando.Float(3f));

	private float _minY;

	private float _waveSize = 1f;

	public TinyBubble(float xpos, float ypos, float startHSpeed, float minY, bool blue = false)
		: base(xpos, ypos)
	{
		base.alpha = 0.7f;
		_minY = minY;
		_gravMult = 0f;
		vSpeed = 0f - Rando.Float(0.5f, 1f);
		hSpeed = startHSpeed;
		base.depth = 0.3f;
		SpriteMap spr = new SpriteMap("tinyBubbles", 8, 8);
		if (blue)
		{
			spr = new SpriteMap("tinyBlueBubbles", 8, 8);
		}
		spr.frame = Rando.Int(0, 1);
		graphic = spr;
		center = new Vec2(4f, 4f);
		_waveSize = Rando.Float(0.1f, 0.3f);
		base.xscale = (base.yscale = 0.1f);
	}

	public override void Update()
	{
		_wave.Update();
		position.x += _wave.value * _waveSize;
		position.x += hSpeed;
		position.y += vSpeed;
		hSpeed = Lerp.Float(hSpeed, 0f, 0.1f);
		float num = (base.yscale = Lerp.Float(base.xscale, 1f, 0.1f));
		base.xscale = num;
		if (base.y < _minY - 4f)
		{
			base.alpha -= 0.025f;
		}
		if (base.y < _minY - 8f)
		{
			base.alpha = 0f;
		}
		if (base.y < _minY)
		{
			base.alpha -= 0.025f;
			if (base.alpha < 0f)
			{
				Level.Remove(this);
			}
		}
	}
}
