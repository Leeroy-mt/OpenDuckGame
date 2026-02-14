using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[Serializable]
public struct Plane : IEquatable<Plane>
{
    public float d;

    public Vector3 normal;

    public Plane(Vector4 value)
        : this(new Vector3(value.X, value.Y, value.Z), value.W)
    {
    }

    public Plane(Vector3 normal, float d)
    {
        this.normal = normal;
        this.d = d;
    }

    public Plane(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 vector = b - a;
        Vector3 ac = c - a;
        Vector3 cross = Vector3.Cross(vector, ac);
        normal = Vector3.Normalize(cross);
        d = 0f - Vector3.Dot(cross, a);
    }

    public Plane(float a, float b, float c, float d)
        : this(new Vector3(a, b, c), d)
    {
    }

    public float Dot(Vector4 value)
    {
        return normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z + d * value.W;
    }

    public void Dot(ref Vector4 value, out float result)
    {
        result = normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z + d * value.W;
    }

    public float DotCoordinate(Vector3 value)
    {
        return normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z + d;
    }

    public void DotCoordinate(ref Vector3 value, out float result)
    {
        result = normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z + d;
    }

    public float DotNormal(Vector3 value)
    {
        return normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z;
    }

    public void DotNormal(ref Vector3 value, out float result)
    {
        result = normal.X * value.X + normal.Y * value.Y + normal.Z * value.Z;
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
        Vector3 normal = this.normal;
        this.normal = Vector3.Normalize(this.normal);
        float factor = (float)Math.Sqrt(this.normal.X * this.normal.X + this.normal.Y * this.normal.Y + this.normal.Z * this.normal.Z) / (float)Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        d *= factor;
    }

    public static Plane Normalize(Plane value)
    {
        Normalize(ref value, out var ret);
        return ret;
    }

    public static void Normalize(ref Plane value, out Plane result)
    {
        result.normal = Vector3.Normalize(value.normal);
        float factor = (float)Math.Sqrt(result.normal.X * result.normal.X + result.normal.Y * result.normal.Y + result.normal.Z * result.normal.Z) / (float)Math.Sqrt(value.normal.X * value.normal.X + value.normal.Y * value.normal.Y + value.normal.Z * value.normal.Z);
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
