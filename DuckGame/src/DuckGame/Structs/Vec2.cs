using Microsoft.Xna.Framework;

namespace DuckGame;

public static class Vec2
{
    static Vector2 MaxVector = new(float.MaxValue),
                   MinVector = new(float.MinValue);

    static Vector2 NetMaxVector = new(10000),
                   NetMinVector = new(-10000);

    extension(Vector2 vector2)
    {
        public static Vector2 MaxValue => MaxVector;

        public static Vector2 MinValue => MinVector;

        public static Vector2 NetMax => NetMaxVector;

        public static Vector2 NetMin => NetMinVector;

        public Vector2 Rotate(float radians, Vector2 pivot)
        {
            float cos = float.Cos(radians);
            float sin = float.Sin(radians);

            Vector2 translatedPoint = vector2 - pivot;
            return new(
                translatedPoint.X * cos - translatedPoint.Y * sin + pivot.X,
                translatedPoint.X * sin + translatedPoint.Y * cos + pivot.Y
                );
        }
    }
}

//[Serializable]
//public struct Vector2 : IEquatable<Vector2>
//{
//    #region Public Fields
//    public float X;

//    public float Y;
//    #endregion

//    #region Private Fields
//    static Vector2 zeroVector = new(0);

//    static Vector2 unitVector = new(1);

//    static Vector2 unitxVector = new(1, 0);

//    static Vector2 unityVector = new(0, 1);

//    static Vector2 maxVector = new(float.MaxValue, float.MaxValue);

//    static Vector2 minVector = new(float.MinValue, float.MinValue);
//    #endregion

//    #region Public Properties
//    public static Vector2 Zero => zeroVector;

//    public static Vector2 One => unitVector;

//    public static Vector2 MinValue => minVector;

//    public static Vector2 MaxValue => maxVector;

//    public static Vector2 UnitX => unitxVector;

//    public static Vector2 UnitY => unityVector;

//    public static Vector2 NetMin => new(-10000);

//    public static Vector2 NetMax => new(10000);

//    public float lengthSq => LengthSquared();

//    public Vector2 Normalized
//    {
//        get
//        {
//            var len = Length();
//            if (len != 0)
//                return this / len;
//            return Zero;
//        }
//    }
//    #endregion

//    #region Public Constructors
//    public Vector2(float x, float y) =>
//        (X, Y) = (x, y);

//    public Vector2(float value) =>
//        X = Y = value;

//    public Vector2(Vector2 vec) =>
//        (X, Y) = (vec.X, vec.Y);
//    #endregion

//    #region Public Methods
//    public static Vector2 Add(Vector2 value1, Vector2 value2) =>
//        new(value1.X + value2.X, value1.Y + value2.Y);

//    public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = value1.X + value2.X;
//        result.Y = value1.Y + value2.Y;
//    }

//    public static Vector2 Barycentric(Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2) =>
//        new(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));

//    public static void Barycentric(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, float amount1, float amount2, out Vector2 result) =>
//        result = new(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));

//    public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount) =>
//        new(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));

//    public static void CatmullRom(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, ref Vector2 value4, float amount, out Vector2 result) =>
//        result = new(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));

//    public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max) =>
//        new(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));

//    public static void Clamp(ref Vector2 value1, ref Vector2 min, ref Vector2 max, out Vector2 result) =>
//        result = new(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));

//    public static float Distance(Vector2 value1, Vector2 value2)
//    {
//        float num = value1.X - value2.X;
//        float v2 = value1.Y - value2.Y;
//        return float.Sqrt(num * num + v2 * v2);
//    }

//    public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
//    {
//        float v1 = value1.X - value2.X;
//        float v2 = value1.Y - value2.Y;
//        result = float.Sqrt(v1 * v1 + v2 * v2);
//    }

//    public static float DistanceSquared(Vector2 value1, Vector2 value2)
//    {
//        float num = value1.X - value2.X;
//        float v2 = value1.Y - value2.Y;
//        return num * num + v2 * v2;
//    }

//    public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
//    {
//        float v1 = value1.X - value2.X;
//        float v2 = value1.Y - value2.Y;
//        result = v1 * v1 + v2 * v2;
//    }

//    public static Vector2 Divide(Vector2 value1, Vector2 value2)
//    {
//        value1.X /= value2.X;
//        value1.Y /= value2.Y;
//        return value1;
//    }

//    public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = value1.X / value2.X;
//        result.Y = value1.Y / value2.Y;
//    }

//    public static Vector2 Divide(Vector2 value1, float divider)
//    {
//        float factor = 1f / divider;
//        value1.X *= factor;
//        value1.Y *= factor;
//        return value1;
//    }

//    public static void Divide(ref Vector2 value1, float divider, out Vector2 result)
//    {
//        float factor = 1f / divider;
//        result.X = value1.X * factor;
//        result.Y = value1.Y * factor;
//    }

//    public static float Dot(Vector2 value1, Vector2 value2) =>
//        value1.X * value2.X + value1.Y * value2.Y;

//    public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result) =>
//        result = value1.X * value2.X + value1.Y * value2.Y;

//    public override bool Equals(object obj)
//    {
//        if (obj is Vector2 vec)
//            return Equals(vec);
//        return false;
//    }

//    public readonly bool Equals(Vector2 other) =>
//        X == other.X && Y == other.Y;

//    public static Vector2 Reflect(Vector2 vector, Vector2 normal)
//    {
//        float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
//        Vector2 result = default;
//        result.X = vector.X - normal.X * val;
//        result.Y = vector.Y - normal.Y * val;
//        return result;
//    }

//    public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
//    {
//        float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
//        result.X = vector.X - normal.X * val;
//        result.Y = vector.Y - normal.Y * val;
//    }

//    public override readonly int GetHashCode() =>
//        X.GetHashCode() + Y.GetHashCode();

//    public static Vector2 Hermite(Vector2 value1, Vector2 tangent1, Vector2 value2, Vector2 tangent2, float amount)
//    {
//        Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out Vector2 result);
//        return result;
//    }

//    public static void Hermite(ref Vector2 value1, ref Vector2 tangent1, ref Vector2 value2, ref Vector2 tangent2, float amount, out Vector2 result)
//    {
//        result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
//        result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
//    }

//    public readonly float Length() =>
//        float.Sqrt(X * X + Y * Y);

//    public readonly float LengthSquared() =>
//        X * X + Y * Y;

//    public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount) =>
//        DuckGame.Lerp.Vec2Smooth(value1, value2, amount);

//    public static void Lerp(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result) =>
//        result = new Vector2(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount));

//    public static Vector2 Max(Vector2 value1, Vector2 value2) =>
//        new((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y);

//    public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = ((value1.X > value2.X) ? value1.X : value2.X);
//        result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
//    }

//    public static Vector2 Min(Vector2 value1, Vector2 value2) =>
//        new((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y);

//    public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = ((value1.X < value2.X) ? value1.X : value2.X);
//        result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
//    }

//    public static Vector2 Multiply(Vector2 value1, Vector2 value2)
//    {
//        value1.X *= value2.X;
//        value1.Y *= value2.Y;
//        return value1;
//    }

//    public static Vector2 Multiply(Vector2 value1, float scaleFactor)
//    {
//        value1.X *= scaleFactor;
//        value1.Y *= scaleFactor;
//        return value1;
//    }

//    public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result)
//    {
//        result.X = value1.X * scaleFactor;
//        result.Y = value1.Y * scaleFactor;
//    }

//    public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = value1.X * value2.X;
//        result.Y = value1.Y * value2.Y;
//    }

//    public static Vector2 Negate(Vector2 value)
//    {
//        value.X = -value.X;
//        value.Y = -value.Y;
//        return value;
//    }

//    public static void Negate(ref Vector2 value, out Vector2 result)
//    {
//        result.X = -value.X;
//        result.Y = -value.Y;
//    }

//    public void Normalize()
//    {
//        float len = Length();
//        if (len != 0)
//            this /= len;
//    }

//    public static Vector2 Normalize(Vector2 value)
//    {
//        float len = float.Sqrt(value.X * value.X + value.Y * value.Y);
//        if (len != 0)
//            value /= len;
//        return value;
//    }

//    public static void Normalize(ref Vector2 value, out Vector2 result)
//    {
//        float len = float.Sqrt(value.X * value.X + value.Y * value.Y);
//        result = len != 0 ? value / len : Zero;
//    }

//    public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount) =>
//        new(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));

//    public static void SmoothStep(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result) =>
//        result = new(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));

//    public static Vector2 Subtract(Vector2 value1, Vector2 value2)
//    {
//        value1.X -= value2.X;
//        value1.Y -= value2.Y;
//        return value1;
//    }

//    public static void Subtract(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
//    {
//        result.X = value1.X - value2.X;
//        result.Y = value1.Y - value2.Y;
//    }

//    public static Vector2 Transform(Vector2 position, Matrix matrix)
//    {
//        Transform(ref position, ref matrix, out position);
//        return position;
//    }

//    public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector2 result)
//    {
//        result = new Vector2(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
//    }

//    public static Vector2 Transform(Vector2 position, Quaternion quat)
//    {
//        Transform(ref position, ref quat, out position);
//        return position;
//    }

//    public static void Transform(ref Vector2 position, ref Quaternion quat, out Vector2 result)
//    {
//        Quaternion v = new Quaternion(position.X, position.Y, 0f, 0f);
//        Quaternion.Inverse(ref quat, out var i);
//        Quaternion.Multiply(ref quat, ref v, out var t);
//        Quaternion.Multiply(ref t, ref i, out v);
//        result = new Vector2(v.x, v.y);
//    }

//    public static void Transform(Vector2[] sourceArray, ref Matrix matrix, Vector2[] destinationArray)
//    {
//        Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
//    }

//    public static void Transform(Vector2[] sourceArray, int sourceIndex, ref Matrix matrix, Vector2[] destinationArray, int destinationIndex, int length)
//    {
//        for (int x = 0; x < length; x++)
//        {
//            Vector2 position = sourceArray[sourceIndex + x];
//            Vector2 destination = destinationArray[destinationIndex + x];
//            destination.X = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
//            destination.Y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
//            destinationArray[destinationIndex + x] = destination;
//        }
//    }

//    public static Vector2 TransformNormal(Vector2 normal, Matrix matrix)
//    {
//        TransformNormal(ref normal, ref matrix, out normal);
//        return normal;
//    }

//    public static void TransformNormal(ref Vector2 normal, ref Matrix matrix, out Vector2 result)
//    {
//        result = new Vector2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
//    }

//    public override string ToString()
//    {
//        CultureInfo currentCulture = CultureInfo.CurrentCulture;
//        return string.Format(currentCulture, "{{x:{0} y:{1}}}", new object[2]
//        {
//            X.ToString(currentCulture),
//            Y.ToString(currentCulture)
//        });
//    }

//    public Vector2 Rotate(float radians, Vector2 pivot)
//    {
//        float cosRadians = (float)Math.Cos(radians);
//        float sinRadians = (float)Math.Sin(radians);
//        Vector2 translatedPoint = new Vector2
//        {
//            X = X - pivot.X,
//            Y = Y - pivot.Y
//        };
//        return new Vector2
//        {
//            X = translatedPoint.X * cosRadians - translatedPoint.Y * sinRadians + pivot.X,
//            Y = translatedPoint.X * sinRadians + translatedPoint.Y * cosRadians + pivot.Y
//        };
//    }
//    #endregion

//    #region Operators
//    public static Vector2 operator -(Vector2 value)
//    {
//        value.X = 0f - value.X;
//        value.Y = 0f - value.Y;
//        return value;
//    }

//    public static bool operator ==(Vector2 value1, Vector2 value2)
//    {
//        if (value1.X == value2.X)
//        {
//            return value1.Y == value2.Y;
//        }
//        return false;
//    }

//    public static bool operator !=(Vector2 value1, Vector2 value2)
//    {
//        if (value1.X == value2.X)
//        {
//            return value1.Y != value2.Y;
//        }
//        return true;
//    }

//    public static Vector2 operator +(Vector2 value1, Vector2 value2)
//    {
//        value1.X += value2.X;
//        value1.Y += value2.Y;
//        return value1;
//    }

//    public static Vector2 operator -(Vector2 value1, Vector2 value2)
//    {
//        value1.X -= value2.X;
//        value1.Y -= value2.Y;
//        return value1;
//    }

//    public static Vector2 operator *(Vector2 value1, Vector2 value2)
//    {
//        value1.X *= value2.X;
//        value1.Y *= value2.Y;
//        return value1;
//    }

//    public static Vector2 operator *(Vector2 value, float scaleFactor)
//    {
//        value.X *= scaleFactor;
//        value.Y *= scaleFactor;
//        return value;
//    }

//    public static Vector2 operator *(float scaleFactor, Vector2 value)
//    {
//        value.X *= scaleFactor;
//        value.Y *= scaleFactor;
//        return value;
//    }

//    public static Vector2 operator /(Vector2 value1, Vector2 value2)
//    {
//        value1.X /= value2.X;
//        value1.Y /= value2.Y;
//        return value1;
//    }

//    public static Vector2 operator /(Vector2 value1, float divider)
//    {
//        float factor = 1f / divider;
//        value1.X *= factor;
//        value1.Y *= factor;
//        return value1;
//    }

//    public static implicit operator Vector2(Vector2 vec)
//    {
//        return new Vector2(vec.X, vec.Y);
//    }

//    public static implicit operator Vector2(Vector2 vec)
//    {
//        return new Vector2(vec.X, vec.Y);
//    }
//    #endregion
//}