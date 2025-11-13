namespace DuckGame;

public class WetEnterEffect : Thing
{
	private SpriteMap _sprite;

	public WetEnterEffect(float xpos, float ypos, Vec2 dir, Thing attach)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("wetEnter", 16, 16);
		_sprite.AddAnimation("splash", 0.45f, false, 0, 1);
		_sprite.SetAnimation("splash");
		center = new Vec2(0f, 7f);
		graphic = _sprite;
		base.depth = 0.7f;
		base.alpha = 0.6f;
		angle = Maths.DegToRad(0f - Maths.PointDirection(Vec2.Zero, dir));
		base.anchor = new Anchor(attach);
		base.anchor.offset = new Vec2(xpos, ypos) - attach.position;
	}

	public override void Update()
	{
		if (_sprite.finished)
		{
			Level.Remove(this);
		}
	}

	public override void Draw()
	{
		base.Draw();
	}
}
