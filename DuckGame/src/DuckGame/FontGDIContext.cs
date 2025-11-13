using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace DuckGame;

internal class FontGDIContext
{
	public struct ABC
	{
		public int abcA;

		public uint abcB;

		public int abcC;
	}

	public struct ABCFloat
	{
		public float abcA;

		public float abcB;

		public float abcC;
	}

	public struct FontRange
	{
		public ushort Low;

		public ushort High;
	}

	private static System.Drawing.Graphics _graphicsContext;

	private static Bitmap _drawingImage;

	private static bool _dimensionsDirty = false;

	private static float _setWidth = 0f;

	private static float _setHeight = 0f;

	private static Font _systemFont;

	private static List<string> _loadedFonts = new List<string>();

	private static string _fontPath;

	private static float _size;

	private static Color _color;

	private static bool _dirty = false;

	private static bool _contextDirty = false;

	private static Brush _brush;

	private static int _numCharactersToRender = -1;

	private static bool _antiAliasing;

	private static FontStyle _fontStyle = FontStyle.Regular;

	private static StringFormat _formatting;

	private static Font _lastFont;

	private static IntPtr _hfont;

	public static Dictionary<string, RasterFont.Data> _fontDatas = new Dictionary<string, RasterFont.Data>();

	private static List<FontRange> curFont;

	public static int numCharactersToRender => _numCharactersToRender;

	public static void SetSize(float pSize)
	{
		if (pSize != _size)
		{
			_size = pSize;
			_dirty = true;
		}
	}

	public static void SetColor(Color pColor)
	{
		if (_color != pColor)
		{
			_color = pColor;
			_dirty = true;
		}
	}

	public void SetNumCharactersToRender(int pNum)
	{
		if (_numCharactersToRender != pNum)
		{
			_numCharactersToRender = pNum;
			_dirty = true;
		}
	}

	public static void SetAntiAliasing(bool pAnti)
	{
		if (_antiAliasing != pAnti)
		{
			_antiAliasing = pAnti;
			_dirty = true;
			_contextDirty = true;
		}
	}

	public static void SetFontStyle(FontStyle pStyle)
	{
		if (_fontStyle != pStyle)
		{
			_fontStyle = pStyle;
			_dirty = true;
		}
	}

	private static StringFormat GetStringFormatting(bool pCenter = false)
	{
		if (_formatting == null)
		{
			_formatting = new StringFormat();
			_formatting.Alignment = StringAlignment.Center;
			_formatting.Trimming = StringTrimming.None;
			_formatting.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
		}
		return _formatting;
	}

	[DllImport("gdi32.dll", ExactSpelling = true)]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

	[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern int DeleteObject(IntPtr hObj);

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	public static extern bool GetCharABCWidthsW(IntPtr hdc, uint uFirstChar, uint uLastChar, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeConst = 1)] ABC[] lpabc);

	private static ABC GetCharWidthABC(char ch, Font font, System.Drawing.Graphics gr)
	{
		ABC[] _temp = new ABC[1];
		IntPtr hdc = gr.GetHdc();
		IntPtr hFt = ((Font)font.Clone()).ToHfont();
		SelectObject(hdc, hFt);
		GetCharABCWidthsW(hdc, ch, ch, _temp);
		DeleteObject(hFt);
		gr.ReleaseHdc();
		return _temp[0];
	}

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	public static extern int SetMapMode(IntPtr hdc, int value);

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	public static extern bool GetCharABCWidthsFloatW(IntPtr hdc, uint uFirstChar, uint uLastChar, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeConst = 1)] ABCFloat[] lpabc);

	private static ABCFloat GetCharABCWidthsFloat(char ch, Font font, System.Drawing.Graphics gr)
	{
		ABCFloat[] _temp = new ABCFloat[1];
		IntPtr hdc = gr.GetHdc();
		if (_lastFont != font)
		{
			_hfont = ((Font)font.Clone()).ToHfont();
		}
		SelectObject(hdc, _hfont);
		GetCharABCWidthsFloatW(hdc, ch, ch, _temp);
		DeleteObject(_hfont);
		gr.ReleaseHdc();
		return _temp[0];
	}

	private static ABCFloat[] GetCharABCWidthsRange(char ch, char chend, Font font, System.Drawing.Graphics gr)
	{
		ABCFloat[] _temp = new ABCFloat[chend - ch + 2];
		IntPtr hdc = gr.GetHdc();
		if (_lastFont != font)
		{
			_hfont = ((Font)font.Clone()).ToHfont();
		}
		SelectObject(hdc, _hfont);
		GetCharABCWidthsFloatW(hdc, ch, chend, _temp);
		DeleteObject(_hfont);
		gr.ReleaseHdc();
		return _temp;
	}

	public static string GetName(string fontFamilyName)
	{
		fontFamilyName = fontFamilyName.Replace("@BOLD@", "");
		fontFamilyName = fontFamilyName.Replace("@ITALIC@", "");
		try
		{
			using FontFamily family = new FontFamily(fontFamilyName);
			if (family.IsStyleAvailable(FontStyle.Regular))
			{
				return family.Name;
			}
			if (family.IsStyleAvailable(FontStyle.Bold))
			{
				return family.Name;
			}
			if (family.IsStyleAvailable(FontStyle.Italic))
			{
				return family.Name;
			}
			if (family.IsStyleAvailable(FontStyle.Bold | FontStyle.Italic))
			{
				return family.Name;
			}
			return null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public unsafe static RasterFont.Data CreateRasterFontData(string pFullFontPath, float pSize = 12f, FontStyle pStyle = FontStyle.Regular, bool pSmooth = true)
	{
		if (pFullFontPath.Contains("@BOLD@"))
		{
			pStyle = FontStyle.Bold;
			pFullFontPath = pFullFontPath.Replace("@BOLD@", "");
		}
		if (pFullFontPath.Contains("@ITALIC@"))
		{
			pStyle = FontStyle.Italic;
			pFullFontPath = pFullFontPath.Replace("@ITALIC@", "");
		}
		string dataString = pFullFontPath + pSize + pStyle;
		RasterFont.Data rasterData = null;
		if (_fontDatas.TryGetValue(dataString, out rasterData))
		{
			return rasterData;
		}
		if (_fontDatas.Count > 8)
		{
			_fontDatas.Clear();
		}
		if (pSize > 120f)
		{
			pSize = 120f;
		}
		_size = pSize;
		_fontStyle = pStyle;
		rasterData = new RasterFont.Data();
		rasterData.fontSize = pSize;
		_fontDatas[dataString] = rasterData;
		_fontPath = pFullFontPath;
		_ = Resolution.current.y / 72;
		_systemFont = new Font(_fontPath, _size * RasterFont.fontScaleFactor, _fontStyle, GraphicsUnit.Pixel);
		_fontStyle = pStyle;
		_size = pSize;
		rasterData.name = _systemFont.Name;
		_graphicsContext = System.Drawing.Graphics.FromImage(new Bitmap(32, 32, PixelFormat.Format32bppArgb));
		_graphicsContext.PageUnit = GraphicsUnit.Pixel;
		ABCFloat[] fs = GetCharABCWidthsRange('\0', 'ё', _systemFont, _graphicsContext);
		ABCFloat largest = fs[87];
		float largestWidth = largest.abcB + Math.Abs(largest.abcA) + Math.Abs(largest.abcC);
		float size = (float)(int)(largestWidth * (float)Math.Sqrt(FancyBitmapFont._characters.Length) / largestWidth) * (_systemFont.GetHeight() + 8f);
		size = (MonoMain.hidef ? Math.Min(size, 4096f) : Math.Min(size, 2048f));
		_drawingImage = new Bitmap((int)size, (int)size, PixelFormat.Format32bppPArgb);
		_graphicsContext = System.Drawing.Graphics.FromImage(_drawingImage);
		_graphicsContext.PageUnit = GraphicsUnit.Pixel;
		if (pSmooth)
		{
			_graphicsContext.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		}
		else
		{
			_graphicsContext.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
			_graphicsContext.SmoothingMode = SmoothingMode.None;
			_graphicsContext.InterpolationMode = InterpolationMode.NearestNeighbor;
			_graphicsContext.PixelOffsetMode = PixelOffsetMode.None;
		}
		_graphicsContext.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
		_brush = new SolidBrush(System.Drawing.Color.FromArgb(255, 255, 255, 255));
		new Pen(System.Drawing.Color.FromArgb(100, 0, 255, 0));
		rasterData.fontHeight = _systemFont.GetHeight();
		float xPos = 0f;
		float yPos = 0f;
		char[] characters = FancyBitmapFont._characters;
		for (int i = 0; i < characters.Length; i++)
		{
			char c = characters[i];
			ABCFloat charSize = fs[(uint)c];
			BitmapFont_CharacterInfo ch = new BitmapFont_CharacterInfo();
			ch.leading = charSize.abcA;
			ch.width = charSize.abcB;
			ch.trailing = charSize.abcC;
			float trueWidth = charSize.abcB;
			int high = (int)rasterData.fontHeight + 8;
			float addWidth = trueWidth + Math.Abs(ch.leading) + Math.Abs(ch.trailing) + 8f;
			if (xPos + addWidth > (float)_drawingImage.Width)
			{
				yPos += (float)(high + 2);
				xPos = 0f;
			}
			ch.area = new Rectangle(xPos, yPos, addWidth - 2f, high);
			rasterData.characters.Add(ch);
			_graphicsContext.DrawString(c.ToString() ?? "", _systemFont, _brush, xPos + ch.trailing / 2f - ch.leading / 2f + trueWidth / 2f + 2f, yPos, GetStringFormatting());
			xPos += addWidth;
		}
		_graphicsContext.Flush();
		uint[] cols2 = new uint[_drawingImage.Width * _drawingImage.Height];
		uint* byteData = (uint*)(void*)_drawingImage.LockBits(new System.Drawing.Rectangle(0, 0, _drawingImage.Width, _drawingImage.Height), ImageLockMode.ReadOnly, _drawingImage.PixelFormat).Scan0;
		for (int j = 0; j < cols2.Length; j++)
		{
			uint col = (byteData[j] << 8) | (byteData[j] >> 24);
			cols2[j] = col;
		}
		byteData = null;
		rasterData.colors = cols2;
		rasterData.colorsWidth = _drawingImage.Width;
		rasterData.colorsHeight = _drawingImage.Height;
		_graphicsContext.Dispose();
		_drawingImage.Dispose();
		_systemFont.Dispose();
		return rasterData;
	}

	[DllImport("gdi32.dll")]
	public static extern uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs);

	public static List<FontRange> GetUnicodeRangesForFont(Font font)
	{
		if (curFont == null)
		{
			System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
			IntPtr hdc = g.GetHdc();
			IntPtr hFont = font.ToHfont();
			IntPtr old = SelectObject(hdc, hFont);
			IntPtr glyphSet = Marshal.AllocHGlobal((int)GetFontUnicodeRanges(hdc, IntPtr.Zero));
			GetFontUnicodeRanges(hdc, glyphSet);
			List<FontRange> fontRanges = new List<FontRange>();
			int count = Marshal.ReadInt32(glyphSet, 12);
			for (int i = 0; i < count; i++)
			{
				FontRange range = default(FontRange);
				range.Low = (ushort)Marshal.ReadInt16(glyphSet, 16 + i * 4);
				range.High = (ushort)(range.Low + Marshal.ReadInt16(glyphSet, 18 + i * 4) - 1);
				fontRanges.Add(range);
			}
			SelectObject(hdc, old);
			Marshal.FreeHGlobal(glyphSet);
			g.ReleaseHdc(hdc);
			g.Dispose();
			curFont = fontRanges;
		}
		return curFont;
	}

	public static bool CheckIfCharInFont(char character, Font font)
	{
		ushort intval = Convert.ToUInt16(character);
		List<FontRange> unicodeRangesForFont = GetUnicodeRangesForFont(font);
		bool isCharacterPresent = false;
		foreach (FontRange range in unicodeRangesForFont)
		{
			if (intval >= range.Low && intval <= range.High)
			{
				isCharacterPresent = true;
				break;
			}
		}
		return isCharacterPresent;
	}
}
