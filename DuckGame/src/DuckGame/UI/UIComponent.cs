using System;
using System.Collections.Generic;

namespace DuckGame;

public class UIComponent : Thing
{
    #region Public Fields
    public bool isEnabled = true;
    public bool inWorld;
    public bool debug;

    public MenuItemMode mode;

    public Vec2 borderSize;

    public Func<bool> condition;
    public UIMenuAction _backFunction;
    public UIMenuAction _closeFunction;
    public UIMenuAction _acceptFunction;
    #endregion

    #region Protected Fields
    protected bool _didResize;
    protected bool _dirty;
    protected bool _vertical;
    protected bool _canFit;
    protected bool _close;
    protected bool _animating;
    protected bool _autoSizeVert;
    protected bool _autoSizeHor;

    protected Vec2 _offset;

    protected UIComponent _parent;

    protected List<UIComponent> _components = [];
    #endregion

    #region Private Fields
    bool _animate;
    bool _startInitialized;
    bool _initialSizingComplete;
    bool _isPauseMenu;

    UIFit _fit;
    UIAlign _align;

    Vec2 _startPosition;
    #endregion

    #region Public Properties
    public bool canFit => _canFit;

    public bool autoSizeVert => _autoSizeVert;

    public bool autoSizeHor => _autoSizeHor;

    public bool animate => _animate;

    public bool open => !_close;

    public bool dirty
    {
        get => _dirty;
        set => _dirty = value;
    }

    public bool vertical
    {
        get => _vertical;
        set => _vertical = value;
    }

    public bool animating
    {
        get => _animating;
        set
        {
            foreach (UIComponent component in _components)
                component.animating = value;
            _animating = value;
        }
    }

    public bool isPauseMenu
    {
        get
        {
            if (!_isPauseMenu)
            {
                if (_parent != null)
                    return _parent.isPauseMenu;
                return false;
            }
            return true;
        }
        set => _isPauseMenu = value;
    }

    public UIFit fit
    {
        get => _fit;
        set
        {
            if (_fit != value)
                _dirty = true;
            _fit = value;
        }
    }

    public UIAlign align
    {
        get => _align;
        set
        {
            if (_align != value)
                _dirty = true;
            _align = value;
        }
    }

    public Vec2 offset
    {
        get => _offset;
        set => _offset = value;
    }

    public UIComponent parent => _parent;

    public UIMenu rootMenu => this is UIMenu menu ? menu : _parent?.rootMenu;

    public IList<UIComponent> components => _components;
    #endregion

    public UIComponent(float xpos, float ypos, float wide, float high)
        : base(xpos, ypos)
    {
        _collisionSize = new Vec2(wide, high);
        layer = Layer.HUD;
        Depth = 0;
        _autoSizeHor = wide < 0;
        _autoSizeVert = high < 0;
    }

    #region Public Methods
    public virtual void Open()
    {
        MonoMain.menuOpenedThisFrame = true;
        _close = false;
        animating = true;
        foreach (UIComponent component in _components)
            if (component.anchor == this)
                component.Open();
        _initialSizingComplete = false;
    }

    public virtual void Close()
    {
        _close = true;
        animating = true;
        foreach (UIComponent component in _components)
            component.Close();
        if (!inWorld && rootMenu == this && !MonoMain.closeMenuUpdate.Contains(this))
            MonoMain.closeMenuUpdate.Add(this);
        OnClose();
    }

    public virtual void OnClose() { }

    public virtual void UpdateParts() { }

    public virtual void Add(UIComponent component, bool doAnchor = true)
    {
        _components.Add(component);
        component._parent = this;
        _dirty = true;
        component.dirty = true;
        if (doAnchor)
            component.anchor = this;
    }

    public virtual void Insert(UIComponent component, int position, bool doAnchor = true)
    {
        if (position >= _components.Count)
            position = _components.Count;
        _components.Insert(position, component);
        component._parent = this;
        _dirty = true;
        component.dirty = true;
        if (doAnchor)
            component.anchor = this;
    }

    public virtual void Remove(UIComponent component)
    {
        _components.Remove(component);
        if (component._parent == this)
            component._parent = null;
        if (component.anchor == this)
            component.anchor = null;
        _dirty = true;
    }

    public override void Added(Level parent)
    {
        inWorld = true;
        base.Added(parent);
    }

    public override void Update()
    {
        if (!_startInitialized)
        {
            _startInitialized = true;
            _startPosition = Position;
            Y = layer.camera.height * 2f;
        }
        if (anchor == null)
        {
            float yLerp = (_close ? (layer.camera.height * 2) : _startPosition.Y);
            Y = Lerp.FloatSmooth(Y, yLerp, 0.2f, 1.05f);
            bool isAnimating = Y != yLerp;
            if (animating != isAnimating)
                animating = isAnimating;
        }
        if (open && !animating)
            UpdateParts();
        if (_parent != null || open || animating)
        {
            SizeChildren();
            foreach (UIComponent component in _components)
            {
                if (component.condition == null || component.condition())
                {
                    component.DoUpdate();
                    if (component._didResize)
                        _dirty = true;
                    component._didResize = false;
                }
            }
        }
        if (_dirty)
            Resize();
        if (!UIMenu.globalUILock && !MonoMain.menuOpenedThisFrame && MonoMain.pauseMenu == this && ((Input.Pressed("START") && isPauseMenu) || MonoMain.closeMenus))
        {
            MonoMain.closeMenus = false;
            _closeFunction?.Activate();
            Close();
        }
        _dirty = false;
        _initialSizingComplete = true;
    }

    public override void DoDraw()
    {
        if (_initialSizingComplete && (_animating || !_close))
            base.DoDraw();
    }

    public override void Draw()
    {
        if (HUD.hide)
            return;

        foreach (UIComponent component in _components)
            if (component.condition == null || component.condition())
            {
                if (component is UIMenuItem)
                    UIMenu.disabledDraw = component.mode == MenuItemMode.Disabled;
                component.Depth = Depth + 10;
                if (component.visible && component.mode != MenuItemMode.Hidden)
                    component.Draw();
                if (component is UIMenuItem)
                    UIMenu.disabledDraw = false;
            }
        _ = debug;
    }

    public void Resize()
    {
        _dirty = false;
        _didResize = true;
        OnResize();
    }
    #endregion

    #region Protected Methods
    protected virtual void SizeChildren() { }

    protected virtual void OnResize() { }
    #endregion
}