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
        SpriteMap s = new("levelJetIdle", 32, 13);
        graphic = s;
        _leftJet = new("jet", 16, 16);
        _leftJet.AddAnimation("idle", 0.4f, true, 0, 1, 2);
        _leftJet.SetAnimation("idle");
        _leftJet.Center = new(8, 0f);
        _leftJet.Alpha = 0.7f;
        _rightJet = new("jet", 16, 16);
        _rightJet.AddAnimation("idle", 0.4f, true, 1, 2, 0);
        _rightJet.SetAnimation("idle");
        _rightJet.Center = new(8, 0);
        _rightJet.Alpha = 0.7f;
        Center = new(16, 8);
        _collisionSize = new(16, 14);
        _collisionOffset = new(-8);
        editorTooltip = "Things gotta float somehow.";
        hugWalls = WallHug.Ceiling;
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
        Graphics.Draw(_leftJet, X - 8, Y + 5);
        Graphics.Draw(_rightJet, X + 8, Y + 5);
    }
}
