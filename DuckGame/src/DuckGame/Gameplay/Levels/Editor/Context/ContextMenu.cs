using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ContextMenu : Thing, IContextListener
{
    public class SearchPair
    {
        public double relevance;

        public ContextObject thing;
    }

    public Mod mod;

    public string tooltip;

    protected List<ContextMenu> _items = new List<ContextMenu>();

    public Vector2 menuSize;

    protected Sprite _contextArrow;

    public Vector2 itemSize;

    public bool isModRoot;

    public bool isModPath;

    public object zipItem;

    public Thing contextThing;

    protected string _text = "";

    protected string _data = "";

    protected bool _canExpand;

    protected int _selectedIndex;

    protected bool _showBackground = true;

    public bool greyOut;

    public bool drawControls = true;

    public bool disabled;

    private bool _opened;

    public float _openedOffset;

    public float _openedOffsetX;

    protected bool _dragMode;

    public bool _opening;

    private bool _dontPush;

    public bool pinOpened;

    protected bool _hover;

    protected bool _root;

    protected bool _collectionChanged;

    protected new IContextListener _owner;

    public Vector2 offset;

    protected Sprite _image;

    protected bool _previewPriority;

    protected Sprite _arrow = new Sprite("tinyUpArrow");

    protected Sprite _pin = new Sprite("pinIcon");

    protected Sprite _pinPinned = new Sprite("pinIconPinned");

    private bool _hasToproot;

    public Vector2 _toprootPosition;

    private bool _hoverBackArrow;

    private bool _pinned;

    public bool isPinnable;

    public bool isToggle;

    protected bool _alwaysDrawLast;

    private bool _takingInput;

    public bool closeOnRight;

    protected Vector2 _lastDrawPos;

    public int _autoSelectItem = -1;

    protected static bool _didContextScroll;

    protected bool _sliding;

    protected bool _canEditSlide;

    protected bool _enteringSlideMode;

    public Sprite customIcon;

    private bool _hoverPin;

    public bool fancy;

    protected int _drawIndex;

    protected int _maxNumToDraw = 9999;

    public int scrollButtonDirection;

    private bool waitInputFrame;

    public string text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
        }
    }

    public string data
    {
        get
        {
            return _data;
        }
        set
        {
            _data = value;
        }
    }

    public int selectedIndex
    {
        get
        {
            return _selectedIndex;
        }
        set
        {
            _selectedIndex = value;
        }
    }

    public bool dontPush
    {
        get
        {
            return _dontPush;
        }
        set
        {
            if (value)
            {
                if (!pinned)
                {
                    pinOpened = true;
                }
                _dontPush = true;
            }
            else
            {
                _dontPush = false;
                pinOpened = false;
            }
        }
    }

    public bool opened
    {
        get
        {
            return _opened;
        }
        set
        {
            if (!_opened && value)
            {
                _lastDrawPos = Vector2.Zero;
                pinOpened = false;
                foreach (ContextMenu item in _items)
                {
                    item._lastDrawPos = Vector2.Zero;
                    item.opened = false;
                    item._hover = false;
                    if (item.pinned && !Editor.ignorePinning)
                    {
                        pinOpened = true;
                    }
                }
                _openedOffset = 0f;
                PositionItems();
                _selectedIndex = 0;
                if (_items.Count > 0)
                {
                    while (_selectedIndex < _items.Count() - 1 && _items[_selectedIndex].greyOut)
                    {
                        _selectedIndex++;
                    }
                    if (!_items[_selectedIndex].greyOut)
                    {
                        _opening = true;
                    }
                }
                else
                {
                    _opening = true;
                }
                foreach (ContextMenu m in _items)
                {
                    m.dontPush = false;
                    if (m.pinned && !Editor.ignorePinning)
                    {
                        m.dontPush = true;
                        m.opened = true;
                    }
                }
                PushLeft();
            }
            if (_opened && !value)
            {
                foreach (ContextMenu item2 in _items)
                {
                    item2.opened = false;
                    item2._hover = false;
                    item2.OnClose();
                }
                Closed();
                if (_root)
                {
                    Editor.ignorePinning = false;
                }
            }
            if (!value)
            {
                foreach (ContextMenu item3 in _items)
                {
                    item3.ParentCloseAction();
                }
            }
            if (value)
            {
                if (_autoSelectItem >= 0)
                {
                    _selectedIndex = _autoSelectItem;
                }
                _autoSelectItem = -1;
            }
            _opened = value;
        }
    }

    public bool hover
    {
        get
        {
            return _hover;
        }
        set
        {
            _hover = value;
        }
    }

    public bool root
    {
        get
        {
            return _root;
        }
        set
        {
            _root = value;
        }
    }

    public Sprite image
    {
        get
        {
            return _image;
        }
        set
        {
            _image = value;
        }
    }

    public bool pinned
    {
        get
        {
            if ((_pinned && Editor.pretendPinned == null) || Editor.pretendPinned == this)
            {
                return true;
            }
            foreach (ContextMenu item in _items)
            {
                if (item.pinned)
                {
                    return true;
                }
            }
            return false;
        }
        set
        {
            _pinned = value;
        }
    }

    protected virtual void OnClose()
    {
    }

    public virtual void ParentCloseAction()
    {
    }

    public ContextMenu(IContextListener owner, SpriteMap img = null, bool hasToproot = false, Vector2 topRoot = default(Vector2))
    {
        _owner = owner;
        if (Level.current is Editor)
        {
            base.layer = Editor.objectMenuLayer;
        }
        else
        {
            base.layer = Layer.HUD;
        }
        _contextArrow = new Sprite("contextArrowRight");
        itemSize.X = 100f;
        itemSize.Y = 16f;
        _root = owner == null;
        _image = img;
        base.Depth = 0.8f;
        _arrow.CenterOrigin();
        _pin.CenterOrigin();
        _pinPinned.CenterOrigin();
        _toprootPosition = topRoot;
        _hasToproot = hasToproot;
    }

    public override void Initialize()
    {
    }

    public virtual bool HasOpen()
    {
        foreach (ContextMenu item in _items)
        {
            if (item.opened)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void Toggle(ContextMenu item)
    {
        if (_owner != null)
        {
            isToggle = true;
            _owner.Selected(item);
            isToggle = false;
        }
    }

    public bool IsPartOf(Thing menu)
    {
        if (menu == null || this == menu)
        {
            return true;
        }
        if (_owner != null && _owner is ContextMenu men)
        {
            return men.IsPartOf(menu);
        }
        return false;
    }

    public override void DoUpdate()
    {
        if (!Editor.clickedMenu)
        {
            base.DoUpdate();
        }
    }

    public void PushLeft()
    {
        ContextMenu openedMenu = null;
        foreach (ContextMenu i in _items)
        {
            if (i.opened)
            {
                i.PushLeft();
                openedMenu = i;
            }
        }
        if (Keyboard.Down(Keys.Y))
        {
            return;
        }
        Vector2 pos = new Vector2(base.X, base.Y);
        Vector2 offset = new Vector2(0f, 0f);
        if (!_root && !dontPush)
        {
            offset = new Vector2(itemSize.X + 4f, -2f);
        }
        pos += offset;
        bool adjusted = false;
        if (_lastDrawPos.X + menuSize.X + 4f > base.layer.camera.width)
        {
            if (Editor.bigInterfaceMode)
            {
                pos.X -= menuSize.X;
            }
            else
            {
                pos.X = base.layer.camera.width - menuSize.X - 4f;
            }
            adjusted = true;
        }
        if (openedMenu != null && openedMenu.X != pos.X && !pinOpened)
        {
            if (_root)
            {
                pos.X = openedMenu.X;
            }
            else if (_pinned || Editor.pretendPinned == this)
            {
                pos.X = openedMenu.X - 4f;
            }
            else
            {
                pos.X = openedMenu.X - 2f;
            }
            adjusted = true;
        }
        pos -= offset;
        Position = pos;
        if (adjusted)
        {
            PositionItems();
        }
    }

    private void PinChanged(ContextMenu c)
    {
        if (_owner != null && _owner is ContextMenu)
        {
            (_owner as ContextMenu).PinChanged(c);
        }
        else
        {
            PinChangedDrillDown(c);
        }
    }

    private void PinChangedDrillDown(ContextMenu c)
    {
        if (c != this)
        {
            pinned = false;
        }
        foreach (ContextMenu item in _items)
        {
            item.PinChangedDrillDown(c);
        }
    }

    public bool OpenInto(object t, List<ContextMenu> pRecurseStack = null)
    {
        bool isRecurse = true;
        if (pRecurseStack == null)
        {
            pRecurseStack = new List<ContextMenu>();
            isRecurse = false;
        }
        int idx = 0;
        foreach (ContextMenu m in _items)
        {
            if (m == t || (m is ContextObject && (m as ContextObject).thing.GetType() == t.GetType()))
            {
                _autoSelectItem = idx;
                return true;
            }
            if (m is EditorGroupMenu && m.OpenInto(t, pRecurseStack))
            {
                pRecurseStack.Add(m);
                if (!isRecurse)
                {
                    ContextMenu prevMenu = this;
                    for (int i = pRecurseStack.Count - 1; i >= 0; i--)
                    {
                        prevMenu.selectedIndex = prevMenu._items.IndexOf(pRecurseStack[i]);
                        pRecurseStack[i].opened = true;
                        pRecurseStack[i].hover = true;
                        pRecurseStack[i].UpdatePositioning();
                        prevMenu = pRecurseStack[i];
                    }
                }
                return true;
            }
            idx++;
        }
        return false;
    }

    private void CloseChildren()
    {
        if (!HasOpen() || !_canExpand)
        {
            return;
        }
        bool kidsOpen = false;
        foreach (ContextMenu item in _items)
        {
            if (item.HasOpen())
            {
                kidsOpen = true;
                break;
            }
        }
        if (!kidsOpen)
        {
            Editor.tookInput = true;
            Selected(null);
        }
    }

    public virtual void Disappear()
    {
    }

    public override void Update()
    {
        if (!visible || disabled || _opening)
        {
            _opening = false;
        }
        else
        {
            if (!IsPartOf(Editor.lockInput))
            {
                return;
            }
            Vector2 realPos = _lastDrawPos;
            if (opened)
            {
                if (Editor.inputMode == EditorInput.Touch && TouchScreen.GetTap().Check(new Rectangle(_lastDrawPos.X, _lastDrawPos.Y, _lastDrawPos.X + menuSize.X, _lastDrawPos.Y + menuSize.Y), base.layer.camera) && !pinOpened)
                {
                    Editor.clickedContextBackground = true;
                }
                if (!pinOpened)
                {
                    Vector2 pinTL = new Vector2(realPos.X + menuSize.X - 5f, realPos.Y - 4f);
                    _hoverPin = false;
                    Vector2 selectPos = Vector2.Zero;
                    bool press = false;
                    if (Editor.inputMode == EditorInput.Mouse)
                    {
                        selectPos = Mouse.position;
                        press = Mouse.left == InputState.Pressed;
                    }
                    else if (Editor.inputMode == EditorInput.Touch)
                    {
                        selectPos = TouchScreen.GetTap().Transform(base.layer.camera);
                        press = TouchScreen.GetTap() != Touch.None;
                    }
                    if (selectPos.X > pinTL.X - 5f && selectPos.X < pinTL.X + 3f && selectPos.Y > pinTL.Y - 4f && selectPos.Y < pinTL.Y + 4f)
                    {
                        _hoverPin = true;
                        if (press && (!_root || pinned))
                        {
                            if (_root && pinned)
                            {
                                _pinned = false;
                                PinChanged(this);
                                Editor.openPosition = _lastDrawPos;
                                Editor.reopenContextMenu = true;
                                Editor.ignorePinning = false;
                                Editor.clickedMenu = true;
                                _autoSelectItem = _selectedIndex;
                            }
                            else
                            {
                                bool wasPinned = pinned;
                                _pinned = !_pinned;
                                if (_owner != null && _owner is ContextMenu)
                                {
                                    (_owner as ContextMenu).PinChanged(this);
                                }
                                if (_pinned != wasPinned && Editor.pretendPinned != this)
                                {
                                    Editor.openPosition = _lastDrawPos;
                                    Editor.reopenContextMenu = true;
                                    Editor.ignorePinning = false;
                                }
                                Editor.clickedMenu = true;
                                _autoSelectItem = _selectedIndex;
                            }
                        }
                    }
                    if (!waitInputFrame && Editor.inputMode == EditorInput.Gamepad && drawControls)
                    {
                        if (Input.Pressed("MENU1"))
                        {
                            if (isPinnable && _root && pinned)
                            {
                                _pinned = false;
                                PinChanged(this);
                                Editor.openPosition = _lastDrawPos;
                                Editor.reopenContextMenu = true;
                                Editor.ignorePinning = false;
                                _autoSelectItem = _selectedIndex;
                            }
                            else if (isPinnable && owner != null)
                            {
                                _pinned = !_pinned;
                                if (_owner != null && _owner is ContextMenu)
                                {
                                    (_owner as ContextMenu).PinChanged(this);
                                }
                                Editor.openPosition = _lastDrawPos;
                                Editor.reopenContextMenu = true;
                                Editor.ignorePinning = false;
                                _autoSelectItem = _selectedIndex;
                            }
                        }
                        else if (pinned && _owner != null && _owner is ContextMenu && (_owner as ContextMenu).pinOpened && Input.Pressed("MENULEFT"))
                        {
                            bool cancel = false;
                            foreach (ContextMenu item in _items)
                            {
                                if (item.opened)
                                {
                                    cancel = true;
                                    break;
                                }
                            }
                            if (!cancel)
                            {
                                if (!Editor.bigInterfaceMode || !IsEditorPlacementMenu())
                                {
                                    Editor.ignorePinning = true;
                                    Editor.reopenContextMenu = true;
                                }
                                else
                                {
                                    Editor.reopenContextMenu = true;
                                    Editor.clickedMenu = true;
                                    Editor.tookInput = true;
                                    Editor.openContextThing = (owner as ContextMenu)._items[0];
                                    Editor.pretendPinned = owner;
                                }
                            }
                        }
                    }
                    Vector2 backButtonSize = new Vector2(12f, 12f);
                    if (Editor.inputMode == EditorInput.Touch)
                    {
                        backButtonSize = new Vector2(24f, 24f);
                    }
                    Vector2 backButtonTL = realPos + new Vector2(0f - (backButtonSize.X + 2f), 0f);
                    Vector2 backButtonBR = realPos + new Vector2(-2f, backButtonSize.Y);
                    _hoverBackArrow = false;
                    if (selectPos.X > backButtonTL.X && selectPos.X < backButtonBR.X && selectPos.Y > backButtonTL.Y && selectPos.Y < backButtonBR.Y)
                    {
                        _hoverBackArrow = true;
                        if (press)
                        {
                            Editor.reopenContextMenu = true;
                            Editor.clickedMenu = true;
                            Editor.tookInput = true;
                            if (Editor.inputMode == EditorInput.Touch)
                            {
                                Editor.openContextThing = (owner as ContextMenu)._items[0];
                                Editor.pretendPinned = owner;
                            }
                            else
                            {
                                Editor.ignorePinning = true;
                            }
                        }
                    }
                }
                PushLeft();
                int count = _items.Count;
                for (int i = 0; i < count; i++)
                {
                    if (pinOpened && !_items[i].pinned)
                    {
                        continue;
                    }
                    if (_alwaysDrawLast)
                    {
                        if (i == _items.Count - 1 || (i >= _drawIndex && i < _drawIndex + _maxNumToDraw && (i == _items.Count - 1 || i != _drawIndex + _maxNumToDraw - 1)))
                        {
                            _items[i].DoUpdate();
                        }
                    }
                    else if (i >= _drawIndex && i < _drawIndex + _maxNumToDraw)
                    {
                        _items[i].DoUpdate();
                    }
                    if (_collectionChanged)
                    {
                        _collectionChanged = false;
                        return;
                    }
                }
            }
            waitInputFrame = false;
            if (pinOpened)
            {
                return;
            }
            if (_items.Count > 0)
            {
                _canExpand = true;
            }
            bool clicked = false;
            if (!Editor.HasFocus())
            {
                if (_hover && _dragMode && Editor.inputMode == EditorInput.Mouse && ((!_sliding && Mouse.left == InputState.Pressed) || (_sliding && Mouse.left == InputState.Down)))
                {
                    clicked = true;
                }
                if (Editor.inputMode == EditorInput.Mouse && Mouse.right == InputState.Pressed && closeOnRight)
                {
                    Disappear();
                    _owner.Selected(null);
                    opened = false;
                    return;
                }
            }
            if (_hover && tooltip != null)
            {
                Editor.tooltip = tooltip;
            }
            if (!Editor.HasFocus())
            {
                if ((Editor.lockInput == null || IsPartOf(Editor.lockInput)) && !Editor.tookInput && Editor.inputMode == EditorInput.Gamepad)
                {
                    bool open = HasOpen();
                    if (open && Input.Pressed("MENULEFT") && _canExpand)
                    {
                        bool kidsOpen = false;
                        foreach (ContextMenu item2 in _items)
                        {
                            if (item2.HasOpen())
                            {
                                kidsOpen = true;
                                break;
                            }
                        }
                        if (!kidsOpen)
                        {
                            Editor.tookInput = true;
                            Selected(null);
                        }
                    }
                    _takingInput = false;
                    if (!open)
                    {
                        if (opened && _items.Count > 0)
                        {
                            _takingInput = true;
                            bool _tookInput = false;
                            if (Input.Pressed("MENUUP"))
                            {
                                _tookInput = true;
                                if (_selectedIndex == _items.Count - 1 && _alwaysDrawLast)
                                {
                                    if (_drawIndex + _maxNumToDraw < _items.Count)
                                    {
                                        _selectedIndex = _drawIndex + _maxNumToDraw - 2;
                                    }
                                    else
                                    {
                                        _selectedIndex = _drawIndex + _maxNumToDraw - 1;
                                    }
                                    if (_selectedIndex > _items.Count - 1)
                                    {
                                        _selectedIndex = _items.Count - 1;
                                    }
                                }
                                _selectedIndex--;
                                for (int itemsTried = 0; itemsTried < _items.Count; itemsTried++)
                                {
                                    if (_selectedIndex < 0)
                                    {
                                        _selectedIndex = _items.Count - 1;
                                        if (_alwaysDrawLast)
                                        {
                                            _drawIndex = 0;
                                        }
                                        else
                                        {
                                            _drawIndex = _selectedIndex - _maxNumToDraw + 1;
                                            if (_drawIndex < 0)
                                            {
                                                _drawIndex = 0;
                                            }
                                        }
                                    }
                                    if (!_items[_selectedIndex].greyOut)
                                    {
                                        break;
                                    }
                                    _selectedIndex--;
                                    _drawIndex = _selectedIndex;
                                }
                            }
                            else if (Input.Pressed("MENUDOWN"))
                            {
                                _tookInput = true;
                                _ = _selectedIndex;
                                _selectedIndex++;
                                for (int j = 0; j < _items.Count; j++)
                                {
                                    if (_selectedIndex > _items.Count - 1)
                                    {
                                        _selectedIndex = 0;
                                        _drawIndex = _selectedIndex;
                                    }
                                    if (!_items[_selectedIndex].greyOut)
                                    {
                                        break;
                                    }
                                    _selectedIndex++;
                                    _drawIndex = _selectedIndex;
                                }
                            }
                            if (_tookInput)
                            {
                                PositionItems();
                            }
                            int index = 0;
                            foreach (ContextMenu m in _items)
                            {
                                if (index == _selectedIndex)
                                {
                                    m._hover = true;
                                    m.Hover();
                                }
                                else
                                {
                                    m._hover = false;
                                }
                                index++;
                            }
                        }
                        new Rectangle(base.X, base.Y, itemSize.X, itemSize.Y);
                        if (_hover && (Input.Pressed("SELECT") || (_canExpand && Input.Pressed("MENURIGHT")) || scrollButtonDirection != 0))
                        {
                            if (owner is ContextMenu ownerMenu)
                            {
                                foreach (ContextMenu item3 in ownerMenu._items)
                                {
                                    item3._hover = false;
                                }
                            }
                            _hover = true;
                            clicked = true;
                        }
                    }
                }
                else if (Editor.inputMode == EditorInput.Mouse)
                {
                    bool bUpdateSelectedIndex = false;
                    if (Editor.inputMode == EditorInput.Mouse && Mouse.x >= base.X && Mouse.x <= base.X + itemSize.X && Mouse.y >= base.Y + 1f && Mouse.y <= base.Y + itemSize.Y - 1f)
                    {
                        if (Mouse.left == InputState.Pressed)
                        {
                            clicked = true;
                        }
                        Editor.hoverUI = true;
                        _hover = true;
                        bUpdateSelectedIndex = true;
                    }
                    if (bUpdateSelectedIndex)
                    {
                        if (owner is ContextMenu ownerMenu2)
                        {
                            int index2 = 0;
                            foreach (ContextMenu item4 in ownerMenu2._items)
                            {
                                if (item4 == this)
                                {
                                    ownerMenu2._selectedIndex = index2;
                                    break;
                                }
                                index2++;
                            }
                        }
                    }
                    else if (!_dragMode || Mouse.left != InputState.Down || (TouchScreen.IsTouchScreenActive() && !TouchScreen.IsScreenTouched()))
                    {
                        _hover = false;
                    }
                }
                else if (Editor.inputMode == EditorInput.Touch)
                {
                    Rectangle plotRect = new Rectangle(base.X, base.Y, itemSize.X, itemSize.Y);
                    if (TouchScreen.GetTap().Check(plotRect, base.layer.camera))
                    {
                        clicked = true;
                        if (!_hover)
                        {
                            _canEditSlide = false;
                            _enteringSlideMode = true;
                        }
                        _hover = true;
                        if (owner is ContextMenu ownerMenu3)
                        {
                            int index3 = 0;
                            foreach (ContextMenu m2 in ownerMenu3._items)
                            {
                                if (m2 == this)
                                {
                                    ownerMenu3._selectedIndex = index3;
                                }
                                else
                                {
                                    m2._hover = false;
                                }
                                index3++;
                            }
                        }
                    }
                }
                if (Editor.inputMode == EditorInput.Mouse && Mouse.x > realPos.X && Mouse.x < realPos.X + menuSize.X && Mouse.y > realPos.Y && Mouse.y < realPos.Y + menuSize.Y)
                {
                    if (Mouse.scroll != 0f && !_didContextScroll)
                    {
                        _drawIndex += ((Mouse.scroll > 0f) ? 1 : (-1));
                        _drawIndex = Maths.Clamp(_drawIndex, 0, _items.Count - _maxNumToDraw);
                        SFX.Play("highClick", 0.3f, 0.2f);
                        foreach (ContextMenu item5 in _items)
                        {
                            item5.opened = false;
                            item5._hover = false;
                        }
                        PositionItems();
                    }
                    _didContextScroll = false;
                }
            }
            if (!Editor.HasFocus() && _hover && _dragMode && Editor.inputMode == EditorInput.Touch && TouchScreen.GetTouch() != Touch.None && TouchScreen.GetTouch().Check(new Rectangle(base.X, base.Y, itemSize.X, itemSize.Y), base.layer.camera))
            {
                clicked = true;
            }
            if (clicked)
            {
                _ = _level;
                Editor.clickedMenu = true;
                Selected();
            }
        }
    }

    public override void Terminate()
    {
        _ = _level;
    }

    public void ClearItems()
    {
        _collectionChanged = true;
        _items.Clear();
    }

    public virtual void Hover()
    {
    }

    private void UpdatePositioning()
    {
        bool reposition = false;
        if (base.Y + _openedOffset + menuSize.Y + 16f > base.layer.height)
        {
            _openedOffset = base.layer.height - menuSize.Y - base.Y - 16f;
            reposition = true;
        }
        if (base.Y + _openedOffset < 0f)
        {
            _openedOffset = 0f - base.Y;
            reposition = true;
        }
        if (reposition)
        {
            PositionItems();
        }
    }

    public override void Draw()
    {
        Position += offset;
        if (!_root && !pinOpened && !dontPush)
        {
            float cMult = 1f;
            if (greyOut)
            {
                cMult = 0.3f;
            }
            cMult *= base.Alpha;
            if (_hover && !greyOut)
            {
                Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70) * base.Alpha, base.Depth);
            }
            if (scrollButtonDirection != 0)
            {
                _arrow.Depth = base.Depth + 1;
                if (scrollButtonDirection > 0)
                {
                    _arrow.flipV = true;
                    Graphics.Draw(_arrow, Position.X + (_owner as ContextMenu).menuSize.X / 2f, Position.Y + 8f);
                }
                else
                {
                    _arrow.flipV = false;
                    Graphics.Draw(_arrow, Position.X + (_owner as ContextMenu).menuSize.X / 2f, Position.Y + 8f);
                }
            }
            else
            {
                if (_image != null)
                {
                    _image.Depth = base.Depth + 3;
                    _image.X = base.X + 1f;
                    _image.Y = base.Y;
                    _image.color = Color.White * cMult;
                    _image.Draw();
                    Graphics.DrawString(_text, Position + new Vector2(20f, 4f), Color.White * cMult, base.Depth + 1);
                }
                else if (_text == "custom")
                {
                    Graphics.DrawString(_text, Position + new Vector2(2f, 4f), Colors.DGBlue * cMult, base.Depth + 1);
                }
                else if (fancy)
                {
                    float xOff = 0f;
                    if (customIcon != null)
                    {
                        Vector2 pos = Position + new Vector2(2f, 4f);
                        Graphics.Draw(customIcon.texture, pos.X, pos.Y, 1f, 1f, base.Depth + 1);
                        xOff += 8f;
                    }
                    Graphics.DrawFancyString(_text, Position + new Vector2(2f + xOff, 4f), Color.White * cMult, base.Depth + 1);
                    Vector2 previewPos = Position + new Vector2(itemSize.X - 24f, 0f);
                    int numDraw = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (numDraw == 4)
                        {
                            break;
                        }
                        foreach (ContextMenu mz in _items)
                        {
                            if (numDraw == 4)
                            {
                                break;
                            }
                            for (int j = 0; j < mz._items.Count + 1; j++)
                            {
                                ContextMenu m = mz;
                                if (i == 1)
                                {
                                    if (j == mz._items.Count)
                                    {
                                        break;
                                    }
                                    m = mz._items[j];
                                }
                                if (m._image != null && ((i == 2 && !m._previewPriority) || (i != 2 && m._previewPriority)))
                                {
                                    Sprite s = m._image;
                                    if (s != null)
                                    {
                                        s.Depth = base.Depth + 3;
                                        s.X = previewPos.X + 1f;
                                        s.Y = previewPos.Y;
                                        s.Scale = new Vector2(0.5f);
                                        s.Draw();
                                        previewPos.X += 8f;
                                        numDraw++;
                                        if (numDraw == 2)
                                        {
                                            previewPos.X -= 16f;
                                            previewPos.Y += 8f;
                                        }
                                        else if (numDraw == 4)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (i != 1)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Graphics.DrawString(_text, Position + new Vector2(2f, 4f), Color.White * cMult, base.Depth + 1);
                }
                if (_items.Count > 0)
                {
                    _contextArrow.color = Color.White * cMult;
                    Graphics.Draw(_contextArrow, base.X + itemSize.X - 8f, base.Y + 4f, base.Depth + 1);
                    _contextArrow.color = Color.White;
                }
            }
        }
        if (opened)
        {
            if (!pinOpened)
            {
                UpdatePositioning();
                float menuWidth = menuSize.X;
                float menuHeight = menuSize.Y;
                Vector2 pos2 = new Vector2(base.X + _openedOffsetX, base.Y + _openedOffset) + new Vector2(-2f, -2f);
                if (!_root && !dontPush)
                {
                    pos2.X += itemSize.X + 6f;
                }
                if (_showBackground)
                {
                    Graphics.DrawRect(pos2, pos2 + new Vector2(menuWidth, menuHeight), new Color(70, 70, 70) * base.Alpha, base.Depth);
                    Graphics.DrawRect(pos2 + new Vector2(1f, 1f), pos2 + new Vector2(menuWidth - 1f, menuHeight - 1f), new Color(30, 30, 30) * base.Alpha, base.Depth + 1);
                    _lastDrawPos = pos2;
                    if (_items.Count > 0 && isPinnable && (!_root || pinned))
                    {
                        Sprite pin = _pin;
                        if ((pinned && Editor.pretendPinned != this) || _pinned)
                        {
                            pin = _pinPinned;
                        }
                        pin.Depth = base.Depth + 2;
                        if (_hoverPin)
                        {
                            pin.Alpha = 1f;
                        }
                        else
                        {
                            pin.Alpha = 0.5f;
                        }
                        Vector2 pinTL = new Vector2(pos2.X + menuWidth - 5f, pos2.Y - 4f);
                        Graphics.Draw(pin, pinTL.X, pinTL.Y);
                        Vector2 pinBackTL = pinTL + new Vector2(-6f, -6f);
                        Vector2 pinBackBR = pinBackTL + new Vector2(11f, 11f);
                        if (Editor.inputMode == EditorInput.Gamepad && _takingInput)
                        {
                            pinBackTL.X -= 10f;
                        }
                        Graphics.DrawRect(pinBackTL, pinBackBR, new Color(70, 70, 70) * base.Alpha, base.Depth);
                        Graphics.DrawRect(pinBackTL + new Vector2(1f, 1f), pinBackBR + new Vector2(-1f, 0f), new Color(30, 30, 30) * base.Alpha, base.Depth + 1);
                        if (_owner != null && _owner is ContextMenu && (_owner as ContextMenu).pinOpened)
                        {
                            Vector2 backButtonSize = new Vector2(12f, 12f);
                            if (Editor.bigInterfaceMode)
                            {
                                backButtonSize = new Vector2(24f, 24f);
                            }
                            Vector2 backButtonTL = pos2 + new Vector2(0f - (backButtonSize.X + 2f), 0f);
                            Vector2 backButtonBR = pos2 + new Vector2(-2f, backButtonSize.Y);
                            Graphics.DrawRect(backButtonTL, backButtonBR, new Color(70, 70, 70) * base.Alpha, base.Depth);
                            Graphics.DrawRect(backButtonTL + new Vector2(1f, 1f), backButtonBR + new Vector2(-1f, -1f), new Color(30, 30, 30) * base.Alpha, base.Depth + 1);
                            _contextArrow.flipH = true;
                            _contextArrow.Depth = base.Depth + 2;
                            if (_hoverBackArrow)
                            {
                                _contextArrow.Alpha = 1f;
                            }
                            else
                            {
                                _contextArrow.Alpha = 0.5f;
                            }
                            _contextArrow.Alpha = 1f;
                            Graphics.Draw(_contextArrow, backButtonTL.X + backButtonSize.X / 2f + (float)(_contextArrow.width / 2), backButtonTL.Y + backButtonSize.Y / 2f - (float)(_contextArrow.height / 2));
                            _contextArrow.flipH = false;
                        }
                    }
                }
                if (Editor.inputMode == EditorInput.Gamepad && drawControls && _takingInput)
                {
                    bool group = false;
                    string inputText = "";
                    bool noBack = false;
                    foreach (ContextMenu m2 in _items)
                    {
                        if (!m2.hover)
                        {
                            continue;
                        }
                        if ((m2._items.Count > 0 || m2 is ContextBackgroundTile) ? true : false)
                        {
                            inputText = "@SELECT@@RIGHT@EXPAND";
                        }
                        else if (m2 is ContextSlider)
                        {
                            if ((m2 as ContextSlider).adjust)
                            {
                                inputText = "@WASD@EDIT @STRAFE@SLOW @RAGDOLL@FAST ";
                                noBack = true;
                            }
                            else
                            {
                                inputText = "@SELECT@EDIT";
                            }
                        }
                        else
                        {
                            inputText = ((!(m2 is ContextTextbox)) ? ((!(m2 is ContextRadio) && !(m2 is ContextCheckBox)) ? "@SELECT@SELECT" : "@SELECT@TOGGLE") : "@SELECT@ENTER TEXT");
                        }
                    }
                    if (!_root && !noBack)
                    {
                        inputText += "  @LEFT@BACK";
                    }
                    Graphics.DrawRect(pos2 + new Vector2(0f, menuHeight), pos2 + new Vector2(menuWidth, menuHeight + 15f), Color.Black * base.Alpha, base.Depth);
                    Graphics.DrawString(inputText, pos2 + new Vector2(0f, menuHeight + 4f), Color.White * base.Alpha, base.Depth + 1);
                    if (isPinnable && (!_root || pinned))
                    {
                        Graphics._biosFont.spriteScale = new Vector2(0.75f);
                        Graphics.DrawString("@MENU1@", pos2 + new Vector2(menuSize.X - 20f, -7f), Color.White * base.Alpha, base.Depth + 4, null, 0.5f);
                        Graphics._biosFont.spriteScale = new Vector2(1f);
                    }
                }
                if (_hasToproot && !dontPush)
                {
                    Graphics.DrawRect(_toprootPosition, _toprootPosition + new Vector2(16f, 32f), new Color(70, 70, 70) * base.Alpha, base.Depth - 4);
                }
            }
            int index = 0;
            foreach (ContextMenu menu in _items)
            {
                if (!pinOpened || menu.opened)
                {
                    menu.scrollButtonDirection = 0;
                    if (index == _drawIndex && _drawIndex > 0)
                    {
                        menu.scrollButtonDirection = -1;
                    }
                    else if ((index - _drawIndex == _maxNumToDraw - 1 || (_alwaysDrawLast && index - _drawIndex == _maxNumToDraw - 2)) && _items.Count - ((!_alwaysDrawLast) ? 1 : 2) > index)
                    {
                        menu.scrollButtonDirection = 1;
                    }
                    if (_alwaysDrawLast)
                    {
                        if (index == _items.Count - 1)
                        {
                            menu.scrollButtonDirection = 0;
                        }
                        if ((index == _items.Count - 1 || (index >= _drawIndex && index < _drawIndex + _maxNumToDraw && (index == _items.Count - 1 || index != _drawIndex + _maxNumToDraw - 1))) && menu.visible)
                        {
                            menu.DoDraw();
                        }
                    }
                    else if (index >= _drawIndex && index - _drawIndex < _maxNumToDraw && menu.visible)
                    {
                        menu.DoDraw();
                    }
                }
                index++;
            }
        }
        Position -= offset;
    }

    public virtual void Selected()
    {
        if (!greyOut && _owner != null)
        {
            _owner.Selected(this);
        }
    }

    public bool IsEditorPlacementMenu()
    {
        return true;
    }

    public virtual void Selected(ContextMenu item)
    {
        if (greyOut)
        {
            return;
        }
        if (item != null && item.scrollButtonDirection != 0)
        {
            _drawIndex += item.scrollButtonDirection;
            _drawIndex = Maths.Clamp(_drawIndex, 0, _items.Count - _maxNumToDraw);
            SFX.Play("highClick", 0.3f, 0.2f);
            foreach (ContextMenu item2 in _items)
            {
                item2.opened = false;
                item2._hover = false;
            }
            PositionItems();
            return;
        }
        foreach (ContextMenu i in _items)
        {
            if (i != item)
            {
                i.opened = false;
            }
        }
        if (item != null)
        {
            if ((IsEditorPlacementMenu() && Editor.bigInterfaceMode && item is EditorGroupMenu) || item.text == "More...")
            {
                if (!item.opened)
                {
                    Editor.ignorePinning = false;
                    Editor.reopenContextMenu = true;
                    Editor.clickedMenu = true;
                    Editor.tookInput = true;
                    Editor.openContextThing = item;
                    Editor.pretendPinned = item;
                    SFX.Play("highClick", 0.3f, 0.2f);
                }
            }
            else
            {
                item.opened = true;
                Editor.clickedMenu = true;
                Editor.tookInput = true;
                SFX.Play("highClick", 0.3f, 0.2f);
            }
        }
        waitInputFrame = true;
    }

    public void AddItem(ContextMenu item)
    {
        AddItem(item, -1);
    }

    public void AddItem(ContextMenu item, int index)
    {
        item.Initialize();
        if (index >= 0)
        {
            _items.Insert(index, item);
        }
        else
        {
            _items.Add(item);
        }
        item.owner = this;
        PositionItems();
    }

    public Thing GetPlacementType(Type pType)
    {
        if (this is ContextObject && (this as ContextObject).thing.GetType() == pType)
        {
            return (this as ContextObject).thing;
        }
        foreach (ContextMenu item in _items)
        {
            Thing t = item.GetPlacementType(pType);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// Calculate percentage similarity of two strings
    /// <param name="source">Source String to Compare with</param>
    /// <param name="target">Targeted String to Compare</param>
    /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
    /// </summary>
    private double CalculateSimilarity(string source, string target)
    {
        if (source == null || target == null)
        {
            return 0.0;
        }
        if (source.Length == 0 || target.Length == 0)
        {
            return 0.0;
        }
        if (source == target)
        {
            return 1.0;
        }
        int stepsToSame = ComputeLevenshteinDistance(source, target);
        return 1.0 - (double)stepsToSame / (double)Math.Max(source.Length, target.Length);
    }

    /// <summary>
    /// Returns the number of steps required to transform the source string
    /// into the target string.
    /// </summary>
    private int ComputeLevenshteinDistance(string source, string target)
    {
        if (source == null || target == null)
        {
            return 0;
        }
        if (source.Length == 0 || target.Length == 0)
        {
            return 0;
        }
        if (source == target)
        {
            return source.Length;
        }
        int sourceWordCount = source.Length;
        int targetWordCount = target.Length;
        if (sourceWordCount == 0)
        {
            return targetWordCount;
        }
        if (targetWordCount == 0)
        {
            return sourceWordCount;
        }
        int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];
        int i = 0;
        while (i <= sourceWordCount)
        {
            distance[i, 0] = i++;
        }
        int j = 0;
        while (j <= targetWordCount)
        {
            distance[0, j] = j++;
        }
        for (int k = 1; k <= sourceWordCount; k++)
        {
            for (int l = 1; l <= targetWordCount; l++)
            {
                int cost = ((target[l - 1] != source[k - 1]) ? 1 : 0);
                distance[k, l] = Math.Min(Math.Min(distance[k - 1, l] + 1, distance[k, l - 1] + 1), distance[k - 1, l - 1] + cost);
            }
        }
        return distance[sourceWordCount, targetWordCount];
    }

    public List<SearchPair> Search(string pTerm)
    {
        return (from x in Search(pTerm.ToLowerInvariant(), new List<SearchPair>())
                orderby 0.0 - x.relevance
                select x).ToList();
    }

    private List<SearchPair> Search(string pTerm, List<SearchPair> pCurrentTerms)
    {
        if (this is ContextObject)
        {
            string comp = _text.ToLowerInvariant();
            double sim = CalculateSimilarity(pTerm, comp);
            float simChars = 1f;
            for (int i = 0; i < pTerm.Length && i < comp.Length; i++)
            {
                if (pTerm[i] == comp[i])
                {
                    sim += (double)(0.1f * simChars);
                    simChars += 1f;
                }
                else
                {
                    sim -= (double)(0.05f * simChars);
                }
            }
            if (comp.Contains(pTerm))
            {
                sim += 0.6000000238418579;
            }
            if (sim > 0.25)
            {
                pCurrentTerms.Add(new SearchPair
                {
                    relevance = sim,
                    thing = (this as ContextObject)
                });
            }
        }
        foreach (ContextMenu item in _items)
        {
            item.Search(pTerm, pCurrentTerms);
        }
        return pCurrentTerms;
    }

    public void PositionItems()
    {
        float largestWidth = 0f;
        float ypos = base.Y + _openedOffset;
        _openedOffsetX = 0f;
        if (Editor.inputMode != EditorInput.Mouse && !_root)
        {
            ypos = 16f;
            _openedOffset = 0f - base.Y + 16f;
        }
        for (int i = 0; i < _items.Count; i++)
        {
            ContextMenu item = _items[i];
            if (!item.opened || (item is ContextToolbarItem && (item as ContextToolbarItem).isPushingUp))
            {
                if (!_root && !dontPush)
                {
                    item.X = base.X + 3f + itemSize.X + 3f;
                }
                else
                {
                    item.X = base.X;
                }
                if ((_pinned || Editor.pretendPinned == this) && !_root)
                {
                    if (Editor.bigInterfaceMode)
                    {
                        item.X += 14f;
                        _openedOffsetX = 14f;
                    }
                    else
                    {
                        item.X += 4f;
                        _openedOffsetX = 4f;
                    }
                }
                item.Y = ypos;
            }
            if (i >= _drawIndex && !pinOpened && ((_alwaysDrawLast && (i == _items.Count - 1 || (i != _drawIndex + _maxNumToDraw - 1 && i < _drawIndex + _maxNumToDraw))) || !_alwaysDrawLast))
            {
                ypos += item.itemSize.Y + 1f;
            }
            if (item.itemSize.X < 107f)
            {
                item.itemSize.X = 107f;
            }
            if (item.itemSize.X + 4f > menuSize.X)
            {
                menuSize.X = item.itemSize.X + 4f;
            }
            item.Depth = base.Depth + 2;
            if (item.itemSize.X > largestWidth)
            {
                largestWidth = item.itemSize.X;
            }
        }
        int numItemsSized = 0;
        float hval = 0f;
        foreach (ContextMenu item2 in _items)
        {
            if (numItemsSized < _maxNumToDraw)
            {
                hval += item2.itemSize.Y + 1f;
            }
            item2.itemSize.X = largestWidth;
            numItemsSized++;
        }
        menuSize.Y = hval + 3f;
    }

    public void CloseMenus()
    {
        foreach (ContextMenu item in _items)
        {
            item.opened = false;
        }
    }

    public virtual void Closed()
    {
    }
}
