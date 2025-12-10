using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIConnectionInfo : UIMenuItem
{
    private UIMenu _kickMenu;

    private UIMenu _banMenu;

    private UIMenu _blockMenu;

    private UIMenu _rootMenu;

    private Profile _profile;

    private BitmapFont _littleFont;

    private string _nameText;

    private string _nameTextWithoutColor;

    private string _name;

    private bool _showKickMenu;

    private bool _showMuteMenu;

    private int _additionalOptionIndex;

    private int _muteOptionIndex;

    private int _aoKickIndex;

    private int _aoBanIndex;

    private int _aoMuteIndex;

    private int _aoBlockIndex;

    private List<string> _additionalOptions = new List<string> { "Kick", "Ban", "Mute", "Block" };

    private List<string> _muteOptions = new List<string> { "Chat", "Hats", "Room", "Name" };

    public UIConnectionInfo(Profile p, UIMenu rootMenu, UIMenu kickMenu, UIMenu banMenu, UIMenu blockMenu)
        : base(p.nameUI)
    {
        _profile = p;
        _littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        SetFont(_littleFont);
        UpdateName();
        _kickMenu = kickMenu;
        _banMenu = banMenu;
        _blockMenu = blockMenu;
        _rootMenu = rootMenu;
    }

    private void UpdateName()
    {
        Profile p = _profile;
        string colorString = "|" + p.persona.colorUsable.r + "," + p.persona.colorUsable.g + "," + p.persona.colorUsable.b + "|";
        if (p.slotType == SlotType.Spectator)
        {
            colorString = "|DGPURPLE|";
        }
        string name = p.nameUI;
        int nameCount = name.Count();
        bool host = false;
        if (p.connection != null && p.connection.isHost)
        {
            host = true;
            nameCount++;
        }
        int maxName = 17;
        if (host)
        {
            maxName = 16;
        }
        if (nameCount > maxName)
        {
            name = name.Substring(0, maxName - 1) + ".";
            nameCount = maxName;
        }
        for (; nameCount < maxName + 2; nameCount++)
        {
            name += " ";
        }
        if (host)
        {
            name = "@HOSTCROWN@" + name;
        }
        if (p.slotType == SlotType.Spectator)
        {
            name = "@SPECTATOR@" + name;
        }
        if (_profile.muteChat || _profile.muteHat || _profile.muteName || _profile.muteRoom)
        {
            name = "@MUTEICON@" + name;
        }
        if (_profile.blocked)
        {
            name = "@BLOCKICON@" + name;
        }
        _nameTextWithoutColor = name;
        _nameText = colorString + name;
        int pingval = GetPing();
        string ping = pingval.ToString();
        ping += "|WHITE|MS";
        int count = ping.Count();
        ping = ((pingval < 150) ? ("|DGGREEN|" + ping + "@SIGNALGOOD@") : ((pingval < 250) ? ("|DGYELLOW|" + ping + "@SIGNALNORMAL@") : ((_profile.connection == null) ? ("|DGRED|" + ping + "@SIGNALDEAD@") : ("|DGRED|" + ping + "@SIGNALBAD@"))));
        for (; count < 5; count++)
        {
            ping = " " + ping;
        }
        string text = colorString + name + ping;
        _textElement.text = text;
        controlString = "";
        if (p.connection != DuckNetwork.localConnection)
        {
            if (Network.isServer)
            {
                if (p.blocked)
                {
                    _additionalOptions = new List<string> { "Kick", "Ban", "Mute..", "|DGRED|Un-block" };
                }
                else
                {
                    _additionalOptions = new List<string> { "Kick", "Ban", "Mute..", "|DGRED|Block" };
                }
                _aoKickIndex = 0;
                _aoBanIndex = 1;
                _aoMuteIndex = 2;
                _aoBlockIndex = 3;
                controlString = "@MENU2@KICK..";
            }
            else
            {
                if (p.blocked)
                {
                    _additionalOptions = new List<string> { "Mute..", "|DGRED|Un-block" };
                }
                else
                {
                    _additionalOptions = new List<string> { "Mute..", "|DGRED|Block" };
                }
                _aoKickIndex = 99;
                _aoBanIndex = 99;
                _aoMuteIndex = 0;
                _aoBlockIndex = 1;
                controlString = "@MENU2@MUTE..";
            }
        }
        if (Network.canSetObservers)
        {
            if (p.slotType == SlotType.Spectator)
            {
                controlString += " @MENU1@PLAYER";
            }
            else
            {
                controlString += " @MENU1@SPECTATOR";
            }
        }
        if (p.connection.data is User || NetworkDebugger.enabled)
        {
            controlString += " @SELECT@@STEAMICON@";
        }
    }

    public override void Activate(string trigger)
    {
        if (_showKickMenu)
        {
            return;
        }
        if (_profile.connection != null)
        {
            if (_profile.connection != DuckNetwork.localConnection && trigger == "MENU2")
            {
                _additionalOptionIndex = 0;
                _showKickMenu = true;
                UIMenu.globalUILock = true;
                HUD.ClearCorners();
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@SELECT@SELECT");
                HUD.AddCornerControl(HUDCorner.BottomRight, "@CANCEL@BACK");
            }
            else if (Network.isServer && trigger == "MENU1" && Network.canSetObservers && _profile.readyForSpectatorChange)
            {
                if (_profile.slotType != SlotType.Spectator)
                {
                    DuckNetwork.MakeSpectator(_profile);
                    SFX.Play("menuBlip01");
                    UpdateName();
                }
                else
                {
                    DuckNetwork.MakePlayer(_profile);
                    SFX.Play("menuBlip01");
                    UpdateName();
                }
            }
            if (trigger == "SELECT")
            {
                if (_profile.connection.data is User)
                {
                    Steam.OverlayOpenURL("http://steamcommunity.com/profiles/" + (_profile.connection.data as User).id);
                }
                else if (NetworkDebugger.enabled && Steam.user != null)
                {
                    Steam.OverlayOpenURL("http://steamcommunity.com/profiles/" + Steam.user.id);
                }
            }
        }
        base.Activate(trigger);
    }

    public int GetPing()
    {
        int pingval = 1000;
        if (_profile.connection != null)
        {
            pingval = ((_profile.connection != DuckNetwork.localConnection) ? ((int)Math.Round(_profile.connection.manager.ping * 1000f)) : 0);
            _ = _profile.connection.status;
        }
        else
        {
            pingval = 1000;
        }
        return pingval;
    }

    public override void Update()
    {
        if (_showMuteMenu)
        {
            if (Input.Pressed("CANCEL"))
            {
                _showMuteMenu = false;
            }
            else if (Input.Pressed("UP"))
            {
                if (_muteOptionIndex > 0)
                {
                    _muteOptionIndex--;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            else if (Input.Pressed("DOWN"))
            {
                if (_muteOptionIndex < _muteOptions.Count - 1)
                {
                    _muteOptionIndex++;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            else if (Input.Pressed("SELECT"))
            {
                if (_muteOptionIndex == 0)
                {
                    _profile.muteChat = !_profile.muteChat;
                }
                else if (_muteOptionIndex == 1)
                {
                    _profile.muteHat = !_profile.muteHat;
                }
                else if (_muteOptionIndex == 2)
                {
                    _profile.muteRoom = !_profile.muteRoom;
                }
                else if (_muteOptionIndex == 3)
                {
                    _profile.muteName = !_profile.muteName;
                }
                _profile._blockStatusDirty = true;
                SFX.Play("textLetter", 0.7f);
            }
        }
        else if (_showKickMenu)
        {
            if (Input.Pressed("CANCEL"))
            {
                HUD.ClearCorners();
                _showKickMenu = false;
                UIMenu.globalUILock = false;
                Options.Save();
            }
            else if (Input.Pressed("SELECT"))
            {
                DuckNetwork.core.kickContext = _profile;
                bool close = true;
                if (_additionalOptionIndex == _aoKickIndex)
                {
                    _rootMenu.Close();
                    _kickMenu.Open();
                    if (MonoMain.pauseMenu == _rootMenu)
                    {
                        MonoMain.pauseMenu = _kickMenu;
                    }
                }
                else if (_additionalOptionIndex == _aoBanIndex)
                {
                    _rootMenu.Close();
                    _banMenu.Open();
                    if (MonoMain.pauseMenu == _rootMenu)
                    {
                        MonoMain.pauseMenu = _banMenu;
                    }
                }
                else if (_additionalOptionIndex == _aoMuteIndex)
                {
                    _showMuteMenu = true;
                    close = false;
                }
                else if (_additionalOptionIndex == _aoBlockIndex)
                {
                    if (_profile.blocked)
                    {
                        DuckNetwork.UnblockPlayer(_profile);
                        close = true;
                        UpdateName();
                    }
                    else
                    {
                        _rootMenu.Close();
                        _blockMenu.Open();
                        if (MonoMain.pauseMenu == _rootMenu)
                        {
                            MonoMain.pauseMenu = _blockMenu;
                        }
                    }
                }
                if (close)
                {
                    HUD.ClearCorners();
                    _showKickMenu = false;
                    UIMenu.globalUILock = false;
                }
            }
            else if (Input.Pressed("UP"))
            {
                if (_additionalOptionIndex > 0)
                {
                    _additionalOptionIndex--;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            else if (Input.Pressed("DOWN") && _additionalOptionIndex < _additionalOptions.Count - 1)
            {
                _additionalOptionIndex++;
                SFX.Play("textLetter", 0.7f);
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        _textElement.text = "";
        _littleFont.Draw(_nameText, position + new Vec2(-88f, -3f), Color.White, base.depth + 10);
        int pingval = GetPing();
        string ping = pingval.ToString();
        ping += "|WHITE|MS";
        ping.Count();
        ping = ((pingval < 150) ? ("|DGGREEN|" + ping + "@SIGNALGOOD@") : ((pingval < 250) ? ("|DGYELLOW|" + ping + "@SIGNALNORMAL@") : ((_profile.connection == null) ? ("|DGRED|" + ping + "@SIGNALDEAD@") : ("|DGRED|" + ping + "@SIGNALBAD@"))));
        _littleFont.Draw(ping, position + new Vec2(90f - _littleFont.GetWidth(ping), -3f), Color.White, base.depth + 10);
        if (_showKickMenu)
        {
            Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Layer.HUD.width, Layer.HUD.height), Color.Black * 0.5f, 0.85f);
            Vec2 posTL = position + new Vec2(-60f, 4f);
            Vec2 posBR = posTL + new Vec2(76f, _additionalOptions.Count * 8 + 3);
            Graphics.DrawRect(posTL, posBR, Color.Black, 0.9f);
            Graphics.DrawRect(posTL, posBR, Color.White, 0.9f, filled: false);
            for (int i = 0; i < _additionalOptions.Count; i++)
            {
                _littleFont.Draw(_additionalOptions[i], posTL + new Vec2(10f, 3 + i * 8), (_additionalOptionIndex == i) ? Color.White : (Color.White * 0.6f), 0.91f);
                if (_additionalOptionIndex == i)
                {
                    Graphics.Draw(_arrow._image, posTL.x + 4f, posTL.y + 6f + (float)(i * 8), 0.91f);
                }
                if (i != _aoMuteIndex || !_showMuteMenu)
                {
                    continue;
                }
                Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Layer.HUD.width, Layer.HUD.height), Color.Black * 0.5f, 0.92f);
                Vec2 muteTL = posTL + new Vec2(8f, 26f);
                Vec2 muteBR = muteTL + new Vec2(60f, _muteOptions.Count * 8 + 4);
                Graphics.DrawRect(muteTL, muteBR, Color.Black, 0.93f);
                Graphics.DrawRect(muteTL, muteBR, Color.White, 0.93f, filled: false);
                for (int j = 0; j < _muteOptions.Count; j++)
                {
                    string muteText = _muteOptions[j];
                    muteText = ((j == 0 && _profile.muteChat) ? ("@DELETEFLAG_ON@" + muteText) : ((j == 1 && _profile.muteHat) ? ("@DELETEFLAG_ON@" + muteText) : ((j == 2 && _profile.muteRoom) ? ("@DELETEFLAG_ON@" + muteText) : ((j != 3 || !_profile.muteName) ? ("@DELETEFLAG_OFF@" + muteText) : ("@DELETEFLAG_ON@" + muteText)))));
                    _littleFont.Draw(muteText, muteTL + new Vec2(10f, 4 + j * 8), (_muteOptionIndex == j) ? Color.White : (Color.White * 0.6f), 0.94f);
                    if (_muteOptionIndex == j)
                    {
                        Graphics.Draw(_arrow._image, muteTL.x + 4f, muteTL.y + 6f + (float)(j * 8), 0.94f);
                    }
                }
            }
        }
        base.Draw();
    }
}
