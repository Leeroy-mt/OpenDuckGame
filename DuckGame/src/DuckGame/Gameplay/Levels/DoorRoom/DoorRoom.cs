using System;
using System.Collections.Generic;

namespace DuckGame;

public class DoorRoom : Level
{
    private Sprite _door;

    private Sprite _unlitDoor;

    private int _selectorPosition;

    private int _desiredSelectorPosition;

    private float _slide;

    private float _slideTo;

    private DoorTransition _transition;

    private DoorTransition _desiredTransition;

    private List<Profile> _profiles;

    private Profile _profile;

    private float _fade;

    public DoorRoom(Profile p = null)
    {
        _centeredView = true;
        if (p != null)
        {
            _profiles = Profiles.allCustomProfiles;
            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i] == p)
                {
                    _selectorPosition = i;
                    break;
                }
            }
            _desiredSelectorPosition = _selectorPosition;
        }
        _profile = p;
    }

    public override void Initialize()
    {
        if (Music.currentSong != "RaceDay")
        {
            Music.Play("RaceDay");
        }
        _door = new Sprite("litDoor");
        _door.CenterOrigin();
        _unlitDoor = new Sprite("unlitDoor");
        _unlitDoor.CenterOrigin();
        _profiles = Profiles.allCustomProfiles;
        if (_profiles.Count == 0)
        {
            _profile = Profiles.DefaultPlayer1;
        }
        else
        {
            _profile = _profiles[_selectorPosition];
        }
        if (_profiles.Count > 0)
        {
            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@STATS");
        }
        HUD.AddCornerMessage(HUDCorner.BottomMiddle, "@MENU2@ALBUM");
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
        base.backgroundColor = Color.Black;
        base.Initialize();
    }

    public override void Update()
    {
        if (Input.CheckCode("LEFT|LEFT|RIGHT|RIGHT|SHOOT|SHOOT|UP|UP|DOWN|DOWN|SHOOT|SHOOT"))
        {
            Global.data.onlineMatches.value = 100;
        }
        if (Input.CheckCode("UP|UP|UP|SHOOT|DOWN|DOWN|DOWN|SHOOT|LEFT|RIGHT|SHOOT"))
        {
            Global.data.winsAsHair.value = 100;
        }
        if (_desiredTransition != _transition)
        {
            _fade = Lerp.Float(_fade, 0f, 0.06f);
            if (_fade <= 0f)
            {
                _transition = _desiredTransition;
                if (_transition == DoorTransition.Profile)
                {
                    Graphics.fade = 0f;
                    Level.current = new LockerRoom(_profile);
                }
                else if (_transition == DoorTransition.Exit)
                {
                    Graphics.fade = 0f;
                    Level.current = new TitleScreen();
                }
                else if (_transition == DoorTransition.Album)
                {
                    Graphics.fade = 0f;
                    Level.current = new Album();
                }
            }
        }
        else
        {
            _fade = Lerp.Float(_fade, 1f, 0.06f);
            if (_selectorPosition == _desiredSelectorPosition)
            {
                if (InputProfile.active.Down("MENULEFT"))
                {
                    SelectUp();
                }
                if (InputProfile.active.Down("MENURIGHT"))
                {
                    SelectDown();
                }
                if (InputProfile.active.Pressed("SELECT") && _profile != null)
                {
                    _desiredTransition = DoorTransition.Profile;
                    HUD.CloseAllCorners();
                }
            }
            if (InputProfile.active.Pressed("CANCEL"))
            {
                _desiredTransition = DoorTransition.Exit;
                HUD.CloseAllCorners();
            }
            if (InputProfile.active.Pressed("MENU2"))
            {
                _desiredTransition = DoorTransition.Album;
                HUD.CloseAllCorners();
            }
            if (_slideTo != 0f && _slide != _slideTo)
            {
                _slide = Lerp.Float(_slide, _slideTo, 0.05f);
            }
            else if (_slideTo != 0f && _slide == _slideTo)
            {
                _slide = 0f;
                _slideTo = 0f;
                SFX.Play("textLetter", 0.7f);
                _selectorPosition = _desiredSelectorPosition;
                if (_profiles.Count > 0)
                {
                    _profile = _profiles[_selectorPosition];
                }
            }
        }
        base.Update();
    }

    public void SelectDown()
    {
        if (_desiredSelectorPosition >= _profiles.Count - 1)
        {
            _desiredSelectorPosition = 0;
        }
        else
        {
            _desiredSelectorPosition++;
        }
        _slideTo = 1f;
    }

    public void SelectUp()
    {
        if (_desiredSelectorPosition <= 0)
        {
            _desiredSelectorPosition = _profiles.Count - 1;
        }
        else
        {
            _desiredSelectorPosition--;
        }
        _slideTo = -1f;
    }

    private int ProfileIndexAdd(int index, int plus)
    {
        if (_profiles.Count == 0)
        {
            return -1;
        }
        int val;
        for (val = index + plus; val >= _profiles.Count; val -= _profiles.Count)
        {
        }
        for (; val < 0; val += _profiles.Count)
        {
        }
        return val;
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Background)
        {
            Vec2 pos = new Vec2(0f, 0f);
            float xOff = -260f;
            float itemWidth = 140f;
            for (int i = 0; i < 7; i++)
            {
                int add = -3 + i;
                int index = ProfileIndexAdd(_selectorPosition, add);
                string name = "NO PROFILE";
                if (index != -1)
                {
                    name = _profiles[index].name;
                }
                float middle = pos.x + xOff + 3f * itemWidth;
                float xpos = pos.x + xOff + (float)i * itemWidth + (0f - _slide) * itemWidth;
                float num = Maths.Clamp((100f - Math.Abs(xpos - middle)) / 100f, 0f, 1f);
                float colorMult = num * Maths.NormalizeSection(num, 0f, 0.9f);
                _door.color = Color.White * colorMult * _fade;
                _door.depth = colorMult * 0.8f;
                if (colorMult < 1f)
                {
                    _unlitDoor.alpha = (1f - colorMult) * 0.5f * _fade;
                    Graphics.Draw(_unlitDoor, xpos, 90f);
                }
                if (colorMult > 0f)
                {
                    Graphics.Draw(_door, xpos, 90f);
                }
                string text = name;
                float mul = (colorMult + 1f) * 0.5f;
                float vOffset = 0f;
                float hOffset = 0f;
                Vec2 fontscale = new Vec2(1f, 1f);
                if (text.Length > 9)
                {
                    fontscale = new Vec2(0.75f, 0.75f);
                    vOffset = 1f;
                    hOffset = 1f;
                }
                if (text.Length > 12)
                {
                    fontscale = new Vec2(0.5f, 0.5f);
                    vOffset = 2f;
                    hOffset = 1f;
                }
                Graphics.DrawString(text, new Vec2(xpos - Graphics.GetStringWidth(text, thinButtons: false, fontscale.x) / 2f + hOffset, 35f + vOffset), new Color((byte)Math.Round(165f * mul), (byte)Math.Round(100f * mul), (byte)Math.Round(34f * mul)) * _fade, 0.9f, null, fontscale.x);
            }
            _door.scale = new Vec2(1f, 1f);
            _door.depth = 0.4f;
        }
        base.PostDrawLayer(layer);
    }
}
