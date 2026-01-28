using Microsoft.Xna.Framework;
using System;
using System.Text;

namespace DuckGame;

[Serializable]
public struct Vec4 : IEquatable<Vec4>
{
    private static Vec4 zeroVector = default(Vec4);

    private static Vec4 unitVector = new Vec4(1f, 1f, 1f, 1f);

    private static Vec4 unitXVector = new Vec4(1f, 0f, 0f, 0f);

    private static Vec4 unitYVector = new Vec4(0f, 1f, 0f, 0f);

    private static Vec4 unitZVector = new Vec4(0f, 0f, 1f, 0f);

    private static Vec4 unitWVector = new Vec4(0f, 0f, 0f, 1f);

    public float x;

    public float y;

    public float z;

    public float w;

    public static Vec4 Zero => zeroVector;

    public static Vec4 One => unitVector;

    public static Vec4 UnitX => unitXVector;

    public static Vec4 UnitY => unitYVector;

    public static Vec4 UnitZ => unitZVector;

    public static Vec4 UnitW => unitWVector;

    public Vec4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Vec4(Vec2 value, float z, float w)
    {
        x = value.X;
        y = value.Y;
        this.z = z;
        this.w = w;
    }

    public Vec4(Vec3 value, float w)
    {
        x = value.x;
        y = value.y;
        z = value.z;
        this.w = w;
    }

    public Vec4(float value)
    {
        x = value;
        y = value;
        z = value;
        w = value;
    }

    public static Vec4 Add(Vec4 value1, Vec4 value2)
    {
        value1.w += value2.w;
        value1.x += value2.x;
        value1.y += value2.y;
        value1.z += value2.z;
        return value1;
    }

    public static void Add(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result.w = value1.w + value2.w;
        result.x = value1.x + value2.x;
        result.y = value1.y + value2.y;
        result.z = value1.z + value2.z;
    }

    public static Vec4 Barycentric(Vec4 value1, Vec4 value2, Vec4 value3, float amount1, float amount2)
    {
        return new Vec4(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2), MathHelper.Barycentric(value1.z, value2.z, value3.z, amount1, amount2), MathHelper.Barycentric(value1.w, value2.w, value3.w, amount1, amount2));
    }

    public static void Barycentric(ref Vec4 value1, ref Vec4 value2, ref Vec4 value3, float amount1, float amount2, out Vec4 result)
    {
        result = new Vec4(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2), MathHelper.Barycentric(value1.z, value2.z, value3.z, amount1, amount2), MathHelper.Barycentric(value1.w, value2.w, value3.w, amount1, amount2));
    }

    public static Vec4 CatmullRom(Vec4 value1, Vec4 value2, Vec4 value3, Vec4 value4, float amount)
    {
        return new Vec4(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount), MathHelper.CatmullRom(value1.z, value2.z, value3.z, value4.z, amount), MathHelper.CatmullRom(value1.w, value2.w, value3.w, value4.w, amount));
    }

    public static void CatmullRom(ref Vec4 value1, ref Vec4 value2, ref Vec4 value3, ref Vec4 value4, float amount, out Vec4 result)
    {
        result = new Vec4(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount), MathHelper.CatmullRom(value1.z, value2.z, value3.z, value4.z, amount), MathHelper.CatmullRom(value1.w, value2.w, value3.w, value4.w, amount));
    }

    public static Vec4 Clamp(Vec4 value1, Vec4 min, Vec4 max)
    {
        return new Vec4(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y), MathHelper.Clamp(value1.z, min.z, max.z), MathHelper.Clamp(value1.w, min.w, max.w));
    }

    public static void Clamp(ref Vec4 value1, ref Vec4 min, ref Vec4 max, out Vec4 result)
    {
        result = new Vec4(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y), MathHelper.Clamp(value1.z, min.z, max.z), MathHelper.Clamp(value1.w, min.w, max.w));
    }

    public static float Distance(Vec4 value1, Vec4 value2)
    {
        return (float)Math.Sqrt((value1.w - value2.w) * (value1.w - value2.w) + (value1.x - value2.x) * (value1.x - value2.x) + (value1.y - value2.y) * (value1.y - value2.y) + (value1.z - value2.z) * (value1.z - value2.z));
    }

    public static void Distance(ref Vec4 value1, ref Vec4 value2, out float result)
    {
        result = (float)Math.Sqrt((value1.w - value2.w) * (value1.w - value2.w) + (value1.x - value2.x) * (value1.x - value2.x) + (value1.y - value2.y) * (value1.y - value2.y) + (value1.z - value2.z) * (value1.z - value2.z));
    }

    public static float DistanceSquared(Vec4 value1, Vec4 value2)
    {
        return (value1.w - value2.w) * (value1.w - value2.w) + (value1.x - value2.x) * (value1.x - value2.x) + (value1.y - value2.y) * (value1.y - value2.y) + (value1.z - value2.z) * (value1.z - value2.z);
    }

    public static void DistanceSquared(ref Vec4 value1, ref Vec4 value2, out float result)
    {
        result = (value1.w - value2.w) * (value1.w - value2.w) + (value1.x - value2.x) * (value1.x - value2.x) + (value1.y - value2.y) * (value1.y - value2.y) + (value1.z - value2.z) * (value1.z - value2.z);
    }

    public static Vec4 Divide(Vec4 value1, Vec4 value2)
    {
        value1.w /= value2.w;
        value1.x /= value2.x;
        value1.y /= value2.y;
        value1.z /= value2.z;
        return value1;
    }

    public static Vec4 Divide(Vec4 value1, float divider)
    {
        float factor = 1f / divider;
        value1.w *= factor;
        value1.x *= factor;
        value1.y *= factor;
        value1.z *= factor;
        return value1;
    }

    public static void Divide(ref Vec4 value1, float divider, out Vec4 result)
    {
        float factor = 1f / divider;
        result.w = value1.w * factor;
        result.x = value1.x * factor;
        result.y = value1.y * factor;
        result.z = value1.z * factor;
    }

    public static void Divide(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result.w = value1.w / value2.w;
        result.x = value1.x / value2.x;
        result.y = value1.y / value2.y;
        result.z = value1.z / value2.z;
    }

    public static float Dot(Vec4 vector1, Vec4 vector2)
    {
        return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z + vector1.w * vector2.w;
    }

    public static void Dot(ref Vec4 vector1, ref Vec4 vector2, out float result)
    {
        result = vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z + vector1.w * vector2.w;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vec4))
        {
            return false;
        }
        return this == (Vec4)obj;
    }

    public bool Equals(Vec4 other)
    {
        if (w == other.w && x == other.x && y == other.y)
        {
            return z == other.z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (int)(w + x + y + y);
    }

    public static Vec4 Hermite(Vec4 value1, Vec4 tangent1, Vec4 value2, Vec4 tangent2, float amount)
    {
        value1.w = MathHelper.Hermite(value1.w, tangent1.w, value2.w, tangent2.w, amount);
        value1.x = MathHelper.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
        value1.y = MathHelper.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        value1.z = MathHelper.Hermite(value1.z, tangent1.z, value2.z, tangent2.z, amount);
        return value1;
    }

    public static void Hermite(ref Vec4 value1, ref Vec4 tangent1, ref Vec4 value2, ref Vec4 tangent2, float amount, out Vec4 result)
    {
        result.w = MathHelper.Hermite(value1.w, tangent1.w, value2.w, tangent2.w, amount);
        result.x = MathHelper.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
        result.y = MathHelper.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        result.z = MathHelper.Hermite(value1.z, tangent1.z, value2.z, tangent2.z, amount);
    }

    public float Length()
    {
        return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
    }

    public float LengthSquared()
    {
        return x * x + y * y + z * z + w * w;
    }

    public static Vec4 Lerp(Vec4 value1, Vec4 value2, float amount)
    {
        return new Vec4(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount), MathHelper.Lerp(value1.z, value2.z, amount), MathHelper.Lerp(value1.w, value2.w, amount));
    }

    public static void Lerp(ref Vec4 value1, ref Vec4 value2, float amount, out Vec4 result)
    {
        result = new Vec4(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount), MathHelper.Lerp(value1.z, value2.z, amount), MathHelper.Lerp(value1.w, value2.w, amount));
    }

    public static Vec4 Max(Vec4 value1, Vec4 value2)
    {
        return new Vec4(MathHelper.Max(value1.x, value2.x), MathHelper.Max(value1.y, value2.y), MathHelper.Max(value1.z, value2.z), MathHelper.Max(value1.w, value2.w));
    }

    public static void Max(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result = new Vec4(MathHelper.Max(value1.x, value2.x), MathHelper.Max(value1.y, value2.y), MathHelper.Max(value1.z, value2.z), MathHelper.Max(value1.w, value2.w));
    }

    public static Vec4 Min(Vec4 value1, Vec4 value2)
    {
        return new Vec4(MathHelper.Min(value1.x, value2.x), MathHelper.Min(value1.y, value2.y), MathHelper.Min(value1.z, value2.z), MathHelper.Min(value1.w, value2.w));
    }

    public static void Min(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result = new Vec4(MathHelper.Min(value1.x, value2.x), MathHelper.Min(value1.y, value2.y), MathHelper.Min(value1.z, value2.z), MathHelper.Min(value1.w, value2.w));
    }

    public static Vec4 Multiply(Vec4 value1, Vec4 value2)
    {
        value1.w *= value2.w;
        value1.x *= value2.x;
        value1.y *= value2.y;
        value1.z *= value2.z;
        return value1;
    }

    public static Vec4 Multiply(Vec4 value1, float scaleFactor)
    {
        value1.w *= scaleFactor;
        value1.x *= scaleFactor;
        value1.y *= scaleFactor;
        value1.z *= scaleFactor;
        return value1;
    }

    public static void Multiply(ref Vec4 value1, float scaleFactor, out Vec4 result)
    {
        result.w = value1.w * scaleFactor;
        result.x = value1.x * scaleFactor;
        result.y = value1.y * scaleFactor;
        result.z = value1.z * scaleFactor;
    }

    public static void Multiply(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result.w = value1.w * value2.w;
        result.x = value1.x * value2.x;
        result.y = value1.y * value2.y;
        result.z = value1.z * value2.z;
    }

    public static Vec4 Negate(Vec4 value)
    {
        value.x = 0f - value.x;
        value.y = 0f - value.y;
        value.z = 0f - value.z;
        value.w = 0f - value.w;
        return value;
    }

    public static void Negate(ref Vec4 value, out Vec4 result)
    {
        result.x = 0f - value.x;
        result.y = 0f - value.y;
        result.z = 0f - value.z;
        result.w = 0f - value.w;
    }

    public void Normalize()
    {
        float factor = 1f / (float)Math.Sqrt(x * x + y * y + z * z + w * w);
        w *= factor;
        x *= factor;
        y *= factor;
        z *= factor;
    }

    public static Vec4 Normalize(Vec4 vector)
    {
        float factor = 1f / (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z + vector.w * vector.w);
        vector.w *= factor;
        vector.x *= factor;
        vector.y *= factor;
        vector.z *= factor;
        return vector;
    }

    public static void Normalize(ref Vec4 vector, out Vec4 result)
    {
        float factor = 1f / (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z + vector.w * vector.w);
        result.w = vector.w * factor;
        result.x = vector.x * factor;
        result.y = vector.y * factor;
        result.z = vector.z * factor;
    }

    public static Vec4 SmoothStep(Vec4 value1, Vec4 value2, float amount)
    {
        return new Vec4(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount), MathHelper.SmoothStep(value1.z, value2.z, amount), MathHelper.SmoothStep(value1.w, value2.w, amount));
    }

    public static void SmoothStep(ref Vec4 value1, ref Vec4 value2, float amount, out Vec4 result)
    {
        result = new Vec4(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount), MathHelper.SmoothStep(value1.z, value2.z, amount), MathHelper.SmoothStep(value1.w, value2.w, amount));
    }

    public static Vec4 Subtract(Vec4 value1, Vec4 value2)
    {
        value1.w -= value2.w;
        value1.x -= value2.x;
        value1.y -= value2.y;
        value1.z -= value2.z;
        return value1;
    }

    public static void Subtract(ref Vec4 value1, ref Vec4 value2, out Vec4 result)
    {
        result.w = value1.w - value2.w;
        result.x = value1.x - value2.x;
        result.y = value1.y - value2.y;
        result.z = value1.z - value2.z;
    }

    public static Vec4 Transform(Vec2 position, Matrix matrix)
    {
        Transform(ref position, ref matrix, out var result);
        return result;
    }

    public static Vec4 Transform(Vec2 value, Quaternion rotation)
    {
        throw new NotImplementedException();
    }

    public static Vec4 Transform(Vec3 value, Quaternion rotation)
    {
        throw new NotImplementedException();
    }

    public static Vec4 Transform(Vec4 value, Quaternion rotation)
    {
        throw new NotImplementedException();
    }

    public static void Transform(ref Vec2 value, ref Quaternion rotation, out Vec4 result)
    {
        throw new NotImplementedException();
    }

    public static void Transform(ref Vec3 value, ref Quaternion rotation, out Vec4 result)
    {
        throw new NotImplementedException();
    }

    public static void Transform(ref Vec4 value, ref Quaternion rotation, out Vec4 result)
    {
        throw new NotImplementedException();
    }

    public static void Transform(Vec4[] sourceArray, ref Quaternion rotation, Vec4[] destinationArray)
    {
        throw new NotImplementedException();
    }

    public static void Transform(Vec4[] sourceArray, ref Matrix matrix, Vec4[] destinationArray)
    {
        throw new NotImplementedException();
    }

    public static void Transform(Vec4[] sourceArray, int sourceIndex, ref Matrix matrix, Vec4[] destinationArray, int destinationIndex, int length)
    {
        throw new NotImplementedException();
    }

    public static void Transform(Vec4[] sourceArray, int sourceIndex, ref Quaternion rotation, Vec4[] destinationArray, int destinationIndex, int length)
    {
        throw new NotImplementedException();
    }

    public static Vec4 Transform(Vec3 position, Matrix matrix)
    {
        Transform(ref position, ref matrix, out var result);
        return result;
    }

    public static Vec4 Transform(Vec4 vector, Matrix matrix)
    {
        Transform(ref vector, ref matrix, out vector);
        return vector;
    }

    public static void Transform(ref Vec2 position, ref Matrix matrix, out Vec4 result)
    {
        result = new Vec4(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42, position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43, position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44);
    }

    public static void Transform(ref Vec3 position, ref Matrix matrix, out Vec4 result)
    {
        result = new Vec4(position.x * matrix.M11 + position.y * matrix.M21 + position.z * matrix.M31 + matrix.M41, position.x * matrix.M12 + position.y * matrix.M22 + position.z * matrix.M32 + matrix.M42, position.x * matrix.M13 + position.y * matrix.M23 + position.z * matrix.M33 + matrix.M43, position.x * matrix.M14 + position.y * matrix.M24 + position.z * matrix.M34 + matrix.M44);
    }

    public static void Transform(ref Vec4 vector, ref Matrix matrix, out Vec4 result)
    {
        result = new Vec4(vector.x * matrix.M11 + vector.y * matrix.M21 + vector.z * matrix.M31 + vector.w * matrix.M41, vector.x * matrix.M12 + vector.y * matrix.M22 + vector.z * matrix.M32 + vector.w * matrix.M42, vector.x * matrix.M13 + vector.y * matrix.M23 + vector.z * matrix.M33 + vector.w * matrix.M43, vector.x * matrix.M14 + vector.y * matrix.M24 + vector.z * matrix.M34 + vector.w * matrix.M44);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder(32);
        stringBuilder.Append("{X:");
        stringBuilder.Append(x);
        stringBuilder.Append(" Y:");
        stringBuilder.Append(y);
        stringBuilder.Append(" Z:");
        stringBuilder.Append(z);
        stringBuilder.Append(" W:");
        stringBuilder.Append(w);
        stringBuilder.Append("}");
        return stringBuilder.ToString();
    }

    public static Vec4 operator -(Vec4 value)
    {
        value.x = 0f - value.x;
        value.y = 0f - value.y;
        value.z = 0f - value.z;
        value.w = 0f - value.w;
        return value;
    }

    public static bool operator ==(Vec4 value1, Vec4 value2)
    {
        if (value1.w == value2.w && value1.x == value2.x && value1.y == value2.y)
        {
            return value1.z == value2.z;
        }
        return false;
    }

    public static bool operator !=(Vec4 value1, Vec4 value2)
    {
        if (value1.w == value2.w && value1.x == value2.x && value1.y == value2.y)
        {
            return value1.z != value2.z;
        }
        return true;
    }

    public static Vec4 operator +(Vec4 value1, Vec4 value2)
    {
        value1.w += value2.w;
        value1.x += value2.x;
        value1.y += value2.y;
        value1.z += value2.z;
        return value1;
    }

    public static Vec4 operator -(Vec4 value1, Vec4 value2)
    {
        value1.w -= value2.w;
        value1.x -= value2.x;
        value1.y -= value2.y;
        value1.z -= value2.z;
        return value1;
    }

    public static Vec4 operator *(Vec4 value1, Vec4 value2)
    {
        value1.w *= value2.w;
        value1.x *= value2.x;
        value1.y *= value2.y;
        value1.z *= value2.z;
        return value1;
    }

    public static Vec4 operator *(Vec4 value1, float scaleFactor)
    {
        value1.w *= scaleFactor;
        value1.x *= scaleFactor;
        value1.y *= scaleFactor;
        value1.z *= scaleFactor;
        return value1;
    }

    public static Vec4 operator *(float scaleFactor, Vec4 value1)
    {
        value1.w *= scaleFactor;
        value1.x *= scaleFactor;
        value1.y *= scaleFactor;
        value1.z *= scaleFactor;
        return value1;
    }

    public static Vec4 operator /(Vec4 value1, Vec4 value2)
    {
        value1.w /= value2.w;
        value1.x /= value2.x;
        value1.y /= value2.y;
        value1.z /= value2.z;
        return value1;
    }

    public static Vec4 operator /(Vec4 value1, float divider)
    {
        float factor = 1f / divider;
        value1.w *= factor;
        value1.x *= factor;
        value1.y *= factor;
        value1.z *= factor;
        return value1;
    }

    public static implicit operator Vector4(Vec4 vec)
    {
        return new Vector4(vec.x, vec.y, vec.z, vec.w);
    }

    public static implicit operator Vec4(Vector4 vec)
    {
        return new Vec4(vec.X, vec.Y, vec.Z, vec.W);
    }
}
