using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

internal class Startup(MonoMain main)
{
    public event Action<string> Log;

    Thread AsyncThread;

    List<StartupAction> Actions = [
        new(Persona.Initialize, TaskThread.Main) { LoadingMessage = "Persona..." },
        new(ManagedContent.PreInitializeMods, TaskThread.Async) { LoadingMessage = "PreInitializeMods..." },
        new(Content.InitializeTextureSizeDictionary, TaskThread.Async) { LoadingMessage = "Texture Sizes..." },
        new(Network.Initialize, TaskThread.Async) { LoadingMessage = "Network..." },
        new(Teams.Initialize, TaskThread.Async) { LoadingMessage = "Teams..." },
        new(Chancy.Initialize, TaskThread.Async) { LoadingMessage = "Some RNG Magic..." },
        new(DuckNetwork.Initialize, TaskThread.Async) { LoadingMessage = "DuckNetwork..." },
        //new(Persona.Initialize, TaskThread.Async) { LoadingMessage = "Persona..." },
        new(DuckRig.Initialize, TaskThread.Async) { LoadingMessage = "DuckRig..." },
        new(Input.Initialize, TaskThread.Async) { LoadingMessage = "Input..." },
        new(main.DownloadWorkshopItems, TaskThread.Async) { LoadingMessage = "Workshop..." },
        new(ManagedContent.InitializeMods, TaskThread.Async) { LoadingMessage = "Mods..." },
        new(Network.InitializeMessageTypes, TaskThread.Async) { LoadingMessage = "Message Types..." },
        new(DeathCrate.InitializeDeathCrateSettings, TaskThread.Async) { LoadingMessage = "DeathCrates..." },
        new(Editor.InitializeConstructorLists, TaskThread.Async) { LoadingMessage = "Constructors..." },
        new(Team.DeserializeCustomHats, TaskThread.Async) { LoadingMessage = "Custom Hats..." },
        new(Content.InitializeLevels, TaskThread.Async) { LoadingMessage = "Levels..." },
        new(Content.InitializeEffects, TaskThread.Async) { LoadingMessage = "Effects..." },
        new(Input.InitializeGraphics, TaskThread.Async) { LoadingMessage = "Graphics..." },
        new(Music.Initialize, TaskThread.Async) { LoadingMessage = "Music..." },
        new(DevConsole.InitializeFont, TaskThread.Async) { LoadingMessage = "Console Font..." },
        new(DevConsole.InitializeCommands, TaskThread.Async) { LoadingMessage = "Console Commands..." },
        new(Editor.InitializePlaceableGroup, TaskThread.Async) { LoadingMessage = "Editor Groups..." },
        new(Challenges.Initialize, TaskThread.Async) { LoadingMessage = "Challanges..." },
        new(Collision.Initialize, TaskThread.Async){ LoadingMessage = "Collision..." },
        new(Level.InitializeCollisionLists, TaskThread.Async) { LoadingMessage = "Collision Lists..." },
        new(Keyboard.InitTriggerImages, TaskThread.Async) { LoadingMessage = "Input Images..." },
        new(MapPack.RegeneratePreviewsIfNecessary, TaskThread.Async) { LoadingMessage = "MapPack Previews..." },
        //new(main.StartLazyLoad, TaskThread.Async) { LoadingMessage = "Starting Lazy Load" },
        new(SFX.Initialize, TaskThread.Async) { LoadingMessage = "SFX..." },
        new(Content.Initialize, TaskThread.Async) { LoadingMessage = "Content..." },
        new(main.SetStarted, TaskThread.Async) { LoadingMessage = "Starting!" }
        ];

    public float CalculateProgress()
    {
        var finished = from Task in Actions
                       where !Task.IsActive
                       select Task;
        return finished.Count() / (float)Actions.Count;
    }

    public void RunMainTasks()
    {
        Queue<StartupAction> main =
            new(from Task in Actions
                where Task.Thread is 0 && Task.IsActive
                select Task);

        BindTask(main);

        while (main.Count > 0)
        {
            var task = main.Peek();
            if (task.Run())
                main.Dequeue();
        }
    }

    public void RunAsyncTasks()
    {
        AsyncThread = new(AsyncTasks);
        AsyncThread.Start();
    }

    void AsyncTasks()
    {
        Queue<StartupAction> async =
            new(from Task in Actions
                where Task.Thread is TaskThread.Async && Task.IsActive
                select Task);

        BindTask(async);

        while (async.Count > 0)
        {
            var task = async.Peek();
            if (task.Run())
                async.Dequeue();
        }
    }

    void BindTask(IEnumerable<StartupAction> tasks)
    {
        foreach (var item in tasks)
        {
            item.LoadStarted += Log;
        }
    }
}