using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace DuckGame;

[Serializable]
public struct Vec2 : IEquatable<Vec2>
{
    #region Public Fields
    public float X;

    public float Y;
    #endregion

    #region Private Fields
    static Vec2 zeroVector = new(0);

    static Vec2 unitVector = new(1);

    static Vec2 unitxVector = new(1, 0);

    static Vec2 unityVector = new(0, 1);

    static Vec2 maxVector = new(float.MaxValue, float.MaxValue);

    static Vec2 minVector = new(float.MinValue, float.MinValue);
    #endregion

    #region Public Properties
    public static Vec2 Zero => zeroVector;

    public static Vec2 One => unitVector;

    public static Vec2 MinValue => minVector;

    public static Vec2 MaxValue => maxVector;

    public static Vec2 UnitX => unitxVector;

    public static Vec2 UnitY => unityVector;

    public static Vec2 NetMin => new(-10000);

    public static Vec2 NetMax => new(10000);

    public float lengthSq => LengthSquared();

    public Vec2 Normalized
    {
        get
        {
            var len = Length();
            if (len != 0)
                return this / len;
            return Zero;
        }
    }
    #endregion

    #region Public Constructors
    public Vec2(float x, float y) =>
        (X, Y) = (x, y);

    public Vec2(float value) =>
        X = Y = value;

    public Vec2(Vec2 vec) =>
        (X, Y) = (vec.X, vec.Y);
    #endregion

    #region Public Methods
    public static Vec2 Add(Vec2 value1, Vec2 value2) =>
        new(value1.X + value2.X, value1.Y + value2.Y);

    public static void Add(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = value1.X + value2.X;
        result.Y = value1.Y + value2.Y;
    }

    public static Vec2 Barycentric(Vec2 value1, Vec2 value2, Vec2 value3, float amount1, float amount2) =>
        new(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));

    public static void Barycentric(ref Vec2 value1, ref Vec2 value2, ref Vec2 value3, float amount1, float amount2, out Vec2 result) =>
        result = new(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));

    public static Vec2 CatmullRom(Vec2 value1, Vec2 value2, Vec2 value3, Vec2 value4, float amount) =>
        new(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));

    public static void CatmullRom(ref Vec2 value1, ref Vec2 value2, ref Vec2 value3, ref Vec2 value4, float amount, out Vec2 result) =>
        result = new(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));

    public static Vec2 Clamp(Vec2 value1, Vec2 min, Vec2 max) =>
        new(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));

    public static void Clamp(ref Vec2 value1, ref Vec2 min, ref Vec2 max, out Vec2 result) =>
        result = new(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));

    public static float Distance(Vec2 value1, Vec2 value2)
    {
        float num = value1.X - value2.X;
        float v2 = value1.Y - value2.Y;
        return float.Sqrt(num * num + v2 * v2);
    }

    public static void Distance(ref Vec2 value1, ref Vec2 value2, out float result)
    {
        float v1 = value1.X - value2.X;
        float v2 = value1.Y - value2.Y;
        result = float.Sqrt(v1 * v1 + v2 * v2);
    }

    public static float DistanceSquared(Vec2 value1, Vec2 value2)
    {
        float num = value1.X - value2.X;
        float v2 = value1.Y - value2.Y;
        return num * num + v2 * v2;
    }

    public static void DistanceSquared(ref Vec2 value1, ref Vec2 value2, out float result)
    {
        float v1 = value1.X - value2.X;
        float v2 = value1.Y - value2.Y;
        result = v1 * v1 + v2 * v2;
    }

    public static Vec2 Divide(Vec2 value1, Vec2 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    public static void Divide(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = value1.X / value2.X;
        result.Y = value1.Y / value2.Y;
    }

    public static Vec2 Divide(Vec2 value1, float divider)
    {
        float factor = 1f / divider;
        value1.X *= factor;
        value1.Y *= factor;
        return value1;
    }

    public static void Divide(ref Vec2 value1, float divider, out Vec2 result)
    {
        float factor = 1f / divider;
        result.X = value1.X * factor;
        result.Y = value1.Y * factor;
    }

    public static float Dot(Vec2 value1, Vec2 value2) =>
        value1.X * value2.X + value1.Y * value2.Y;

    public static void Dot(ref Vec2 value1, ref Vec2 value2, out float result) =>
        result = value1.X * value2.X + value1.Y * value2.Y;

    public override bool Equals(object obj)
    {
        if (obj is Vec2 vec)
            return Equals(vec);
        return false;
    }

    public readonly bool Equals(Vec2 other) =>
        X == other.X && Y == other.Y;

    public static Vec2 Reflect(Vec2 vector, Vec2 normal)
    {
        float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
        Vec2 result = default;
        result.X = vector.X - normal.X * val;
        result.Y = vector.Y - normal.Y * val;
        return result;
    }

    public static void Reflect(ref Vec2 vector, ref Vec2 normal, out Vec2 result)
    {
        float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
        result.X = vector.X - normal.X * val;
        result.Y = vector.Y - normal.Y * val;
    }

    public override readonly int GetHashCode() =>
        X.GetHashCode() + Y.GetHashCode();

    public static Vec2 Hermite(Vec2 value1, Vec2 tangent1, Vec2 value2, Vec2 tangent2, float amount)
    {
        Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out Vec2 result);
        return result;
    }

    public static void Hermite(ref Vec2 value1, ref Vec2 tangent1, ref Vec2 value2, ref Vec2 tangent2, float amount, out Vec2 result)
    {
        result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
        result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
    }

    public readonly float Length() =>
        float.Sqrt(X * X + Y * Y);

    public readonly float LengthSquared() =>
        X * X + Y * Y;

    public static Vec2 Lerp(Vec2 value1, Vec2 value2, float amount) =>
        DuckGame.Lerp.Vec2Smooth(value1, value2, amount);

    public static void Lerp(ref Vec2 value1, ref Vec2 value2, float amount, out Vec2 result) =>
        result = new Vec2(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount));

    public static Vec2 Max(Vec2 value1, Vec2 value2) =>
        new((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y);

    public static void Max(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = ((value1.X > value2.X) ? value1.X : value2.X);
        result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
    }

    public static Vec2 Min(Vec2 value1, Vec2 value2) =>
        new((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y);

    public static void Min(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = ((value1.X < value2.X) ? value1.X : value2.X);
        result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
    }

    public static Vec2 Multiply(Vec2 value1, Vec2 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    public static Vec2 Multiply(Vec2 value1, float scaleFactor)
    {
        value1.X *= scaleFactor;
        value1.Y *= scaleFactor;
        return value1;
    }

    public static void Multiply(ref Vec2 value1, float scaleFactor, out Vec2 result)
    {
        result.X = value1.X * scaleFactor;
        result.Y = value1.Y * scaleFactor;
    }

    public static void Multiply(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = value1.X * value2.X;
        result.Y = value1.Y * value2.Y;
    }

    public static Vec2 Negate(Vec2 value)
    {
        value.X = -value.X;
        value.Y = -value.Y;
        return value;
    }

    public static void Negate(ref Vec2 value, out Vec2 result)
    {
        result.X = -value.X;
        result.Y = -value.Y;
    }

    public void Normalize()
    {
        float len = Length();
        if (len != 0)
            this /= len;
    }

    public static Vec2 Normalize(Vec2 value)
    {
        float len = float.Sqrt(value.X * value.X + value.Y * value.Y);
        if (len != 0)
            value /= len;
        return value;
    }

    public static void Normalize(ref Vec2 value, out Vec2 result)
    {
        float len = float.Sqrt(value.X * value.X + value.Y * value.Y);
        result = len != 0 ? value / len : Zero;
    }

    public static Vec2 SmoothStep(Vec2 value1, Vec2 value2, float amount) =>
        new(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));

    public static void SmoothStep(ref Vec2 value1, ref Vec2 value2, float amount, out Vec2 result) =>
        result = new(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));

    public static Vec2 Subtract(Vec2 value1, Vec2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    public static void Subtract(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
    {
        result.X = value1.X - value2.X;
        result.Y = value1.Y - value2.Y;
    }

    public static Vec2 Transform(Vec2 position, Matrix matrix)
    {
        Transform(ref position, ref matrix, out position);
        return position;
    }

    public static void Transform(ref Vec2 position, ref Matrix matrix, out Vec2 result)
    {
        result = new Vec2(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
    }

    public static Vec2 Transform(Vec2 position, Quaternion quat)
    {
        Transform(ref position, ref quat, out position);
        return position;
    }

    public static void Transform(ref Vec2 position, ref Quaternion quat, out Vec2 result)
    {
        Quaternion v = new Quaternion(position.X, position.Y, 0f, 0f);
        Quaternion.Inverse(ref quat, out var i);
        Quaternion.Multiply(ref quat, ref v, out var t);
        Quaternion.Multiply(ref t, ref i, out v);
        result = new Vec2(v.x, v.y);
    }

    public static void Transform(Vec2[] sourceArray, ref Matrix matrix, Vec2[] destinationArray)
    {
        Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
    }

    public static void Transform(Vec2[] sourceArray, int sourceIndex, ref Matrix matrix, Vec2[] destinationArray, int destinationIndex, int length)
    {
        for (int x = 0; x < length; x++)
        {
            Vec2 position = sourceArray[sourceIndex + x];
            Vec2 destination = destinationArray[destinationIndex + x];
            destination.X = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
            destination.Y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
            destinationArray[destinationIndex + x] = destination;
        }
    }

    public static Vec2 TransformNormal(Vec2 normal, Matrix matrix)
    {
        TransformNormal(ref normal, ref matrix, out normal);
        return normal;
    }

    public static void TransformNormal(ref Vec2 normal, ref Matrix matrix, out Vec2 result)
    {
        result = new Vec2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
    }

    public override string ToString()
    {
        CultureInfo currentCulture = CultureInfo.CurrentCulture;
        return string.Format(currentCulture, "{{x:{0} y:{1}}}", new object[2]
        {
            X.ToString(currentCulture),
            Y.ToString(currentCulture)
        });
    }

    public Vec2 Rotate(float radians, Vec2 pivot)
    {
        float cosRadians = (float)Math.Cos(radians);
        float sinRadians = (float)Math.Sin(radians);
        Vec2 translatedPoint = new Vec2
        {
            X = X - pivot.X,
            Y = Y - pivot.Y
        };
        return new Vec2
        {
            X = translatedPoint.X * cosRadians - translatedPoint.Y * sinRadians + pivot.X,
            Y = translatedPoint.X * sinRadians + translatedPoint.Y * cosRadians + pivot.Y
        };
    }
    #endregion

    #region Operators
    public static Vec2 operator -(Vec2 value)
    {
        value.X = 0f - value.X;
        value.Y = 0f - value.Y;
        return value;
    }

    public static bool operator ==(Vec2 value1, Vec2 value2)
    {
        if (value1.X == value2.X)
        {
            return value1.Y == value2.Y;
        }
        return false;
    }

    public static bool operator !=(Vec2 value1, Vec2 value2)
    {
        if (value1.X == value2.X)
        {
            return value1.Y != value2.Y;
        }
        return true;
    }

    public static Vec2 operator +(Vec2 value1, Vec2 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    public static Vec2 operator -(Vec2 value1, Vec2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    public static Vec2 operator *(Vec2 value1, Vec2 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    public static Vec2 operator *(Vec2 value, float scaleFactor)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    public static Vec2 operator *(float scaleFactor, Vec2 value)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    public static Vec2 operator /(Vec2 value1, Vec2 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    public static Vec2 operator /(Vec2 value1, float divider)
    {
        float factor = 1f / divider;
        value1.X *= factor;
        value1.Y *= factor;
        return value1;
    }

    public static implicit operator Vector2(Vec2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }

    public static implicit operator Vec2(Vector2 vec)
    {
        return new Vec2(vec.X, vec.Y);
    }
    #endregion
}