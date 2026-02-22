using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DuckGame;

internal class FontContext
{
    #region Private Fields

    static FontCollection ContextFonts = new();

    #endregion

    public static Dictionary<string, RasterFont.Data> _fontDatas = [];

    static FontContext()
    {
        FontCollectionExtensions.AddSystemFonts(ContextFonts);
    }

    #region Public Methods

    public static string GetName(string fontFamilyName)
    {
        fontFamilyName = fontFamilyName
            .Replace("@BOLD@", "")
            .Replace("@ITALIC@", "");

        if (ContextFonts.TryGet(fontFamilyName, out var font))
            foreach (var style in font.GetAvailableStyles())
            {
                return style switch
                {
                    FontStyle.Regular
                    or FontStyle.Bold
                    or FontStyle.Italic
                    or FontStyle.BoldItalic => font.Name,
                    _ => null,
                };
            }

        return null;
    }

    // TODO: улучшить внешний вид шрифта
    public static RasterFont.Data CreateFontData(
        string familyName,
        float size = 12,
        FontStyle style = FontStyle.Regular,
        bool smooth = true
        )
    {
        if (familyName.Contains("@BOLD@"))
        {
            style = FontStyle.Bold;
            familyName = familyName.Replace("@BOLD@", "");
        }
        if (familyName.Contains("@ITALIC@"))
        {
            style = FontStyle.Italic;
            familyName = familyName.Replace("@ITALIC@", "");
        }
        string dataString = familyName + size + style;
        if (_fontDatas.TryGetValue(dataString, out var data))
            return data;
        if (_fontDatas.Count > 8)
            _fontDatas.Clear();

        if (!ContextFonts.TryGet(familyName, out var fontFamily))
            return null;

        size *= RasterFont.fontScaleFactor;

        Font font = new(fontFamily, size, style);
        RichTextOptions textOptions = new(font);

        var fontBounds = TextMeasurer.MeasureBounds(FancyBitmapFont._characters.AsSpan(), textOptions);
        var imageSize = float.Sqrt(FancyBitmapFont._characters.Length) * (font.FontMetrics.VerticalMetrics.LineHeight / font.FontMetrics.UnitsPerEm * size + 8);
        imageSize = float.Min(imageSize, MonoMain.hidef ? 4096 : 2048);
        var image = new Image<Rgba32>((int)imageSize, (int)imageSize);

        data = new()
        {
            fontSize = size,
            name = font.Name
        };

        image.Mutate(p =>
        {
            /* Settings */
            DrawingOptions drawingOptions = new();
            drawingOptions.GraphicsOptions.Antialias = smooth;

            /* Loop Variables */
            var indent = 2;
            var chars = FancyBitmapFont._characters;
            Vector2 boundsAdjust = new(2);
            Vector2 position = new(indent);
            var stroke = "";

            /* Draw Loop */
            for (int i = 0; i < chars.Length; i++)
            {
                var text = chars[i].ToString();
                stroke += text;

                var bounds = TextMeasurer.MeasureBounds(text, textOptions)
                                         .Inflate(boundsAdjust);

                var addWidth = bounds.Width + indent;
                if ((position.X + addWidth) >= image.Width)
                {
                    var strokeBounds = TextMeasurer.MeasureBounds(stroke, textOptions)
                                                   .Inflate(boundsAdjust);

                    position.X = indent;
                    position.Y += (fontBounds.Height + boundsAdjust.Y * 2) + indent;
                    stroke = "";
                }

                data.fontHeight = float.Max(data.fontHeight, bounds.Height);
                data.characters.Add(new()
                {
                    area = new(position.X, position.Y, bounds.Width, (fontBounds.Height + boundsAdjust.Y * 2)),
                    width = bounds.Width
                });
                p.DrawText(text, font, Color.White, new(position.X - bounds.X, position.Y - fontBounds.Y + boundsAdjust.Y));

                position.X += addWidth;
            }
        });

        Span<Rgba32> colors = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(colors);
        data.colorsWidth = image.Width;
        data.colorsHeight = image.Height;
        data.colors = new uint[image.Width * image.Height];
        for (int i = 0; i < colors.Length; i++)
            data.colors[i] =
                (uint)(colors[i].A << 24
                | colors[i].B << 16
                | colors[i].G << 8
                | colors[i].R);

        _fontDatas.Add(dataString, data);
        return data;
    }

    #endregion
}
