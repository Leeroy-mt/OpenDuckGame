using Microsoft.Xna.Framework;

namespace DuckGame;

public class EyeCloseWing : Thing
{
    private SpriteMap _sprite;

    private float _move;

    private int _dir;

    private Duck _closer;

    public EyeCloseWing(float xpos, float ypos, int dir, SpriteMap s, Duck own, Duck closer)
        : base(xpos, ypos)
    {
        _sprite = s.CloneMap();
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        _dir = dir;
        base.Depth = 0.9f;
        if (_dir < 0)
        {
            base.AngleDegrees = 70f;
        }
        else
        {
            base.AngleDegrees = 120f;
        }
        owner = own;
        _closer = closer;
        if (_dir < 0)
        {
            base.X += 14f;
        }
    }

    public override void Update()
    {
        float inc = 0.3f;
        base.X += (float)_dir * inc;
        _move += inc;
        if (_dir < 0)
        {
            base.AngleDegrees += 2f;
        }
        else
        {
            base.AngleDegrees -= 2f;
        }
        if (_move > 4f)
        {
            _closer.eyesClosed = true;
        }
        if (_move > 8f)
        {
            Level.Remove(this);
            (_owner as Duck).closingEyes = false;
        }
    }

    public override void Draw()
    {
        int frame = _sprite.frame;
        _sprite.flipV = _dir <= 0;
        _sprite.flipH = false;
        _sprite.frame = 18;
        base.Draw();
        _sprite.frame = frame;
    }
}
