using System;
using System.Collections.Generic;

namespace DuckGame;

public class RasterFont : FancyBitmapFont
{
	public class Data
	{
		public string name;

		public float fontHeight;

		public float fontSize;

		public List<BitmapFont_CharacterInfo> characters = new List<BitmapFont_CharacterInfo>();

		public uint[] colors;

		public int colorsWidth;

		public int colorsHeight;
	}

	public static readonly RasterFont None = new RasterFont(null, 0f);

	public Data data;

	public static float fontScaleFactor => (float)(Resolution.current.y / 72) / 10f;

	public float size
	{
		get
		{
			return data.fontSize;
		}
		set
		{
			if (value != data.fontSize)
			{
				Rebuild(data.name, value);
			}
		}
	}

	public override Sprite _texture
	{
		get
		{
			if (_textureInternal == null)
			{
				Tex2D tex = new Tex2D(data.colorsWidth, data.colorsHeight);
				if (data.colors != null)
				{
					tex.SetData(data.colors);
				}
				_textureInternal = new Sprite(tex);
			}
			return _textureInternal;
		}
		set
		{
			_textureInternal = value;
		}
	}

	public RasterFont(string pFont, float pSize)
	{
		Rebuild(pFont, pSize);
	}

	~RasterFont()
	{
		if (_textureInternal != null)
		{
			_textureInternal.texture.Dispose();
		}
	}

	public static string GetName(string pFont)
	{
		return FontGDIContext.GetName(pFont);
	}

	public void Rebuild(string pFont, float pSize)
	{
		if (pFont != null && pFont != "NULLDUCKFONTDATA")
		{
			data = FontGDIContext.CreateRasterFontData(pFont, pSize);
			_texture = null;
			_widths = new List<Rectangle>();
			foreach (BitmapFont_CharacterInfo inf in data.characters)
			{
				_widths.Add(inf.area);
			}
			_charHeight = (int)data.fontHeight;
			_characterInfos = data.characters;
			_rasterData = data;
		}
		else
		{
			data = new Data
			{
				name = "NULLDUCKFONTDATA",
				fontSize = pSize
			};
		}
		if (_widths == null || _widths.Count == 0)
		{
			Construct("smallFont");
		}
	}

	public string Serialize()
	{
		return data.name + "^" + data.fontSize;
	}

	public static RasterFont Deserialize(string pData)
	{
		try
		{
			string[] parts = pData.Split('^');
			if (parts.Length == 2)
			{
				string name = parts[0];
				int size = Math.Min(Convert.ToInt32(parts[1]), 120);
				if (FontGDIContext.GetName(name) != null)
				{
					return new RasterFont(name, size);
				}
			}
		}
		catch (Exception)
		{
		}
		return None;
	}
}
