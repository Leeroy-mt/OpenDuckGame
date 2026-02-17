using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace DuckGame;

internal static class TextureConverter
{
    const int Replace = -65281;

    const int Destination = 0;

    public static bool LastLoadResultedInResize { get; private set; }

    #region Public Methods

    public static Stream TextureResize(
        Texture2D texture,
        float xBound,
        float yBound
        )
    {
        LastLoadResultedInResize = false;

        float scale = Math.Min(xBound / texture.Width, yBound / texture.Height);
        MemoryStream memory = new();

        if (xBound < texture.Width || yBound < texture.Height)
        {
            LastLoadResultedInResize = true;

            if (texture.Width * scale < xBound)
                xBound = texture.Width * scale;
            if (texture.Height * scale < yBound)
                yBound = texture.Height * scale;

            texture.SaveAsPng(memory, (int)xBound, (int)yBound);
            texture.Dispose();

            return memory;
        }

        texture.SaveAsPng(memory, texture.Width, texture.Height);
        texture.Dispose();

        return memory;
    }

    public static PNGData PNGDataFromStream(
        Stream stream,
        bool replace
        )
    {
        Texture2D.TextureDataFromStreamEXT(stream, out var width, out var height, out var data, -1, -1, false);
        var intData = new int[data.Length / 4];
        for (int i = 0; i < intData.Length; i++)
        {
            var c = BitConverter.ToInt32(data, i * 4);
            intData[i] = replace && c == Replace ? Destination : c;
        }
        return new()
        {
            width = width,
            height = height,
            data = intData
        };
    }

    public static PNGData PNGDataFromTexture(
        Texture2D texture,
        float xBound,
        float yBound,
        bool replace
        )
    {
        var bounded = TextureResize(texture, xBound, yBound);
        var replaced = PNGDataFromStream(bounded, replace);
        return replaced;
    }

    public static Texture2D TextureFromPNGData(
        GraphicsDevice device,
        PNGData data
        )
    {
        Texture2D texture = new(device, data.width, data.height);
        texture.SetData(data.data);
        return texture;
    }

    public static Texture2D TextureFromStream(
        GraphicsDevice device,
        Stream stream,
        bool replace
        )
    {
        var data = PNGDataFromStream(stream, replace);
        var texture = TextureFromPNGData(device, data);
        return texture;
    }

    public static Texture2D TextureFromFileName(
        GraphicsDevice device,
        string fileName,
        int wBound,
        int hBound,
        bool replace
        )
    {
        using FileStream fileStream = new(fileName, FileMode.Open);

        using var tex = Texture2D.FromStream(device, fileStream);
        var data = PNGDataFromTexture(tex, wBound, hBound, replace);

        Texture2D texture = new(device, data.width, data.height);
        texture.SetData(data.data);
        return texture;
    }

    public static Texture2D TextureFromFileName(
        GraphicsDevice device,
        string fileName,
        bool replace
        )
    {
        using FileStream fileStream = new(fileName, FileMode.Open);

        var data = PNGDataFromStream(fileStream, replace);
        Texture2D texture = new(device, data.width, data.height);
        texture.SetData(data.data);

        return texture;
    }

    #endregion
}

