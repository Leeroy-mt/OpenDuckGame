using System.Collections.Generic;

namespace DuckGame;

public class BigTitle : Thing
{
    private Sprite _sprite;

    private int _wait;

    private int _count;

    private int _maxCount = 50;

    private float _alpha = 1;

    private float _fartWait = 1;

    private bool _showFart;

    private bool _fade;

    private int _lerpNum;

    private readonly List<Color> _lerpColors =
    [
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
    ];

    private Color _currentColor;

    private Sprite _demo;

    public bool Fade
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
        _sprite = new("duckGameTitle");
        _demo = new("demoPro");
        graphic = _sprite;
        depth = 0.6f;
        graphic.color = Color.Black;
        centery = graphic.height / 2;
        alpha = 0;
        layer = Layer.HUD;
        _currentColor = _lerpColors[0];
    }

    public override void Draw()
    {
        Graphics.DrawRect(position + new Vec2(-300, -30), position + new Vec2(300, 30), Color.Black * 0.6f * alpha, depth - 100);
        if (_showFart)
        {
            _demo.alpha = alpha;
            _demo.depth = 0.7f;
            Graphics.Draw(_demo, x + 28, y + 32);
        }
        base.Draw();
    }

    public override void Update()
    {
        if (Main.isDemo)
        {
            _fartWait -= 0.008f;
            if (_fartWait < 0 && !_showFart)
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
            alpha -= 0.05f;
            if (alpha < 0f)
                Level.Remove(this);
        }
        else if (_wait > 30 && _count < _maxCount)
        {
            float num = _count / (float)_maxCount;
            _lerpNum = (int)(num * _lerpColors.Count - 0.01f);
            _ = _maxCount / _lerpColors.Count;
            _currentColor = Color.Lerp(_currentColor, _lerpColors[_lerpNum], 0.1f);
            _currentColor.a = (byte)(_alpha * 255);
            _alpha -= 0.02f;
            if (_alpha < 0)
                _alpha = 0;
            _count++;
        }
    }
}
