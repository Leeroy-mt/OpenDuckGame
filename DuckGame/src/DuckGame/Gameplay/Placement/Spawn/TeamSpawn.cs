namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
public class TeamSpawn : SpawnPoint
{
    public EditorProperty<bool> eightPlayerOnly = new EditorProperty<bool>(val: false);

    private SpriteMap _eight;

    public TeamSpawn(float xpos = 0f, float ypos = 0f)
        : base(xpos, ypos)
    {
        GraphicList list = new GraphicList();
        for (int i = 0; i < 3; i++)
        {
            SpriteMap duck = new SpriteMap("duck", 32, 32);
            duck.CenterOrigin();
            duck.Depth = 0.9f + 0.01f * (float)i;
            duck.Position = new Vec2(-16f + (float)i * 9.411764f + 16f, -2f);
            list.Add(duck);
        }
        graphic = list;
        _editorName = "Team Spawn";
        Center = new Vec2(8f, 5f);
        collisionSize = new Vec2(32f, 16f);
        collisionOffset = new Vec2(-16f, -8f);
        _visibleInGame = false;
        editorTooltip = "Spawn point for a whole team of Ducks.";
    }

    public override void Draw()
    {
        if (eightPlayerOnly.value)
        {
            if (_eight == null)
            {
                _eight = new SpriteMap("redEight", 10, 10);
                _eight.CenterOrigin();
            }
            Graphics.Draw(_eight, base.X - 5f, base.Y + 7f, 1f);
        }
        base.Draw();
    }
}
