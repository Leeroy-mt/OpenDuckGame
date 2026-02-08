using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class ContextToolbarItem : ContextMenu, IContextListener
{
    private MessageDialogue _unsavedChangesDialogue;

    private ToolbarButton _newButton;

    private ToolbarButton _saveButton;

    private ToolbarButton _loadButton;

    private ToolbarButton _playButton;

    private ToolbarButton _gridButton;

    private ToolbarButton _quitButton;

    private ContextMenu _newMenu;

    private ContextMenu _saveMenu;

    private ContextMenu _gridMenu;

    private ContextMenu _quitMenu;

    private ContextMenu _uploadMenu;

    public string toolBarToolTip;

    private List<ToolbarButton> _buttons = new List<ToolbarButton>();

    private bool _doLoad;

    public bool isPushingUp;

    private bool menuOpen
    {
        get
        {
            if ((_gridMenu == null || !_gridMenu.opened) && (_saveMenu == null || !_saveMenu.opened) && (_newMenu == null || !_newMenu.opened) && (_quitMenu == null || !_quitMenu.opened))
            {
                if (_uploadMenu != null)
                {
                    return _uploadMenu.opened;
                }
                return false;
            }
            return true;
        }
    }

    public ContextToolbarItem(ContextMenu owner)
        : base(owner)
    {
        toolBarToolTip = "";
    }

    public void ShowUnsavedChangesDialogue(string pContext, string pDescription, ContextMenu pConfirmItem)
    {
        Closed();
        _unsavedChangesDialogue.Open("UNSAVED CHANGES", "", "Your current level has unsaved\nchanges! Are you sure you want to\n" + pDescription + "?");
        Editor.lockInput = _unsavedChangesDialogue;
        _unsavedChangesDialogue.result = false;
        _unsavedChangesDialogue.contextString = pContext;
        _unsavedChangesDialogue.confirmItem = pConfirmItem;
    }

    public override void Selected(ContextMenu item)
    {
        if (item.text == "SPECIAL")
        {
            base.Selected(item);
            return;
        }
        if (item.text == "CANCEL")
        {
            (Level.current as Editor).CloseMenu();
        }
        if (item.text == "NEW")
        {
            if (!Editor.hasUnsavedChanges || (_unsavedChangesDialogue.contextString == "NEW" && _unsavedChangesDialogue.result))
            {
                Editor obj = Level.current as Editor;
                obj.ClearEverything();
                obj.saveName = "";
                Editor._currentLevelData.metaData.onlineMode = false;
                obj.CloseMenu();
                _unsavedChangesDialogue.result = false;
            }
            else
            {
                ShowUnsavedChangesDialogue("NEW", "create a new level", item);
            }
        }
        if (item.text == "NEW ONLINE")
        {
            if (!Editor.hasUnsavedChanges || (_unsavedChangesDialogue.contextString == "NEW ONLINE" && _unsavedChangesDialogue.result))
            {
                Editor obj2 = Level.current as Editor;
                obj2.ClearEverything();
                obj2.saveName = "";
                Editor._currentLevelData.metaData.onlineMode = true;
                obj2.CloseMenu();
                _unsavedChangesDialogue.result = false;
            }
            else
            {
                ShowUnsavedChangesDialogue("NEW ONLINE", "create a new online level", item);
            }
        }
        if (item.text == "NEW ARCADE MACHINE")
        {
            if (!Editor.hasUnsavedChanges || (_unsavedChangesDialogue.contextString == "NEW ARCADE MACHINE" && _unsavedChangesDialogue.result))
            {
                Editor obj3 = Level.current as Editor;
                obj3.ClearEverything();
                obj3.saveName = "";
                obj3.AddObject(new ImportMachine(0f, 0f));
                obj3.CloseMenu();
                _unsavedChangesDialogue.result = false;
            }
            else
            {
                ShowUnsavedChangesDialogue("NEW ARCADE MACHINE", "Create a new Arcade Machine.", item);
            }
        }
        if (item.text == "NEW MAP PART")
        {
            if (!Editor.hasUnsavedChanges || (_unsavedChangesDialogue.contextString == "NEW RANDOM MAP PART" && _unsavedChangesDialogue.result))
            {
                Editor obj4 = Level.current as Editor;
                obj4.ClearEverything();
                obj4.saveName = "";
                obj4._miniMode = true;
                obj4.CloseMenu();
                _unsavedChangesDialogue.result = false;
            }
            else
            {
                ShowUnsavedChangesDialogue("NEW MAP PART", "Create a new Map Part", item);
            }
        }
        if (item.text == "SAVE")
        {
            Editor obj5 = Level.current as Editor;
            obj5.Save();
            obj5.CloseMenu();
        }
        if (item.text.Contains("PUBLISH"))
        {
            Editor obj6 = Level.current as Editor;
            obj6.SteamUpload();
            obj6.CloseMenu();
        }
        if (item.text == "SAVE AS")
        {
            Editor obj7 = Level.current as Editor;
            obj7.SaveAs();
            obj7.CloseMenu();
        }
        if (item.text == "8x8")
        {
            Editor obj8 = Level.current as Editor;
            obj8.cellSize = 8f;
            obj8.CloseMenu();
        }
        if (item.text == "16x16")
        {
            Editor obj9 = Level.current as Editor;
            obj9.cellSize = 16f;
            obj9.CloseMenu();
        }
        if (item.text == "32x32")
        {
            Editor obj10 = Level.current as Editor;
            obj10.cellSize = 32f;
            obj10.CloseMenu();
        }
        if (item.text == "QUIT")
        {
            if (!Editor.hasUnsavedChanges || (_unsavedChangesDialogue.contextString == "QUIT" && _unsavedChangesDialogue.result))
            {
                Editor obj11 = Level.current as Editor;
                obj11.Quit();
                obj11.CloseMenu();
                _unsavedChangesDialogue.result = false;
            }
            else
            {
                ShowUnsavedChangesDialogue("QUIT", "quit to the main menu", item);
            }
        }
    }

    public override void Hover()
    {
        base.opened = true;
    }

    public override void Initialize()
    {
        _unsavedChangesDialogue = new MessageDialogue(this);
        Level.Add(_unsavedChangesDialogue);
        _newButton = new ToolbarButton(this, 0, "NEW LEVEL");
        _saveButton = new ToolbarButton(this, 2, "SAVE LEVEL");
        _loadButton = new ToolbarButton(this, 1, "LOAD LEVEL");
        _playButton = new ToolbarButton(this, 10, "TEST LEVEL");
        _gridButton = new ToolbarButton(this, 11, "CHANGE GRID");
        _quitButton = new ToolbarButton(this, 12, "EXIT EDITOR");
        itemSize.Y = 16f;
        float xpos = Position.X;
        _playButton.X = xpos;
        _playButton.Y = Position.Y;
        _buttons.Add(_playButton);
        xpos += 18f;
        _saveButton.X = xpos;
        _saveButton.Y = Position.Y;
        _buttons.Add(_saveButton);
        xpos += 18f;
        _loadButton.X = xpos;
        _loadButton.Y = Position.Y;
        _buttons.Add(_loadButton);
        xpos += 18f;
        _newButton.X = xpos;
        _newButton.Y = Position.Y;
        _buttons.Add(_newButton);
        xpos += 18f;
        _gridButton.X = xpos;
        _gridButton.Y = Position.Y;
        _buttons.Add(_gridButton);
        xpos += 18f;
        _quitButton.X = xpos;
        _quitButton.Y = Position.Y;
        _buttons.Add(_quitButton);
    }

    public override void ParentCloseAction()
    {
        _selectedIndex = -1;
        foreach (ToolbarButton button in _buttons)
        {
            button.hover = false;
        }
    }

    public override void Update()
    {
        if (_unsavedChangesDialogue.opened)
        {
            return;
        }
        if (_doLoad)
        {
            _doLoad = false;
            if (!Editor.hasUnsavedChanges || _unsavedChangesDialogue.result)
            {
                Editor obj = Level.current as Editor;
                obj.Load();
                obj.CloseMenu();
            }
            _unsavedChangesDialogue.result = false;
        }
        toolBarToolTip = "";
        if (!_opening && base.opened && Editor.inputMode == EditorInput.Gamepad)
        {
            if (menuOpen)
            {
                return;
            }
            if (Input.Pressed("MENUUP"))
            {
                base.opened = false;
                if (owner is ContextMenu o)
                {
                    o._opening = true;
                    foreach (ToolbarButton button in _buttons)
                    {
                        button.hover = false;
                    }
                    toolBarToolTip = "";
                    return;
                }
            }
            if (Input.Pressed("MENUDOWN"))
            {
                base.opened = false;
                if (owner is ContextMenu o2)
                {
                    o2.selectedIndex++;
                    foreach (ToolbarButton button2 in _buttons)
                    {
                        button2.hover = false;
                    }
                    toolBarToolTip = "";
                    return;
                }
            }
            if (Input.Pressed("MENULEFT"))
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _buttons.Count - 1;
                }
            }
            else if (Input.Pressed("MENURIGHT"))
            {
                _selectedIndex++;
                if (_selectedIndex > _buttons.Count - 1)
                {
                    _selectedIndex = 0;
                }
            }
            int index = 0;
            foreach (ToolbarButton b in _buttons)
            {
                if (_selectedIndex == index)
                {
                    b.hover = true;
                    toolBarToolTip = b.hoverText;
                    if (Input.Pressed("SELECT"))
                    {
                        ButtonPressed(b);
                    }
                }
                else
                {
                    b.hover = false;
                }
                index++;
            }
        }
        float xpos = Position.X;
        _playButton.X = xpos;
        _playButton.Y = Position.Y;
        xpos += 18f;
        _saveButton.X = xpos;
        _saveButton.Y = Position.Y;
        xpos += 18f;
        _loadButton.X = xpos;
        _loadButton.Y = Position.Y;
        xpos += 18f;
        _newButton.X = xpos;
        _newButton.Y = Position.Y;
        xpos += 18f;
        _gridButton.X = xpos;
        _gridButton.Y = Position.Y;
        xpos += 18f;
        _quitButton.X = xpos;
        _quitButton.Y = Position.Y;
        foreach (ToolbarButton button3 in _buttons)
        {
            button3.DoUpdate();
        }
        base.Update();
    }

    public override void Terminate()
    {
        Level.current.RemoveThing(_newButton);
        Level.current.RemoveThing(_saveButton);
        Level.current.RemoveThing(_loadButton);
        Level.current.RemoveThing(_playButton);
        Level.current.RemoveThing(_gridButton);
        Level.current.RemoveThing(_quitButton);
        Closed();
        base.Terminate();
    }

    public override bool HasOpen()
    {
        return base.opened;
    }

    public void ButtonPressed(ToolbarButton button)
    {
        SFX.Play("highClick", 0.3f, 0.2f);
        ContextMenu menu = null;
        Vector2 menuOffset = new Vector2(2f, 21f);
        if (button == _newButton)
        {
            Closed();
            _newMenu = new ContextMenu(this, null, hasToproot: true, button.Position);
            _newMenu.X = Position.X - menuOffset.X;
            _newMenu.Y = Position.Y + menuOffset.Y;
            _newMenu.root = true;
            _newMenu.Depth = base.Depth + 10;
            Selected();
            ContextMenu confirmItem = new ContextMenu(this);
            confirmItem.itemSize.X = 60f;
            confirmItem.text = "NEW";
            _newMenu.AddItem(confirmItem);
            confirmItem = new ContextMenu(this);
            confirmItem.itemSize.X = 60f;
            confirmItem.text = "NEW ONLINE";
            _newMenu.AddItem(confirmItem);
            confirmItem = new ContextMenu(this);
            confirmItem.itemSize.X = 60f;
            confirmItem.text = "SPECIAL";
            ContextMenu subItem = new ContextMenu(this);
            subItem.itemSize.X = 90f;
            subItem.text = "NEW ARCADE MACHINE";
            confirmItem.AddItem(subItem);
            _newMenu.AddItem(confirmItem);
            ContextMenu cancelItem = new ContextMenu(this);
            cancelItem.itemSize.X = 60f;
            cancelItem.text = "CANCEL";
            _newMenu.AddItem(cancelItem);
            Level.Add(_newMenu);
            _newMenu.opened = true;
            menu = _newMenu;
        }
        if (button == _saveButton)
        {
            Closed();
            _saveMenu = new ContextMenu(this, null, hasToproot: true, button.Position);
            _saveMenu.X = Position.X - menuOffset.X;
            _saveMenu.Y = Position.Y + menuOffset.Y;
            _saveMenu.root = true;
            _saveMenu.Depth = base.Depth + 10;
            Selected();
            ContextMenu saveItem = new ContextMenu(this);
            saveItem.itemSize.X = 40f;
            saveItem.text = "SAVE";
            _saveMenu.AddItem(saveItem);
            ContextMenu loadItem = new ContextMenu(this);
            loadItem.itemSize.X = 40f;
            loadItem.text = "SAVE AS";
            _saveMenu.AddItem(loadItem);
            if (Steam.IsInitialized())
            {
                ContextMenu publishItem = new ContextMenu(this);
                publishItem.itemSize.X = 40f;
                publishItem.text = "@STEAMICON@ PUBLISH";
                _saveMenu.AddItem(publishItem);
            }
            Level.Add(_saveMenu);
            _saveMenu.opened = true;
            menu = _saveMenu;
        }
        if (button == _gridButton)
        {
            Closed();
            _gridMenu = new ContextMenu(this, null, hasToproot: true, button.Position);
            _gridMenu.X = Position.X - menuOffset.X;
            _gridMenu.Y = Position.Y + menuOffset.Y;
            _gridMenu.root = true;
            _gridMenu.Depth = base.Depth + 10;
            Selected();
            ContextMenu item = new ContextMenu(this);
            item.itemSize.X = 60f;
            item.text = "8x8";
            _gridMenu.AddItem(item);
            item = new ContextMenu(this);
            item.itemSize.X = 60f;
            item.text = "16x16";
            _gridMenu.AddItem(item);
            item = new ContextMenu(this);
            item.itemSize.X = 60f;
            item.text = "32x32";
            _gridMenu.AddItem(item);
            Level.Add(_gridMenu);
            _gridMenu.opened = true;
            menu = _gridMenu;
        }
        if (button == _loadButton)
        {
            _doLoad = true;
            if (Editor.hasUnsavedChanges)
            {
                ShowUnsavedChangesDialogue("LOAD", "load a new level", null);
            }
        }
        if (button == _playButton)
        {
            (Level.current as Editor).Play();
        }
        if (button == _quitButton)
        {
            Closed();
            _quitMenu = new ContextMenu(this, null, hasToproot: true, button.Position);
            _quitMenu.X = Position.X - menuOffset.X;
            _quitMenu.Y = Position.Y + menuOffset.Y;
            _quitMenu.root = true;
            _quitMenu.Depth = base.Depth + 10;
            Selected();
            ContextMenu confirmItem2 = new ContextMenu(this);
            confirmItem2.itemSize.X = 60f;
            confirmItem2.text = "QUIT";
            _quitMenu.AddItem(confirmItem2);
            ContextMenu cancelItem2 = new ContextMenu(this);
            cancelItem2.itemSize.X = 60f;
            cancelItem2.text = "CANCEL";
            _quitMenu.AddItem(cancelItem2);
            Level.Add(_quitMenu);
            _quitMenu.opened = true;
            menu = _quitMenu;
        }
        if (menu != null && menu.Y + menu.menuSize.Y > base.layer.camera.height - 4f)
        {
            isPushingUp = true;
            float oldY = menu.Y;
            menu.Y = base.layer.camera.height - 4f - menu.menuSize.Y;
            menu._toprootPosition.Y += menu.Y - oldY;
            ContextMenu m = owner as ContextMenu;
            if (m != null)
            {
                m._openedOffset = 0f;
                m.Y = menu.Y - 16f - m.menuSize.Y;
            }
            m.PositionItems();
            menu.PositionItems();
            isPushingUp = false;
        }
    }

    public override void Closed()
    {
        if (_newMenu != null)
        {
            Level.Remove(_newMenu);
            _newMenu = null;
        }
        if (_saveMenu != null)
        {
            Level.Remove(_saveMenu);
            _saveMenu = null;
        }
        if (_gridMenu != null)
        {
            Level.Remove(_gridMenu);
            _gridMenu = null;
        }
        if (_quitMenu != null)
        {
            Level.Remove(_quitMenu);
            _quitMenu = null;
        }
        if (_uploadMenu != null)
        {
            Level.Remove(_uploadMenu);
            _uploadMenu = null;
        }
        toolBarToolTip = "";
    }

    public override void Draw()
    {
        if (toolBarToolTip != "" && toolBarToolTip != null && !menuOpen)
        {
            Vector2 toolBarPosition = new Vector2(Position.X - 2f, Position.Y + 18f);
            Graphics.DrawRect(toolBarPosition, toolBarPosition + new Vector2(100f, 15f), Color.Black * base.Alpha, base.Depth + 10);
            if (Editor.inputMode == EditorInput.Mouse)
            {
                Graphics.DrawString(toolBarToolTip, toolBarPosition + new Vector2(4f, 4f), Color.White * base.Alpha, base.Depth + 11);
            }
            else
            {
                Graphics.DrawString("@SELECT@" + toolBarToolTip, toolBarPosition + new Vector2(0f, 4f), Color.White * base.Alpha, base.Depth + 11);
            }
        }
        float xpos = Position.X;
        _playButton.X = xpos;
        _playButton.Y = Position.Y;
        xpos += 18f;
        _saveButton.X = xpos;
        _saveButton.Y = Position.Y;
        xpos += 18f;
        _loadButton.X = xpos;
        _loadButton.Y = Position.Y;
        xpos += 18f;
        _newButton.X = xpos;
        _newButton.Y = Position.Y;
        xpos += 18f;
        _gridButton.X = xpos;
        _gridButton.Y = Position.Y;
        xpos += 18f;
        _quitButton.X = xpos;
        _quitButton.Y = Position.Y;
        foreach (ToolbarButton button in _buttons)
        {
            button.DoDraw();
        }
    }
}
