namespace DuckGame;

public class UIMenuItemSlider : UIMenuItem
{
	private FieldBinding _field;

	private float _step;

	public UIMenuItemSlider(string text, UIMenuAction action = null, FieldBinding field = null, float step = 0.1f, Color c = default(Color))
		: base(action)
	{
		if (c == default(Color))
		{
			c = Colors.MenuOption;
		}
		UIDivider splitter = new UIDivider(vert: true, 0f);
		UIText t = new UIText(text, c)
		{
			align = UIAlign.Left
		};
		splitter.leftSection.Add(t);
		UIProgressBar bar = new UIProgressBar((step < 1f / 19f) ? 26 : 30, 7f, field, step)
		{
			align = UIAlign.Right
		};
		splitter.rightSection.Add(bar);
		base.rightSection.Add(splitter);
		_arrow = new UIImage("contextArrowRight");
		_arrow.align = UIAlign.Right;
		_arrow.visible = false;
		base.leftSection.Add(_arrow);
		_field = field;
		_step = step;
		controlString = "@CANCEL@BACK @WASD@ADJUST";
	}

	public override void Activate(string trigger)
	{
		float newVal = 0f;
		if (trigger == "MENULEFT")
		{
			newVal = Maths.Clamp((float)_field.value - _step, _field.min, _field.max);
		}
		else
		{
			if (!(trigger == "MENURIGHT"))
			{
				return;
			}
			newVal = Maths.Clamp((float)_field.value + _step, _field.min, _field.max);
		}
		if (newVal != (float)_field.value)
		{
			SFX.Play("textLetter", 0.7f);
		}
		_field.value = newVal;
	}
}
