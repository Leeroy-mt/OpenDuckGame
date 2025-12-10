namespace DuckGame;

public class DartShell : PhysicsParticle
{
    private SpriteMap _sprite;

    private float _rotSpeed;

    private bool _die;

    public DartShell(float xpos, float ypos, float rotSpeed, bool flip)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("dart", 16, 16);
        _sprite.flipH = flip;
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        _bounceSound = "";
        _rotSpeed = rotSpeed;
        base.depth = 0.3f;
    }

    public override void Update()
    {
        base.Update();
        angle += _rotSpeed;
        if (vSpeed < 0f || _grounded)
        {
            _die = true;
        }
        if (_die)
        {
            base.alpha -= 0.05f;
        }
        if (base.alpha <= 0f)
        {
            Level.Remove(this);
        }
    }
}
