namespace DuckGame;

[EditorGroup("Details")]
public class BackgroundJets : Thing
{
	public SpriteMap _leftJet;

	public SpriteMap _rightJet;

	private bool _leftAlternate;

	private bool _rightAlternate = true;

	public BackgroundJets(float xpos, float ypos)
		: base(xpos, ypos)
	{
		SpriteMap s = new SpriteMap("levelJetIdle", 32, 13);
		graphic = s;
		_leftJet = new SpriteMap("jet", 16, 16);
		_leftJet.AddAnimation("idle", 0.4f, true, 0, 1, 2);
		_leftJet.SetAnimation("idle");
		_leftJet.center = new Vec2(8f, 0f);
		_leftJet.alpha = 0.7f;
		_rightJet = new SpriteMap("jet", 16, 16);
		_rightJet.AddAnimation("idle", 0.4f, true, 1, 2, 0);
		_rightJet.SetAnimation("idle");
		_rightJet.center = new Vec2(8f, 0f);
		_rightJet.alpha = 0.7f;
		center = new Vec2(16f, 8f);
		_collisionSize = new Vec2(16f, 14f);
		_collisionOffset = new Vec2(-8f, -8f);
		editorTooltip = "Things gotta float somehow.";
		base.hugWalls = WallHug.Ceiling;
		_canFlip = false;
	}

	public override void Update()
	{
		_leftAlternate = !_leftAlternate;
		_rightAlternate = !_rightAlternate;
	}

	public override void Draw()
	{
		base.Draw();
		Graphics.Draw(_leftJet, base.x - 8f, base.y + 5f);
		Graphics.Draw(_rightJet, base.x + 8f, base.y + 5f);
	}
}
