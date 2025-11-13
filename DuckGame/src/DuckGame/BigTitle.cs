using System.Collections.Generic;

namespace DuckGame;

public class BigTitle : Thing
{
	private Sprite _sprite;

	private int _wait;

	private int _count;

	private int _maxCount = 50;

	private float _alpha = 1f;

	private float _fartWait = 1f;

	private bool _showFart;

	private bool _fade;

	private int _lerpNum;

	private List<Color> _lerpColors = new List<Color>
	{
		Color.White,
		Color.PaleVioletRed,
		Color.Red,
		Color.OrangeRed,
		Color.Orange,
		Color.Yellow,
		Color.YellowGreen,
		Color.Green,
		Color.BlueViolet,
		Color.Purple,
		Color.Pink
	};

	private Color _currentColor;

	private Sprite _demo;

	public bool fade
	{
		get
		{
			return _fade;
		}
		set
		{
			_fade = value;
		}
	}

	public BigTitle()
	{
		_sprite = new Sprite("duckGameTitle");
		_demo = new Sprite("demoPro");
		graphic = _sprite;
		base.depth = 0.6f;
		graphic.color = Color.Black;
		base.centery = graphic.height / 2;
		base.alpha = 0f;
		base.layer = Layer.HUD;
		_currentColor = _lerpColors[0];
	}

	public override void Initialize()
	{
	}

	public override void Draw()
	{
		Graphics.DrawRect(position + new Vec2(-300f, -30f), position + new Vec2(300f, 30f), Color.Black * 0.6f * base.alpha, base.depth - 100);
		if (_showFart)
		{
			_demo.alpha = base.alpha;
			_demo.depth = 0.7f;
			Graphics.Draw(_demo, base.x + 28f, base.y + 32f);
		}
		base.Draw();
	}

	public override void Update()
	{
		if (Main.isDemo)
		{
			_fartWait -= 0.008f;
			if (_fartWait < 0f && !_showFart)
			{
				_showFart = true;
				SFX.Play("fart" + Rando.Int(3));
			}
		}
		_wait++;
		_ = _wait;
		_ = 30;
		if (_fade)
		{
			base.alpha -= 0.05f;
			if (base.alpha < 0f)
			{
				Level.Remove(this);
			}
		}
		else if (_wait > 30 && _count < _maxCount)
		{
			float num = (float)_count / (float)_maxCount;
			_lerpNum = (int)(num * (float)_lerpColors.Count - 0.01f);
			_ = _maxCount / _lerpColors.Count;
			_currentColor = Color.Lerp(_currentColor, _lerpColors[_lerpNum], 0.1f);
			_currentColor.a = (byte)(_alpha * 255f);
			_alpha -= 0.02f;
			if (_alpha < 0f)
			{
				_alpha = 0f;
			}
			_count++;
		}
	}
}
