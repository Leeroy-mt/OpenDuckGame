using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DuckGame;

public class TVSpinScreen
{
    public bool LoadingStarted { get; private set; }

    int Frames;

    float Progress;

    float Indent;

    Color Gradient => Color.Gradient(
                Maths.Spikes(Frames / 120f),
                Color.FromHexString("ffffff"),
                Color.FromHexString("7d7d7d"),
                Color.FromHexString("f7e05a"),
                Color.FromHexString("cd6b1d"),
                Color.FromHexString("00854a"),
                Color.FromHexString("ff6975"),
                Color.FromHexString("31a2f2"),
                Color.FromHexString("af55dd")
                );

    SpriteMap Noise;

    Sprite Frame;

    Startup Startup;

    readonly List<string> LoadMessages = [];

    public TVSpinScreen(MonoMain main)
    {
        Startup = new(main);

        Startup.Output += s =>
        {
            Console.WriteLine(s);

            LoadMessages.Add($"{Gradient.ToDGColorString()}{s}");
        };

        Frame = new Sprite("tv2");
        Frame.CenterOrigin();

        Noise = new SpriteMap("tvnoise", 8, 6);
        Noise.AddAnimation("noise", 0.6f, true, 0, 1, 2);
        Noise.currentAnimation = "noise";
        Noise.Center = new(4, 4);
    }

    public void Start()
    {
        Startup.Start();

        LoadingStarted = true;
    }

    public void Update(GameTime gameTime)
    {
        if (LoadMessages.Count > 100)
            LoadMessages.RemoveAt(0);
    }

    public void Draw(GameTime gameTime)
    {
        Frames++;
        Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        Indent = Easing.OutExpo((float)double.Min(gameTime.TotalGameTime.TotalSeconds * 2 - 1, 1)) * 50;
        Vector2 loadBarSize = new(Graphics.width - Indent * 2, 20);
        Vector2 loadBarPos = new(Indent, Graphics.height - Indent - loadBarSize.Y);
        Vector2 loadBarSize2 = new(4);
        Graphics.DrawRect(loadBarPos, loadBarPos + loadBarSize, Color.White, 0.5f, false);

        var actualProgress = LoadMessages.Count / 30f;
        Progress += (actualProgress - Progress) * 0.1f;
        Graphics.DrawRect(loadBarPos + loadBarSize2, loadBarPos + new Vector2(loadBarSize.X * Progress, loadBarSize.Y) - loadBarSize2, Gradient, 0.6f);

        for (int i = 0; i < LoadMessages.Count; i++)
            Graphics.DrawString(LoadMessages[LoadMessages.Count - i - 1], loadBarPos + new Vector2(0, -24 - i * 24), Color.White, 1, null, 2);

        Vector2 loadpos = new(Graphics.width - Frame.width * 4 - loadBarPos.X, loadBarPos.Y - Frame.height * 4);
        var scale = (float.Sin(Frames / 200f) / 2 + .5f) * 3 + 3;
        Vector2 offset = new(float.Cos(Frames / 300f) * 32, float.Sin(Frames / 300f) * 32);

        Noise.Scale = new(scale);
        Noise.AngleDegrees = Frames / 3f;
        Noise.Position = loadpos + offset;
        Noise.color = Color.Lerp(
            Gradient,
            Color.Black,
            float.Abs(float.Min(float.Cos(Frames / 22f) * 50, float.Sin(Frames / 77f) * 50)) % 1
            );
        Noise.Draw();

        Frame.Scale = new(scale);
        Frame.AngleDegrees = Frames / 3f;
        Frame.Position = loadpos + offset;
        Frame.Draw();

        Graphics.screen.End();
    }
}
