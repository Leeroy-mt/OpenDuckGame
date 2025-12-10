using System.Collections.Generic;

namespace DuckGame;

public class DuckChannelLogo : Thing
{
    private Sprite _duck;

    private Sprite _channel;

    private Sprite _five;

    private float _duckLerp;

    private float _channelLerp;

    private float _fiveLerp;

    private List<float> _swipeLines = new List<float>();

    private List<float> _swipeSpeeds = new List<float>();

    private float _slideOutWait;

    private float _transitionWait;

    private bool _playSwipe;

    private bool _doTransition;

    public bool doTransition => _doTransition;

    public void PlaySwipe()
    {
        if (!_playSwipe)
        {
            _playSwipe = true;
            _duckLerp = 0f;
            _channelLerp = 0f;
            _slideOutWait = 0f;
            _fiveLerp = 0f;
            _doTransition = false;
            _transitionWait = 0f;
            for (int i = 0; i < 10; i++)
            {
                _swipeLines[i] = Rando.Float(0.1f);
                _swipeSpeeds[i] = Rando.Float(0.01f, 0.012f);
            }
        }
    }

    public DuckChannelLogo()
    {
        _duck = new Sprite("newsTitleDuck");
        _channel = new Sprite("newsTitleChannel");
        _five = new Sprite("newsTitle5");
        base.layer = Layer.HUD;
        for (int i = 0; i < 10; i++)
        {
            _swipeLines.Add(Rando.Float(0.1f));
            _swipeSpeeds.Add(Rando.Float(0.01f, 0.012f));
        }
    }

    public override void Update()
    {
        if (_playSwipe)
        {
            _transitionWait += 0.02f;
            if (_transitionWait > 1f)
            {
                _doTransition = true;
            }
            if (_slideOutWait < 1f)
            {
                _duckLerp = Lerp.FloatSmooth(_duckLerp, 1f, 0.1f, 1.1f);
                _channelLerp = Lerp.FloatSmooth(_channelLerp, 1f, 0.1f, 1.1f);
                _fiveLerp = Lerp.FloatSmooth(_fiveLerp, 1f, 0.1f, 1.1f);
                _slideOutWait += 0.012f;
            }
            else
            {
                _duckLerp = Lerp.FloatSmooth(_duckLerp, 0f, 0.1f, 1.1f);
                _channelLerp = Lerp.FloatSmooth(_channelLerp, 0f, 0.1f, 1.1f);
                _fiveLerp = Lerp.FloatSmooth(_fiveLerp, 0f, 0.1f, 1.1f);
                if (_duckLerp < 0.01f)
                {
                    _playSwipe = false;
                }
            }
            for (int i = 0; i < _swipeLines.Count; i++)
            {
                _swipeLines[i] = Lerp.Float(_swipeLines[i], 1f, _swipeSpeeds[i]);
            }
        }
        else
        {
            _doTransition = false;
        }
    }

    public override void Draw()
    {
        Vec2 posOffset = new Vec2(10f, 12f);
        Vec2 duckOffset = new Vec2(-200f * (1f - _duckLerp), 0f);
        Vec2 channelOffset = new Vec2(200f * (1f - _channelLerp), 0f);
        Vec2 fiveOffset = new Vec2(300f * (1f - _channelLerp), 0f);
        _duck.depth = 0.85f;
        Graphics.Draw(_duck, posOffset.x + 80f + duckOffset.x, posOffset.y + 60f + duckOffset.y);
        _channel.depth = 0.86f;
        Graphics.Draw(_channel, posOffset.x + 64f + channelOffset.x, posOffset.y + 74f + channelOffset.y);
        _five.depth = 0.85f;
        Graphics.Draw(_five, posOffset.x + 144f + fiveOffset.x, posOffset.y + 64f + fiveOffset.y);
        Vec2 swipeStart = new Vec2(30f, 20f);
        float lineWidth = 500f;
        float lineHeight = 16f;
        float offRight = 600f;
        for (int i = 0; i < _swipeLines.Count; i++)
        {
            float xOff = _swipeLines[i] * -1200f;
            Graphics.DrawRect(new Vec2(swipeStart.x + offRight + xOff, swipeStart.y + (float)i * lineHeight), new Vec2(swipeStart.x + lineWidth + offRight + xOff, swipeStart.y + (float)i * lineHeight + lineHeight), Color.Black, 0.83f);
        }
    }
}
