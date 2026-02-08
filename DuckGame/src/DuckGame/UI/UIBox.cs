using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIBox : UIComponent
{
    #region Public Fields
    public bool allowBackButton = true;
    public bool _isMenu;

    public int _defaultSelection;

    public string _hoverControlString;

    public UIMenuItem _backButton;
    #endregion

    #region Protected Fields
    protected bool _inputLock;

    protected int _selection;
    #endregion

    #region Private Fields
    bool _borderVisible = true;
    bool _willSelectLast;

    float _seperation = 1;

    SpriteMap _sections;

    List<UIComponent> _currentMenuItemSelection;
    #endregion

    #region Public Properties
    public int selection => _selection;
    #endregion

    #region Public Constructors
    public UIBox(float xpos, float ypos, float wide = -1f, float high = -1f, bool vert = true, bool isVisible = true)
        : base(xpos, ypos, wide, high)
    {
        _sections = new SpriteMap("uiBox", 10, 10);
        _vertical = vert;
        _borderVisible = isVisible;
        borderSize = (_borderVisible ? new Vector2(8) : Vector2.Zero);
        _canFit = true;
    }

    public UIBox(bool vert = true, bool isVisible = true)
        : base(0f, 0f, -1f, -1f)
    {
        _sections = new SpriteMap("uiBox", 10, 10);
        _vertical = vert;
        _borderVisible = isVisible;
        borderSize = (_borderVisible ? new Vector2(8) : Vector2.Zero);
        _canFit = true;
    }
    #endregion

    #region Public Methods
    public virtual void AssignDefaultSelection()
    {
        List<UIComponent> items = [.. _components.Where(val => val is UIMenuItem && (val.condition == null || val.condition()))];
        _defaultSelection = items.Count - 1;
    }

    public virtual void SelectLastMenuItem()
    {
        List<UIComponent> items = [.. _components.Where(val => val is UIMenuItem)];
        _selection = items.Count - 1;
        _willSelectLast = true;
    }

    public override void Add(UIComponent component, bool doAnchor = true)
    {
        if (component is UIMenuItem)
        {
            _isMenu = true;
            if ((component as UIMenuItem).isBackButton)
                _backButton = component as UIMenuItem;
        }
        base.Add(component, doAnchor);
    }

    public override void Insert(UIComponent component, int position, bool doAnchor = true)
    {
        if (component is UIMenuItem)
        {
            _isMenu = true;
            if ((component as UIMenuItem).isBackButton)
                _backButton = component as UIMenuItem;
        }
        base.Insert(component, position, doAnchor);
    }

    public override void Open()
    {
        Graphics.fade = 1;
        if (!MonoMain.dontResetSelection)
        {
            _selection = _defaultSelection;
            if (_willSelectLast)
            {
                List<UIComponent> items = [.. _components.Where(val => val is UIMenuItem)];
                _selection = items.Count - 1;
            }
        }
        base.Open();
    }

    public override void Update()
    {
        if (!UIMenu.globalUILock && !_close && !_inputLock)
        {
            if (Input.Pressed("CANCEL") && allowBackButton)
            {
                if (_backButton != null || _backFunction != null)
                {
                    if (!_animating)
                    {
                        MonoMain.dontResetSelection = true;
                        if (_backButton != null)
                            _backButton.Activate("SELECT");
                        else
                            _backFunction.Activate();
                        MonoMain.menuOpenedThisFrame = true;
                    }
                }
                else if (!MonoMain.menuOpenedThisFrame && _isMenu)
                    MonoMain.closeMenus = true;
            }
            else if (Input.Pressed("SELECT") && _acceptFunction != null && !_animating)
            {
                MonoMain.dontResetSelection = true;
                _acceptFunction.Activate();
                MonoMain.menuOpenedThisFrame = true;
            }
            if (_isMenu)
            {
                _currentMenuItemSelection = [.. _components.Where((UIComponent val) => val is UIMenuItem && (val.condition == null || val.condition()))];
                if (_vertical)
                {
                    if (!_animating && Input.Pressed("MENUUP"))
                        SelectPrevious();
                    if (!_animating && Input.Pressed("MENUDOWN"))
                        SelectNext();
                }
                else
                {
                    if (!_animating && Input.Pressed("MENULEFT"))
                        SelectPrevious();
                    if (!_animating && Input.Pressed("MENURIGHT"))
                        SelectNext();
                }
                _hoverControlString = null;
                for (int i = 0; i < _currentMenuItemSelection.Count; i++)
                {
                    UIMenuItem item = _currentMenuItemSelection[i] as UIMenuItem;
                    item.selected = i == _selection;
                    if (i != _selection)
                        continue;
                    _hoverControlString = item.controlString;
                    if (item.isEnabled)
                    {
                        if (!_animating && Input.Pressed("SELECT"))
                        {
                            item.Activate("SELECT");
                            SFX.Play("rockHitGround", 0.7f);
                        }
                        else if (!_animating && Input.Pressed("MENU1"))
                            item.Activate("MENU1");
                        else if (!_animating && Input.Pressed("MENU2"))
                            item.Activate("MENU2");
                        else if (!_animating && Input.Pressed("RAGDOLL"))
                            item.Activate("RAGDOLL");
                        else if (!_animating && Input.Pressed("STRAFE"))
                            item.Activate("STRAFE");
                        else if (!_animating && Input.Pressed("MENULEFT"))
                            item.Activate("MENULEFT");
                        else if (!_animating && Input.Pressed("MENURIGHT"))
                            item.Activate("MENURIGHT");
                    }
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_borderVisible)
        {
            _sections.Scale = Scale;
            _sections.Alpha = Alpha;
            _sections.Depth = Depth;
            _sections.frame = 0;
            Graphics.Draw(_sections, 0f - halfWidth + X, 0f - halfHeight + Y);
            _sections.frame = 2;
            Graphics.Draw(_sections, halfWidth + X - _sections.w * Scale.X, 0f - halfHeight + Y);
            _sections.frame = 1;
            _sections.ScaleX = (_collisionSize.X - (_sections.w * 2)) / _sections.w * ScaleX;
            Graphics.Draw(_sections, 0f - halfWidth + X + _sections.w * Scale.X, 0f - halfHeight + Y);
            _sections.ScaleX = ScaleX;
            _sections.frame = 3;
            _sections.ScaleY = (_collisionSize.Y - (_sections.h * 2)) / _sections.h * ScaleY;
            Graphics.Draw(_sections, 0f - halfWidth + X, 0f - halfHeight + Y + _sections.h * Scale.Y);
            _sections.frame = 5;
            Graphics.Draw(_sections, halfWidth + X - _sections.w * Scale.X, 0f - halfHeight + Y + _sections.h * Scale.Y);
            _sections.frame = 4;
            _sections.ScaleX = (_collisionSize.X - (_sections.w * 2)) / _sections.w * ScaleX;
            Graphics.Draw(_sections, 0f - halfWidth + X + _sections.w * Scale.X, 0f - halfHeight + Y + _sections.h * Scale.Y);
            _sections.ScaleX = ScaleX;
            _sections.ScaleY = ScaleY;
            _sections.frame = 6;
            Graphics.Draw(_sections, 0f - halfWidth + X, halfHeight + Y - _sections.h * Scale.Y);
            _sections.frame = 8;
            Graphics.Draw(_sections, halfWidth + X - _sections.w * Scale.X, halfHeight + Y - _sections.h * Scale.Y);
            _sections.frame = 7;
            _sections.ScaleX = (_collisionSize.X - (_sections.w * 2)) / _sections.w * ScaleX;
            Graphics.Draw(_sections, 0f - halfWidth + X + _sections.w * Scale.X, halfHeight + Y - _sections.h * Scale.Y);
        }
        base.Draw();
    }
    #endregion

    #region Protected Methods
    protected override void SizeChildren()
    {
        foreach (UIComponent component in _components)
            if ((component.condition == null || component.condition()) && component.canFit)
            {
                if (vertical)
                    component.collisionSize = new Vector2(collisionSize.X - borderSize.X * 2, component.collisionSize.Y);
                else
                    component.collisionSize = new Vector2(component.collisionSize.X, collisionSize.Y - borderSize.Y * 2);
            }
    }

    protected override void OnResize()
    {
        if (_vertical)
        {
            float wide = 0,
                  high = 0;

            foreach (UIComponent component in _components)
                if (component.condition == null || component.condition())
                {
                    high += component.collisionSize.Y + _seperation;
                    if (component.collisionSize.X > wide)
                        wide = component.collisionSize.X;
                }
            wide += borderSize.X * 2;
            high -= _seperation;
            high += borderSize.Y * 2;
            if (_autoSizeHor && (fit & UIFit.Horizontal) == 0 && wide > _collisionSize.X)
                _collisionSize.X = wide;
            if (_autoSizeVert && (fit & UIFit.Vertical) == 0 && high > _collisionSize.Y)
                _collisionSize.Y = high;
            float yDraw = (0 - high) / 2 + borderSize.Y;
            {
                foreach (UIComponent component2 in _components)
                    if (component2.condition == null || component2.condition())
                    {
                        component2.anchor.offset.X = 0f;
                        if ((component2.align & UIAlign.Left) > UIAlign.Center)
                            component2.anchor.offset.X = (0f - collisionSize.X) / 2f + borderSize.X + component2.collisionSize.X / 2f;
                        else if ((component2.align & UIAlign.Right) > UIAlign.Center)
                            component2.anchor.offset.X = collisionSize.X / 2f - borderSize.X - component2.collisionSize.X / 2f;
                        component2.anchor.offset.Y = yDraw * base.Scale.Y + component2.height / 2f;
                        yDraw += component2.collisionSize.Y + _seperation;
                    }
                return;
            }
        }
        float wide2 = 0,
              high2 = 0;
        foreach (UIComponent component3 in _components)
            if (component3.condition == null || component3.condition())
            {
                wide2 += component3.collisionSize.X + _seperation;
                if (component3.collisionSize.Y > high2)
                    high2 = component3.collisionSize.Y;
            }
        high2 += borderSize.Y * 2;
        wide2 -= _seperation;
        wide2 += borderSize.X * 2;
        if (_autoSizeHor && (fit & UIFit.Horizontal) == 0 && wide2 > _collisionSize.X)
            _collisionSize.X = wide2;
        if (_autoSizeVert && (fit & UIFit.Vertical) == 0 && high2 > _collisionSize.Y)
            _collisionSize.Y = high2;
        float xDraw = (0 - wide2) / 2 + borderSize.X;
        foreach (UIComponent component4 in _components)
            if (component4.condition == null || component4.condition())
            {
                component4.anchor.offset.X = xDraw * Scale.X + component4.width / 2;
                component4.anchor.offset.Y = 0f;
                xDraw += component4.collisionSize.X + _seperation;
            }
    }
    #endregion

    #region Private Methods
    void SelectPrevious()
    {
        int sel = _selection;
        do
        {
            _selection--;
            if (_selection < 0)
                _selection = _currentMenuItemSelection.Count - 1;
        }
        while (_currentMenuItemSelection[_selection].mode != MenuItemMode.Normal && sel != _selection);
        SFX.Play("textLetter", 0.7f);
    }

    void SelectNext()
    {
        int sel = _selection;
        do
        {
            _selection++;
            if (_selection >= _currentMenuItemSelection.Count)
                _selection = 0;
        }
        while (_currentMenuItemSelection[_selection].mode != MenuItemMode.Normal && sel != _selection);
        SFX.Play("textLetter", 0.7f);
    }
    #endregion
}