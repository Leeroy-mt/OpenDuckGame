using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public abstract class DestroyType
{
    private static Map<byte, Type> _types = new Map<byte, Type>();

    private Thing _thing;

    public static Map<byte, Type> indexTypeMap => _types;

    public Thing thing => _thing;

    public Type killThingType
    {
        get
        {
            if (_thing == null)
            {
                return null;
            }
            return _thing.killThingType;
        }
    }

    public Profile responsibleProfile
    {
        get
        {
            if (_thing == null)
            {
                return null;
            }
            return _thing.responsibleProfile;
        }
    }

    public static void InitializeTypes()
    {
        if (MonoMain.moddingEnabled)
        {
            byte num = 0;
            {
                foreach (Type type in ManagedContent.DestroyTypes.SortedTypes)
                {
                    _types.Add(num, type);
                    num++;
                }
                return;
            }
        }
        List<Type> list = Editor.GetSubclasses(typeof(DestroyType)).ToList();
        byte index = 0;
        foreach (Type t in list)
        {
            _types.Add(index, t);
            index++;
        }
    }

    public DestroyType(Thing t = null)
    {
        _thing = t;
    }
}
