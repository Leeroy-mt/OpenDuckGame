using System;

namespace DuckGame;

public class UIProgressBar : UIComponent
{
	private FieldBinding _field;

	private Vec2 _barSize;

	private float _step;

	public UIProgressBar(float wide, float high, FieldBinding field, float increment, Color c = default(Color))
		: base(0f, 0f, 0f, 0f)
	{
		_field = field;
		_barSize = new Vec2(wide, high);
		_collisionSize = _barSize;
		_step = increment;
	}

	public override void Draw()
	{
		float sizeX = _barSize.x * base.scale.x;
		float sizeY = _barSize.y * base.scale.y;
		int numParts = (int)Math.Ceiling((_field.max - _field.min) / _step);
		for (int i = 0; i < numParts; i++)
		{
			Vec2 tl = position - new Vec2(base.halfWidth, sizeY / 2f) + new Vec2(i * (int)Math.Round(sizeX / (float)numParts), 0f);
			Vec2 br = position - new Vec2(base.halfWidth, (0f - sizeY) / 2f) + new Vec2((float)((i + 1) * (int)Math.Round(sizeX / (float)numParts)) - 1f, 0f);
			if ((base.align & UIAlign.Center) > UIAlign.Center)
			{
				tl.x += base.halfWidth - sizeX / 2f;
				br.x += base.halfWidth - sizeX / 2f;
			}
			else if ((base.align & UIAlign.Right) > UIAlign.Center)
			{
				tl.x += base.width - sizeX;
				br.x += base.width - sizeX;
			}
			if (tl.x == br.x)
			{
				br.x += 1f;
			}
			float value = (float)_field.value;
			Graphics.DrawRect(tl, br, (value > (float)i * _step) ? Color.White : new Color(70, 70, 70), base.depth);
		}
	}
}
