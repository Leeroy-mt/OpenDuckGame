using System;

namespace DuckGame;

[Serializable]
public struct Plane : IEquatable<Plane>
{
    public float d;

    public Vec3 normal;

    public Plane(Vec4 value)
        : this(new Vec3(value.x, value.y, value.z), value.w)
    {
    }

    public Plane(Vec3 normal, float d)
    {
        this.normal = normal;
        this.d = d;
    }

    public Plane(Vec3 a, Vec3 b, Vec3 c)
    {
        Vec3 vector = b - a;
        Vec3 ac = c - a;
        Vec3 cross = Vec3.Cross(vector, ac);
        normal = Vec3.Normalize(cross);
        d = 0f - Vec3.Dot(cross, a);
    }

    public Plane(float a, float b, float c, float d)
        : this(new Vec3(a, b, c), d)
    {
    }

    public float Dot(Vec4 value)
    {
        return normal.x * value.x + normal.y * value.y + normal.z * value.z + d * value.w;
    }

    public void Dot(ref Vec4 value, out float result)
    {
        result = normal.x * value.x + normal.y * value.y + normal.z * value.z + d * value.w;
    }

    public float DotCoordinate(Vec3 value)
    {
        return normal.x * value.x + normal.y * value.y + normal.z * value.z + d;
    }

    public void DotCoordinate(ref Vec3 value, out float result)
    {
        result = normal.x * value.x + normal.y * value.y + normal.z * value.z + d;
    }

    public float DotNormal(Vec3 value)
    {
        return normal.x * value.x + normal.y * value.y + normal.z * value.z;
    }

    public void DotNormal(ref Vec3 value, out float result)
    {
        result = normal.x * value.x + normal.y * value.y + normal.z * value.z;
    }

    public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
    {
        throw new NotImplementedException();
    }

    public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
    {
        throw new NotImplementedException();
    }

    public static Plane Transform(Plane plane, Quaternion rotation)
    {
        throw new NotImplementedException();
    }

    public static Plane Transform(Plane plane, Matrix matrix)
    {
        throw new NotImplementedException();
    }

    public void Normalize()
    {
        Vec3 normal = this.normal;
        this.normal = Vec3.Normalize(this.normal);
        float factor = (float)Math.Sqrt(this.normal.x * this.normal.x + this.normal.y * this.normal.y + this.normal.z * this.normal.z) / (float)Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
        d *= factor;
    }

    public static Plane Normalize(Plane value)
    {
        Normalize(ref value, out var ret);
        return ret;
    }

    public static void Normalize(ref Plane value, out Plane result)
    {
        result.normal = Vec3.Normalize(value.normal);
        float factor = (float)Math.Sqrt(result.normal.x * result.normal.x + result.normal.y * result.normal.y + result.normal.z * result.normal.z) / (float)Math.Sqrt(value.normal.x * value.normal.x + value.normal.y * value.normal.y + value.normal.z * value.normal.z);
        result.d = value.d * factor;
    }

    public static bool operator !=(Plane plane1, Plane plane2)
    {
        return !plane1.Equals(plane2);
    }

    public static bool operator ==(Plane plane1, Plane plane2)
    {
        return plane1.Equals(plane2);
    }

    public override bool Equals(object other)
    {
        if (!(other is Plane))
        {
            return false;
        }
        return Equals((Plane)other);
    }

    public bool Equals(Plane other)
    {
        if (normal == other.normal)
        {
            return d == other.d;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return normal.GetHashCode() ^ d.GetHashCode();
    }

    public override string ToString()
    {
        return $"{{Normal:{normal} D:{d}}}";
    }
}
