using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class JetpackSmoke : Thing
{
    private float _orbitInc = Rando.Float(5f);

    private SpriteMap _sprite2;

    private SpriteMap _sprite;

    private SpriteMap _orbiter;

    private float _life = 1f;

    private float _rotSpeed = Rando.Float(0.05f, 0.15f);

    private float _distPulseSpeed = Rando.Float(0.05f, 0.15f);

    private float _distPulse = Rando.Float(5f);

    private float s1 = 1f;

    private float s2 = 1f;

    private float lifeTake = 0.05f;

    public SpriteMap sprite => _sprite;

    public JetpackSmoke(float xpos, float ypos)
    {
        _sprite = new SpriteMap("tinySmokeTestFront", 16, 16);
        int off = Rando.Int(3) * 4;
        _sprite.AddAnimation("idle", 0.1f, true, off);
        _sprite.AddAnimation("puff", Rando.Float(0.15f, 0.25f), false, off, 1 + off, 2 + off, 3 + off);
        _orbiter = new SpriteMap("tinySmokeTestFront", 16, 16);
        off = Rando.Int(3) * 4;
        _orbiter.AddAnimation("idle", 0.1f, true, off);
        _orbiter.AddAnimation("puff", Rando.Float(0.15f, 0.25f), false, off, 1 + off, 2 + off, 3 + off);
        _sprite2 = new SpriteMap("tinySmokeTestBack", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        Init(xpos, ypos);
    }

    private void Init(float xpos, float ypos)
    {
        _orbitInc += 0.2f;
        _life = 1;
        X = xpos;
        Y = ypos;
        _sprite.SetAnimation("idle");
        _sprite.AngleDegrees = Rando.Float(360f);
        _orbiter.AngleDegrees = Rando.Float(360f);
        s1 = Rando.Float(0.8f, 1.1f);
        s2 = Rando.Float(0.8f, 1.1f);
        hSpeed = -0.4f + Rando.Float(0.8f);
        vSpeed = 0.1f + Rando.Float(0.4f);
        //vSpeed = Rando.Float(0.2f, 2);
        _life += Rando.Float(0.2f);
        float lightness = 0.6f - Rando.Float(0.2f);
        lightness = 1;
        _sprite.color = new Color(lightness, lightness, lightness);
        Depth = -0.4f;
        Alpha = 1;
        layer = Layer.Game;
    }

    public override void Initialize()
    {
    }

    public override void Update()
    {
        base.ScaleX = 1f;
        base.ScaleY = base.ScaleX;
        _orbitInc += _rotSpeed;
        _distPulse += _distPulseSpeed;
        vSpeed -= 0.01f;
        hSpeed *= 0.95f;
        _life -= lifeTake;
        if (_life < 0f && _sprite.currentAnimation != "puff")
        {
            _sprite.SetAnimation("puff");
        }
        if (_sprite.currentAnimation == "puff" && _sprite.finished)
        {
            Level.Remove(this);
        }
        base.X += hSpeed;
        base.Y += vSpeed;
    }

    public override void Draw()
    {
        float distPulse = (float)Math.Sin(_distPulse);
        float xOff = (0f - (float)Math.Sin(_orbitInc) * distPulse) * s1;
        float yOff = (float)Math.Cos(_orbitInc) * distPulse * s1;
        _sprite.imageIndex = _sprite.imageIndex;
        _sprite.Depth = base.Depth;
        _sprite.Scale = new Vector2(s1);
        _sprite.Center = Center;
        Graphics.Draw(_sprite, base.X + xOff, base.Y + yOff);
        _sprite2.imageIndex = _sprite.imageIndex;
        _sprite2.Angle = _sprite.Angle;
        _sprite2.Depth = -0.5f;
        _sprite2.Scale = _sprite.Scale;
        _sprite2.Center = Center;
        float lightness = 0.6f - Rando.Float(0.2f);
        lightness = 0.4f;
        _sprite2.color = new Color(lightness, lightness, lightness);
        Graphics.Draw(_sprite2, base.X + xOff, base.Y + yOff);
        _orbiter.imageIndex = _sprite.imageIndex;
        _orbiter.color = _sprite.color;
        _orbiter.Depth = base.Depth;
        _orbiter.Scale = new Vector2(s2);
        _orbiter.Center = Center;
        Graphics.Draw(_orbiter, base.X - xOff, base.Y - yOff);
        _sprite2.imageIndex = _orbiter.imageIndex;
        _sprite2.Angle = _orbiter.Angle;
        _sprite2.Depth = -0.5f;
        _sprite2.Scale = _orbiter.Scale;
        _sprite2.Center = Center;
        _sprite2.color = new Color(lightness, lightness, lightness);
        Graphics.Draw(_sprite2, base.X - xOff, base.Y - yOff);
    }
}
