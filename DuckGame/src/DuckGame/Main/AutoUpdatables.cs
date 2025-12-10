using System;
using System.Collections.Generic;

namespace DuckGame;

public static class AutoUpdatables
{
    public class Core
    {
        public List<WeakReference> _updateables = new List<WeakReference>();

        public bool ignoreAdditions;
    }

    private static Core _core = new Core();

    public static Core core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
    }

    private static List<WeakReference> _updateables
    {
        get
        {
            return _core._updateables;
        }
        set
        {
            _core._updateables = value;
        }
    }

    public static bool ignoreAdditions
    {
        get
        {
            return _core.ignoreAdditions;
        }
        set
        {
            _core.ignoreAdditions = value;
        }
    }

    public static void Add(IAutoUpdate update)
    {
        if (!ignoreAdditions)
        {
            _updateables.Add(new WeakReference(update));
        }
    }

    public static void Clear()
    {
        _updateables.Clear();
    }

    public static void ClearSounds()
    {
        for (int i = 0; i < _updateables.Count; i++)
        {
            if (_updateables[i] != null && _updateables[i].Target != null && _updateables[i].Target is ConstantSound)
            {
                (_updateables[i].Target as ConstantSound).Kill();
            }
        }
    }

    public static void MuteSounds()
    {
        for (int i = 0; i < _updateables.Count; i++)
        {
            if (_updateables[i] != null && _updateables[i].Target != null && _updateables[i].Target is ConstantSound)
            {
                (_updateables[i].Target as ConstantSound).Mute();
            }
        }
    }

    public static void Update()
    {
        int numRemove = 25;
        for (int i = 0; i < _updateables.Count; i++)
        {
            if (_updateables[i] == null)
            {
                if (numRemove > 0)
                {
                    _updateables.RemoveAt(i);
                    i--;
                    numRemove--;
                }
            }
            else
            {
                IAutoUpdate t = _updateables[i].Target as IAutoUpdate;
                if (!_updateables[i].IsAlive || t == null)
                {
                    _updateables[i] = null;
                }
                else
                {
                    t.Update();
                }
            }
        }
    }
}
