using System;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// The class that stores content managers.
/// </summary>
public static class ContentManagers
{
    readonly static Dictionary<Type, IManageContent> contentManagers = [];

    private static IManageContent AddContentManager(Type t)
    {
        IManageContent mgr = (IManageContent)Activator.CreateInstance(t);
        contentManagers.Add(t, mgr);
        return mgr;
    }

    internal static IManageContent GetContentManager(Type t)
    {
        if (t == null)
            t = typeof(DefaultContentManager);

        if (contentManagers.TryGetValue(t, out var mgr))
            return mgr;

        return AddContentManager(t);
    }
}
