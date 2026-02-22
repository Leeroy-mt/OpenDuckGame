using DuckGame;
using System;
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
            new(ManagedContent.PreInitializeMods, true),
            new(Content.InitializeTextureSizeDictionary, true),
            new(Network.Initialize, true),
            new(Teams.Initialize, true),
            new(Chancy.Initialize, true),
            new(DuckNetwork.Initialize, true),
            new(DuckRig.Initialize, true),
            new(Input.Initialize, true),
            new(main.DownloadWorkshopItems, true),
            new(ManagedContent.InitializeMods, true),
            new(Network.InitializeMessageTypes, true),
            new(DeathCrate.InitializeDeathCrateSettings, true),
            new(Editor.InitializeConstructorLists, true),
            new(Team.DeserializeCustomHats, true),
            new(Content.InitializeLevels, true),
            new(Content.InitializeEffects, true),
            new(Input.InitializeGraphics, true),
            new(Music.Initialize, true),
            //new(DevConsole.InitializeFont, true),
            //new(DevConsole.InitializeCommands, true),
            new(Editor.InitializePlaceableGroup, true),
            //new(Challenges.Initialize, true),
            //new(Collision.Initialize, true),
            //new(Level.InitializeCollisionLists, true),
            new(Keyboard.InitTriggerImages, true),
            //new(MapPack.RegeneratePreviewsIfNecessary, true),
            new(SFX.Initialize, true),
            //new(Content.Initialize, true),
            new(main.SetStarted, true)
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