using System;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// The class that stores content managers.
/// </summary>
public static class ContentManagers
{
    private static Dictionary<Type, IManageContent> _contentManagers = new Dictionary<Type, IManageContent>();

    private static IManageContent AddContentManager(Type t)
    {
        IManageContent mgr = (IManageContent)Activator.CreateInstance(t);
        _contentManagers.Add(t, mgr);
        return mgr;
    }

    internal static IManageContent GetContentManager(Type t)
    {
        if (t == null)
        {
            t = typeof(DefaultContentManager);
        }
        if (_contentManagers.TryGetValue(t, out var mgr))
        {
            return mgr;
        }
        return AddContentManager(t);
    }
}
