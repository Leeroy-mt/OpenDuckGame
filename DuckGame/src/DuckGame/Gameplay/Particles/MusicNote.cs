using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class MusicNote : Thing
{
    private Color _color;

    private SinWaveManualUpdate _sin;

    private float _size;

    private float _speed;

    private SpriteMap _sprite;

    private Vector2 _dir;

    public MusicNote(float xpos, float ypos, Vector2 dir)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("notes", 8, 8);
        _sprite.frame = Rando.Int(1);
        _sprite.CenterOrigin();
        int num = Rando.ChooseInt(0, 1, 2, 3);
        if (num == 0)
        {
            _color = Color.Violet;
        }
        if (num == 1)
        {
            _color = Color.SkyBlue;
        }
        if (num == 2)
        {
            _color = Color.Wheat;
        }
        if (num == 4)
        {
            _color = Color.GreenYellow;
        }
        _dir = dir;
        float mul = 1f;
        if (Rando.Float(1f) <= 0.5f)
        {
            mul = -1f;
        }
        _sin = new SinWaveManualUpdate(0.03f + Rando.Float(0.1f), mul * ((float)Math.PI * 2f));
        _size = 3f + Rando.Float(6f);
        _speed = 0.8f + Rando.Float(1.4f);
        base.Depth = 0.95f;
        base.Scale = new Vector2(0.1f, 0.1f);
    }

    public override void Update()
    {
        _sin.Update();
        base.X += _dir.X;
        Vector2 s = base.Scale;
        s.X = (s.Y = Lerp.Float(s.X, 1f, 0.05f));
        base.Scale = s;
        if (base.Scale.X > 0.9f)
        {
            base.Alpha -= 0.01f;
            if (base.Alpha <= 0f)
            {
                Level.Remove(this);
            }
        }
    }

    public override void Draw()
    {
        Vector2 pos = Position;
        pos.Y += _sin.value * _size;
        _sprite.Alpha = base.Alpha;
        _sprite.Scale = base.Scale;
        Graphics.Draw(_sprite, pos.X, pos.Y);
    }
}
