using Microsoft.Xna.Framework;

namespace DuckGame;

public class NetDebugElement
{
    public float width;

    public NetDebugElement right;

    public Depth depth;

    public float indent;

    public float leading;

    protected NetDebugInterface _interface;

    protected string _name;

    public NetDebugElement(NetDebugInterface pInterface)
    {
        _interface = pInterface;
    }

    public virtual bool DoDraw(Vector2 position, bool allowInput)
    {
        bool val = Draw(position, allowInput);
        if (right != null)
        {
            val |= right.DoDraw(position + new Vector2(width, 0f), !val);
        }
        return val;
    }

    protected virtual bool Draw(Vector2 position, bool allowInput)
    {
        return false;
    }
}
