using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DuckGame;

public class DuckRunningScreen : ILoadingScreen
{
    public bool LoadingStarted { get; private set; }

    public bool IsLoaded { get; private set; }

    int Frames;

    float Progress;

    float Indent = 0;

    SpriteMap DuckRun, DuckArm;

    Startup Startup;

    readonly List<string> LoadMessages = [];

    public DuckRunningScreen(MonoMain main)
    {
        Startup = new(main);

        DuckRun = new("duck", 32, 32);
        DuckRun.AddAnimation("run", 1, true, 1, 2, 3, 4, 5, 6);
        DuckRun.SetAnimation("run");
        DuckArm = new("duckArms", 16, 16);

        Startup.Log += LoadMessages.Add;
    }

    public void Start()
    {
        Startup.RunMainTasks();
        Startup.RunAsyncTasks();

        LoadingStarted = true;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {
        Frames++;
        Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        //var velocity = (50 - Indent) * 0.25f;
        //Indent += velocity;
        //if (velocity < .1f)
        //    Indent = 50;
        Indent = Easing.OutExpo((float)double.Min(gameTime.TotalGameTime.TotalSeconds * 2 - 1, 1)) * 50;
        Vec2 loadBarSize = new(Graphics.width - Indent * 2, 20);
        Vec2 loadBarPos = new(Indent, Graphics.height - Indent - loadBarSize.Y);
        Vec2 loadBarSize2 = new(4);
        Graphics.DrawRect(loadBarPos, loadBarPos + loadBarSize, Color.White, 0.5f, false);

        var actualProgress = Startup?.CalculateProgress() ?? 0;
        Progress += (actualProgress - Progress) * 0.1f;
        Graphics.DrawRect(loadBarPos + loadBarSize2, loadBarPos + new Vec2(loadBarSize.X * Progress, loadBarSize.Y) - loadBarSize2, Color.White, 0.6f);

        for (int i = 0; i < LoadMessages.Count; i++)
            Graphics.DrawString(LoadMessages[LoadMessages.Count - i - 1], loadBarPos + new Vec2(0, -24 - i * 24), Color.White, 1, null, 2);

        DuckRun.speed = 0.15f;
        DuckRun.Scale = new Vec2(4);
        DuckRun.Depth = 0.7f;
        DuckRun.color = Color.Lerp(Color.White, Color.Gray, float.Sin(Frames / 10f) / 2 + .5f);
        DuckRun.frame = (int)(Frames / 5f) % 6;
        Vec2 duckPos = new(Graphics.width - DuckRun.width * 4 - loadBarPos.X, loadBarPos.Y - DuckRun.height * 4);
        Graphics.Draw(DuckRun, duckPos.X, duckPos.Y);

        DuckArm.Scale = new Vec2(4);
        DuckArm.Depth = 0.6f;
        DuckArm.color = DuckRun.color;
        DuckArm.frame = DuckRun.imageIndex;
        Graphics.Draw(DuckArm, duckPos.X + 20, duckPos.Y + 56);

        Graphics.screen.End();
    }
}
