using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class HotnessAnimation
{
    private Sprite _redBar;

    private Sprite _blueBar;

    private SpriteMap _icon;

    private BitmapFont _font;

    private List<int> _sampleCool = new List<int>();

    private List<int> _cool = new List<int>();

    private List<int> _lastFrame = new List<int>();

    private List<float> _upScale = new List<float>();

    private List<string> _hotnessStrings = new List<string> { "Absolute Zero", "Icy Moon", "Antarctica", "Ice Cube", "Ice Cream", "Coffee", "Fire", "A Volcanic Eruption", "The Sun" };

    private List<int> _tempMap = new List<int>
    {
        -250, -200, -100, -40, -20, 0, 100, 1200, 4000, 4500,
        5000
    };

    private bool _readyToTalk;

    private float _wait;

    public bool ready => _readyToTalk;

    public HotnessAnimation()
    {
        _redBar = new Sprite("newscast/redBar");
        _blueBar = new Sprite("newscast/blueBar");
        _icon = new SpriteMap("newscast/hotness", 18, 18);
        _icon.CenterOrigin();
        _font = new BitmapFont("biosFontDegree", 8);
        for (int i = 0; i < Profiles.active.Count; i++)
        {
            if (Profiles.active.Count > i)
            {
                int val = Profiles.active[i].endOfRoundStats.GetProfileScore();
                if (val < 0)
                {
                    val = 0;
                }
                _sampleCool.Add(val);
            }
            else
            {
                _sampleCool.Add(0);
            }
            _cool.Add(-50);
            _lastFrame.Add(0);
            _upScale.Add(0f);
        }
    }

    public void Draw()
    {
        if (_wait > 1f)
        {
            bool allDone = true;
            for (int j = 0; j < _cool.Count; j++)
            {
                if (_sampleCool[j] < _cool[j])
                {
                    _cool[j]--;
                    allDone = false;
                }
                else if (_sampleCool[j] > _cool[j])
                {
                    _cool[j]++;
                    allDone = false;
                }
                if (_upScale[j] > 0f)
                {
                    _upScale[j] -= 0.05f;
                }
            }
            if (allDone)
            {
                _wait += 0.015f;
                if (_wait > 2f)
                {
                    _readyToTalk = true;
                }
            }
        }
        else
        {
            _wait += 0.01f;
        }
        _redBar.Depth = 0.2f;
        Graphics.Draw(_redBar, 30f, 25f);
        _font.Depth = 0.25f;
        if (DG.isHalloween)
        {
            _font.Draw("SPOOKY  REPORT", 44f, 28f, Color.White, 0.25f);
        }
        else
        {
            _font.Draw("HOTNESS REPORT", 44f, 28f, Color.White, 0.25f);
        }
        _blueBar.Depth = 0.1f;
        Graphics.Draw(_blueBar, 30f, 18f);
        Graphics.DrawRect(new Vector2(20f, 135f), new Vector2(260f, 160f), new Color(12, 90, 182), 0.1f);
        Vector2 duckAreaTL = new Vector2(60f, 50f);
        Vector2 duckAreaBR = new Vector2(200f, 150f);
        Vector2 duckAreaSize = new Vector2(duckAreaBR.X - duckAreaTL.X, duckAreaBR.Y - duckAreaTL.Y);
        List<Profile> profiles = Profiles.active;
        int i = 0;
        foreach (Profile p in profiles)
        {
            float centerAdd = 0f;
            centerAdd = ((profiles.Count == 1) ? (duckAreaSize.X / 2f) : ((profiles.Count != 2) ? ((float)i * (duckAreaSize.X / (float)(profiles.Count - 1))) : (duckAreaSize.X / 2f - duckAreaSize.X / 4f + (float)i * (duckAreaSize.X / 2f))));
            float normalizedHotness = (float)(_cool[i] + 50) / 250f;
            float zoneSize = 1f / (float)(_tempMap.Count - 2);
            int curZone = (int)(normalizedHotness * (float)(_tempMap.Count - 2));
            if (curZone < 0)
            {
                curZone = 0;
            }
            _ = _tempMap[curZone];
            float travel = Maths.NormalizeSection(normalizedHotness, zoneSize * (float)curZone, zoneSize * (float)(curZone + 1));
            int temp = (int)((float)_tempMap[curZone] + (float)(_tempMap[curZone + 1] - _tempMap[curZone]) * travel);
            float barMaxHeight = 50f;
            float pedHeight = normalizedHotness + 0.28f;
            float xpos = duckAreaTL.X + centerAdd;
            float ypos = duckAreaBR.Y - 32f - pedHeight * barMaxHeight;
            p.persona.sprite.Depth = 0.3f;
            p.persona.sprite.color = Color.White;
            Graphics.Draw(p.persona.sprite, 0, xpos, ypos);
            Vector2 offset = DuckRig.GetHatPoint(p.persona.sprite.imageIndex);
            p.team.hat.Depth = 0.31f;
            p.team.hat.Center = new Vector2(16f, 16f) + p.team.hatOffset;
            Graphics.Draw(p.team.hat, p.team.hat.frame, xpos + offset.X, ypos + offset.Y);
            if (_cool.Count > 4)
            {
                Graphics.DrawRect(new Vector2(xpos - 9f, ypos + 16f), new Vector2(xpos + 9f, 160f), p.persona.colorUsable, 0.05f);
            }
            else
            {
                Graphics.DrawRect(new Vector2(xpos - 17f, ypos + 16f), new Vector2(xpos + 16f, 160f), p.persona.colorUsable, 0.05f);
            }
            string text = temp + "=";
            _font.Depth = 0.25f;
            if (_cool.Count > 4)
            {
                _font.Scale = new Vector2(0.5f);
            }
            _font.Draw(text, new Vector2(xpos - _font.GetWidth(text) / 2f + 3f, 140f), Color.White, 0.25f);
            _font.Scale = new Vector2(1f);
            _icon.Depth = 0.3f;
            _icon.frame = (int)Math.Floor(normalizedHotness * 8.99f);
            if (_icon.frame != _lastFrame[i])
            {
                _lastFrame[i] = _icon.frame;
                _upScale[i] = 0.5f;
            }
            _icon.Scale = new Vector2(1f + _upScale[i]);
            Graphics.Draw(_icon, xpos, ypos + 28f);
            i++;
        }
    }
}
