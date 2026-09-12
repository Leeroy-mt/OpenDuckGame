using System;
using System.Linq;

namespace DuckGame;

internal class SyncLoaderTask : ILoaderTask
{
    Delegate loadingMethod;

    Progress<float> progress;

    public bool IsCompleted { get; private set; }

    public float CompletionProgress { get; private set; }

    public string Name => $"{loadingMethod?.Method.DeclaringType.Name}.{loadingMethod?.Method.Name}";

    public IGameLoader Loader { get; init; }

    public string[] Requirements { get; } = [];

    public SyncLoaderTask(Delegate del, IGameLoader loader, params string[] requirements)
    {
        loadingMethod = del;
        Loader = loader;

        if (ILoaderTask.GetProgress(del) is Progress<float> progress)
        {
            progress.ProgressChanged += (sender, newProgress) => CompletionProgress = newProgress;
            this.progress = progress;
        }

        Requirements = requirements;
    }

    public void Start()
    {
        if (progress is null)
            loadingMethod.DynamicInvoke();
        else
            loadingMethod.DynamicInvoke(progress);

        {
            IsCompleted = true;

            var method = loadingMethod.Method;
            Loader.OnTaskComplete($"{method.DeclaringType.Name}.{method.Name}");
        }
    }
}
