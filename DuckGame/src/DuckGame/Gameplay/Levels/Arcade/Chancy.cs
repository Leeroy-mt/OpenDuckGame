using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class Chancy
{
    #region Public Fields

    public static bool atCounter = true;
    public static bool lookingAtList;
    public static bool lookingAtChallenge;
    public static bool hover;
    public static bool afterChallenge;

    public static int _giveTickets;

    public static float alpha;
    public static float afterChallengeWait;

    public static Vector2 standingPosition = Vector2.Zero;

    public static Chancy context = new();
    public static Sprite body;
    public static Sprite hoverSprite;
    public static Sprite listPaper;
    public static Sprite challengePaper;

    #endregion

    #region Private Fields

    static int _challengeSelection;

    static float _waitLetter = 1;
    static float _waitAfterLine = 1;
    static float _talkMove;
    static float _listLerp;
    static float _challengeLerp;
    static float _chancyLerp;
    static float _stampAngle;
    static float _paperAngle;
    static float _tapeAngle;

    static string _currentLine = "";

    static DealerMood _mood;

    static FancyBitmapFont _font;
    static SpriteMap _dealer;
    static Sprite _tail;
    static Sprite _photo;
    static Sprite _tape;
    static Sprite _tapePaper;
    static SpriteMap _paperclip;
    static SpriteMap _sticker;
    static Sprite _completeStamp;
    static Sprite _pencil;
    static SpriteMap _tinyStars;
    static ChallengeSaveData _save;
    static ChallengeSaveData _realSave;
    static SpriteMap _previewPhoto;
    static ChallengeData _challengeData;
    static RenderTarget2D _bestTextTarget;
    static Random _random;

    static List<string> _lines = [];
    static List<TextLine> _lineProgress = [];
    static List<ChallengeData> _chancyChallenges = [];

    #endregion

    #region Public Properties

    public static int frame
    {
        get
        {
            if (_mood == DealerMood.Concerned)
                return _dealer.frame - 4;

            if (_mood == DealerMood.Point)
                return _dealer.frame - 2;

            return _dealer.frame;
        }
        set
        {
            if (_mood == DealerMood.Concerned)
                _dealer.frame = value + 4;
            else if (_mood == DealerMood.Point)
                _dealer.frame = value + 2;
            else
                _dealer.frame = value;
        }
    }

    public static ChallengeData activeChallenge
    {
        get => _challengeData;
        set => _challengeData = value;
    }

    public static ChallengeData selectedChallenge =>
        _chancyChallenges.Count == 0 ? null : _chancyChallenges[_challengeSelection];

    #endregion

    #region Public Methods

    public static void Clear()
    {
        _lines.Clear();
        _waitLetter = 0;
        _waitAfterLine = 0;
        _currentLine = "";
        _mood = DealerMood.Normal;
    }

    public static void Add(string line)
    {
        _lines.Add(line);
    }

    public static void UpdateRandoms()
    {
        if (_challengeData != null && _paperclip != null)
        {
            _random = new Random(_challengeData.name.GetHashCode());
            Random generator = Rando.Generator;
            Rando.Generator = _random;
            _paperclip.frame = Rando.Int(5);
            _stampAngle = Rando.Float(14f) - 7f;
            Rando.Generator = new Random(GetChallengeBestString(_save, _challengeData).GetHashCode());
            _paperAngle = Rando.Float(4f) - 2f;
            _tapeAngle = _paperAngle + Rando.Float(-1f, 1f);
            Rando.Generator = generator;
        }
    }

    public static void AddProposition(ChallengeData challenge, Vector2 duckPos)
    {
        if (challenge.preview != null)
        {
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(challenge.preview));
            Texture2D tex = Texture2D.FromStream(Graphics.device, stream);
            _previewPhoto = new SpriteMap(tex, tex.Width, tex.Height)
            {
                Scale = new Vector2(0.25f)
            };
        }

        _challengeData = challenge;
        _realSave = Profiles.active[0].GetSaveData(_challengeData.levelID);
        _save = _realSave.Clone();
        UpdateRandoms();
        atCounter = false;
        Vector2 realPos = duckPos;
        var found = false;

        if (Level.CheckLine<Block>(duckPos, duckPos + new Vector2(36, 0), out var hit) != null)
        {
            hit.X -= 8;
            if ((hit - duckPos).Length() > 16)
            {
                realPos = hit;
                found = true;
            }
        }
        else
        {
            realPos = duckPos + new Vector2(36f, 0f);
            found = true;
        }

        if (found)
        {
            if (Level.CheckLine<Block>(realPos, realPos + new Vector2(0, 20), out hit) == null)
                found = false;
            else
            {
                standingPosition = hit - new Vector2(0f, 25f);
                body.flipH = true;
            }
        }

        if (!found)
        {
            if (Level.CheckLine<Block>(duckPos, duckPos + new Vector2(-36, 0), out hit) != null)
            {
                hit.X += 8;
                realPos = hit;
                found = true;
            }
            else
            {
                realPos = duckPos + new Vector2(-36, 0);
                found = true;
            }

            Level.CheckLine<Block>(realPos, realPos + new Vector2(0, 20), out hit);
            standingPosition = hit - new Vector2(0, 25);
            body.flipH = false;
        }
    }

    public static void Initialize()
    {
        if (_dealer == null)
        {
            _dealer = new SpriteMap("arcade/schooly", 100, 100);
            _tail = new Sprite("arcade/bubbleTail");
            body = new Sprite("arcade/chancy");
            hoverSprite = new Sprite("arcade/chancyHover");
            challengePaper = new Sprite("arcade/challengePaper");
            listPaper = new Sprite("arcade/challengePaperTall");
            _font = new FancyBitmapFont("smallFont");
            _photo = new Sprite("arcade/challengePhoto");
            _paperclip = new SpriteMap("arcade/paperclips", 13, 45);
            _sticker = new SpriteMap("arcade/stickers", 29, 29)
            {
                frame = 2
            };
            _tinyStars = new SpriteMap("arcade/tinyStars", 10, 8);
            _tinyStars.CenterOrigin();
            _completeStamp = new Sprite("arcade/completeStamp");
            _completeStamp.CenterOrigin();
            _pencil = new Sprite("arcade/pencil")
            {
                Center = new Vector2(127f, 4f)
            };
            _tape = new Sprite("arcade/tape");
            _tape.CenterOrigin();
            _tapePaper = new Sprite("arcade/tapePaper");
            _tapePaper.CenterOrigin();
        }
    }

    public static void OpenChallengeView()
    {
        ResetChallengeDialogue();
    }

    public static void ResetChallengeDialogue()
    {
        Clear();
        var skillIndex = Challenges.GetChallengeSkillIndex();
        List<string> dialogue = ["You interested in a little challenge?", "Bet you can't finish this one!", "You look up for a challenge."];

        if (_save == null)
        {
            if (skillIndex > 0.75f)
                dialogue = ["You could do this one easy.", "This should be no problem for you!", "This one's gonna be a breeze.", "Hot off the grill, just for you."];
            else if (skillIndex > 0.3f)
                dialogue = ["Wanna try something different?", "Hey, check this out.", "I've been playin with this new thing."];
        }
        else if (_save != null && _save.trophy > TrophyType.Gold)
        {
            dialogue = skillIndex > 0.75f
                ? ["Just never good enough huh?", "You still gotta top that score?"]
                : (!(skillIndex > 0.3f)
                    ? ["You already dominated this one.", "|CONCERNED|Huh? |CALM|You already got PLATINUM!"]
                    : [ "|CONCERNED|Woah, you think you can beat that score?", "|CONCERNED|You're gonna try to beat THAT!?" ]);
        }
        else if (_save != null && _save.trophy > TrophyType.Silver)
        {
            dialogue = skillIndex > 0.75f
                ? ["You know you can do better than gold.", "Pretty good, but you can do better."]
                : (!(skillIndex > 0.3f)
                    ? ["Gold is pretty rad, you still wanna do better?", "Still wanna improve that score?"]
                    : [ "Not bad.", "Yeah that's getting there, gold is alright." ]);
        }
        else if (_save != null && _save.trophy > TrophyType.Baseline)
        {
            dialogue = skillIndex > 0.75f
                ? ["Nice, lets try to top that.", "What? Not bad but you really could do better.", "I know you're not just gonna leave it at that."]
                : (!(skillIndex > 0.3f)
                    ? ["Well, you beat it! Can you do better?", "Not bad, you managed to do it!"]
                    : ["You did it, but you can do better.", "You're pretty good, you beat it."]);
        }

        Add(dialogue[Rando.Int(dialogue.Count - 1)]);
    }

    public static void OpenChallengeList()
    {
        _challengeSelection = 0;
        _chancyChallenges = Challenges.GetEligibleChancyChallenges(Profiles.active[0]);
    }

    public static void MakeConfetti()
    {
        for (int i = 0; i < 40; i++)
            Level.Add(new ChallengeConfetti((i * 8) + Rando.Float(-10, 10), -124 + Rando.Float(110)));
    }

    public static bool HasNewTrophy()
    {
        return _realSave.trophy != _save.trophy;
    }

    public static bool HasNewTime()
    {
        if (_realSave.bestTime == _save.bestTime && _realSave.goodies == _save.goodies)
            return _realSave.targets != _save.targets;
        return true;
    }

    public static int GiveTrophy()
    {
        var give = 0;
        if (_save.trophy != _realSave.trophy)
        {
            for (int i = (int)(_save.trophy + 1); i <= (int)_realSave.trophy; i++)
            {
                switch (i)
                {
                    case 1:
                        give += Challenges.valueBronze;
                        break;
                    case 2:
                        give += Challenges.valueSilver;
                        break;
                    case 3:
                        give += Challenges.valueGold;
                        break;
                    case 4:
                        give += Challenges.valuePlatinum;
                        break;
                }
            }
            _save.trophy = _realSave.trophy;
        }
        return give;
    }

    public static void GiveTime()
    {
        if (_save.bestTime != _realSave.bestTime)
            _save.bestTime = _realSave.bestTime;

        if (_save.goodies != _realSave.goodies)
            _save.goodies = _realSave.goodies;

        if (_save.targets != _realSave.targets)
            _save.targets = _realSave.targets;

        UpdateRandoms();
    }

    public static void StopShowingChallengeList()
    {
        _listLerp = 0;
        _challengeLerp = 0;
        _challengeLerp = 0;
        lookingAtChallenge = false;
        lookingAtList = false;
    }

    public static void Update()
    {
        var lerpList = lookingAtList && _challengeLerp < 0.3f;
        var lerpChallenge = lookingAtChallenge && _listLerp < 0.3f;
        var lerpChancy = (lookingAtChallenge || UnlockScreen.open) && _listLerp < 0.3f;
        _listLerp = Lerp.FloatSmooth(_listLerp, lerpList ? 1 : 0, 0.2f, 1.05f);
        _challengeLerp = Lerp.FloatSmooth(_challengeLerp, lerpChallenge ? 1 : 0, 0.2f, 1.05f);
        _chancyLerp = Lerp.FloatSmooth(_chancyLerp, lerpChancy ? 1 : 0, 0.2f, 1.05f);
        if (lookingAtList)
        {
            _ = _challengeSelection;
            if (Input.Pressed("MENUUP"))
            {
                _challengeSelection--;

                if (_challengeSelection < 0)
                    _challengeSelection = 0;
                else
                    SFX.Play("textLetter", 0.7f);
            }

            if (Input.Pressed("MENUDOWN"))
            {
                _challengeSelection++;

                if (_challengeSelection > _chancyChallenges.Count - 1)
                    _challengeSelection = _chancyChallenges.Count - 1;
                else
                    SFX.Play("textLetter", 0.7f);
            }
        }

        if (!UnlockScreen.open && !lookingAtChallenge)
            return;

        if (UnlockScreen.open || lookingAtChallenge)
            alpha = Lerp.Float(alpha, 1, 0.05f);
        else
            alpha = Lerp.Float(alpha, 0, 0.05f);

        if (afterChallenge)
        {
            if (afterChallengeWait > 0)
            {
                afterChallengeWait -= 0.03f;
            }
            else if (HasNewTime() || HasNewTrophy())
            {
                SFX.Play("dacBang", 1, -0.7f);
                GiveTime();
                _giveTickets = GiveTrophy();
                afterChallengeWait = 1;
                MakeConfetti();
            }
            else if (_giveTickets != 0)
            {
                Profiles.active[0].ticketCount += _giveTickets;
                afterChallengeWait = 2;
                _giveTickets = 0;
                SFX.Play("ching");
            }
            else
            {
                ResetChallengeDialogue();
                afterChallengeWait = 0;
                afterChallenge = false;
                foreach (ArcadeHUD item in Level.current.things[typeof(ArcadeHUD)])
                {
                    item.FinishChallenge();
                    item.launchChallenge = false;
                    item.selected = null;
                }
                HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@ACCEPT");
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@CANCEL");
                Profiles.Save(Profiles.active[0]);
            }
        }

        if (_save != null && activeChallenge != null)
        {
            _bestTextTarget ??= RenderTarget2D.CreateSetUpTarget(120, 8);
            Graphics.SetRenderTarget(_bestTextTarget);
            Graphics.Clear(Color.Transparent);
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, Matrix.Identity);
            string text = GetChallengeBestString(_save, activeChallenge);
            _font.Draw(text, new Vector2((int)Math.Round(_bestTextTarget.Width / 2f - _font.GetWidth(text) / 2), 0), Color.Black * 0.7f);
            Graphics.screen.End();
            Graphics.SetRenderTarget(null);
        }

        Initialize();

        if (_lines.Count > 0 && _currentLine == "")
        {
            _waitAfterLine -= 0.03f;
            _talkMove += 0.75f;

            if (_talkMove > 1)
            {
                frame = 0;
                _talkMove = 0;
            }

            if (_waitAfterLine <= 0)
            {
                _lineProgress.Clear();
                _currentLine = _lines[0];
                _lines.RemoveAt(0);
                _waitAfterLine = 1.5f;
                _mood = DealerMood.Normal;
            }
        }

        if (_currentLine != "")
        {
            _waitLetter -= 0.8f;
            if (!(_waitLetter < 0))
                return;

            _talkMove += 0.75f;
            if (_talkMove > 1)
            {
                if (_currentLine[0] != ' ' && frame == 0)
                    frame = Rando.Int(1);
                else
                    frame = 0;

                _talkMove = 0;
            }

            _waitLetter = 1;
            while (_currentLine[0] == '@')
            {
                var val = _currentLine[0].ToString() ?? "";
                _currentLine = _currentLine[1..];

                while (_currentLine[0] != '@' && _currentLine.Length > 0)
                {
                    val += _currentLine[0];
                    _currentLine = _currentLine[1..];
                }

                _currentLine = _currentLine[1..];
                val += "@";
                _lineProgress[0].Add(val);
                _waitLetter = 3;

                if (_currentLine.Length == 0)
                {
                    _currentLine = "";
                    return;
                }
            }

            while (_currentLine[0] == '|')
            {
                _currentLine = _currentLine[1..];
                var read = "";
                while (_currentLine[0] != '|' && _currentLine.Length > 0)
                {
                    read += _currentLine[0];
                    _currentLine = _currentLine.Remove(0, 1);
                }

                if (_currentLine.Length <= 1)
                {
                    _currentLine = "";
                    return;
                }

                _currentLine = _currentLine[1..];
                var c = Color.White;
                var foundColor = false;

                switch (read)
                {
                    case "RED":
                        foundColor = true;
                        c = Color.Red;
                        break;
                    case "WHITE":
                        foundColor = true;
                        c = Color.White;
                        break;
                    case "BLUE":
                        foundColor = true;
                        c = Color.Blue;
                        break;
                    case "ORANGE":
                        foundColor = true;
                        c = new Color(235, 137, 51);
                        break;
                    case "YELLOW":
                        foundColor = true;
                        c = new Color(247, 224, 90);
                        break;
                    case "GREEN":
                        foundColor = true;
                        c = Color.LimeGreen;
                        break;
                    case "CONCERNED":
                        _mood = DealerMood.Concerned;
                        break;
                    case "CALM":
                        _mood = DealerMood.Normal;
                        break;
                    case "PEEK":
                        _mood = DealerMood.Point;
                        break;
                }

                if (foundColor)
                {
                    if (_lineProgress.Count == 0)
                    {
                        _lineProgress.Insert(0, new TextLine
                        {
                            lineColor = c
                        });
                    }
                    else
                    {
                        _lineProgress[0].SwitchColor(c);
                    }
                }
            }
            var nextWord = "";
            int index = 1;
            if (_currentLine[0] == ' ')
            {
                while (index < _currentLine.Length && _currentLine[index] != ' ' && _currentLine[index] != '^')
                {
                    if (_currentLine[index] == '|')
                    {
                        for (index++; index < _currentLine.Length && _currentLine[index] != '|'; index++)
                        {
                        }
                        index++;
                    }
                    else if (_currentLine[index] == '@')
                    {
                        for (index++; index < _currentLine.Length && _currentLine[index] != '@'; index++)
                        {
                        }
                        index++;
                    }
                    else
                    {
                        nextWord += _currentLine[index];
                        index++;
                    }
                }
            }

            if (_lineProgress.Count == 0 || _currentLine[0] == '^' || (_currentLine[0] == ' ' && _lineProgress[0].Length() + nextWord.Length > 34))
            {
                var c2 = Color.White;
                if (_lineProgress.Count > 0)
                    c2 = _lineProgress[0].lineColor;

                _lineProgress.Insert(0, new TextLine
                {
                    lineColor = c2
                });

                if (_currentLine[0] == ' ' || _currentLine[0] == '^')
                    _currentLine = _currentLine[1..];

                return;
            }

            if (_currentLine[0] == '!' || _currentLine[0] == '?' || _currentLine[0] == '.')
                _waitLetter = 5;
            else if (_currentLine[0] == ',')
                _waitLetter = 3;

            if (_currentLine[0] == '*')
            {
                _waitLetter = 5;
            }
            else
            {
                _lineProgress[0].Add(_currentLine[0]);
                char c3 = _currentLine[0].ToString().ToLowerInvariant()[0];
                if ((c3 < 'a' || c3 > 'z') && c3 >= '0')
                    _ = 57;
            }
            _currentLine = _currentLine[1..];
        }
        else
        {
            _talkMove += 0.75f;
            if (_talkMove > 1)
            {
                frame = 0;
                _talkMove = 0;
            }
        }
    }

    public static string GetChallengeBestString(ChallengeSaveData dat, ChallengeData chal, bool canNull = false)
    {
        if (chal.trophies[1].timeRequirement > 0 || chal.trophies[2].timeRequirement > 0 || chal.trophies[3].timeRequirement > 0)
        {
            var timeString = MonoMain.TimeString(TimeSpan.FromMilliseconds(dat.bestTime), 3, small: true);
            if (dat.bestTime <= 0)
            {
                if (!canNull)
                    return "|RED|N/A";
                return null;
            }
            return $"BEST: {timeString}";
        }

        if (chal.trophies[1].targets != -1)
        {
            if (dat.targets <= 0)
            {
                if (!canNull)
                    return "|RED|N/A";
                return null;
            }
            return $"BEST: {dat.targets}";
        }

        if (chal.trophies[1].goodies != -1)
        {
            if (dat.goodies <= 0)
            {
                if (!canNull)
                    return "|RED|N/A";
                return null;
            }
            return $"BEST: {dat.goodies}";
        }
        return "";
    }

    public static void Draw()
    {
        Vector2 paperPos = new(-200 + _listLerp * 270, 20);

        if (lookingAtList || _listLerp > 0.01f)
        {
            listPaper.Depth = 0.8f;
            Graphics.Draw(listPaper, paperPos.X, paperPos.Y);
            _font.Depth = 0.85f;
            _font.Scale = new Vector2(1);
            _font.Draw("Chancy Challenges", paperPos + new Vector2(11f, 6f), Colors.BlueGray, 0.85f);
            var yOff = 9F;
            List<ChallengeData> chancyChallenges = _chancyChallenges;
            var idx = 0;

            foreach (ChallengeData c in chancyChallenges)
            {
                _font.Draw(c.name, paperPos + new Vector2(19, 12 + yOff), Colors.DGRed, 0.85f);
                Vector2 pencilPos = paperPos + new Vector2(12, 12 + yOff + 4);

                if (idx == _challengeSelection)
                {
                    _pencil.Depth = 0.9f;
                    Graphics.Draw(_pencil, pencilPos.X, pencilPos.Y);
                    Graphics.DrawLine(paperPos + new Vector2(19, 12 + yOff + 8.5f), paperPos + new Vector2(19 + _font.GetWidth(c.name), 12 + yOff + 8.5f), Colors.SuperDarkBlueGray, 1, 0.9f);
                }

                var savedat = Profiles.active[0].GetSaveData(_chancyChallenges[idx].levelID);
                if (savedat != null && savedat.trophy > TrophyType.Baseline)
                {
                    _tinyStars.frame = (int)(savedat.trophy - 1);
                    _tinyStars.Depth = 0.85f;
                    Graphics.Draw(_tinyStars, pencilPos.X + 2, pencilPos.Y);
                }

                yOff += 9;
                idx++;
            }
        }

        if (_challengeLerp < 0.01f && _chancyLerp < 0.01f)
            return;

        Vector2 dealerOffset = new(100 * (1 - _chancyLerp), 100 * (1 - _chancyLerp));
        Vector2 descSize = new(280, 20);
        Vector2 descPos = new Vector2(20, 132) + dealerOffset;
        Graphics.DrawRect(descPos + new Vector2(-2, 0), descPos + descSize + new Vector2(2, 0), Color.Black);
        var index = 0;
        for (int i = _lineProgress.Count - 1; i >= 0; i--)
        {
            var wide = Graphics.GetStringWidth(_lineProgress[i].text);
            var ypos = descPos.Y + 2 + (index * 9);
            var xpos = descPos.X + descSize.X / 2 - wide / 2;

            for (int j = _lineProgress[i].segments.Count - 1; j >= 0; j--)
            {
                Graphics.DrawString(_lineProgress[i].segments[j].text, new Vector2(xpos, ypos), _lineProgress[i].segments[j].color, 0.85f);
                xpos += (float)(_lineProgress[i].segments[j].text.Length * 8);
            }

            index++;
        }

        if (_challengeLerp > 0.01f && _challengeData != null)
        {
            paperPos = new Vector2(-200 + _challengeLerp * 240, 28);
            challengePaper.Depth = 0.8f;
            Graphics.Draw(challengePaper, paperPos.X, paperPos.Y);
            _paperclip.Depth = 0.92f;
            _photo.Depth = 0.87f;
            Graphics.Draw(_photo, paperPos.X + 135, paperPos.Y - 3);
            Graphics.Draw(_paperclip, paperPos.X + 140, paperPos.Y - 10);

            if (_previewPhoto != null)
            {
                _previewPhoto.Depth = 0.89f;
                _previewPhoto.AngleDegrees = 12;
                Graphics.Draw(_previewPhoto, paperPos.X + 146, paperPos.Y);
            }

            if (_save != null)
            {
                if (_save.trophy > TrophyType.Baseline)
                {
                    _sticker.Depth = 0.9f;
                    _sticker.frame = (int)(_save.trophy - 1);
                    Graphics.Draw(_sticker, paperPos.X + 123, paperPos.Y + 2);
                    _completeStamp.Depth = 0.9f;
                    _completeStamp.AngleDegrees = _stampAngle;
                    _completeStamp.Alpha = 0.9f;
                    Graphics.Draw(_completeStamp, paperPos.X + 72, paperPos.Y + 82);
                }

                var bestString = GetChallengeBestString(_save, _challengeData, canNull: true);
                if (bestString != null && bestString != "")
                {
                    _tapePaper.Depth = 0.9f;
                    _tapePaper.AngleDegrees = _paperAngle;
                    Graphics.Draw(_tapePaper, paperPos.X + 64, paperPos.Y + 22);
                    _tape.Depth = 0.95f;
                    _tape.AngleDegrees = _tapeAngle;
                    Graphics.Draw(_tape, paperPos.X + 64, paperPos.Y + 22);

                    if (_bestTextTarget != null)
                        Graphics.Draw(_bestTextTarget, new Vector2(paperPos.X + 64, paperPos.Y + 22), null, Color.White, Maths.DegToRad(_paperAngle), new Vector2(_bestTextTarget.Width / 2, _bestTextTarget.Height / 2), new Vector2(1, 1), SpriteEffects.None, 0.92f);
                }
            }
            _font.Depth = 0.85f;
            _font.Scale = new Vector2(1);
            _font.Draw(_challengeData.name, paperPos + new Vector2(9, 7), Colors.DGRed, 0.85f);
            _font.Scale = new Vector2(1);
            _font.maxWidth = 120;
            _font.Draw(_challengeData.description, paperPos + new Vector2(5, 30), Colors.BlueGray, 0.85f);
            _font.Scale = new Vector2(1);
            _font.maxWidth = 300;

            var u = Unlockables.GetUnlock(_challengeData.reward);
            if (u != null)
            {
                if (u is UnlockableHat)
                    _font.Draw($"|MENUORANGE|Reward - {u.name} hat", paperPos + new Vector2(5, 84), Colors.BlueGray, 0.85f);
            }
            else
            {
                _font.Draw("|MENUORANGE|Reward - TICKETS", paperPos + new Vector2(5, 84), Colors.BlueGray, 0.85f);
            }

            var t = _challengeData.trophies[1];
            if (t.targets != -1)
                _font.Draw($"|DGBLUE|break at least {t.targets} targets", paperPos + new Vector2(5, 75), Colors.BlueGray, 0.85f);
            else if (t.timeRequirement > 0)
                _font.Draw($"|DGBLUE|beat it in {t.timeRequirement} seconds", paperPos + new Vector2(5, 75), Colors.BlueGray, 0.85f);
        }

        _tail.flipV = true;
        Graphics.Draw(_tail, 222 + dealerOffset.X, 117 + dealerOffset.Y);
        var hasKey = true;
        if (Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]))
            hasKey = false;

        if (!hasKey)
            _dealer.frame += 6;

        _dealer.Depth = 0.5f;
        _dealer.Alpha = alpha;
        Graphics.Draw(_dealer, 216f + dealerOffset.X, 32f + dealerOffset.Y);

        if (!hasKey)
            _dealer.frame -= 6;
    }

    public static void DrawGameLayer()
    {
        if (atCounter)
            return;

        body.Depth = 0;
        Graphics.Draw(body, standingPosition.X, standingPosition.Y);

        if (hover)
            hoverSprite.Alpha = Lerp.Float(hoverSprite.Alpha, 1f, 0.05f);
        else
            hoverSprite.Alpha = Lerp.Float(hoverSprite.Alpha, 0f, 0.05f);

        if (hoverSprite.Alpha > 0.01f)
        {
            hoverSprite.Depth = 0f;
            hoverSprite.flipH = body.flipH;

            if (hoverSprite.flipH)
                Graphics.Draw(hoverSprite, standingPosition.X + 1, standingPosition.Y - 1);
            else
                Graphics.Draw(hoverSprite, standingPosition.X - 1, standingPosition.Y - 1);
        }
    }

    #endregion
}