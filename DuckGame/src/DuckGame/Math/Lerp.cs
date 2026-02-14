using Microsoft.Xna.Framework;

namespace DuckGame;

public static class Lerp
{
    public static float Float(float current, float to, float amount)
    {
        if (to > current)
        {
            current += amount;
            if (to < current)
            {
                current = to;
            }
        }
        else if (to < current)
        {
            current -= amount;
            if (to > current)
            {
                current = to;
            }
        }
        return current;
    }

    public static float FloatSmooth(float current, float to, float amount, float toMul = 1f)
    {
        float toVal = to - (1f - toMul) * to;
        if (to < current)
        {
            toVal = to + (1f - toMul) * to;
        }
        float newVal = current + amount * (toVal - current);
        if ((to >= current && newVal > to) || (to <= current && newVal < to))
        {
            newVal = to;
        }
        return newVal;
    }

    public static Vector2 Vector2(Vector2 current, Vector2 to, float amount)
    {
        Vector2 c = current;
        Vector2 c2 = to;
        Vector2 c3norm = c2 - c;
        if (c3norm.Length() < 0.0001f)
        {
            return current;
        }
        c3norm.Normalize();
        Vector2 ret = c + c3norm * amount;
        if (c2.X > c.X && ret.X > c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.X < c.X && ret.X < c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.Y > c.Y && ret.Y > c2.Y)
        {
            ret.Y = c2.Y;
        }
        if (c2.Y < c.Y && ret.Y < c2.Y)
        {
            ret.Y = c2.Y;
        }
        return ret;
    }

    public static Vector2 Vec2Smooth(Vector2 current, Vector2 to, float amount)
    {
        return current + amount * (to - current);
    }

    public static Vector2 Vec2Smooth(Vector2 current, Vector2 to, float amount, float thresh = 0f)
    {
        Vector2 newval = current + amount * (to - current);
        if ((newval - to).Length() < thresh)
        {
            return to;
        }
        return newval;
    }

    public static T Generic<T>(T current, T to, float amount)
    {
        if (current is Vector2)
        {
            return (T)(object)Vec2Smooth((Vector2)(object)current, (Vector2)(object)to, amount);
        }
        if (current is float)
        {
            return (T)(object)FloatSmooth((float)(object)current, (float)(object)to, amount);
        }
        return current;
    }

    public static Vector3 Vector3(Vector3 current, Vector3 to, float amount)
    {
        Vector3 c = current;
        Vector3 c2 = to;
        Vector3 c3norm = c2 - c;
        if (c3norm.Length() < 0.0001f)
        {
            return current;
        }
        c3norm.Normalize();
        Vector3 ret = c + c3norm * amount;
        if (c2.X > c.X && ret.X > c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.X < c.X && ret.X < c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.Y > c.Y && ret.Y > c2.Y)
        {
            ret.Y = c2.Y;
        }
        if (c2.Y < c.Y && ret.Y < c2.Y)
        {
            ret.Y = c2.Y;
        }
        if (c2.Z > c.Z && ret.Z > c2.Z)
        {
            ret.Z = c2.Z;
        }
        if (c2.Z < c.Z && ret.Z < c2.Z)
        {
            ret.Z = c2.Z;
        }
        return ret;
    }

    public static Color Color(Color current, Color to, float amount)
    {
        Vector4 c = current.ToVector4();
        Vector4 c2 = to.ToVector4();
        Vector4 c3norm = c2 - c;
        if (c3norm.Length() < 0.0001f)
        {
            return current;
        }
        c3norm.Normalize();
        Vector4 ret = c + c3norm * amount;
        if (c2.X > c.X && ret.X > c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.X < c.X && ret.X < c2.X)
        {
            ret.X = c2.X;
        }
        if (c2.Y > c.Y && ret.Y > c2.Y)
        {
            ret.Y = c2.Y;
        }
        if (c2.Y < c.Y && ret.Y < c2.Y)
        {
            ret.Y = c2.Y;
        }
        if (c2.Z > c.Z && ret.Z > c2.Z)
        {
            ret.Z = c2.Z;
        }
        if (c2.Z < c.Z && ret.Z < c2.Z)
        {
            ret.Z = c2.Z;
        }
        if (c2.W > c.W && ret.W > c2.W)
        {
            ret.W = c2.W;
        }
        if (c2.W < c.W && ret.W < c2.W)
        {
            ret.W = c2.W;
        }
        return new Color(ret.X, ret.Y, ret.Z, ret.W);
    }

    public static Color ColorSmooth(Color current, Color to, float amount)
    {
        Vector4 c = current.ToVector4();
        Vector4 c2 = to.ToVector4();
        Vector4 ret = c + (c2 - c) * amount;
        return new Color(ret.X, ret.Y, ret.Z, ret.W);
    }

    public static Color ColorSmoothNoAlpha(Color current, Color to, float amount)
    {
        Vector4 c = current.ToVector4();
        Vector4 c2 = to.ToVector4();
        Vector4 ret = c + (c2 - c) * amount;
        ret.W = 1f;
        return new Color(ret.X, ret.Y, ret.Z, ret.W);
    }
}
