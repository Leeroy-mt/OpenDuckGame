using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIBox : UIComponent
{
	private SpriteMap _sections;

	private float _seperation = 1f;

	public string _hoverControlString;

	private bool _borderVisible = true;

	protected int _selection;

	public bool _isMenu;

	public UIMenuItem _backButton;

	public int _defaultSelection;

	private bool _willSelectLast;

	private List<UIComponent> _currentMenuItemSelection;

	public bool allowBackButton = true;

	protected bool _inputLock;

	public int selection => _selection;

	public UIBox(float xpos, float ypos, float wide = -1f, float high = -1f, bool vert = true, bool isVisible = true)
		: base(xpos, ypos, wide, high)
	{
		_sections = new SpriteMap("uiBox", 10, 10);
		_vertical = vert;
		_borderVisible = isVisible;
		borderSize = (_borderVisible ? new Vec2(8f, 8f) : Vec2.Zero);
		_canFit = true;
	}

	public UIBox(bool vert = true, bool isVisible = true)
		: base(0f, 0f, -1f, -1f)
	{
		_sections = new SpriteMap("uiBox", 10, 10);
		_vertical = vert;
		_borderVisible = isVisible;
		borderSize = (_borderVisible ? new Vec2(8f, 8f) : Vec2.Zero);
		_canFit = true;
	}

	public override void Add(UIComponent component, bool doAnchor = true)
	{
		if (component is UIMenuItem)
		{
			_isMenu = true;
			if ((component as UIMenuItem).isBackButton)
			{
				_backButton = component as UIMenuItem;
			}
		}
		base.Add(component, doAnchor);
	}

	public override void Insert(UIComponent component, int position, bool doAnchor = true)
	{
		if (component is UIMenuItem)
		{
			_isMenu = true;
			if ((component as UIMenuItem).isBackButton)
			{
				_backButton = component as UIMenuItem;
			}
		}
		base.Insert(component, position, doAnchor);
	}

	public virtual void AssignDefaultSelection()
	{
		List<UIComponent> items = _components.Where((UIComponent val) => val is UIMenuItem && (val.condition == null || val.condition())).ToList();
		_defaultSelection = items.Count - 1;
	}

	public override void Open()
	{
		Graphics.fade = 1f;
		if (!MonoMain.dontResetSelection)
		{
			_selection = _defaultSelection;
			if (_willSelectLast)
			{
				List<UIComponent> items = _components.Where((UIComponent val) => val is UIMenuItem).ToList();
				_selection = items.Count - 1;
			}
		}
		base.Open();
	}

	protected override void SizeChildren()
	{
		foreach (UIComponent component in _components)
		{
			if ((component.condition == null || component.condition()) && component.canFit)
			{
				if (base.vertical)
				{
					component.collisionSize = new Vec2(collisionSize.x - borderSize.x * 2f, component.collisionSize.y);
				}
				else
				{
					component.collisionSize = new Vec2(component.collisionSize.x, collisionSize.y - borderSize.y * 2f);
				}
			}
		}
	}

	protected override void OnResize()
	{
		if (_vertical)
		{
			float wide = 0f;
			float high = 0f;
			foreach (UIComponent component in _components)
			{
				if (component.condition == null || component.condition())
				{
					high += component.collisionSize.y + _seperation;
					if (component.collisionSize.x > wide)
					{
						wide = component.collisionSize.x;
					}
				}
			}
			wide += borderSize.x * 2f;
			high -= _seperation;
			high += borderSize.y * 2f;
			if (_autoSizeHor && (base.fit & UIFit.Horizontal) == 0 && wide > _collisionSize.x)
			{
				_collisionSize.x = wide;
			}
			if (_autoSizeVert && (base.fit & UIFit.Vertical) == 0 && high > _collisionSize.y)
			{
				_collisionSize.y = high;
			}
			float yDraw = (0f - high) / 2f + borderSize.y;
			{
				foreach (UIComponent component2 in _components)
				{
					if (component2.condition == null || component2.condition())
					{
						component2.anchor.offset.x = 0f;
						if ((component2.align & UIAlign.Left) > UIAlign.Center)
						{
							component2.anchor.offset.x = (0f - collisionSize.x) / 2f + borderSize.x + component2.collisionSize.x / 2f;
						}
						else if ((component2.align & UIAlign.Right) > UIAlign.Center)
						{
							component2.anchor.offset.x = collisionSize.x / 2f - borderSize.x - component2.collisionSize.x / 2f;
						}
						component2.anchor.offset.y = yDraw * base.scale.y + component2.height / 2f;
						yDraw += component2.collisionSize.y + _seperation;
					}
				}
				return;
			}
		}
		float wide2 = 0f;
		float high2 = 0f;
		foreach (UIComponent component3 in _components)
		{
			if (component3.condition == null || component3.condition())
			{
				wide2 += component3.collisionSize.x + _seperation;
				if (component3.collisionSize.y > high2)
				{
					high2 = component3.collisionSize.y;
				}
			}
		}
		high2 += borderSize.y * 2f;
		wide2 -= _seperation;
		wide2 += borderSize.x * 2f;
		if (_autoSizeHor && (base.fit & UIFit.Horizontal) == 0 && wide2 > _collisionSize.x)
		{
			_collisionSize.x = wide2;
		}
		if (_autoSizeVert && (base.fit & UIFit.Vertical) == 0 && high2 > _collisionSize.y)
		{
			_collisionSize.y = high2;
		}
		float xDraw = (0f - wide2) / 2f + borderSize.x;
		foreach (UIComponent component4 in _components)
		{
			if (component4.condition == null || component4.condition())
			{
				component4.anchor.offset.x = xDraw * base.scale.x + component4.width / 2f;
				component4.anchor.offset.y = 0f;
				xDraw += component4.collisionSize.x + _seperation;
			}
		}
	}

	public virtual void SelectLastMenuItem()
	{
		List<UIComponent> items = _components.Where((UIComponent val) => val is UIMenuItem).ToList();
		_selection = items.Count - 1;
		_willSelectLast = true;
	}

	private void SelectPrevious()
	{
		int sel = _selection;
		do
		{
			_selection--;
			if (_selection < 0)
			{
				_selection = _currentMenuItemSelection.Count - 1;
			}
		}
		while (_currentMenuItemSelection[_selection].mode != MenuItemMode.Normal && sel != _selection);
		SFX.Play("textLetter", 0.7f);
	}

	private void SelectNext()
	{
		int sel = _selection;
		do
		{
			_selection++;
			if (_selection >= _currentMenuItemSelection.Count)
			{
				_selection = 0;
			}
		}
		while (_currentMenuItemSelection[_selection].mode != MenuItemMode.Normal && sel != _selection);
		SFX.Play("textLetter", 0.7f);
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
						{
							_backButton.Activate("SELECT");
						}
						else
						{
							_backFunction.Activate();
						}
						MonoMain.menuOpenedThisFrame = true;
					}
				}
				else if (!MonoMain.menuOpenedThisFrame && _isMenu)
				{
					MonoMain.closeMenus = true;
				}
			}
			else if (Input.Pressed("SELECT") && _acceptFunction != null && !_animating)
			{
				MonoMain.dontResetSelection = true;
				_acceptFunction.Activate();
				MonoMain.menuOpenedThisFrame = true;
			}
			if (_isMenu)
			{
				_currentMenuItemSelection = _components.Where((UIComponent val) => val is UIMenuItem && (val.condition == null || val.condition())).ToList();
				if (_vertical)
				{
					if (!_animating && Input.Pressed("MENUUP"))
					{
						SelectPrevious();
					}
					if (!_animating && Input.Pressed("MENUDOWN"))
					{
						SelectNext();
					}
				}
				else
				{
					if (!_animating && Input.Pressed("MENULEFT"))
					{
						SelectPrevious();
					}
					if (!_animating && Input.Pressed("MENURIGHT"))
					{
						SelectNext();
					}
				}
				_hoverControlString = null;
				for (int i = 0; i < _currentMenuItemSelection.Count; i++)
				{
					UIMenuItem item = _currentMenuItemSelection[i] as UIMenuItem;
					item.selected = i == _selection;
					if (i != _selection)
					{
						continue;
					}
					_hoverControlString = item.controlString;
					if (item.isEnabled)
					{
						if (!_animating && Input.Pressed("SELECT"))
						{
							item.Activate("SELECT");
							SFX.Play("rockHitGround", 0.7f);
						}
						else if (!_animating && Input.Pressed("MENU1"))
						{
							item.Activate("MENU1");
						}
						else if (!_animating && Input.Pressed("MENU2"))
						{
							item.Activate("MENU2");
						}
						else if (!_animating && Input.Pressed("RAGDOLL"))
						{
							item.Activate("RAGDOLL");
						}
						else if (!_animating && Input.Pressed("STRAFE"))
						{
							item.Activate("STRAFE");
						}
						else if (!_animating && Input.Pressed("MENULEFT"))
						{
							item.Activate("MENULEFT");
						}
						else if (!_animating && Input.Pressed("MENURIGHT"))
						{
							item.Activate("MENURIGHT");
						}
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
			_sections.scale = base.scale;
			_sections.alpha = base.alpha;
			_sections.depth = base.depth;
			_sections.frame = 0;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x, 0f - base.halfHeight + base.y);
			_sections.frame = 2;
			Graphics.Draw(_sections, base.halfWidth + base.x - (float)_sections.w * base.scale.x, 0f - base.halfHeight + base.y);
			_sections.frame = 1;
			_sections.xscale = (_collisionSize.x - (float)(_sections.w * 2)) / (float)_sections.w * base.xscale;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x + (float)_sections.w * base.scale.x, 0f - base.halfHeight + base.y);
			_sections.xscale = base.xscale;
			_sections.frame = 3;
			_sections.yscale = (_collisionSize.y - (float)(_sections.h * 2)) / (float)_sections.h * base.yscale;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x, 0f - base.halfHeight + base.y + (float)_sections.h * base.scale.y);
			_sections.frame = 5;
			Graphics.Draw(_sections, base.halfWidth + base.x - (float)_sections.w * base.scale.x, 0f - base.halfHeight + base.y + (float)_sections.h * base.scale.y);
			_sections.frame = 4;
			_sections.xscale = (_collisionSize.x - (float)(_sections.w * 2)) / (float)_sections.w * base.xscale;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x + (float)_sections.w * base.scale.x, 0f - base.halfHeight + base.y + (float)_sections.h * base.scale.y);
			_sections.xscale = base.xscale;
			_sections.yscale = base.yscale;
			_sections.frame = 6;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x, base.halfHeight + base.y - (float)_sections.h * base.scale.y);
			_sections.frame = 8;
			Graphics.Draw(_sections, base.halfWidth + base.x - (float)_sections.w * base.scale.x, base.halfHeight + base.y - (float)_sections.h * base.scale.y);
			_sections.frame = 7;
			_sections.xscale = (_collisionSize.x - (float)(_sections.w * 2)) / (float)_sections.w * base.xscale;
			Graphics.Draw(_sections, 0f - base.halfWidth + base.x + (float)_sections.w * base.scale.x, base.halfHeight + base.y - (float)_sections.h * base.scale.y);
		}
		base.Draw();
	}
}
