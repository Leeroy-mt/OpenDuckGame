using System;

namespace DuckGame;

public class UIMenuItem : UIDivider
{
	public string controlString;

	private bool _selected;

	protected UIImage _arrow;

	protected UIMenuAction _action;

	protected bool _isBackButton;

	protected UIText _textElement;

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
		}
	}

	public bool isBackButton => _isBackButton;

	public UIMenuAction menuAction
	{
		get
		{
			return _action;
		}
		set
		{
			_action = value;
		}
	}

	public string text
	{
		get
		{
			return _textElement.text;
		}
		set
		{
			_textElement.text = value;
			bool flag = (_textElement.dirty = true);
			_dirty = flag;
		}
	}

	public void SetFont(BitmapFont font)
	{
		_textElement.SetFont(font);
	}

	public UIMenuItem(string text, UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default(Color), bool backButton = false)
		: base(vert: true, 8)
	{
		if (c == default(Color))
		{
			c = Colors.MenuOption;
		}
		_textElement = new UIText(text, c);
		_textElement.align = UIAlign.Left;
		base.rightSection.Add(_textElement);
		_arrow = new UIImage("contextArrowRight");
		_arrow.align = UIAlign.Right;
		_arrow.visible = false;
		base.leftSection.Add(_arrow);
		_action = action;
		base.align = al;
		_isBackButton = backButton;
	}

	public UIMenuItem(Func<string> pTextFunc, UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default(Color), bool backButton = false)
		: base(vert: true, 8)
	{
		if (c == default(Color))
		{
			c = Colors.MenuOption;
		}
		_textElement = new UIText(pTextFunc, c);
		_textElement.align = UIAlign.Left;
		base.rightSection.Add(_textElement);
		_arrow = new UIImage("contextArrowRight");
		_arrow.align = UIAlign.Right;
		_arrow.visible = false;
		base.leftSection.Add(_arrow);
		_action = action;
		base.align = al;
		_isBackButton = backButton;
	}

	public UIMenuItem(UIMenuAction action = null, Color c = default(Color))
		: base(vert: true, 8)
	{
		_action = action;
	}

	public override void Update()
	{
		_arrow.visible = _selected;
		if (_action != null)
		{
			_action.Update();
		}
		base.Update();
	}

	public virtual void Activate(string trigger)
	{
		if (_action != null && trigger == "SELECT")
		{
			_action.Activate();
		}
	}
}
