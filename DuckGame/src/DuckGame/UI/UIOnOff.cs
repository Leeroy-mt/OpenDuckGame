namespace DuckGame;

public class UIOnOff : UIText
{
	private FieldBinding _field;

	private FieldBinding _filterBinding;

	public UIOnOff(float wide, float high, FieldBinding field, FieldBinding filterBinding)
		: base("ON OFF", Color.White)
	{
		_field = field;
		_filterBinding = filterBinding;
	}

	public override void Draw()
	{
		_font.scale = base.scale;
		_font.alpha = base.alpha;
		string display = "ON OFF";
		float textWidth = _font.GetWidth(display);
		float xOffset = 0f;
		xOffset = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth) / 2f) : (base.width / 2f - textWidth)));
		float yOffset = 0f;
		yOffset = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
		bool val = (bool)_field.value;
		if (_filterBinding != null)
		{
			if (!(bool)_filterBinding.value)
			{
				_font.Draw("   ANY", base.x + xOffset, base.y + yOffset, Color.White, base.depth);
			}
			else if (val)
			{
				_font.Draw("    ON", base.x + xOffset, base.y + yOffset, Color.White, base.depth);
			}
			else
			{
				_font.Draw("   OFF", base.x + xOffset, base.y + yOffset, Color.White, base.depth);
			}
		}
		else
		{
			_font.Draw("ON", base.x + xOffset, base.y + yOffset, val ? Color.White : new Color(70, 70, 70), base.depth);
			_font.Draw("   OFF", base.x + xOffset, base.y + yOffset, (!val) ? Color.White : new Color(70, 70, 70), base.depth);
		}
	}
}
