using Microsoft.Xna.Framework;
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
            x = value.X - width / 2f;
            y = value.Y - height / 2f;
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
        if (tl.X > br.X)
        {
            float temp = br.X;
            br.X = tl.X;
            tl.X = temp;
        }
        if (tl.Y > br.Y)
        {
            float temp2 = br.Y;
            br.Y = tl.Y;
            tl.Y = temp2;
        }
        x = tl.X;
        y = tl.Y;
        width = br.X - tl.X;
        height = br.Y - tl.Y;
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
        if (position.X >= x && position.Y >= y && position.X <= x + width)
        {
            return position.Y <= y + height;
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

    public Vector4 ToVector4()
    {
        return new(x, y, width, height);
    }
}
