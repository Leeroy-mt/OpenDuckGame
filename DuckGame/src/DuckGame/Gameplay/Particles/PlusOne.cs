using System;

namespace DuckGame;

public class PlusOne : Thing
{
    public Duck _duck;

    private BitmapFont _font;

    private Profile _profile;

    private float pulse;

    private bool _temp;

    private float _wait = 1f;

    private bool _testMode;

    private int _num = 1;

    public PlusOne(float xpos, float ypos, Profile p, bool temp = false, bool testMode = false)
        : base(xpos, ypos)
    {
        _font = new BitmapFont("biosFont", 8);
        _profile = p;
        _temp = temp;
        base.layer = Layer.Blocks;
        base.depth = 0.9f;
        _testMode = testMode;
    }

    public override void Initialize()
    {
        if (!_testMode && _profile != null)
        {
            if (Teams.active.Count > 1 && Network.isActive && _profile.connection == DuckNetwork.localConnection)
            {
                DuckNetwork.GiveXP("Rounds Won", 1, 4, 4, 10, 20);
            }
            _profile.stats.lastWon = DateTime.Now;
            _profile.stats.matchesWon++;
        }
        base.Initialize();
    }

    public void Pulse()
    {
        _wait = 1f;
        pulse = 1f;
        _num++;
    }

    public override void Update()
    {
        pulse = Lerp.FloatSmooth(pulse, 0f, 0.1f);
        if (pulse < 0.03f)
        {
            pulse = 0f;
        }
        if (!_temp)
        {
            _wait -= 0.01f;
        }
        if (_wait < 0f)
        {
            Level.Remove(this);
            if (_duck != null)
            {
                _duck.currentPlusOne = null;
            }
        }
    }

    public override void Draw()
    {
        if (_profile != null && _profile.persona != null && !(base.anchor == null))
        {
            position = base.anchor.position;
            _font.scale = new Vec2(1f + pulse * 0.5f);
            _num = 1;
            string text = "+" + _num;
            float xposit = base.x - _font.GetWidth(text) / 2f;
            _font.Draw(text, xposit - 1f, base.y - 1f, Color.Black, 0.8f);
            _font.Draw(text, xposit + 1f, base.y - 1f, Color.Black, 0.8f);
            _font.Draw(text, xposit - 1f, base.y + 1f, Color.Black, 0.8f);
            _font.Draw(text, xposit + 1f, base.y + 1f, Color.Black, 0.8f);
            Color c = new Color((byte)_profile.persona.color.x, (byte)_profile.persona.color.y, (byte)_profile.persona.color.z);
            _font.Draw(text, xposit, base.y, c, 0.9f);
        }
    }
}
