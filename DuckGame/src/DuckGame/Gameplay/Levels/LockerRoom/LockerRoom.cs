using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class LockerRoom : Level
{
    private Sprite _background;

    private Sprite _boardHighlight;

    private Sprite _trophiesHighlight;

    private SinWave _pulse = 0.1f;

    private LockerSelection _selection;

    private LockerScreen _screen = LockerScreen.Stats;

    private LockerScreen _desiredScreen = LockerScreen.Stats;

    private float _statScroll;

    private List<LockerStat> _stats = new List<LockerStat>();

    private float _fade = 1f;

    private Profile _profile;

    private UIComponent _confirmGroup;

    private UIMenu _confirmMenu;

    private MenuBoolean _clearStats = new MenuBoolean();

    public LockerRoom(Profile p)
    {
        _centeredView = true;
        _profile = p;
    }

    public override void Initialize()
    {
        _background = new Sprite("gym");
        _boardHighlight = new Sprite("boardHighlight");
        _boardHighlight.CenterOrigin();
        _trophiesHighlight = new Sprite("trophiesHighlight");
        _trophiesHighlight.CenterOrigin();
        HUD.AddCornerMessage(HUDCorner.TopLeft, _profile.name);
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
        HUD.AddCornerControl(HUDCorner.BottomRight, "@MENU2@RESET");
        base.backgroundColor = Color.Black;
        _confirmGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _confirmMenu = new UIMenu("RESET STATS?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@SELECT@SELECT");
        _confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenu(_confirmGroup)));
        _confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_confirmGroup, _clearStats)));
        _confirmMenu.Close();
        _confirmGroup.Add(_confirmMenu, doAnchor: false);
        _confirmGroup.Close();
        Level.Add(_confirmGroup);
        Profile profile = _profile;
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("QUACKS: " + Change.ToString(profile.stats.quacks), Color.Orange));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("FANS: " + Change.ToString(profile.stats.GetFans()), Color.Lime));
        int fans = profile.stats.GetFans();
        int fanLoyalty = 0;
        if (fans > 0)
        {
            fanLoyalty = (int)Math.Round((float)profile.stats.loyalFans / (float)profile.stats.GetFans() * 100f);
        }
        _stats.Add(new LockerStat("FAN LOYALTY: " + Change.ToString(fanLoyalty) + "%", Color.Lime));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("KILLS: " + Change.ToString(profile.stats.kills), Color.GreenYellow));
        _stats.Add(new LockerStat("DEATHS: " + Change.ToString(profile.stats.timesKilled), Color.Red));
        _stats.Add(new LockerStat("SUICIDES: " + Change.ToString(profile.stats.suicides), Color.Red));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("ROUNDS WON: " + Change.ToString(profile.stats.matchesWon), Color.GreenYellow));
        _stats.Add(new LockerStat("ROUNDS LOST: " + Change.ToString(profile.stats.timesSpawned - profile.stats.matchesWon), Color.Red));
        _stats.Add(new LockerStat("GAMES WON: " + Change.ToString(profile.stats.trophiesWon), Color.GreenYellow));
        _stats.Add(new LockerStat("GAMES LOST: " + Change.ToString(profile.stats.gamesPlayed - profile.stats.trophiesWon), Color.Red));
        float accuracy = 0f;
        if (profile.stats.bulletsFired > 0)
        {
            accuracy = (float)profile.stats.bulletsThatHit / (float)profile.stats.bulletsFired;
        }
        _stats.Add(new LockerStat("ACCURACY: " + Change.ToString((int)Math.Round(accuracy * 100f)) + "%", (accuracy > 0.6f) ? Color.Green : Color.Red));
        _stats.Add(new LockerStat("TRICK SHOT KILLS: " + profile.stats.trickShots, Color.Green));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("MINES STEPPED ON: " + Change.ToString(profile.stats.minesSteppedOn), Color.Orange));
        _stats.Add(new LockerStat("PRESENTS OPENED: " + Change.ToString(profile.stats.presentsOpened), Color.Orange));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("SPIRITUALITY", Color.White));
        _stats.Add(new LockerStat("FUNERALS: " + Change.ToString(profile.stats.funeralsPerformed), Color.Orange));
        _stats.Add(new LockerStat("CONVERSIONS: " + Change.ToString(profile.stats.conversions), Color.Orange));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("TIME SPENT", Color.White));
        _stats.Add(new LockerStat("IN NET: " + TimeSpan.FromSeconds(profile.stats.timeInNet).ToString("hh\\:mm\\:ss"), Color.Orange));
        _stats.Add(new LockerStat("ON FIRE: " + TimeSpan.FromSeconds(profile.stats.timeOnFire).ToString("hh\\:mm\\:ss"), Color.Orange));
        _stats.Add(new LockerStat("BRAINWASHED: " + TimeSpan.FromSeconds(profile.stats.timesMindControlled).ToString("hh\\:mm\\:ss"), Color.Orange));
        _stats.Add(new LockerStat("MOUTH OPEN: " + TimeSpan.FromSeconds(profile.stats.timeWithMouthOpen).ToString("hh\\:mm\\:ss"), Color.Orange));
        base.Initialize();
    }

    public override void Update()
    {
        int val = (int)_selection;
        if (_desiredScreen != _screen)
        {
            _fade = Lerp.Float(_fade, 0f, 0.06f);
            if (_fade <= 0f)
            {
                _screen = _desiredScreen;
                if (_screen == LockerScreen.Stats)
                {
                    _statScroll = 0f;
                    HUD.AddCornerControl(HUDCorner.TopLeft, "@WASD@MOVE");
                    HUD.AddCornerMessage(HUDCorner.TopRight, _profile.name);
                    HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
                }
                else if (_screen == LockerScreen.Trophies)
                {
                    _statScroll = 0f;
                    HUD.AddCornerControl(HUDCorner.TopLeft, "@WASD@MOVE");
                    HUD.AddCornerMessage(HUDCorner.TopRight, "TROPHIES");
                    HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
                }
                else if (_screen == LockerScreen.Locker)
                {
                    HUD.AddCornerControl(HUDCorner.TopLeft, "@WASD@MOVE");
                    HUD.AddCornerMessage(HUDCorner.TopRight, "LOCKER ROOM");
                    HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@EXIT");
                }
                else if (_screen == LockerScreen.Exit)
                {
                    Graphics.fade = 0f;
                    Level.current = new DoorRoom(_profile);
                }
            }
        }
        else
        {
            _fade = Lerp.Float(_fade, 1f, 0.06f);
            if (_screen == LockerScreen.Locker)
            {
                if (InputProfile.active.Pressed("MENULEFT"))
                {
                    val--;
                    if (val < 0)
                    {
                        val = 1;
                    }
                }
                if (InputProfile.active.Pressed("MENURIGHT"))
                {
                    val++;
                    if (val >= 2)
                    {
                        val = 0;
                    }
                }
                _selection = (LockerSelection)val;
                if (InputProfile.active.Pressed("SELECT"))
                {
                    if (_selection == LockerSelection.Stats)
                    {
                        _desiredScreen = LockerScreen.Stats;
                        HUD.CloseAllCorners();
                    }
                    if (_selection == LockerSelection.Trophies)
                    {
                        _desiredScreen = LockerScreen.Trophies;
                        HUD.CloseAllCorners();
                    }
                }
                if (InputProfile.active.Pressed("CANCEL"))
                {
                    _desiredScreen = LockerScreen.Exit;
                    HUD.CloseAllCorners();
                }
            }
            else if (_screen == LockerScreen.Stats)
            {
                if (InputProfile.active.Down("MENUUP"))
                {
                    _statScroll -= 0.02f;
                    if (_statScroll < 0f)
                    {
                        _statScroll = 0f;
                    }
                }
                if (InputProfile.active.Down("MENUDOWN"))
                {
                    _statScroll += 0.02f;
                    if (_statScroll > 1f)
                    {
                        _statScroll = 1f;
                    }
                }
                if (InputProfile.active.Pressed("CANCEL"))
                {
                    _desiredScreen = LockerScreen.Exit;
                    HUD.CloseAllCorners();
                }
                if (_clearStats.value)
                {
                    _clearStats.value = false;
                    _profile.stats = new ProfileStats();
                    Profiles.Save(_profile);
                    Level.current = new LockerRoom(_profile);
                }
                if (InputProfile.active.Pressed("MENU2"))
                {
                    MonoMain.pauseMenu = _confirmGroup;
                    _confirmGroup.Open();
                    _confirmMenu.Open();
                }
            }
            else if (_screen == LockerScreen.Trophies && InputProfile.active.Pressed("CANCEL"))
            {
                _desiredScreen = LockerScreen.Locker;
                HUD.CloseAllCorners();
            }
        }
        base.Update();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Background)
        {
            if (_screen == LockerScreen.Locker)
            {
                _background.Scale = new Vector2(1f, 1f);
                _background.Depth = 0.4f;
                _background.Alpha = _fade;
                Graphics.Draw(_background, 0f, 0f);
                string text = _profile.name;
                Vector2 textPos = new Vector2(115f, 46f);
                Graphics.DrawString(text, textPos + new Vector2((0f - Graphics.GetStringWidth(text)) / 2f, 0f), Color.Gray * _fade, 0.5f);
                if (_selection == LockerSelection.Stats)
                {
                    _boardHighlight.Depth = 0.5f;
                    _boardHighlight.Alpha = (0.5f + _pulse.normalized * 0.5f) * _fade;
                    Sprite boardHighlight = _boardHighlight;
                    float xscale = (_boardHighlight.ScaleY = 1f + _pulse.normalized * 0.1f);
                    boardHighlight.ScaleX = xscale;
                    Graphics.Draw(_boardHighlight, 75 + _boardHighlight.w / 2, 60 + _boardHighlight.h / 2);
                    text = "STATISTICS";
                }
                else if (_selection == LockerSelection.Trophies)
                {
                    _trophiesHighlight.Depth = 0.5f;
                    _trophiesHighlight.Alpha = (0.5f + _pulse.normalized * 0.5f) * _fade;
                    Sprite trophiesHighlight = _trophiesHighlight;
                    float xscale = (_trophiesHighlight.ScaleY = 1f + _pulse.normalized * 0.1f);
                    trophiesHighlight.ScaleX = xscale;
                    Graphics.Draw(_trophiesHighlight, 161 + _trophiesHighlight.w / 2, 53 + _trophiesHighlight.h / 2);
                    text = "TROPHIES";
                }
                textPos = new Vector2(160f, 140f);
                Graphics.DrawString(text, textPos + new Vector2((0f - Graphics.GetStringWidth(text)) / 2f, 0f), new Color(14, 20, 27) * _fade, 0.5f);
            }
            else if (_screen == LockerScreen.Stats)
            {
                int index = 0;
                foreach (LockerStat stat in _stats)
                {
                    Vector2 textPos2 = new Vector2(160f, (float)(18 + index * 10) - _statScroll * (float)(_stats.Count * 10 - 150));
                    string text2 = stat.name;
                    Graphics.DrawString(text2, textPos2 + new Vector2((0f - Graphics.GetStringWidth(text2)) / 2f, 0f), stat.color * _fade, 0.5f);
                    index++;
                }
            }
            else if (_screen == LockerScreen.Trophies)
            {
                Vector2 textPos3 = new Vector2(160f, 84f);
                string text3 = "NOPE";
                Graphics.DrawString(text3, textPos3 + new Vector2((0f - Graphics.GetStringWidth(text3)) / 2f, 0f), Color.White * _fade, 0.5f);
            }
        }
        base.PostDrawLayer(layer);
    }
}
