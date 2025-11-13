namespace DuckGame;

[EditorGroup("Details|Signs")]
public class HardLeft : Thing
{
	private SpriteMap _sprite;

	public HardLeft(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("hardSign", 32, 32);
		graphic = _sprite;
		center = new Vec2(16f, 24f);
		_collisionSize = new Vec2(16f, 16f);
		_collisionOffset = new Vec2(-8f, -8f);
		base.depth = -0.5f;
		_editorName = "Hard Sign";
		base.hugWalls = WallHug.Floor;
	}

	public override void Draw()
	{
		_sprite.frame = ((offDir > 0) ? 1 : 0);
		base.Draw();
	}
}
