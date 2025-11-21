using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIMenuItemResolution : UIMenuItem
{
	public Resolution currentValue;

	public Action selectAction;

	protected FieldBinding _field;

	protected UIText _textItem;

	protected List<Resolution> _values;

	protected int currentIndex;

	private bool showAll;

	private Resolution _current;

	public UIMenuItemResolution(string text, FieldBinding field, UIMenuAction action = null, Color c = default(Color))
		: base(action)
	{
		if (c == default(Color))
		{
			c = Colors.MenuOption;
		}
		if (MonoMain._fullScreen)
		{
			controlString = "@WASD@ADJUST @SELECT@APPLY";
		}
		else
		{
			controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@ALL";
		}
		UIDivider splitter = new UIDivider(vert: true, 0f);
		UIText t = new UIText(text, c)
		{
			align = UIAlign.Left
		};
		splitter.leftSection.Add(t);
		RefreshValueList();
		currentIndex = _values.IndexOf(field.value as Resolution);
		if (currentIndex < 0)
		{
			currentIndex = 0;
		}
		_textItem = new UIChangingText(-1f, -1f, field, null);
		string longest = "";
		foreach (Resolution r in _values)
		{
			if (r.ToShortString().Length > longest.Length)
			{
				longest = r.ToShortString();
			}
		}
		(_textItem as UIChangingText).defaultSizeString = longest + "   ";
		_textItem.minLength = longest.Length + 3;
		_textItem.text = _values[currentIndex].ToShortString() + "   ";
		_textItem.align = UIAlign.Right;
		splitter.rightSection.Add(_textItem);
		base.rightSection.Add(splitter);
		_arrow = new UIImage("contextArrowRight");
		_arrow.align = UIAlign.Right;
		_arrow.visible = false;
		base.leftSection.Add(_arrow);
		_field = field;
		currentValue = _field.value as Resolution;
	}

	private void RefreshValueList()
	{
		if (showAll)
		{
			controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@BASIC";
			_values = Resolution.supportedDisplaySizes[Resolution.current.mode];
			return;
		}
		controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@ALL";
		if (Resolution.current.mode == ScreenMode.Windowed)
		{
			_values = Resolution.supportedDisplaySizes[Resolution.current.mode].Where((Resolution x) => x.recommended || x == Resolution.current).ToList();
		}
		else
		{
			_values = Resolution.supportedDisplaySizes[Resolution.current.mode].Where((Resolution x) => Math.Abs(x.aspect - Resolution.adapterResolution.aspect) < 0.05f || x == Resolution.current).ToList();
		}
	}

	public override void Activate(string trigger)
	{
		if (trigger == "SELECT")
		{
			_field.value = currentValue;
			if (selectAction != null)
			{
				selectAction();
			}
			return;
		}
		if (trigger == "MENU2")
		{
			showAll = !showAll;
			RefreshValueList();
			SFX.Play("textLetter", 0.7f);
			currentIndex = _values.IndexOf(_field.value as Resolution);
			if (currentIndex < 0)
			{
				currentIndex = 0;
			}
			_textItem.text = _values[currentIndex].ToShortString();
			return;
		}
		if (trigger == "SELECT")
		{
			if (selectAction != null)
			{
				selectAction();
			}
			_field.value = currentValue;
			return;
		}
		int num = currentIndex;
		if (trigger == "MENULEFT")
		{
			currentIndex--;
		}
		else if (trigger == "MENURIGHT")
		{
			currentIndex++;
		}
		if (currentIndex < 0)
		{
			currentIndex = 0;
		}
		if (currentIndex > _values.Count - 1)
		{
			currentIndex = _values.Count - 1;
		}
		currentValue = _values[currentIndex];
		if (num != currentIndex)
		{
			SFX.Play("textLetter", 0.7f);
		}
		if (_textItem != null)
		{
			_textItem.text = _values[currentIndex].ToShortString();
		}
	}

	public override void Draw()
	{
		if (Resolution.current != _current)
		{
			RefreshValueList();
			_current = Resolution.current;
		}
		if (!base.selected)
		{
			currentValue = _field.value as Resolution;
		}
		currentIndex = _values.IndexOf(currentValue);
		if (currentIndex < 0)
		{
			currentIndex = _values.Count - 1;
		}
		_textItem.text = _values[currentIndex].ToShortString();
		base.Draw();
	}
}
