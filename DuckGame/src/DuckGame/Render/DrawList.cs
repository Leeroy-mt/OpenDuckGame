using System.Collections.Generic;

namespace DuckGame;

public class DrawList
{
    #region Protected Fields

    protected HashSet<Thing> _transparent = [];

    protected HashSet<Thing> _opaque = [];

    protected HashSet<Thing> _transparentRemove = [];

    protected HashSet<Thing> _opaqueRemove = [];

    #endregion

    #region Public Methods

    public void Add(Thing obj)
    {
        if (obj.opaque)
        {
            _opaque.Add(obj);
            _opaqueRemove.Remove(obj);
        }
        else
        {
            _transparent.Add(obj);
            _transparentRemove.Remove(obj);
        }
    }

    public void Remove(Thing obj)
    {
        if (obj.opaque)
            _opaque.Remove(obj);
        else
            _transparent.Remove(obj);
    }

    public void RemoveSoon(Thing obj)
    {
        if (obj.opaque)
            _opaqueRemove.Add(obj);
        else
            _transparentRemove.Add(obj);
    }

    public void Clear()
    {
        _transparent.Clear();
        _transparentRemove.Clear();
        _opaque.Clear();
        _opaqueRemove.Clear();
    }

    #endregion
}
