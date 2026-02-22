using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public static class DGColor
{
    static Color[] RainbowColors =
        [
            new Color(163, 206, 39),
            new Color(247, 224, 90),
            new Color(235, 137, 49),
            new Color(192, 32, 45),
            new Color(237, 94, 238),
            new Color(138, 38, 190),
            new Color(49, 162, 242)
        ];

    extension(Color color)
    {
        public static Color[] RainbowColors => RainbowColors;

        public string ToDGColorString()
        {
            return $"|{color.R},{color.G},{color.B}|";
        }

        public static Color Gradient(float t, params Color[] colors)
        {
            if (colors.Length is 0) return default;
            if (colors.Length is 1 || t <= 0) return colors[0];
            if (t >= 1) return colors[^1];

            var T = t * (colors.Length - 1);
            return Color.Lerp(
                colors[(int)float.Floor(T)],
                colors[(int)float.Ceiling(T)],
                T % 1
                );
        }

        public static Color FromHexString(string pString)
        {
            try
            {
                if (pString.StartsWith('#'))
                    pString = pString[1..];
                else if (pString.StartsWith("0x"))
                    pString = pString[2..];

                uint newValue = Convert.ToUInt32("0x" + pString, 16);

                Color result = default;
                if (newValue > 16777215)
                {
                    result.R = (byte)((newValue >> 24) & 0xFF);
                    result.G = (byte)((newValue >> 16) & 0xFF);
                    result.B = (byte)((newValue >> 8) & 0xFF);
                    result.A = (byte)(newValue & 0xFF);
                }
                else
                {
                    result.R = (byte)((newValue >> 16) & 0xFF);
                    result.G = (byte)((newValue >> 8) & 0xFF);
                    result.B = (byte)(newValue & 0xFF);
                    result.A = byte.MaxValue;
                }
                return result;
            }
            catch
            {
                return Color.White;
            }
        }

        public static Color operator *(Color left, Color right)
        {
            return new(
                left.R * right.R,
                left.G * right.G,
                left.B * right.B,
                left.A * right.A
                );
        }
    }
}