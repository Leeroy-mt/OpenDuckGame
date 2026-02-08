using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class HighlightLevel : Level
{
    private Sprite _pumpkin;

    private Sprite _tv;

    private Sprite _logo;

    private Sprite _rockImage2;

    private Sprite _background;

    private Sprite _newsTable;

    private SpriteMap _duck;

    private SpriteMap _duckBeak;

    private SpriteMap _tie;

    private float _done = 1f;

    private Vector2 _tl;

    private Vector2 _size;

    private float _waitZoom = 1f;

    private Vector2 _imageDraw = Vector2.Zero;

    private float _tvFade = 1f;

    private TVState _state;

    private TVState _desiredState;

    private Vector2 _cameraOffset = new Vector2(0f, 0f);

    private Teleprompter _talker;

    public static bool didSkip;

    private bool _firedSkipLogic;

    private bool _endOfHighlights;

    private bool _testMode;

    public static int currentTie;

    public static List<DuckStory> _stories;

    private Layer _blurLayer;

    private static Sprite _image;

    private DuckChannelLogo _transition;

    private HotnessAnimation _hotness;

    private int _interviewIndex;

    private bool _skip;

    public static bool _cancelSkip;

    private bool _askedQuestion;

    private float _interviewWait = 1f;

    private float _wait;

    public HighlightLevel(bool endOfHighlights = false, bool testMode = false)
    {
        _centeredView = true;
        _endOfHighlights = endOfHighlights;
        _testMode = testMode;
    }

    public void OnHotnessImage(DuckStory story)
    {
        story.OnStoryBegin -= OnHotnessImage;
        _image = new Sprite("newscast/hotnessImage");
    }

    public void OnInterviewImage(DuckStory story)
    {
        story.OnStoryBegin -= OnInterviewImage;
        _image = new SpriteMap("interviewBox", 63, 47);
    }

    public void OnHotnessStory(DuckStory story)
    {
        story.OnStoryBegin -= OnHotnessStory;
        _desiredState = TVState.ShowHotness;
        _talker.Pause();
    }

    public void OnHotnessEnd(DuckStory story)
    {
        story.OnStoryBegin -= OnHotnessEnd;
        _desiredState = TVState.ShowNewscaster;
        _image = null;
    }

    public void OnInterview(DuckStory story)
    {
        story.OnStoryBegin -= OnHotnessStory;
        _desiredState = TVState.ShowInterview;
        _talker.Pause();
    }

    public override void Initialize()
    {
        if (_testMode)
        {
            _endOfHighlights = true;
            Options.Data.sfxVolume = 0f;
            DuckStory d = new DuckStory();
            d.text = "|SUAVE||RED|John Mallard|WHITE| here dancing|CALM| for you |EXCITED|and wearing ties!";
            _stories = new List<DuckStory>();
            for (int i = 0; i < 9999; i++)
            {
                _stories.Add(d);
            }
        }
        _cancelSkip = false;
        _tv = new Sprite("bigTV");
        _duck = new SpriteMap("newsDuck", 140, 100);
        _duckBeak = new SpriteMap("newsDuckBeak", 140, 100);
        _tie = new SpriteMap("ties", 12, 21);
        _pumpkin = new Sprite("pump");
        _pumpkin.CenterOrigin();
        _newsTable = new Sprite("newsTable");
        _logo = new Sprite("duckGameTitle");
        _logo.CenterOrigin();
        _background = new Sprite("duckChannelBackground");
        _blurLayer = new Layer("BLUR", Layer.HUD.depth + 5, Layer.HUD.camera);
        Layer.Add(_blurLayer);
        _blurLayer.effect = Content.Load<MTEffect>("Shaders/blur");
        _transition = new DuckChannelLogo();
        Level.Add(_transition);
        _tl = new Vector2(30f, 32f);
        _size = new Vector2(207f, 141f);
        _rockImage2 = new Sprite(RockScoreboard.finalImage);
        _talker = new Teleprompter(0f, 0f, _duck);
        Teleprompter talker = _talker;
        bool active = (_talker.visible = false);
        talker.active = active;
        Level.Add(_talker);
        if (didSkip)
        {
            _skip = true;
        }
        if (_endOfHighlights)
        {
            _state = TVState.ShowNewscaster;
            _desiredState = _state;
        }
        else
        {
            _image = null;
            currentTie = Rando.Int(15);
            Music.Play("SportsCap");
            _stories = DuckNews.CalculateStories();
        }
        _hotness = new HotnessAnimation();
        _tie.frame = currentTie;
        int i2;
        for (i2 = 0; i2 < _stories.Count; i2++)
        {
            bool num = _stories[i2].text == "%CUEHIGHLIGHTS%";
            if (_stories[i2].text == "CUE%HOTNESSIMAGE%")
            {
                _stories[i2].OnStoryBegin += OnHotnessImage;
            }
            if (_stories[i2].text == "CUE%CUEHOTNESS%")
            {
                _stories[i2].OnStoryBegin += OnHotnessStory;
            }
            if (_stories[i2].text == "CUE%ENDHOTNESS%")
            {
                _stories[i2].OnStoryBegin += OnHotnessEnd;
            }
            if (_stories[i2].text == "CUE%INTERVIEWIMAGE%")
            {
                _stories[i2].OnStoryBegin += OnInterviewImage;
            }
            if (_stories[i2].text == "CUE%CUEINTERVIEW%")
            {
                _interviewIndex = i2;
                _stories[i2].OnStoryBegin += OnInterview;
            }
            if (!num)
            {
                _talker.ReadLine(_stories[i2]);
            }
            _stories.RemoveAt(i2);
            if (num)
            {
                break;
            }
            i2--;
        }
        Vote.OpenVoting("SKIP", "START");
        List<Team> teams = new List<Team>();
        foreach (Team t in Teams.all)
        {
            if (t.activeProfiles.Count > 0)
            {
                teams.Add(t);
            }
        }
        foreach (Team item in teams)
        {
            foreach (Profile activeProfile in item.activeProfiles)
            {
                Vote.RegisterVote(activeProfile, VoteType.None);
            }
        }
    }

    public void DoSkip()
    {
        if (!_endOfHighlights)
        {
            _talker.ClearLines();
            int i;
            for (i = 0; i < _stories.Count; i++)
            {
                bool num = _stories[i].text == "%CUEHIGHLIGHTS%";
                if (!num)
                {
                    _talker.ReadLine(_stories[i]);
                }
                _stories.RemoveAt(i);
                if (num)
                {
                    break;
                }
                i--;
            }
        }
        _talker.SkipToClose();
    }

    public override void Update()
    {
        if (_testMode)
        {
            _wait += Maths.IncFrameTimer();
            if (Keyboard.Pressed(Keys.F5) || (double)_wait > 0.1)
            {
                _wait = 0f;
                try
                {
                    _tie = new SpriteMap(ContentPack.LoadTexture2D("tieTest.png"), 64, 64);
                    _tie.Center = new Vector2(26f, 27f);
                }
                catch (Exception)
                {
                }
            }
        }
        Graphics.fadeAdd = Lerp.Float(Graphics.fadeAdd, 0f, 0.01f);
        if (Main.isDemo && _skip && !_firedSkipLogic)
        {
            _firedSkipLogic = true;
            Vote.CloseVoting();
            HUD.CloseAllCorners();
            DoSkip();
        }
        if (Graphics.fade > 0.99f && !_skip && Vote.Passed(VoteType.Skip))
        {
            _skip = true;
        }
        if (_talker.finished || (!_cancelSkip && _skip && !Main.isDemo))
        {
            _done -= 0.04f;
        }
        Graphics.fade = Lerp.Float(Graphics.fade, (_done < 0f) ? 0f : 1f, 0.02f);
        if (Graphics.fade < 0.01f && (_talker.finished || _skip))
        {
            if (_endOfHighlights || _skip)
            {
                Vote.CloseVoting();
                Level.current = new RockScoreboard(RockScoreboard.returnLevel, ScoreBoardMode.ShowWinner, afterHighlights: true);
            }
            else
            {
                Level.current = new HighlightPlayback(4);
            }
        }
        if (_state == TVState.ShowPedestals)
        {
            _waitZoom -= 0.008f;
            if (_waitZoom < 0.01f)
            {
                _waitZoom = 0f;
                _desiredState = TVState.ShowNewscaster;
            }
        }
        if (_state == TVState.ShowHotness && _hotness.ready)
        {
            _talker.Resume();
        }
        if (_state == TVState.ShowInterview)
        {
            _interviewWait -= 0.02f;
            if (_interviewWait < 0f && !_askedQuestion)
            {
                _talker.InsertLine(Script.winner() + "! To what do you attribute your success?", _interviewIndex);
                _talker.Resume();
                _askedQuestion = true;
            }
        }
        _cameraOffset.X = Lerp.Float(_cameraOffset.X, (_image != null) ? 20 : 0, 2f);
        Teleprompter talker = _talker;
        bool active = (_talker.visible = _state != TVState.ShowPedestals);
        talker.active = active;
        if (_state != _desiredState)
        {
            _talker.active = false;
            _transition.PlaySwipe();
            if (_transition.doTransition)
            {
                _state = _desiredState;
            }
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        float drawOffX = -20f;
        float drawOffY = -5f;
        if (layer == Layer.Game)
        {
            Graphics.Clear(Color.Black);
        }
        if (layer == _blurLayer)
        {
            if (_state != TVState.ShowPedestals)
            {
            }
        }
        else if (layer == Layer.HUD)
        {
            if (_state == TVState.ShowPedestals)
            {
                if (_rockImage2.texture != null)
                {
                    float sizeNeg = 0f;
                    float size = (Layer.HUD.camera.width - sizeNeg) / (float)_rockImage2.texture.width;
                    _rockImage2.color = new Color(_tvFade, _tvFade, _tvFade);
                    _rockImage2.Scale = new Vector2(size, size);
                    Graphics.Draw(_rockImage2, -10f + drawOffX, drawOffY, 0.8f);
                }
            }
            else if (_state == TVState.ShowNewscaster)
            {
                _background.color = new Color(_tvFade, _tvFade, _tvFade);
                _duck.color = new Color(_tvFade, _tvFade, _tvFade);
                _tie.color = new Color(_tvFade, _tvFade, _tvFade);
                _newsTable.color = new Color(_tvFade, _tvFade, _tvFade);
                Graphics.Draw(_background, 0f + _cameraOffset.X, 3f + _cameraOffset.Y, 0.5f);
                Graphics.Draw(_newsTable, 0f + _cameraOffset.X, 116f + _cameraOffset.Y, 0.6f);
                _duck.Depth = 0.8f;
                Vector2 mallardPos = new Vector2(63f + _cameraOffset.X, 35f + _cameraOffset.Y);
                Graphics.Draw(_duck, mallardPos.X, mallardPos.Y);
                if (_duck.frame == 6)
                {
                    mallardPos.X -= 3f;
                }
                else if (_duck.frame == 7)
                {
                    mallardPos.X += 3f;
                }
                else if (_duck.frame == 8)
                {
                    mallardPos.X += 1f;
                }
                if (DG.isHalloween)
                {
                    _pumpkin.Depth = 0.81f;
                    Graphics.Draw(_pumpkin, mallardPos.X + 69f, mallardPos.Y + 22f);
                }
                _tie.Depth = 0.805f;
                float tieOff = 0f;
                if (_duck.frame == 7)
                {
                    tieOff += 2f;
                }
                else if (_duck.frame == 8)
                {
                    tieOff += 1f;
                }
                Graphics.Draw(_tie, 130f + _cameraOffset.X + tieOff, 96f + _cameraOffset.Y);
                if (!DG.isHalloween)
                {
                    _duckBeak.Depth = 0.81f;
                    _duckBeak.frame = _duck.frame;
                    Graphics.Draw(_duckBeak, 63f + _cameraOffset.X, 35f + _cameraOffset.Y);
                }
                if (_image != null)
                {
                    _image.Depth = 0.65f;
                    if (_cameraOffset.X > 19f)
                    {
                        Graphics.Draw(_image, 50f, 40f);
                    }
                }
            }
            else if (_state == TVState.ShowHotness)
            {
                _hotness.Draw();
            }
            else if (_state == TVState.ShowInterview)
            {
                _image.Scale = new Vector2(2f);
                Graphics.Draw(_image, 40f, 30f);
            }
            Graphics.Draw(_tv, 0f, -10f, 0.9f);
        }
        base.PostDrawLayer(layer);
    }
}
