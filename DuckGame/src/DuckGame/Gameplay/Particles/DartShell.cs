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
        Center = new Vec2(8f, 8f);
        _bounceSound = "";
        _rotSpeed = rotSpeed;
        base.Depth = 0.3f;
    }

    public override void Update()
    {
        base.Update();
        Angle += _rotSpeed;
        if (vSpeed < 0f || _grounded)
        {
            _die = true;
        }
        if (_die)
        {
            base.Alpha -= 0.05f;
        }
        if (base.Alpha <= 0f)
        {
            Level.Remove(this);
        }
    }
}
