using System;
using System.Collections.Generic;
using System.Globalization;

namespace DuckGame;

public class ContextSlider : ContextMenu
{
	private SpriteMap _radioButton;

	private FieldBinding _field;

	private SpriteMap _adjusterHand;

	private float _step;

	private string _minSpecial;

	private bool _adjust;

	private bool _time;

	private Type _myType;

	public bool adjust
	{
		get
		{
			return _adjust;
		}
		set
		{
			_adjust = value;
		}
	}

	public ContextSlider(string text, IContextListener owner, FieldBinding field, float step, string minSpecial, bool time, Type myType, string valTooltip)
		: base(owner)
	{
		itemSize.x = 150f;
		itemSize.y = 16f;
		_text = text;
		_field = field;
		_radioButton = new SpriteMap("Editor/radioButton", 16, 16);
		_dragMode = true;
		_step = step;
		_minSpecial = minSpecial;
		_adjusterHand = new SpriteMap("adjusterHand", 18, 17);
		_time = time;
		_myType = myType;
		tooltip = valTooltip;
		if (_field != null && _field.value != null && _field.value.GetType().IsEnum)
		{
			_step = 1f;
		}
	}

	public ContextSlider(string text, IContextListener owner, FieldBinding field = null, float step = 0.25f, string minSpecial = null, bool time = false, Type myType = null)
		: base(owner)
	{
		itemSize.x = 150f;
		itemSize.y = 16f;
		_text = text;
		_field = field;
		_radioButton = new SpriteMap("Editor/radioButton", 16, 16);
		_dragMode = true;
		_step = step;
		_minSpecial = minSpecial;
		_adjusterHand = new SpriteMap("adjusterHand", 18, 17);
		_time = time;
		_myType = myType;
		if (_field != null && _field.value != null && _field.value.GetType().IsEnum)
		{
			_step = 1f;
		}
	}

	protected override void OnClose()
	{
		_adjust = false;
		_hover = false;
	}

	public override void Selected()
	{
		if (Editor.inputMode == EditorInput.Mouse || (!_enteringSlideMode && Editor.inputMode == EditorInput.Touch && TouchScreen.GetPress().Check(new Rectangle(base.x, base.y, itemSize.x, itemSize.y), base.layer.camera)))
		{
			_canEditSlide = true;
		}
		_enteringSlideMode = false;
		if (_canEditSlide && (Editor.inputMode == EditorInput.Mouse || (Editor.inputMode == EditorInput.Touch && TouchScreen.GetTouch().Check(new Rectangle(base.x, base.y, itemSize.x, itemSize.y), base.layer.camera))))
		{
			_sliding = true;
			float pos = Maths.Clamp(Mouse.x - position.x, 0f, itemSize.x);
			if (Editor.inputMode == EditorInput.Touch)
			{
				pos = Maths.Clamp(TouchScreen.GetTouch().Transform(base.layer.camera).x - position.x, 0f, itemSize.x);
			}
			if (_field.value is List<TypeProbPair>)
			{
				if (_step > 0f)
				{
					pos = (float)Math.Round(pos / itemSize.x * 1f / _step) * _step / 1f * itemSize.x;
				}
				TypeProbPair p = null;
				List<TypeProbPair> list = _field.value as List<TypeProbPair>;
				foreach (TypeProbPair pair in list)
				{
					if (pair.type == _myType)
					{
						p = pair;
						break;
					}
				}
				if (p == null)
				{
					p = new TypeProbPair
					{
						probability = 0f,
						type = _myType
					};
					list.Add(p);
				}
				p.probability = 0f + pos / itemSize.x * 1f;
				if (p.probability == 0f)
				{
					list.Remove(p);
				}
			}
			else
			{
				float fullRange = Math.Abs(_field.max - _field.min);
				if (_step > 0f)
				{
					pos = Maths.Snap(pos / itemSize.x * fullRange, _step) / fullRange * itemSize.x;
				}
				if (_field.value is float)
				{
					_field.value = _field.min + pos / itemSize.x * fullRange;
				}
				else if (_field.value is int)
				{
					_field.value = (int)Math.Round(_field.min + pos / itemSize.x * (Math.Abs(_field.min) + _field.max));
				}
				else if (_field.value != null && _field.value.GetType().IsEnum)
				{
					int num = (int)Math.Round(_field.min + pos / itemSize.x * (Math.Abs(_field.min) + _field.max));
					Array vals = Enum.GetValues(_field.value.GetType());
					if (num >= 0 && num < vals.Length)
					{
						_field.value = vals.GetValue(num);
					}
				}
			}
		}
		else
		{
			_sliding = false;
		}
		Editor.hasUnsavedChanges = true;
	}

	public void Increment()
	{
		if (_field.value is List<TypeProbPair>)
		{
			TypeProbPair p = null;
			List<TypeProbPair> list = _field.value as List<TypeProbPair>;
			foreach (TypeProbPair pair in list)
			{
				if (pair.type == _myType)
				{
					p = pair;
					break;
				}
			}
			if (p == null)
			{
				p = new TypeProbPair
				{
					probability = 0f,
					type = _myType
				};
				list.Add(p);
			}
			p.probability += _step;
			p.probability = Maths.Clamp(p.probability, 0f, 1f);
		}
		else if (_field.value is float)
		{
			float val = (float)_field.value;
			val += _step;
			val = Maths.Clamp(val, _field.min, _field.max);
			_field.value = val;
		}
		else if (_field.value is int)
		{
			int val2 = (int)_field.value;
			val2 += (int)_step;
			val2 = (int)Maths.Clamp(val2, _field.min, _field.max);
			_field.value = val2;
		}
		else if (_field.value != null && _field.value.GetType().IsEnum)
		{
			int val3 = (int)_field.value;
			val3 += (int)_step;
			val3 = (int)Maths.Clamp(val3, _field.min, _field.max);
			Array vals = Enum.GetValues(_field.value.GetType());
			if (val3 >= 0 && val3 < vals.Length)
			{
				_field.value = vals.GetValue(val3);
			}
		}
		Editor.hasUnsavedChanges = true;
	}

	public void Decrement()
	{
		if (_field.value is List<TypeProbPair>)
		{
			TypeProbPair p = null;
			List<TypeProbPair> list = _field.value as List<TypeProbPair>;
			foreach (TypeProbPair pair in list)
			{
				if (pair.type == _myType)
				{
					p = pair;
					break;
				}
			}
			if (p == null)
			{
				p = new TypeProbPair
				{
					probability = 0f,
					type = _myType
				};
				list.Add(p);
			}
			if (p.probability == 0f)
			{
				list.Remove(p);
			}
			else
			{
				p.probability -= _step;
				p.probability = Maths.Clamp(p.probability, 0f, 1f);
			}
		}
		else if (_field.value is float)
		{
			float val = (float)_field.value;
			val -= _step;
			val = Maths.Clamp(val, _field.min, _field.max);
			_field.value = val;
		}
		else if (_field.value is int)
		{
			int val2 = (int)_field.value;
			val2 -= (int)_step;
			val2 = (int)Maths.Clamp(val2, _field.min, _field.max);
			_field.value = val2;
		}
		else if (_field.value != null && _field.value.GetType().IsEnum)
		{
			int val3 = (int)_field.value;
			val3 -= (int)_step;
			val3 = (int)Maths.Clamp(val3, _field.min, _field.max);
			Array vals = Enum.GetValues(_field.value.GetType());
			if (val3 >= 0 && val3 < vals.Length)
			{
				_field.value = vals.GetValue(val3);
			}
		}
		Editor.hasUnsavedChanges = true;
	}

	public override void Update()
	{
		if (Editor.inputMode == EditorInput.Gamepad)
		{
			if (_hover || _adjust)
			{
				if (Input.Pressed("SELECT"))
				{
					_adjust = true;
				}
				if (Input.Released("SELECT"))
				{
					_adjust = false;
				}
			}
			if (_adjust)
			{
				Editor.tookInput = true;
				int times = 1;
				float realStep = _step;
				if (Input.Down("RAGDOLL"))
				{
					times = 5;
				}
				if (Input.Down("STRAFE"))
				{
					_step *= 0.1f;
				}
				if (Input.Pressed("MENULEFT"))
				{
					for (int i = 0; i < times; i++)
					{
						Decrement();
					}
				}
				if (Input.Pressed("MENURIGHT"))
				{
					for (int j = 0; j < times; j++)
					{
						Increment();
					}
				}
				_step = realStep;
			}
		}
		else if (_hover)
		{
			_adjust = true;
			if (Mouse.scroll > 0f)
			{
				Editor.didUIScroll = true;
				Decrement();
				ContextMenu._didContextScroll = true;
			}
			if (Mouse.scroll < 0f)
			{
				Editor.didUIScroll = true;
				Increment();
				ContextMenu._didContextScroll = true;
			}
		}
		else
		{
			_adjust = false;
		}
		base.Update();
	}

	public override void Draw()
	{
		float fVal = 0f;
		string val = "";
		if (_field.value is List<TypeProbPair>)
		{
			TypeProbPair p = null;
			foreach (TypeProbPair pair in _field.value as List<TypeProbPair>)
			{
				if (pair.type == _myType)
				{
					p = pair;
					break;
				}
			}
			fVal = ((p == null) ? 0f : p.probability);
			val = fVal.ToString("0.00", CultureInfo.InvariantCulture);
		}
		else if (_field.value is float)
		{
			fVal = (float)_field.value;
			val = fVal.ToString("0.00", CultureInfo.InvariantCulture);
		}
		else if (_field.value is int)
		{
			fVal = (int)_field.value;
			val = Change.ToString((int)_field.value);
		}
		else if (_field.value != null && _field.value.GetType().IsEnum)
		{
			fVal = (int)_field.value;
			val = Enum.GetName(_field.value.GetType(), _field.value);
		}
		if (_minSpecial != null && fVal == _field.min)
		{
			val = _minSpecial;
		}
		else if (_time)
		{
			val = MonoMain.TimeString(TimeSpan.FromSeconds((int)fVal), 2);
		}
		if (_adjust)
		{
			float gapSize = _field.max - _field.min;
			float offset = 4f + (gapSize - (_field.max - fVal)) / gapSize * (itemSize.x - 8f);
			float leftPos = 0f;
			float rightPos = itemSize.x;
			val = _text + ": " + val;
			Color c = Color.White;
			if (_field.value is List<TypeProbPair>)
			{
				c = ((fVal == 0f) ? Color.DarkGray : ((fVal < 0.3f) ? Colors.DGRed : ((!(fVal < 0.7f)) ? Color.Green : Color.Orange)));
			}
			float depthAdd = 0.1f;
			if (Editor.inputMode == EditorInput.Gamepad)
			{
				depthAdd = 0.05f;
			}
			bool drawFlipped = false;
			float valStringWidth = Graphics.GetStringWidth(val);
			if (position.x + itemSize.x + 8f + valStringWidth > base.layer.width)
			{
				drawFlipped = true;
			}
			if (drawFlipped)
			{
				leftPos = 0f - valStringWidth - 12f;
				Graphics.DrawString(val, position + new Vec2(0f - valStringWidth - 8f, 5f), c, 0.82f + depthAdd);
			}
			else
			{
				Graphics.DrawString(val, position + new Vec2(itemSize.x + 8f, 5f), c, 0.82f + depthAdd);
				rightPos += valStringWidth + 10f;
			}
			Graphics.DrawRect(position + new Vec2(offset - 2f, 3f), position + new Vec2(offset + 2f, itemSize.y - 3f), new Color(250, 250, 250), 0.85f + depthAdd);
			Graphics.DrawRect(position + new Vec2(leftPos, 0f), position + new Vec2(rightPos, itemSize.y), new Color(70, 70, 70), 0.75f + depthAdd);
			Graphics.DrawRect(position + new Vec2(4f, itemSize.y / 2f - 2f), position + new Vec2(itemSize.x - 4f, itemSize.y / 2f + 2f), new Color(150, 150, 150), 0.82f + depthAdd);
			if (Editor.inputMode == EditorInput.Gamepad)
			{
				Vec2 handPos = position + new Vec2(offset, 0f);
				_adjusterHand.depth = 0.9f;
				Graphics.Draw(_adjusterHand, handPos.x - 6f, handPos.y - 6f);
			}
		}
		else
		{
			if (_hover)
			{
				Graphics.DrawRect(position, position + itemSize, new Color(70, 70, 70), base.depth);
			}
			Color c2 = Color.White;
			if (_field.value is List<TypeProbPair>)
			{
				c2 = ((fVal == 0f) ? Color.DarkGray : ((fVal < 0.3f) ? Colors.DGRed : ((!(fVal < 0.7f)) ? Color.Green : Color.Orange)));
			}
			Graphics.DrawString(_text, position + new Vec2(2f, 5f), c2, 0.82f);
			Graphics.DrawString(val, position + new Vec2(itemSize.x - 4f - Graphics.GetStringWidth(val), 5f), Color.White, 0.82f);
		}
	}
}
