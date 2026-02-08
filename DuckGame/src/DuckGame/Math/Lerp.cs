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

    public static Vec3 Vec3(Vec3 current, Vec3 to, float amount)
    {
        Vec3 c = current;
        Vec3 c2 = to;
        Vec3 c3norm = c2 - c;
        if (c3norm.Length() < 0.0001f)
        {
            return current;
        }
        c3norm.Normalize();
        Vec3 ret = c + c3norm * amount;
        if (c2.x > c.x && ret.x > c2.x)
        {
            ret.x = c2.x;
        }
        if (c2.x < c.x && ret.x < c2.x)
        {
            ret.x = c2.x;
        }
        if (c2.y > c.y && ret.y > c2.y)
        {
            ret.y = c2.y;
        }
        if (c2.y < c.y && ret.y < c2.y)
        {
            ret.y = c2.y;
        }
        if (c2.z > c.z && ret.z > c2.z)
        {
            ret.z = c2.z;
        }
        if (c2.z < c.z && ret.z < c2.z)
        {
            ret.z = c2.z;
        }
        return ret;
    }

    public static Color Color(Color current, Color to, float amount)
    {
        Vec4 c = current.ToVector4();
        Vec4 c2 = to.ToVector4();
        Vec4 c3norm = c2 - c;
        if (c3norm.Length() < 0.0001f)
        {
            return current;
        }
        c3norm.Normalize();
        Vec4 ret = c + c3norm * amount;
        if (c2.x > c.x && ret.x > c2.x)
        {
            ret.x = c2.x;
        }
        if (c2.x < c.x && ret.x < c2.x)
        {
            ret.x = c2.x;
        }
        if (c2.y > c.y && ret.y > c2.y)
        {
            ret.y = c2.y;
        }
        if (c2.y < c.y && ret.y < c2.y)
        {
            ret.y = c2.y;
        }
        if (c2.z > c.z && ret.z > c2.z)
        {
            ret.z = c2.z;
        }
        if (c2.z < c.z && ret.z < c2.z)
        {
            ret.z = c2.z;
        }
        if (c2.w > c.w && ret.w > c2.w)
        {
            ret.w = c2.w;
        }
        if (c2.w < c.w && ret.w < c2.w)
        {
            ret.w = c2.w;
        }
        return new Color(ret.x, ret.y, ret.z, ret.w);
    }

    public static Color ColorSmooth(Color current, Color to, float amount)
    {
        Vec4 c = current.ToVector4();
        Vec4 c2 = to.ToVector4();
        Vec4 ret = c + (c2 - c) * amount;
        return new Color(ret.x, ret.y, ret.z, ret.w);
    }

    public static Color ColorSmoothNoAlpha(Color current, Color to, float amount)
    {
        Vec4 c = current.ToVector4();
        Vec4 c2 = to.ToVector4();
        Vec4 ret = c + (c2 - c) * amount;
        ret.w = 1f;
        return new Color(ret.x, ret.y, ret.z, ret.w);
    }
}
