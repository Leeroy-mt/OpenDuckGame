using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class Chancy
{
    public static Chancy context = new Chancy();

    public static float alpha = 0f;

    public static bool atCounter = true;

    public static Vec2 standingPosition = Vec2.Zero;

    public static bool lookingAtList = false;

    public static bool lookingAtChallenge = false;

    public static bool hover = false;

    private static FancyBitmapFont _font;

    private static SpriteMap _dealer;

    private static Sprite _tail;

    private static Sprite _photo;

    private static Sprite _tape;

    private static Sprite _tapePaper;

    private static SpriteMap _paperclip;

    private static SpriteMap _sticker;

    private static Sprite _completeStamp;

    private static Sprite _pencil;

    private static SpriteMap _tinyStars;

    public static Sprite body;

    public static Sprite hoverSprite;

    public static Sprite listPaper;

    public static Sprite challengePaper;

    private static List<string> _lines = new List<string>();

    private static DealerMood _mood;

    private static string _currentLine = "";

    private static List<TextLine> _lineProgress = new List<TextLine>();

    private static float _waitLetter = 1f;

    private static float _waitAfterLine = 1f;

    private static float _talkMove = 0f;

    private static float _listLerp = 0f;

    private static float _challengeLerp = 0f;

    private static float _chancyLerp = 0f;

    private static ChallengeSaveData _save;

    private static ChallengeSaveData _realSave;

    private static SpriteMap _previewPhoto;

    private static ChallengeData _challengeData;

    private static RenderTarget2D _bestTextTarget;

    private static Random _random;

    private static float _stampAngle = 0f;

    private static float _paperAngle = 0f;

    private static float _tapeAngle = 0f;

    private static int _challengeSelection;

    public static int _giveTickets = 0;

    public static bool afterChallenge = false;

    public static float afterChallengeWait = 0f;

    private static List<ChallengeData> _chancyChallenges = new List<ChallengeData>();

    public static int frame
    {
        get
        {
            if (_mood == DealerMood.Concerned)
            {
                return _dealer.frame - 4;
            }
            if (_mood == DealerMood.Point)
            {
                return _dealer.frame - 2;
            }
            return _dealer.frame;
        }
        set
        {
            if (_mood == DealerMood.Concerned)
            {
                _dealer.frame = value + 4;
            }
            else if (_mood == DealerMood.Point)
            {
                _dealer.frame = value + 2;
            }
            else
            {
                _dealer.frame = value;
            }
        }
    }

    public static ChallengeData activeChallenge
    {
        get
        {
            return _challengeData;
        }
        set
        {
            _challengeData = value;
        }
    }

    public static ChallengeData selectedChallenge
    {
        get
        {
            if (_chancyChallenges.Count == 0)
            {
                return null;
            }
            return _chancyChallenges[_challengeSelection];
        }
    }

    public static void Clear()
    {
        _lines.Clear();
        _waitLetter = 0f;
        _waitAfterLine = 0f;
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
            Random generator = Rando.generator;
            Rando.generator = _random;
            _paperclip.frame = Rando.Int(5);
            _stampAngle = Rando.Float(14f) - 7f;
            Rando.generator = new Random(GetChallengeBestString(_save, _challengeData).GetHashCode());
            _paperAngle = Rando.Float(4f) - 2f;
            _tapeAngle = _paperAngle + Rando.Float(-1f, 1f);
            Rando.generator = generator;
        }
    }

    public static void AddProposition(ChallengeData challenge, Vec2 duckPos)
    {
        if (challenge.preview != null)
        {
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(challenge.preview));
            Texture2D tex = Texture2D.FromStream(Graphics.device, stream);
            _previewPhoto = new SpriteMap(tex, tex.Width, tex.Height);
            _previewPhoto.Scale = new Vec2(0.25f);
        }
        _challengeData = challenge;
        _realSave = Profiles.active[0].GetSaveData(_challengeData.levelID);
        _save = _realSave.Clone();
        UpdateRandoms();
        atCounter = false;
        Vec2 realPos = duckPos;
        bool found = false;
        if (Level.CheckLine<Block>(duckPos, duckPos + new Vec2(36f, 0f), out var hit) != null)
        {
            hit.X -= 8f;
            if ((hit - duckPos).Length() > 16f)
            {
                realPos = hit;
                found = true;
            }
        }
        else
        {
            realPos = duckPos + new Vec2(36f, 0f);
            found = true;
        }
        if (found)
        {
            if (Level.CheckLine<Block>(realPos, realPos + new Vec2(0f, 20f), out hit) == null)
            {
                found = false;
            }
            else
            {
                standingPosition = hit - new Vec2(0f, 25f);
                body.flipH = true;
            }
        }
        if (!found)
        {
            if (Level.CheckLine<Block>(duckPos, duckPos + new Vec2(-36f, 0f), out hit) != null)
            {
                hit.X += 8f;
                realPos = hit;
                found = true;
            }
            else
            {
                realPos = duckPos + new Vec2(-36f, 0f);
                found = true;
            }
            Level.CheckLine<Block>(realPos, realPos + new Vec2(0f, 20f), out hit);
            standingPosition = hit - new Vec2(0f, 25f);
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
            _sticker = new SpriteMap("arcade/stickers", 29, 29);
            _sticker.frame = 2;
            _tinyStars = new SpriteMap("arcade/tinyStars", 10, 8);
            _tinyStars.CenterOrigin();
            _completeStamp = new Sprite("arcade/completeStamp");
            _completeStamp.CenterOrigin();
            _pencil = new Sprite("arcade/pencil");
            _pencil.Center = new Vec2(127f, 4f);
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
        float skillIndex = Challenges.GetChallengeSkillIndex();
        List<string> dialogue = new List<string> { "You interested in a little challenge?", "Bet you can't finish this one!", "You look up for a challenge." };
        if (_save == null)
        {
            if (skillIndex > 0.75f)
            {
                dialogue = new List<string> { "You could do this one easy.", "This should be no problem for you!", "This one's gonna be a breeze.", "Hot off the grill, just for you." };
            }
            else if (skillIndex > 0.3f)
            {
                dialogue = new List<string> { "Wanna try something different?", "Hey, check this out.", "I've been playin with this new thing." };
            }
        }
        else if (_save != null && _save.trophy > TrophyType.Gold)
        {
            dialogue = ((skillIndex > 0.75f) ? new List<string> { "Just never good enough huh?", "You still gotta top that score?" } : ((!(skillIndex > 0.3f)) ? new List<string> { "You already dominated this one.", "|CONCERNED|Huh? |CALM|You already got PLATINUM!" } : new List<string> { "|CONCERNED|Woah, you think you can beat that score?", "|CONCERNED|You're gonna try to beat THAT!?" }));
        }
        else if (_save != null && _save.trophy > TrophyType.Silver)
        {
            dialogue = ((skillIndex > 0.75f) ? new List<string> { "You know you can do better than gold.", "Pretty good, but you can do better." } : ((!(skillIndex > 0.3f)) ? new List<string> { "Gold is pretty rad, you still wanna do better?", "Still wanna improve that score?" } : new List<string> { "Not bad.", "Yeah that's getting there, gold is alright." }));
        }
        else if (_save != null && _save.trophy > TrophyType.Baseline)
        {
            dialogue = ((skillIndex > 0.75f) ? new List<string> { "Nice, lets try to top that.", "What? Not bad but you really could do better.", "I know you're not just gonna leave it at that." } : ((!(skillIndex > 0.3f)) ? new List<string> { "Well, you beat it! Can you do better?", "Not bad, you managed to do it!" } : new List<string> { "You did it, but you can do better.", "You're pretty good, you beat it." }));
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
        {
            Level.Add(new ChallengeConfetti((float)(i * 8) + Rando.Float(-10f, 10f), -124f + Rando.Float(110f)));
        }
    }

    public static bool HasNewTrophy()
    {
        return _realSave.trophy != _save.trophy;
    }

    public static bool HasNewTime()
    {
        if (_realSave.bestTime == _save.bestTime && _realSave.goodies == _save.goodies)
        {
            return _realSave.targets != _save.targets;
        }
        return true;
    }

    public static int GiveTrophy()
    {
        int give = 0;
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
        {
            _save.bestTime = _realSave.bestTime;
        }
        if (_save.goodies != _realSave.goodies)
        {
            _save.goodies = _realSave.goodies;
        }
        if (_save.targets != _realSave.targets)
        {
            _save.targets = _realSave.targets;
        }
        UpdateRandoms();
    }

    public static void StopShowingChallengeList()
    {
        _listLerp = 0f;
        _challengeLerp = 0f;
        _challengeLerp = 0f;
        lookingAtChallenge = false;
        lookingAtList = false;
    }

    public static void Update()
    {
        bool lerpList = lookingAtList && _challengeLerp < 0.3f;
        bool lerpChallenge = lookingAtChallenge && _listLerp < 0.3f;
        bool lerpChancy = (lookingAtChallenge || UnlockScreen.open) && _listLerp < 0.3f;
        _listLerp = Lerp.FloatSmooth(_listLerp, lerpList ? 1f : 0f, 0.2f, 1.05f);
        _challengeLerp = Lerp.FloatSmooth(_challengeLerp, lerpChallenge ? 1f : 0f, 0.2f, 1.05f);
        _chancyLerp = Lerp.FloatSmooth(_chancyLerp, lerpChancy ? 1f : 0f, 0.2f, 1.05f);
        if (lookingAtList)
        {
            _ = _challengeSelection;
            if (Input.Pressed("MENUUP"))
            {
                _challengeSelection--;
                if (_challengeSelection < 0)
                {
                    _challengeSelection = 0;
                }
                else
                {
                    SFX.Play("textLetter", 0.7f);
                }
            }
            if (Input.Pressed("MENUDOWN"))
            {
                _challengeSelection++;
                if (_challengeSelection > _chancyChallenges.Count - 1)
                {
                    _challengeSelection = _chancyChallenges.Count - 1;
                }
                else
                {
                    SFX.Play("textLetter", 0.7f);
                }
            }
        }
        if (!UnlockScreen.open && !lookingAtChallenge)
        {
            return;
        }
        if (UnlockScreen.open || lookingAtChallenge)
        {
            alpha = Lerp.Float(alpha, 1f, 0.05f);
        }
        else
        {
            alpha = Lerp.Float(alpha, 0f, 0.05f);
        }
        if (afterChallenge)
        {
            if (afterChallengeWait > 0f)
            {
                afterChallengeWait -= 0.03f;
            }
            else if (HasNewTime() || HasNewTrophy())
            {
                SFX.Play("dacBang", 1f, -0.7f);
                GiveTime();
                _giveTickets = GiveTrophy();
                afterChallengeWait = 1f;
                MakeConfetti();
            }
            else if (_giveTickets != 0)
            {
                Profiles.active[0].ticketCount += _giveTickets;
                afterChallengeWait = 2f;
                _giveTickets = 0;
                SFX.Play("ching");
            }
            else
            {
                ResetChallengeDialogue();
                afterChallengeWait = 0f;
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
            if (_bestTextTarget == null)
            {
                _bestTextTarget = new RenderTarget2D(120, 8);
            }
            Graphics.SetRenderTarget(_bestTextTarget);
            Graphics.Clear(Color.Transparent);
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, Matrix.Identity);
            string text = GetChallengeBestString(_save, activeChallenge);
            _font.Draw(text, new Vec2((int)Math.Round((float)_bestTextTarget.width / 2f - _font.GetWidth(text) / 2f), 0f), Color.Black * 0.7f);
            Graphics.screen.End();
            Graphics.SetRenderTarget(null);
        }
        Initialize();
        if (_lines.Count > 0 && _currentLine == "")
        {
            _waitAfterLine -= 0.03f;
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
            if (_waitAfterLine <= 0f)
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
            if (!(_waitLetter < 0f))
            {
                return;
            }
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                if (_currentLine[0] != ' ' && frame == 0)
                {
                    frame = Rando.Int(1);
                }
                else
                {
                    frame = 0;
                }
                _talkMove = 0f;
            }
            _waitLetter = 1f;
            while (_currentLine[0] == '@')
            {
                string val = _currentLine[0].ToString() ?? "";
                _currentLine = _currentLine.Remove(0, 1);
                while (_currentLine[0] != '@' && _currentLine.Length > 0)
                {
                    val += _currentLine[0];
                    _currentLine = _currentLine.Remove(0, 1);
                }
                _currentLine = _currentLine.Remove(0, 1);
                val += "@";
                _lineProgress[0].Add(val);
                _waitLetter = 3f;
                if (_currentLine.Length == 0)
                {
                    _currentLine = "";
                    return;
                }
            }
            while (_currentLine[0] == '|')
            {
                _currentLine = _currentLine.Remove(0, 1);
                string read = "";
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
                _currentLine = _currentLine.Remove(0, 1);
                Color c = Color.White;
                bool foundColor = false;
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
            string nextWord = "";
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
                Color c2 = Color.White;
                if (_lineProgress.Count > 0)
                {
                    c2 = _lineProgress[0].lineColor;
                }
                _lineProgress.Insert(0, new TextLine
                {
                    lineColor = c2
                });
                if (_currentLine[0] == ' ' || _currentLine[0] == '^')
                {
                    _currentLine = _currentLine.Remove(0, 1);
                }
                return;
            }
            if (_currentLine[0] == '!' || _currentLine[0] == '?' || _currentLine[0] == '.')
            {
                _waitLetter = 5f;
            }
            else if (_currentLine[0] == ',')
            {
                _waitLetter = 3f;
            }
            if (_currentLine[0] == '*')
            {
                _waitLetter = 5f;
            }
            else
            {
                _lineProgress[0].Add(_currentLine[0]);
                char c3 = _currentLine[0].ToString().ToLowerInvariant()[0];
                if ((c3 < 'a' || c3 > 'z') && c3 >= '0')
                {
                    _ = 57;
                }
            }
            _currentLine = _currentLine.Remove(0, 1);
        }
        else
        {
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
        }
    }

    public static string GetChallengeBestString(ChallengeSaveData dat, ChallengeData chal, bool canNull = false)
    {
        if (chal.trophies[1].timeRequirement > 0 || chal.trophies[2].timeRequirement > 0 || chal.trophies[3].timeRequirement > 0)
        {
            string timeString = MonoMain.TimeString(TimeSpan.FromMilliseconds(dat.bestTime), 3, small: true);
            if (dat.bestTime <= 0)
            {
                if (!canNull)
                {
                    return "|RED|N/A";
                }
                return null;
            }
            return "BEST: " + timeString;
        }
        if (chal.trophies[1].targets != -1)
        {
            if (dat.targets <= 0)
            {
                if (!canNull)
                {
                    return "|RED|N/A";
                }
                return null;
            }
            return "BEST: " + dat.targets;
        }
        if (chal.trophies[1].goodies != -1)
        {
            if (dat.goodies <= 0)
            {
                if (!canNull)
                {
                    return "|RED|N/A";
                }
                return null;
            }
            return "BEST: " + dat.goodies;
        }
        return "";
    }

    public static void Draw()
    {
        Vec2 paperPos = new Vec2(-200f + _listLerp * 270f, 20f);
        if (lookingAtList || _listLerp > 0.01f)
        {
            listPaper.Depth = 0.8f;
            Graphics.Draw(listPaper, paperPos.X, paperPos.Y);
            _font.Depth = 0.85f;
            _font.Scale = new Vec2(1f);
            _font.Draw("Chancy Challenges", paperPos + new Vec2(11f, 6f), Colors.BlueGray, 0.85f);
            float yOff = 9f;
            List<ChallengeData> chancyChallenges = _chancyChallenges;
            int idx = 0;
            foreach (ChallengeData c in chancyChallenges)
            {
                _font.Draw(c.name, paperPos + new Vec2(19f, 12f + yOff), Colors.DGRed, 0.85f);
                Vec2 pencilPos = paperPos + new Vec2(12f, 12f + yOff + 4f);
                if (idx == _challengeSelection)
                {
                    _pencil.Depth = 0.9f;
                    Graphics.Draw(_pencil, pencilPos.X, pencilPos.Y);
                    Graphics.DrawLine(paperPos + new Vec2(19f, 12f + yOff + 8.5f), paperPos + new Vec2(19f + _font.GetWidth(c.name), 12f + yOff + 8.5f), Colors.SuperDarkBlueGray, 1f, 0.9f);
                }
                ChallengeSaveData savedat = Profiles.active[0].GetSaveData(_chancyChallenges[idx].levelID);
                if (savedat != null && savedat.trophy > TrophyType.Baseline)
                {
                    _tinyStars.frame = (int)(savedat.trophy - 1);
                    _tinyStars.Depth = 0.85f;
                    Graphics.Draw(_tinyStars, pencilPos.X + 2f, pencilPos.Y);
                }
                yOff += 9f;
                idx++;
            }
        }
        if (_challengeLerp < 0.01f && _chancyLerp < 0.01f)
        {
            return;
        }
        Vec2 dealerOffset = new Vec2(100f * (1f - _chancyLerp), 100f * (1f - _chancyLerp));
        Vec2 descSize = new Vec2(280f, 20f);
        Vec2 descPos = new Vec2(20f, 132f) + dealerOffset;
        Graphics.DrawRect(descPos + new Vec2(-2f, 0f), descPos + descSize + new Vec2(2f, 0f), Color.Black);
        int index = 0;
        for (int i = _lineProgress.Count - 1; i >= 0; i--)
        {
            float wide = Graphics.GetStringWidth(_lineProgress[i].text);
            float ypos = descPos.Y + 2f + (float)(index * 9);
            float xpos = descPos.X + descSize.X / 2f - wide / 2f;
            for (int j = _lineProgress[i].segments.Count - 1; j >= 0; j--)
            {
                Graphics.DrawString(_lineProgress[i].segments[j].text, new Vec2(xpos, ypos), _lineProgress[i].segments[j].color, 0.85f);
                xpos += (float)(_lineProgress[i].segments[j].text.Length * 8);
            }
            index++;
        }
        if (_challengeLerp > 0.01f && _challengeData != null)
        {
            paperPos = new Vec2(40f, 28f);
            paperPos = new Vec2(-200f + _challengeLerp * 240f, 28f);
            challengePaper.Depth = 0.8f;
            Graphics.Draw(challengePaper, paperPos.X, paperPos.Y);
            _paperclip.Depth = 0.92f;
            _photo.Depth = 0.87f;
            Graphics.Draw(_photo, paperPos.X + 135f, paperPos.Y - 3f);
            Graphics.Draw(_paperclip, paperPos.X + 140f, paperPos.Y - 10f);
            if (_previewPhoto != null)
            {
                _previewPhoto.Depth = 0.89f;
                _previewPhoto.AngleDegrees = 12f;
                Graphics.Draw(_previewPhoto, paperPos.X + 146f, paperPos.Y + 0f);
            }
            if (_save != null)
            {
                if (_save.trophy > TrophyType.Baseline)
                {
                    _sticker.Depth = 0.9f;
                    _sticker.frame = (int)(_save.trophy - 1);
                    Graphics.Draw(_sticker, paperPos.X + 123f, paperPos.Y + 2f);
                    _completeStamp.Depth = 0.9f;
                    _completeStamp.AngleDegrees = _stampAngle;
                    _completeStamp.Alpha = 0.9f;
                    Graphics.Draw(_completeStamp, paperPos.X + 72f, paperPos.Y + 82f);
                }
                string bestString = GetChallengeBestString(_save, _challengeData, canNull: true);
                if (bestString != null && bestString != "")
                {
                    _tapePaper.Depth = 0.9f;
                    _tapePaper.AngleDegrees = _paperAngle;
                    Graphics.Draw(_tapePaper, paperPos.X + 64f, paperPos.Y + 22f);
                    _tape.Depth = 0.95f;
                    _tape.AngleDegrees = _tapeAngle;
                    Graphics.Draw(_tape, paperPos.X + 64f, paperPos.Y + 22f);
                    if (_bestTextTarget != null)
                    {
                        Graphics.Draw((Texture2D)_bestTextTarget, new Vec2(paperPos.X + 64f, paperPos.Y + 22f), null, Color.White, Maths.DegToRad(_paperAngle), new Vec2(_bestTextTarget.width / 2, _bestTextTarget.height / 2), new Vec2(1f, 1f), SpriteEffects.None, 0.92f);
                    }
                }
            }
            _font.Depth = 0.85f;
            _font.Scale = new Vec2(1f);
            _font.Draw(_challengeData.name, paperPos + new Vec2(9f, 7f), Colors.DGRed, 0.85f);
            _font.Scale = new Vec2(1f);
            _font.maxWidth = 120;
            _font.Draw(_challengeData.description, paperPos + new Vec2(5f, 30f), Colors.BlueGray, 0.85f);
            _font.Scale = new Vec2(1f);
            _font.maxWidth = 300;
            Unlockable u = Unlockables.GetUnlock(_challengeData.reward);
            if (u != null)
            {
                if (u is UnlockableHat)
                {
                    _font.Draw("|MENUORANGE|Reward - " + u.name + " hat", paperPos + new Vec2(5f, 84f), Colors.BlueGray, 0.85f);
                }
            }
            else
            {
                _font.Draw("|MENUORANGE|Reward - TICKETS", paperPos + new Vec2(5f, 84f), Colors.BlueGray, 0.85f);
            }
            ChallengeTrophy t = _challengeData.trophies[1];
            if (t.targets != -1)
            {
                _font.Draw("|DGBLUE|break at least " + t.targets + " targets", paperPos + new Vec2(5f, 75f), Colors.BlueGray, 0.85f);
            }
            else if (t.timeRequirement > 0)
            {
                _font.Draw("|DGBLUE|beat it in " + t.timeRequirement + " seconds", paperPos + new Vec2(5f, 75f), Colors.BlueGray, 0.85f);
            }
        }
        _tail.flipV = true;
        Graphics.Draw(_tail, 222f + dealerOffset.X, 117f + dealerOffset.Y);
        bool hasKey = true;
        if (Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]))
        {
            hasKey = false;
        }
        if (!hasKey)
        {
            _dealer.frame += 6;
        }
        _dealer.Depth = 0.5f;
        _dealer.Alpha = alpha;
        Graphics.Draw(_dealer, 216f + dealerOffset.X, 32f + dealerOffset.Y);
        if (!hasKey)
        {
            _dealer.frame -= 6;
        }
    }

    public static void DrawGameLayer()
    {
        if (atCounter)
        {
            return;
        }
        body.Depth = 0f;
        Graphics.Draw(body, standingPosition.X, standingPosition.Y);
        if (hover)
        {
            hoverSprite.Alpha = Lerp.Float(hoverSprite.Alpha, 1f, 0.05f);
        }
        else
        {
            hoverSprite.Alpha = Lerp.Float(hoverSprite.Alpha, 0f, 0.05f);
        }
        if (hoverSprite.Alpha > 0.01f)
        {
            hoverSprite.Depth = 0f;
            hoverSprite.flipH = body.flipH;
            if (hoverSprite.flipH)
            {
                Graphics.Draw(hoverSprite, standingPosition.X + 1f, standingPosition.Y - 1f);
            }
            else
            {
                Graphics.Draw(hoverSprite, standingPosition.X - 1f, standingPosition.Y - 1f);
            }
        }
    }
}
