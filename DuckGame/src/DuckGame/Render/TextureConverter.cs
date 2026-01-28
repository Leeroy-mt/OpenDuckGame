using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DuckGame;

internal static class TextureConverter
{
    private const int _fromColor = -65281;

    private const int _toColor = 0;

    public static bool lastLoadResultedInResize = false;

    private static Vec2 _maxDimensions = Vec2.Zero;

    internal unsafe static PNGData LoadPNGDataWithPinkAwesomeness(Bitmap bitmap, bool process)
    {
        lastLoadResultedInResize = false;
        if (_maxDimensions != Vec2.Zero)
        {
            float width = _maxDimensions.X;
            float height = _maxDimensions.Y;
            float scale = Math.Min(width / (float)bitmap.Width, height / (float)bitmap.Height);
            if (width < (float)bitmap.Width || height < (float)bitmap.Height)
            {
                lastLoadResultedInResize = true;
                if ((float)bitmap.Width * scale < width)
                {
                    width = (float)bitmap.Width * scale;
                }
                if ((float)bitmap.Height * scale < height)
                {
                    height = (float)bitmap.Height * scale;
                }
                Bitmap bitmap2 = new Bitmap((int)width, (int)height);
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap2);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                _ = bitmap.Width;
                _ = bitmap.Height;
                System.Drawing.Rectangle destination = new System.Drawing.Rectangle(0, 0, (int)width, (int)height);
                graphics.DrawImage(bitmap, destination, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
                bitmap.Dispose();
                graphics.Dispose();
                bitmap = bitmap2;
            }
        }
        BitmapData locked = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int pt = locked.Width * locked.Height;
        int px = 0;
        int* pixels = (int*)(void*)locked.Scan0;
        while (px < pt)
        {
            if (process && *pixels == -65281)
            {
                *pixels = 0;
            }
            else
            {
                byte* rgba = (byte*)pixels;
                byte r = *rgba;
                *rgba = rgba[2];
                rgba[2] = r;
                float a = (float)(int)rgba[3] / 255f;
                for (int i = 0; i < 3; i++)
                {
                    rgba[i] = (byte)((float)(int)rgba[i] * a);
                }
            }
            px++;
            pixels++;
        }
        int[] copy = new int[locked.Width * locked.Height];
        Marshal.Copy(locked.Scan0, copy, 0, copy.Length);
        PNGData result = new PNGData
        {
            data = copy,
            width = locked.Width,
            height = locked.Height
        };
        bitmap.UnlockBits(locked);
        return result;
    }

    internal static Texture2D LoadPNGWithPinkAwesomeness(GraphicsDevice device, Bitmap bitmap, bool process)
    {
        PNGData dat = LoadPNGDataWithPinkAwesomeness(bitmap, process);
        Texture2D texture2D = new Texture2D(device, dat.width, dat.height);
        texture2D.SetData(dat.data);
        return texture2D;
    }

    internal static Texture2D LoadPNGWithPinkAwesomenessAndMaxDimensions(GraphicsDevice device, Bitmap bitmap, bool process, Vec2 pMaxDimensions)
    {
        _maxDimensions = pMaxDimensions;
        PNGData dat = LoadPNGDataWithPinkAwesomeness(bitmap, process);
        _maxDimensions = Vec2.Zero;
        Texture2D texture2D = new Texture2D(device, dat.width, dat.height);
        texture2D.SetData(dat.data);
        return texture2D;
    }

    internal static Texture2D LoadPNGWithPinkAwesomeness(GraphicsDevice device, Stream stream, bool process)
    {
        using Bitmap bmp = new Bitmap(stream);
        return LoadPNGWithPinkAwesomeness(device, bmp, process);
    }

    internal static PNGData LoadPNGDataWithPinkAwesomeness(Stream stream, bool process)
    {
        using Bitmap bmp = new Bitmap(stream);
        return LoadPNGDataWithPinkAwesomeness(bmp, process);
    }

    internal static Texture2D LoadPNGWithPinkAwesomeness(GraphicsDevice device, string fileName, bool process)
    {
        using Bitmap bmp = new Bitmap(fileName);
        return LoadPNGWithPinkAwesomeness(device, bmp, process);
    }

    internal static Texture2D LoadPNGWithPinkAwesomenessAndMaxDimensions(GraphicsDevice device, string fileName, bool process, Vec2 maxDimensions)
    {
        using Bitmap bmp = new Bitmap(fileName);
        return LoadPNGWithPinkAwesomenessAndMaxDimensions(device, bmp, process, maxDimensions);
    }

    internal static PNGData LoadPNGDataWithPinkAwesomeness(GraphicsDevice device, string fileName, bool process)
    {
        using Bitmap bmp = new Bitmap(fileName);
        return LoadPNGDataWithPinkAwesomeness(bmp, process);
    }
}
