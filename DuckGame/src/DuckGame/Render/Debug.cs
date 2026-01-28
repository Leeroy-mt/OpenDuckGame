using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace DuckGame;

public static class Debug
{
    private static Texture2D _blank;

    [Conditional("DEBUG")]
    public static void Initialize()
    {
        _blank = new Texture2D(Graphics.device, 1, 1, mipMap: false, SurfaceFormat.Color);
        _blank.SetData(new Color[1] { Color.White });
    }

    [Conditional("DEBUG")]
    public static void DrawLine(Vec2 p1, Vec2 p2, Color col, float width = 1f)
    {
        p1 = new Vec2((int)p1.X, (int)p1.Y);
        p2 = new Vec2((int)p2.X, (int)p2.Y);
        float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        float length = (p1 - p2).Length();
        Graphics.Draw(_blank, p1, null, col, angle, Vec2.Zero, new Vec2(length, width), SpriteEffects.None, 1f);
    }

    [Conditional("DEBUG")]
    public static void DrawRect(Vec2 p1, Vec2 p2, Color col)
    {
        Graphics.Draw(_blank, p1, null, col, 0f, Vec2.Zero, new Vec2(0f - (p1.X - p2.X), 0f - (p1.Y - p2.Y)), SpriteEffects.None, 1f);
    }

    [Conditional("DEBUG")]
    [Conditional("SWITCH")]
    public static void Assert(bool cond, string fmt, params object[] vals)
    {
        if (!cond)
        {
            DevConsole.Log(DCSection.General, Verbosity.Very, string.Format(fmt, vals));
            Debugger.Break();
        }
    }
}
