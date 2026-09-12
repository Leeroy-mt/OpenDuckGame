using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DuckGame;

public class LoadingScreen
{
    IGameLoader startup;

    MonoMain main;

    IEnumerable<ILoaderTask> loaderTasks;

    public bool LoadingStarted { get; private set; }

    public LoadingScreen(MonoMain main)
    {
        this.main = main;

        GameLoaderImpl gameLoader = new(main);
        loaderTasks = gameLoader.GetTasks();

        startup = gameLoader;
    }

    public void Start()
    {
        LoadingStarted = true;
    }

    public void Update(GameTime gameTime)
    {
        startup.Update();

        if (startup.IsCompleted)
            main.SetStarted();
    }

    public void Draw(GameTime gameTime)
    {
        Graphics.screen.Begin(
            SpriteSortMode.FrontToBack,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Matrix.Identity
        );

        var y = 16;
        foreach (var task in loaderTasks)
        {
            Graphics.DrawString(
                $"{task.Name}: {(task.IsCompleted ? "|DGGREEN|OK" : $"|DGBLUE|Loading... ({task.CompletionProgress * 100}%)")}",
                new(16, y),
                Color.White,
                default,
                null,
                2
            );
            y += 24;
        }

        Graphics.screen.End();
    }
}
