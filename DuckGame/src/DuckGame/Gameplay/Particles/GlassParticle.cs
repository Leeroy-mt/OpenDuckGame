namespace DuckGame;

public class GlassParticle : PhysicsParticle
{
	private int _tint;

	public GlassParticle(float xpos, float ypos, Vec2 hitAngle, int tint = -1)
		: base(xpos, ypos)
	{
		hSpeed = (0f - hitAngle.x) * 2f * (Rando.Float(1f) + 0.3f);
		vSpeed = (0f - hitAngle.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
		_bounceEfficiency = 0.6f;
		_tint = tint;
	}

	public override void Update()
	{
		base.alpha -= 0.01f;
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
		base.Update();
	}

	public override void Draw()
	{
		Graphics.DrawRect(position, position + new Vec2(1f, 1f), ((_tint > 0) ? Window.windowColors[_tint] : Color.LightBlue) * base.alpha, base.depth);
	}
}
