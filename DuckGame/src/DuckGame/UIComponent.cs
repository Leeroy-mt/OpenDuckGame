using System;
using System.Collections.Generic;

namespace DuckGame;

public class UIComponent : Thing
{
	public MenuItemMode mode;

	public bool debug;

	public Func<bool> condition;

	protected UIComponent _parent;

	public bool isEnabled = true;

	protected bool _didResize;

	protected bool _dirty;

	protected Vec2 _offset = Vec2.Zero;

	protected bool _vertical;

	protected List<UIComponent> _components = new List<UIComponent>();

	protected bool _canFit;

	private UIFit _fit;

	private UIAlign _align;

	public Vec2 borderSize = Vec2.Zero;

	private bool _animate;

	protected bool _close;

	protected bool _animating;

	private Vec2 _startPosition;

	private bool _startInitialized;

	private bool _initialSizingComplete;

	protected bool _autoSizeVert;

	protected bool _autoSizeHor;

	public bool inWorld;

	public UIMenuAction _backFunction;

	public UIMenuAction _closeFunction;

	public UIMenuAction _acceptFunction;

	private bool _isPauseMenu;

	public UIComponent parent => _parent;

	public UIMenu rootMenu
	{
		get
		{
			if (!(this is UIMenu))
			{
				if (_parent != null)
				{
					return _parent.rootMenu;
				}
				return null;
			}
			return this as UIMenu;
		}
	}

	public bool dirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
		}
	}

	public Vec2 offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	public bool vertical
	{
		get
		{
			return _vertical;
		}
		set
		{
			_vertical = value;
		}
	}

	public IList<UIComponent> components => _components;

	public bool canFit => _canFit;

	public UIFit fit
	{
		get
		{
			return _fit;
		}
		set
		{
			if (_fit != value)
			{
				_dirty = true;
			}
			_fit = value;
		}
	}

	public UIAlign align
	{
		get
		{
			return _align;
		}
		set
		{
			if (_align != value)
			{
				_dirty = true;
			}
			_align = value;
		}
	}

	public bool animate => _animate;

	public bool open => !_close;

	public bool animating
	{
		get
		{
			return _animating;
		}
		set
		{
			foreach (UIComponent component in _components)
			{
				component.animating = value;
			}
			_animating = value;
		}
	}

	public bool autoSizeVert => _autoSizeVert;

	public bool autoSizeHor => _autoSizeHor;

	public bool isPauseMenu
	{
		get
		{
			if (!_isPauseMenu)
			{
				if (_parent != null)
				{
					return _parent.isPauseMenu;
				}
				return false;
			}
			return true;
		}
		set
		{
			_isPauseMenu = value;
		}
	}

	public UIComponent(float xpos, float ypos, float wide, float high)
		: base(xpos, ypos)
	{
		_collisionSize = new Vec2(wide, high);
		base.layer = Layer.HUD;
		base.depth = 0f;
		_autoSizeHor = wide < 0f;
		_autoSizeVert = high < 0f;
	}

	public virtual void Open()
	{
		MonoMain.menuOpenedThisFrame = true;
		_close = false;
		animating = true;
		foreach (UIComponent component in _components)
		{
			if (component.anchor == this)
			{
				component.Open();
			}
		}
		_initialSizingComplete = false;
	}

	public virtual void Close()
	{
		_close = true;
		animating = true;
		foreach (UIComponent component in _components)
		{
			component.Close();
		}
		if (!inWorld && rootMenu == this && !MonoMain.closeMenuUpdate.Contains(this))
		{
			MonoMain.closeMenuUpdate.Add(this);
		}
		OnClose();
	}

	public override void DoUpdate()
	{
		base.DoUpdate();
	}

	public virtual void OnClose()
	{
	}

	public override void Added(Level parent)
	{
		inWorld = true;
		base.Added(parent);
	}

	public virtual void UpdateParts()
	{
	}

	public override void Update()
	{
		if (!_startInitialized)
		{
			_startInitialized = true;
			_startPosition = position;
			position.y = base.layer.camera.height * 2f;
		}
		if (base.anchor == null)
		{
			float yLerp = (_close ? (base.layer.camera.height * 2f) : _startPosition.y);
			position.y = Lerp.FloatSmooth(position.y, yLerp, 0.2f, 1.05f);
			bool isAnimating = position.y != yLerp;
			if (animating != isAnimating)
			{
				animating = isAnimating;
			}
		}
		if (open && !animating)
		{
			UpdateParts();
		}
		if (_parent != null || open || animating)
		{
			SizeChildren();
			foreach (UIComponent component in _components)
			{
				if (component.condition == null || component.condition())
				{
					component.DoUpdate();
					if (component._didResize)
					{
						_dirty = true;
					}
					component._didResize = false;
				}
			}
		}
		if (_dirty)
		{
			Resize();
		}
		if (!UIMenu.globalUILock && !MonoMain.menuOpenedThisFrame && MonoMain.pauseMenu == this && ((Input.Pressed("START") && isPauseMenu) || MonoMain.closeMenus))
		{
			MonoMain.closeMenus = false;
			if (_closeFunction != null)
			{
				_closeFunction.Activate();
			}
			Close();
		}
		_dirty = false;
		_initialSizingComplete = true;
	}

	protected virtual void SizeChildren()
	{
	}

	public override void DoDraw()
	{
		if (_initialSizingComplete && (_animating || !_close))
		{
			base.DoDraw();
		}
	}

	public override void Draw()
	{
		if (HUD.hide)
		{
			return;
		}
		foreach (UIComponent component in _components)
		{
			if (component.condition == null || component.condition())
			{
				if (component is UIMenuItem)
				{
					UIMenu.disabledDraw = component.mode == MenuItemMode.Disabled;
				}
				component.depth = base.depth + 10;
				if (component.visible && component.mode != MenuItemMode.Hidden)
				{
					component.Draw();
				}
				if (component is UIMenuItem)
				{
					UIMenu.disabledDraw = false;
				}
			}
		}
		_ = debug;
	}

	public void Resize()
	{
		_dirty = false;
		_didResize = true;
		OnResize();
	}

	protected virtual void OnResize()
	{
	}

	public virtual void Add(UIComponent component, bool doAnchor = true)
	{
		_components.Add(component);
		component._parent = this;
		_dirty = true;
		component.dirty = true;
		if (doAnchor)
		{
			component.anchor = this;
		}
	}

	public virtual void Insert(UIComponent component, int position, bool doAnchor = true)
	{
		if (position >= _components.Count)
		{
			position = _components.Count;
		}
		_components.Insert(position, component);
		component._parent = this;
		_dirty = true;
		component.dirty = true;
		if (doAnchor)
		{
			component.anchor = this;
		}
	}

	public virtual void Remove(UIComponent component)
	{
		_components.Remove(component);
		if (component._parent == this)
		{
			component._parent = null;
		}
		if (component.anchor == this)
		{
			component.anchor = null;
		}
		_dirty = true;
	}
}
