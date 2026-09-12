using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

internal interface ILoaderTask
{
    bool IsCompleted { get; }

    float CompletionProgress { get; }

    string Name { get; }

    IGameLoader Loader { get; }

    string[] Requirements { get; }

    void Start();

    bool CheckRequirements(IEnumerable<string> loadedMethods)
    {
        return Requirements.All(m => loadedMethods.Contains(m));
    }

    protected internal static Progress<float> GetProgress(Delegate del)
    {
        var firstParameterType = del.Method
                .GetParameters()
                .FirstOrDefault()
                ?.ParameterType;

        if (firstParameterType?.IsAssignableFrom(typeof(IProgress<float>)) ?? false)
            return new();

        return null;
    }
}
