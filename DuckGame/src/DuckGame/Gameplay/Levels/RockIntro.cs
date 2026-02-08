using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class RockIntro : Level, IHaveAVirtualTransition, IOnlyTransitionIn
{
    private Sprite _bigDome;

    private Sprite _smallDome;

    private Sprite _smallPillar;

    private SpriteMap _domeBleachers;

    private VirtualBackground _virtualBackground;

    private Sprite _cornerWedge;

    private Sprite _intermissionText;

    private float _intermissionSlide;

    private Level _next;

    private Layer _subHUD;

    private float _panWait = 1f;

    private float _yScrollVel;

    private float _afterDownWait = 1f;

    private float rotter;

    private float _yScroll = 1f;

    public override string networkIdentifier => "@ROCKINTRO";

    private bool ready => true;

    public RockIntro(Level next)
    {
        _next = next;
    }

    public override void Initialize()
    {
        _bigDome = new Sprite("dome");
        _bigDome.CenterOrigin();
        _smallDome = new Sprite("domeSmall");
        _smallDome.CenterOrigin();
        _smallPillar = new Sprite("domePillar");
        _smallPillar.Center = new Vector2(_smallPillar.w / 2, 0f);
        _domeBleachers = new SpriteMap("domeBleachers", 25, 20);
        _domeBleachers.Center = new Vector2(13f, 13f);
        _virtualBackground = new VirtualBackground(0f, 0f, null);
        Level.Add(_virtualBackground);
        _cornerWedge = new Sprite("rockThrow/cornerWedge");
        _intermissionText = new Sprite("rockThrow/intermission");
        _subHUD = new Layer("SUBHUD", -85);
        _subHUD.allowTallAspect = true;
        Layer.Add(_subHUD);
        Layer.Foreground.camera = new Camera(0f, 0f, 320f, 320f / Resolution.current.aspect);
    }

    public override void Update()
    {
        Music.volume = Lerp.Float(Music.volume, 0f, 0.008f);
        if (Music.volume <= 0f)
        {
            Music.Stop();
        }
        _panWait -= 0.04f;
        if (!(_panWait < 0f))
        {
            return;
        }
        _yScrollVel += ((_yScroll < 0.4f) ? (-0.0001f) : 0.0008f);
        if (_yScrollVel > 0.01f)
        {
            _yScrollVel = 0.01f;
        }
        if (_yScrollVel < 0f)
        {
            _yScrollVel = 0f;
        }
        _yScroll -= _yScrollVel;
        _virtualBackground.layer.fade = Lerp.Float(_virtualBackground.layer.fade, 0.5f, 0.01f);
        if (!(_yScroll < 0.4f))
        {
            return;
        }
        _afterDownWait -= 0.05f;
        if (_afterDownWait < 0f)
        {
            _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 1f, 0.1f, 1.1f);
            _subHUD.fade -= 0.02f;
            if (_subHUD.fade < 0f)
            {
                _subHUD.fade = 0f;
            }
            _virtualBackground.layer.fade -= 0.02f;
            if (_virtualBackground.layer.fade < 0f)
            {
                _virtualBackground.layer.fade = 0f;
            }
            if (Network.isServer && _subHUD.fade <= 0f && _intermissionSlide >= 0.99f && ready)
            {
                Music.volume = 1f;
                Level.current = new RockScoreboard(_next);
            }
        }
    }

    public override void PostDrawLayer(Layer l)
    {
        if (l == _subHUD)
        {
            float maxYPos = 160f;
            float ypos = _yScroll * maxYPos;
            _virtualBackground.parallax.Y = (0f - maxYPos) * (1f - ypos / maxYPos);
            _bigDome.Depth = 0.5f;
            Graphics.Draw(_bigDome, 160f, 130f + ypos);
            float degrot = 45f;
            float rot = Maths.DegToRad(degrot);
            float startRot = Maths.DegToRad(25f + rotter);
            rotter -= 0.3f;
            if (rotter <= 0f - degrot)
            {
                rotter += degrot;
            }
            for (int i = 0; i < 8; i++)
            {
                if (i == 0 || i > 4)
                {
                    _smallDome.Depth = 0.6f;
                }
                else
                {
                    _smallDome.Depth = 0.4f;
                }
                Vector2 pos = new Vector2((float)Math.Cos(startRot + (float)i * rot), (0f - (float)Math.Sin(startRot + (float)i * rot)) * (0.4f * (1f - ypos / maxYPos)));
                Vector2 drawPos = new Vector2(160f, 130f + ypos) + pos * 100f;
                Graphics.Draw(_smallDome, drawPos.X, drawPos.Y - 30f);
                _smallPillar.Depth = _smallDome.Depth;
                Graphics.Draw(_smallPillar, drawPos.X, drawPos.Y - 11f);
                _domeBleachers.Depth = _smallDome.Depth + 1;
                _domeBleachers.frame = 7 - (i + 5) % 8;
                Graphics.Draw(_domeBleachers, drawPos.X, drawPos.Y - 30f);
            }
        }
        else if (l == Layer.HUD)
        {
            _cornerWedge.flipH = false;
            _cornerWedge.Depth = 0.7f;
            if (_intermissionSlide > 0.01f)
            {
                float xpos = -320f + _intermissionSlide * 320f;
                float ypos2 = 60f;
                Graphics.DrawRect(new Vector2(xpos, ypos2), new Vector2(xpos + 320f, ypos2 + 30f), Color.Black, 0.9f);
                xpos = 320f - _intermissionSlide * 320f;
                ypos2 = 60f;
                Graphics.DrawRect(new Vector2(xpos, ypos2 + 30f), new Vector2(xpos + 320f, ypos2 + 60f), Color.Black, 0.9f);
                Graphics.Draw(_intermissionText, -320f + _intermissionSlide * 336f, ypos2 + 18f);
                _intermissionText.Depth = 0.91f;
            }
        }
        base.PostDrawLayer(l);
    }

    public override void OnMessage(NetMessage message)
    {
        if (!(message is NMScoresReceived))
        {
            return;
        }
        foreach (Profile profile in DuckNetwork.GetProfiles(message.connection))
        {
            profile.ready = true;
        }
    }
}
