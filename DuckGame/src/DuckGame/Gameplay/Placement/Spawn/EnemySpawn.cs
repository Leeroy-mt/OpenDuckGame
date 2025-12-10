namespace DuckGame;

public class EnemySpawn : Thing
{
    private SpriteMap _spawnSprite;

    public EnemySpawn(float xpos = 0f, float ypos = 0f)
        : base(xpos, ypos)
    {
        GraphicList list = new GraphicList();
        list.Add(new SpriteMap("duck", 32, 32)
        {
            depth = 0.9f,
            position = new Vec2(-8f, -18f)
        });
        _spawnSprite = new SpriteMap("spawnSheet", 16, 16);
        _spawnSprite.depth = 0.95f;
        list.Add(_spawnSprite);
        graphic = list;
        _editorName = "enemy spawn";
        center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
    }

    public override void Draw()
    {
        _spawnSprite.frame = 0;
        base.Draw();
    }
}
