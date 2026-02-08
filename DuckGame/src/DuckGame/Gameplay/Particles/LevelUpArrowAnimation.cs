using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class LevelUpArrowAnimation : EffectAnimation
{
    private float _startWait;

    private float _alph = 2f;

    private float _vel;

    public LevelUpArrowAnimation(Vector2 pos)
        : base(pos, new SpriteMap("levelUpArrow", 16, 16), 0.9f)
    {
        base.layer = Layer.HUD;
        base.Alpha = 0f;
        _startWait = Rando.Float(2.5f);
        _sprite.Depth = 1f;
    }

    public override void Update()
    {
        if (_startWait > 0f)
        {
            _startWait -= 0.1f;
        }
        else
        {
            _vel -= 0.1f;
            base.Y += _vel;
            _alph -= 0.1f;
            base.Alpha = Math.Min(_alph, 1f);
            if (base.Alpha <= 0f)
            {
                Level.Remove(this);
            }
        }
        base.Update();
    }
}
