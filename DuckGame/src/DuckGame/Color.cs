using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DuckGame;

/// <summary>
/// An RGBA color.
/// </summary>
public struct Color : IEquatable<Color>
{
	public byte r;

	public byte g;

	public byte b;

	public byte a;

	public static Color AliceBlue = new Color(240, 248, 255, 255);

	public static Color AntiqueWhite = new Color(250, 235, 215, 255);

	public static Color Aqua = new Color(0, 255, 255, 255);

	public static Color Aquamarine = new Color(127, 255, 212, 255);

	public static Color Azure = new Color(240, 255, 255, 255);

	public static Color Beige = new Color(245, 245, 220, 255);

	public static Color Bisque = new Color(255, 228, 196, 255);

	public static Color Black = new Color(0, 0, 0, 255);

	public static Color BlanchedAlmond = new Color(255, 235, 205, 255);

	public static Color Blue = new Color(0, 0, 255, 255);

	public static Color BlueViolet = new Color(138, 43, 226, 255);

	public static Color Brown = new Color(165, 42, 42, 255);

	public static Color BurlyWood = new Color(222, 184, 135, 255);

	public static Color CadetBlue = new Color(95, 158, 160, 255);

	public static Color Chartreuse = new Color(127, 255, 0, 255);

	public static Color Chocolate = new Color(210, 105, 30, 255);

	public static Color Coral = new Color(255, 127, 80, 255);

	public static Color CornflowerBlue = new Color(100, 149, 237, 255);

	public static Color Cornsilk = new Color(255, 248, 220, 255);

	public static Color Crimson = new Color(220, 20, 60, 255);

	public static Color Cyan = new Color(0, 255, 255, 255);

	public static Color DarkBlue = new Color(0, 0, 139, 255);

	public static Color DarkCyan = new Color(0, 139, 139, 255);

	public static Color DarkGoldenrod = new Color(184, 134, 11, 255);

	public static Color DarkGray = new Color(169, 169, 169, 255);

	public static Color DarkGreen = new Color(0, 100, 0, 255);

	public static Color DarkKhaki = new Color(189, 183, 107, 255);

	public static Color DarkMagenta = new Color(139, 0, 139, 255);

	public static Color DarkOliveGreen = new Color(85, 107, 47, 255);

	public static Color DarkOrange = new Color(255, 140, 0, 255);

	public static Color DarkOrchid = new Color(153, 50, 204, 255);

	public static Color DarkRed = new Color(139, 0, 0, 255);

	public static Color DarkSalmon = new Color(233, 150, 122, 255);

	public static Color DarkSeaGreen = new Color(143, 188, 139, 255);

	public static Color DarkSlateBlue = new Color(72, 61, 139, 255);

	public static Color DarkSlateGray = new Color(47, 79, 79, 255);

	public static Color DarkTurquoise = new Color(0, 206, 209, 255);

	public static Color DarkViolet = new Color(148, 0, 211, 255);

	public static Color DeepPink = new Color(255, 20, 147, 255);

	public static Color DeepSkyBlue = new Color(0, 191, 255, 255);

	public static Color DimGray = new Color(105, 105, 105, 255);

	public static Color DodgerBlue = new Color(30, 144, 255, 255);

	public static Color Firebrick = new Color(178, 34, 34, 255);

	public static Color FloralWhite = new Color(255, 250, 240, 255);

	public static Color ForestGreen = new Color(34, 139, 34, 255);

	public static Color Fuchsia = new Color(255, 0, 255, 255);

	public static Color Gainsboro = new Color(220, 220, 220, 255);

	public static Color GhostWhite = new Color(248, 248, 255, 255);

	public static Color Gold = new Color(255, 215, 0, 255);

	public static Color Goldenrod = new Color(218, 165, 32, 255);

	public static Color Gray = new Color(128, 128, 128, 255);

	public static Color Green = new Color(0, 128, 0, 255);

	public static Color GreenYellow = new Color(173, 255, 47, 255);

	public static Color Honeydew = new Color(240, 255, 240, 255);

	public static Color HotPink = new Color(255, 105, 180, 255);

	public static Color IndianRed = new Color(205, 92, 92, 255);

	public static Color Indigo = new Color(75, 0, 130, 255);

	public static Color Ivory = new Color(255, 255, 240, 255);

	public static Color Khaki = new Color(240, 230, 140, 255);

	public static Color Lavender = new Color(230, 230, 250, 255);

	public static Color LavenderBlush = new Color(255, 240, 245, 255);

	public static Color LawnGreen = new Color(124, 252, 0, 255);

	public static Color LemonChiffon = new Color(255, 250, 205, 255);

	public static Color LightBlue = new Color(173, 216, 230, 255);

	public static Color LightCoral = new Color(240, 128, 128, 255);

	public static Color LightCyan = new Color(224, 255, 255, 255);

	public static Color LightGoldenrodYellow = new Color(250, 250, 210, 255);

	public static Color LightGray = new Color(211, 211, 211, 255);

	public static Color LightGreen = new Color(144, 238, 144, 255);

	public static Color LightPink = new Color(255, 182, 193, 255);

	public static Color LightSalmon = new Color(255, 160, 122, 255);

	public static Color LightSeaGreen = new Color(32, 178, 170, 255);

	public static Color LightSkyBlue = new Color(135, 206, 250, 255);

	public static Color LightSlateGray = new Color(119, 136, 153, 255);

	public static Color LightSteelBlue = new Color(176, 196, 222, 255);

	public static Color LightYellow = new Color(255, 255, 224, 255);

	public static Color Lime = new Color(0, 255, 0, 255);

	public static Color LimeGreen = new Color(50, 205, 50, 255);

	public static Color Linen = new Color(250, 240, 230, 255);

	public static Color Magenta = new Color(255, 0, 255, 255);

	public static Color Maroon = new Color(128, 0, 0, 255);

	public static Color MediumAquamarine = new Color(102, 205, 170, 255);

	public static Color MediumBlue = new Color(0, 0, 205, 255);

	public static Color MediumOrchid = new Color(186, 85, 211, 255);

	public static Color MediumPurple = new Color(147, 112, 219, 255);

	public static Color MediumSeaGreen = new Color(60, 179, 113, 255);

	public static Color MediumSlateBlue = new Color(123, 104, 238, 255);

	public static Color MediumSpringGreen = new Color(0, 250, 154, 255);

	public static Color MediumTurquoise = new Color(72, 209, 204, 255);

	public static Color MediumVioletRed = new Color(199, 21, 133, 255);

	public static Color MidnightBlue = new Color(25, 25, 112, 255);

	public static Color MintCream = new Color(245, 255, 250, 255);

	public static Color MistyRose = new Color(255, 228, 225, 255);

	public static Color Moccasin = new Color(255, 228, 181, 255);

	public static Color NavajoWhite = new Color(255, 222, 173, 255);

	public static Color OldLace = new Color(253, 245, 230, 255);

	public static Color Olive = new Color(128, 128, 0, 255);

	public static Color OliveDrab = new Color(107, 142, 35, 255);

	public static Color Orange = new Color(255, 165, 0, 255);

	public static Color OrangeRed = new Color(255, 69, 0, 255);

	public static Color Orchid = new Color(218, 112, 214, 255);

	public static Color PaleGoldenrod = new Color(238, 232, 170, 255);

	public static Color PaleGreen = new Color(152, 251, 152, 255);

	public static Color PaleTurquoise = new Color(175, 238, 238, 255);

	public static Color PaleVioletRed = new Color(219, 112, 147, 255);

	public static Color PapayaWhip = new Color(255, 239, 213, 255);

	public static Color PeachPuff = new Color(255, 218, 185, 255);

	public static Color Peru = new Color(205, 133, 63, 255);

	public static Color Pink = new Color(255, 192, 203, 255);

	public static Color Plum = new Color(221, 160, 221, 255);

	public static Color PowderBlue = new Color(176, 224, 230, 255);

	public static Color Purple = new Color(128, 0, 128, 255);

	public static Color Red = new Color(255, 0, 0, 255);

	public static Color RosyBrown = new Color(188, 143, 143, 255);

	public static Color RoyalBlue = new Color(65, 105, 225, 255);

	public static Color SaddleBrown = new Color(139, 69, 19, 255);

	public static Color Salmon = new Color(250, 128, 114, 255);

	public static Color SandyBrown = new Color(244, 164, 96, 255);

	public static Color SeaGreen = new Color(46, 139, 87, 255);

	public static Color SeaShell = new Color(255, 245, 238, 255);

	public static Color Sienna = new Color(160, 82, 45, 255);

	public static Color Silver = new Color(192, 192, 192, 255);

	public static Color SkyBlue = new Color(135, 206, 235, 255);

	public static Color SlateBlue = new Color(106, 90, 205, 255);

	public static Color SlateGray = new Color(112, 128, 144, 255);

	public static Color Snow = new Color(255, 250, 250, 255);

	public static Color SpringGreen = new Color(0, 255, 127, 255);

	public static Color SteelBlue = new Color(70, 130, 180, 255);

	public static Color Tan = new Color(210, 180, 140, 255);

	public static Color Teal = new Color(0, 128, 128, 255);

	public static Color Thistle = new Color(216, 191, 216, 255);

	public static Color Tomato = new Color(255, 99, 71, 255);

	public static Color Transparent = new Color(0, 0, 0, 0);

	public static Color Turquoise = new Color(64, 224, 208, 255);

	public static Color Violet = new Color(238, 130, 238, 255);

	public static Color Wheat = new Color(245, 222, 179, 255);

	public static Color White = new Color(255, 255, 255, 255);

	public static Color WhiteSmoke = new Color(245, 245, 245, 255);

	public static Color Yellow = new Color(255, 255, 0, 255);

	public static Color YellowGreen = new Color(154, 205, 50, 255);

	public static readonly List<Color> RainbowColors = new List<Color>
	{
		new Color(163, 206, 39),
		new Color(247, 224, 90),
		new Color(235, 137, 49),
		new Color(192, 32, 45),
		new Color(237, 94, 238),
		new Color(138, 38, 190),
		new Color(49, 162, 242)
	};

	public Color(byte r, byte g, byte b, byte a)
	{
		this = default(Color);
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Color(byte r, byte g, byte b)
		: this(r, g, b, byte.MaxValue)
	{
	}

	public Color(int r, int g, int b, int a)
		: this((byte)MathHelper.Clamp(r, 0, 255), (byte)MathHelper.Clamp(g, 0, 255), (byte)MathHelper.Clamp(b, 0, 255), (byte)MathHelper.Clamp(a, 0, 255))
	{
	}

	public Color(int r, int g, int b)
		: this(r, g, b, 255)
	{
	}

	public Color(float r, float g, float b, float a)
		: this((byte)(MathHelper.Clamp(r, 0f, 1f) * 255f), (byte)(MathHelper.Clamp(g, 0f, 1f) * 255f), (byte)(MathHelper.Clamp(b, 0f, 1f) * 255f), (byte)(MathHelper.Clamp(a, 0f, 1f) * 255f))
	{
	}

	public Color(float r, float g, float b)
		: this(r, g, b, 1f)
	{
	}

	public Color(uint hex)
		: this((byte)(hex & 0xFF), (byte)((hex << 8) & 0xFF), (byte)((hex << 16) & 0xFF), (byte)((hex << 24) & 0xFF))
	{
	}

	public Color(uint hex, bool newHexSystem)
	{
		this = default(Color);
		if (hex > 16777215)
		{
			r = (byte)((hex >> 24) & 0xFF);
			g = (byte)((hex >> 16) & 0xFF);
			b = (byte)((hex >> 8) & 0xFF);
			a = (byte)(hex & 0xFF);
		}
		else
		{
			r = (byte)((hex >> 16) & 0xFF);
			g = (byte)((hex >> 8) & 0xFF);
			b = (byte)(hex & 0xFF);
			a = byte.MaxValue;
		}
	}

	public static Color FromHexString(string pString)
	{
		try
		{
			if (pString.StartsWith("#"))
			{
				pString = pString.Substring(1, pString.Length - 1);
			}
			else if (pString.StartsWith("0x"))
			{
				pString = pString.Substring(2, pString.Length - 2);
			}
			uint newValue = Convert.ToUInt32("0x" + pString, 16);
			return new Color(newValue, newHexSystem: true);
		}
		catch (Exception)
		{
			return White;
		}
	}

	public static explicit operator int(Color color)
	{
		return color.r | (color.g >> 8) | (color.b >> 16) | (color.a >> 24);
	}

	public static explicit operator Color(uint hex)
	{
		return new Color(hex);
	}

	public static Color operator *(Color c, float r)
	{
		return new Color((byte)MathHelper.Clamp((float)(int)c.r * r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.g * r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.b * r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.a * r, 0f, 255f));
	}

	public static Color operator /(Color c, float r)
	{
		return new Color((byte)MathHelper.Clamp((float)(int)c.r / r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.g / r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.b / r, 0f, 255f), (byte)MathHelper.Clamp((float)(int)c.a / r, 0f, 255f));
	}

	public static bool operator ==(Color l, Color r)
	{
		return l.Equals(r);
	}

	public static bool operator !=(Color l, Color r)
	{
		return !l.Equals(r);
	}

	public bool Equals(Color other)
	{
		if (r == other.r && g == other.g && b == other.b)
		{
			return a == other.a;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Color)
		{
			return Equals((Color)obj);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return (int)this;
	}

	public override string ToString()
	{
		return $"{r} {g} {b} {a}";
	}

	public string ToDGColorString()
	{
		return $"|{r},{g},{b}|";
	}

	public static Color Lerp(Color a, Color b, float v)
	{
		return DuckGame.Lerp.ColorSmooth(a, b, v);
	}

	public Color(Vec4 vec)
		: this(vec.x, vec.y, vec.z, vec.w)
	{
	}

	public Color(Vec3 vec)
		: this(vec.x, vec.y, vec.z)
	{
	}

	public Vec4 ToVector4()
	{
		return new Vec4((float)(int)r / 255f, (float)(int)g / 255f, (float)(int)b / 255f, (float)(int)a / 255f);
	}

	public Vec3 ToVector3()
	{
		return new Vec3((float)(int)r / 255f, (float)(int)g / 255f, (float)(int)b / 255f);
	}

	public static implicit operator Microsoft.Xna.Framework.Color(Color c)
	{
		return new Microsoft.Xna.Framework.Color(c.r, c.g, c.b, c.a);
	}

	public static implicit operator Color(Microsoft.Xna.Framework.Color c)
	{
		return new Color(c.R, c.G, c.B, c.A);
	}
}
