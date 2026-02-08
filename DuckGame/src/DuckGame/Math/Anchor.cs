using Microsoft.Xna.Framework;

namespace DuckGame;

public class Anchor
{
    private Thing _thing;

    public Vector2 offset = Vector2.Zero;

    public Thing thing => _thing;

    public Vector2 position => _thing.anchorPosition + offset;

    public Anchor(Thing to)
    {
        _thing = to;
    }

    public static implicit operator Anchor(Thing val)
    {
        return new Anchor(val);
    }

    public static implicit operator Thing(Anchor val)
    {
        return val._thing;
    }

    public static bool operator ==(Anchor c1, Thing c2)
    {
        if ((object)c1 == null)
        {
            if (c2 == null)
            {
                return true;
            }
            return false;
        }
        return c1._thing == c2;
    }

    public static bool operator !=(Anchor c1, Thing c2)
    {
        if ((object)c1 == null)
        {
            if (c2 == null)
            {
                return false;
            }
            return true;
        }
        return c1._thing != c2;
    }

    public bool Equals(Thing p)
    {
        return p == _thing;
    }

    public override bool Equals(object obj)
    {
        if (obj is Thing)
        {
            return Equals(obj as Thing);
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _thing.GetHashCode();
    }
}
