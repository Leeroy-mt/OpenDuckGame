using System;
using System.Drawing;

namespace DuckGame;

public class FontInfo
{
	public float EmHeightPixels;

	public float AscentPixels;

	public float DescentPixels;

	public float CellHeightPixels;

	public float InternalLeadingPixels;

	public float LineSpacingPixels;

	public float ExternalLeadingPixels;

	public float RelTop;

	public float RelBaseline;

	public float RelBottom;

	public FontInfo(System.Drawing.Graphics gr, Font the_font)
	{
		float em_height = the_font.FontFamily.GetEmHeight(the_font.Style);
		EmHeightPixels = ConvertUnits(gr, the_font.Size, the_font.Unit, GraphicsUnit.Pixel);
		float design_to_pixels = EmHeightPixels / em_height;
		AscentPixels = design_to_pixels * (float)the_font.FontFamily.GetCellAscent(the_font.Style);
		DescentPixels = design_to_pixels * (float)the_font.FontFamily.GetCellDescent(the_font.Style);
		CellHeightPixels = AscentPixels + DescentPixels;
		InternalLeadingPixels = CellHeightPixels - EmHeightPixels;
		LineSpacingPixels = design_to_pixels * (float)the_font.FontFamily.GetLineSpacing(the_font.Style);
		ExternalLeadingPixels = LineSpacingPixels - CellHeightPixels;
		RelTop = InternalLeadingPixels;
		RelBaseline = AscentPixels;
		RelBottom = CellHeightPixels;
	}

	private float ConvertUnits(System.Drawing.Graphics gr, float value, GraphicsUnit from_unit, GraphicsUnit to_unit)
	{
		if (from_unit == to_unit)
		{
			return value;
		}
		switch (from_unit)
		{
		case GraphicsUnit.Document:
			value *= gr.DpiX / 300f;
			break;
		case GraphicsUnit.Inch:
			value *= gr.DpiX;
			break;
		case GraphicsUnit.Millimeter:
			value *= gr.DpiX / 25.4f;
			break;
		case GraphicsUnit.Point:
			value *= gr.DpiX / 72f;
			break;
		default:
			throw new Exception("Unknown input unit " + from_unit.ToString() + " in FontInfo.ConvertUnits");
		case GraphicsUnit.Pixel:
			break;
		}
		switch (to_unit)
		{
		case GraphicsUnit.Document:
			value /= gr.DpiX / 300f;
			break;
		case GraphicsUnit.Inch:
			value /= gr.DpiX;
			break;
		case GraphicsUnit.Millimeter:
			value /= gr.DpiX / 25.4f;
			break;
		case GraphicsUnit.Point:
			value /= gr.DpiX / 72f;
			break;
		default:
			throw new Exception("Unknown output unit " + to_unit.ToString() + " in FontInfo.ConvertUnits");
		case GraphicsUnit.Pixel:
			break;
		}
		return value;
	}
}
