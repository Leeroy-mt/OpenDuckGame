using DuckGame;
using System;
using System.Linq;
using System.Threading.Tasks;

internal class Startup(MonoMain main)
{
    class TaskInfo(Action action, bool async)
    {
        public volatile bool IsCompleted;

        public bool IsAsync { get; } = async;

        public Action Action { get; } = action;
    }

    public event Action<string> Output;

    readonly TaskInfo[] Tasks = [
            new(Persona.Initialize, false),
            new(ManagedContent.PreInitializeMods, false),
            new(Network.Initialize, false),
            new(Teams.Initialize, false),
            new(Chancy.Initialize, false),
            new(DuckNetwork.Initialize, false),
            new(DuckRig.Initialize, false),
            new(Input.Initialize, false),
            new(main.DownloadWorkshopItems, false),
            new(ManagedContent.InitializeMods, false),
            new(Network.InitializeMessageTypes, false),
            new(DeathCrate.InitializeDeathCrateSettings, false),
            new(Editor.InitializeConstructorLists, false),
            new(Team.DeserializeCustomHats, false),
            new(Content.InitializeLevels, false),
            new(Content.InitializeEffects, false),
            new(Input.InitializeGraphics, false),
            new(Music.Initialize, false),
            new(DevConsole.InitializeFont, false),
            new(DevConsole.InitializeCommands, false),
            new(Editor.InitializePlaceableGroup, false),
            new(Challenges.Initialize, false),
            new(Keyboard.InitTriggerImages, false),
            new(MapPack.RegeneratePreviewsIfNecessary, false),
            new(SFX.Initialize, false),
            new(Content.Initialize, false),
            new(main.SetStarted, false)
        ];

    public async void Start()
    {
        var waiting = false;
        for (int i = 0; i < Tasks.Length;)
        {
            var task = Tasks[i];
            if (!waiting)
                if (task.IsAsync)
                {
                    var method = task.Action.Method;
                    Output?.Invoke($"{method.DeclaringType.Name}.{method.Name}");
                    waiting = true;
                    await Task.Run(() =>
                    {
                        task.Action();
                        task.IsCompleted = true;
                    });
                }
                else
                {
                    var method = task.Action.Method;
                    Output?.Invoke($"{method.DeclaringType.Name}.{method.Name}");
                    waiting = true;
                    task.Action();
                    task.IsCompleted = true;
                }

            if (task.IsCompleted || true)
            {
                waiting = false;
                i++;
            }
        }
    }
}