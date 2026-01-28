namespace DuckGame;

public class OpenPresent : PhysicsParticle
{
    private SpriteMap _sprite;

    public OpenPresent(float xpos, float ypos, int frame)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("presents", 16, 16);
        _sprite.frame = frame + 8;
        graphic = _sprite;
        Center = new Vec2(8f, 13f);
        hSpeed = 0f;
        vSpeed = 0f;
        _bounceEfficiency = 0f;
        base.Depth = 0.9f;
        _life = 5f;
    }

    public override void Update()
    {
        base.Update();
    }
}
