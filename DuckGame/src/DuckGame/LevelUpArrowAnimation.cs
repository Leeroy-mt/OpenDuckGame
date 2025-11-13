using System;

namespace DuckGame;

public class LevelUpArrowAnimation : EffectAnimation
{
	private float _startWait;

	private float _alph = 2f;

	private float _vel;

	public LevelUpArrowAnimation(Vec2 pos)
		: base(pos, new SpriteMap("levelUpArrow", 16, 16), 0.9f)
	{
		base.layer = Layer.HUD;
		base.alpha = 0f;
		_startWait = Rando.Float(2.5f);
		_sprite.depth = 1f;
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
			base.y += _vel;
			_alph -= 0.1f;
			base.alpha = Math.Min(_alph, 1f);
			if (base.alpha <= 0f)
			{
				Level.Remove(this);
			}
		}
		base.Update();
	}
}
