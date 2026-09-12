using System;
using System.Diagnostics;
using System.Linq;

namespace DuckGame;

internal interface IGameLoader
{
    bool IsCompleted { get; }

    void Update();

    void OnTaskComplete(string taskName);
}