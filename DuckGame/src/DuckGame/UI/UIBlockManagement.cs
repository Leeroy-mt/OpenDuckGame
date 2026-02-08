using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class UIBlockManagement : UIMenu
{
    private List<KeyValuePair<ulong, bool>> items = new List<KeyValuePair<ulong, bool>>();

    private Sprite _downArrow;

    private BitmapFont _littleFont;

    private UIMenu _openOnClose;

    private bool _opening;

    private int _topOffset;

    private readonly int kMaxInView = 13;

    public UIBlockManagement(UIMenu openOnClose)
        : base("BLOCKED USERS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 260f, 160f)
    {
        UIBox box = new UIBox(0f, 0f, 100f, 130f, vert: true, isVisible: false);
        Add(box);
        _littleFont = new BitmapFont("smallBiosFont", 7, 6);
        _downArrow = new Sprite("cloudDown");
        _downArrow.CenterOrigin();
        _openOnClose = openOnClose;
    }

    private void UnblockUsers()
    {
    }

    public override void Open()
    {
        HUD.CloseAllCorners();
        _opening = true;
        RebuildItemList();
        base.Open();
    }

    public void RebuildItemList()
    {
        _selection = 0;
        _topOffset = 0;
        items.Clear();
        foreach (ulong u in Options.Data.blockedPlayers)
        {
            items.Add(new KeyValuePair<ulong, bool>(u, value: true));
        }
        foreach (ulong u2 in Options.Data.unblockedPlayers)
        {
            items.Add(new KeyValuePair<ulong, bool>(u2, value: false));
        }
        bool hasExtraPlayers = false;
        foreach (ulong u3 in Options.Data.recentPlayers)
        {
            if (!Options.Data.blockedPlayers.Contains(u3) && !Options.Data.unblockedPlayers.Contains(u3))
            {
                if (!hasExtraPlayers)
                {
                    hasExtraPlayers = true;
                    items.Add(new KeyValuePair<ulong, bool>(0uL, value: false));
                }
                items.Add(new KeyValuePair<ulong, bool>(u3, Options.Data.blockedPlayers.Contains(u3)));
            }
        }
    }

    public override void Close()
    {
        HUD.CloseAllCorners();
        base.Close();
    }

    public override void Update()
    {
        if (base.open && !_opening)
        {
            if (Input.Pressed("MENUUP") && _selection > 0)
            {
                _selection--;
                if (items[_selection].Key == 0L)
                {
                    _selection--;
                }
                if (_selection < _topOffset)
                {
                    _topOffset = _selection;
                }
                SFX.Play("textLetter", 0.7f);
            }
            if (Input.Pressed("MENUDOWN") && _selection < items.Count - 1)
            {
                _selection++;
                if (items[_selection].Key == 0L)
                {
                    _selection++;
                }
                if (_selection > _topOffset + kMaxInView)
                {
                    _topOffset++;
                }
                SFX.Play("textLetter", 0.7f);
            }
            if (_selection >= 0 && _selection < items.Count && Input.Pressed("MENU1"))
            {
                KeyValuePair<ulong, bool> item = items[_selection];
                if (!item.Value)
                {
                    if (!Options.Data.blockedPlayers.Contains(item.Key))
                    {
                        Options.Data.blockedPlayers.Add(item.Key);
                    }
                    Options.Data.unblockedPlayers.Remove(item.Key);
                    Options.Data.muteSettings[item.Key] = "CHR";
                    SFX.Play("textLetter", 0.7f);
                    MakeDirty();
                }
                else if (item.Value)
                {
                    Options.Data.blockedPlayers.Remove(item.Key);
                    if (!Options.Data.unblockedPlayers.Contains(item.Key))
                    {
                        Options.Data.unblockedPlayers.Add(item.Key);
                    }
                    Options.Data.muteSettings[item.Key] = "";
                    SFX.Play("textLetter", 0.7f);
                    MakeDirty();
                }
            }
            if (Input.Pressed("SELECT"))
            {
                Steam.OverlayOpenURL("http://steamcommunity.com/profiles/" + items[_selection].Key);
            }
            if (Input.Pressed("CANCEL"))
            {
                if (_openOnClose != null)
                {
                    new UIMenuActionOpenMenu(this, _openOnClose).Activate();
                }
                else
                {
                    new UIMenuActionCloseMenu(this).Activate();
                }
            }
        }
        _opening = false;
        base.Update();
    }

    private void MakeDirty()
    {
        foreach (Profile item in Profiles.all)
        {
            item._blockStatusDirty = true;
        }
        RebuildItemList();
        if (_selection >= items.Count)
        {
            _selection = items.Count;
        }
    }

    public override void Draw()
    {
        if (base.open)
        {
            Vector2 pos = new Vector2(base.X - 124f, base.Y - 56f);
            float yOffset = 0f;
            int idx = 0;
            int drawIndex = 0;
            if (items.Count == 0)
            {
                _littleFont.Draw("No blocked users! Happy day!", pos + new Vector2(8f, yOffset), Color.White, 0.5f);
            }
            foreach (KeyValuePair<ulong, bool> pair in items)
            {
                if (idx < _topOffset)
                {
                    idx++;
                    continue;
                }
                if (_topOffset > 0)
                {
                    _downArrow.flipV = true;
                    Graphics.Draw(_downArrow, base.X, pos.Y - 2f, 0.5f);
                }
                if (drawIndex > kMaxInView)
                {
                    _downArrow.flipV = false;
                    Graphics.Draw(_downArrow, base.X, pos.Y + yOffset, 0.5f);
                    break;
                }
                string drawName = pair.Key.ToString();
                if (pair.Key == 0L)
                {
                    drawName = " |DGBLUE|Recently played with:";
                }
                else
                {
                    User user = User.GetUser(pair.Key);
                    if (user != null)
                    {
                        drawName = user.name;
                    }
                    if (drawName.Length > 31)
                    {
                        drawName = drawName.Substring(0, 30) + "..";
                    }
                    drawName = (pair.Value ? "@DELETEFLAG_ON@" : "@DELETEFLAG_OFF@") + drawName;
                    drawName = ((idx != _selection) ? (" " + drawName) : ("@SELECTICON@" + drawName));
                }
                if (pair.Value)
                {
                    drawName = "|DGRED|" + drawName;
                }
                _littleFont.Draw(drawName, pos + new Vector2(0f, yOffset), Color.White, 0.5f);
                yOffset += 8f;
                idx++;
                drawIndex++;
            }
            string controlsText = "@CANCEL@BACK";
            if (items.Count > 0 && _selection >= 0 && _selection < items.Count)
            {
                controlsText = ((!items[_selection].Value) ? (controlsText + " @MENU1@BLOCK") : (controlsText + " @MENU1@UN-BLOCK"));
                controlsText += " @SELECT@@STEAMICON@";
            }
            _littleFont.Draw(controlsText, new Vector2(base.X - _littleFont.GetWidth(controlsText) / 2f, base.Y + 64f), Color.White, 0.5f);
        }
        base.Draw();
    }
}
