using System;

namespace DuckGame;

internal class StartupAction(Action action, TaskThread thread)
{
    public event Action<string> LoadStarted;

    public bool IsActive { get; set; } = true;

    public TaskThread Thread = thread;

    public string LoadingMessage { get; init; }

    public LoadingAction Action { get; } = action;

    bool Logged;

    public bool Run()
    {
        if (!Logged)
        {
            LoadStarted?.Invoke($"|{(Thread is 0 ? "DGYELLOW" : "DGBLUE")}|{LoadingMessage}");
            Logged = true;
        }

        return !(IsActive = !Action.Invoke());
    }
}