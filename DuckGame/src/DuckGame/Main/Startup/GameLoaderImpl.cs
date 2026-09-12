using System;
using System.Collections.Generic;
using System.Threading;

namespace DuckGame;

internal class GameLoaderImpl : IGameLoader
{
    int currentTaskIndex;

    List<string> completedTasks;

    List<ILoaderTask> tasks;

    public bool IsCompleted { get; private set; }

    public GameLoaderImpl(MonoMain main)
    {
        completedTasks = [];
        tasks = [
            new SyncLoaderTask(Persona.Initialize, this),

            new AsyncLoaderTask(Network.Initialize, this),
            new AsyncLoaderTask(Teams.Initialize, this),
            new AsyncLoaderTask(Chancy.Initialize, this),
            new AsyncLoaderTask(DuckNetwork.Initialize, this),
            new AsyncLoaderTask(DuckRig.Initialize, this),
            new AsyncLoaderTask(Input.Initialize, this),
            new AsyncLoaderTask(Team.DeserializeCustomHats, this),
            new AsyncLoaderTask(Content.InitializeEffects, this),
            new AsyncLoaderTask(Input.InitializeGraphics, this),
            new AsyncLoaderTask(Music.Initialize, this),
            new AsyncLoaderTask(DevConsole.InitializeFont, this),
            new AsyncLoaderTask(DevConsole.InitializeCommands, this),
            new AsyncLoaderTask(Keyboard.InitTriggerImages, this),
            new AsyncLoaderTask(MapPack.RegeneratePreviewsIfNecessary, this),
            new AsyncLoaderTask(SFX.Initialize, this),
            new AsyncLoaderTask((Action)Content.Initialize, this),

            new AsyncLoaderTask(Content.InitializeLevels, this),
            new AsyncLoaderTask(Challenges.Initialize, this, "Content.InitializeLevels"),

            new AsyncLoaderTask(main.DownloadWorkshopItems, this),
            new AsyncLoaderTask(ManagedContent.PreInitializeMods, this, "MonoMain.DownloadWorkshopItems"),
            new AsyncLoaderTask(ManagedContent.InitializeMods, this, "ManagedContent.PreInitializeMods"),

            new AsyncLoaderTask(Editor.InitializeConstructorLists, this, "ManagedContent.InitializeMods"),
            new AsyncLoaderTask(Network.InitializeMessageTypes, this, "ManagedContent.InitializeMods"),
            new AsyncLoaderTask(DeathCrate.InitializeDeathCrateSettings, this, "ManagedContent.InitializeMods"),

            new AsyncLoaderTask(Editor.InitializePlaceableGroup, this, "Editor.InitializeConstructorLists", "DeathCrate.InitializeDeathCrateSettings"),
            ];
    }

    public void Update()
    {
        if (IsCompleted)
            return;

        if (tasks.Count == 0)
        {
            IsCompleted = true;
            return;
        }

        if (currentTaskIndex >= tasks.Count)
            currentTaskIndex = 0;

        var task = tasks[currentTaskIndex];

        if (task.CheckRequirements(completedTasks))
        {
            Console.WriteLine($"running task {task.Name}");
            task.Start();
            tasks.Remove(task);
        }
        else
        {
            currentTaskIndex++;
        }
    }

    public IEnumerable<ILoaderTask> GetTasks()
    {
        return [.. tasks];
    }

    void IGameLoader.OnTaskComplete(string taskName)
    {
        completedTasks.Add(taskName);
    }
}
