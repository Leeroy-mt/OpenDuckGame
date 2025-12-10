using System;

namespace DuckGame;

[Serializable]
public struct Rectangle
{
    public float height;

    public float width;

    public float x;

    public float y;

    public float Top
    {
        get
        {
            return y;
        }
        set
        {
            y = value;
        }
    }

    public float Bottom
    {
        get
        {
            return y + height;
        }
        set
        {
            y = value - height;
        }
    }

    public float Left
    {
        get
        {
            return x;
        }
        set
        {
            x = value;
        }
    }

    public float Right
    {
        get
        {
            return x + width;
        }
        set
        {
            x = value - width;
        }
    }

    public Vec2 tl => new Vec2(x, y);

    public Vec2 tr => new Vec2(x + width, y);

    public Vec2 bl => new Vec2(x, y + height);

    public Vec2 br => new Vec2(x + width, y + height);

    public Vec2 Center
    {
        get
        {
            return new Vec2(x + width / 2f, y + height / 2f);
        }
        set
        {
            x = value.x - width / 2f;
            y = value.y - height / 2f;
        }
    }

    public float aspect => width / height;

    public Rectangle(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public Rectangle(Vec2 tl, Vec2 br)
    {
        if (tl.x > br.x)
        {
            float temp = br.x;
            br.x = tl.x;
            tl.x = temp;
        }
        if (tl.y > br.y)
        {
            float temp2 = br.y;
            br.y = tl.y;
            tl.y = temp2;
        }
        x = tl.x;
        y = tl.y;
        width = br.x - tl.x;
        height = br.y - tl.y;
    }

    public static implicit operator Microsoft.Xna.Framework.Rectangle(Rectangle r)
    {
        return new Microsoft.Xna.Framework.Rectangle((int)r.x, (int)r.y, (int)r.width, (int)r.height);
    }

    public static implicit operator Rectangle(Microsoft.Xna.Framework.Rectangle r)
    {
        return new Rectangle(r.X, r.Y, r.Width, r.Height);
    }

    public bool Contains(Vec2 position)
    {
        if (position.x >= x && position.y >= y && position.x <= x + width)
        {
            return position.y <= y + height;
        }
        return false;
    }

    public Rectangle GetQuadrant(int pQuadrantStartingWithTLClockwise)
    {
        return pQuadrantStartingWithTLClockwise switch
        {
            0 => new Rectangle(x, y, width / 2f, height / 2f),
            1 => new Rectangle(x + width / 2f, y, width / 2f, height / 2f),
            2 => new Rectangle(x + width / 2f, y + height / 2f, width / 2f, height / 2f),
            _ => new Rectangle(x, y + height / 2f, width / 2f, height / 2f),
        };
    }
}
