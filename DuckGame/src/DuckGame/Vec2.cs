using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace DuckGame;

[Serializable]
public struct Vec2 : IEquatable<Vec2>
{
	private static Vec2 zeroVector = new Vec2(0f, 0f);

	private static Vec2 unitVector = new Vec2(1f, 1f);

	private static Vec2 unitxVector = new Vec2(1f, 0f);

	private static Vec2 unityVector = new Vec2(0f, 1f);

	private static Vec2 maxVector = new Vec2(float.MaxValue, float.MaxValue);

	private static Vec2 minVector = new Vec2(float.MinValue, float.MinValue);

	public float x;

	public float y;

	public float length => Length();

	public float lengthSq => LengthSquared();

	public static Vec2 Zero => zeroVector;

	public static Vec2 One => unitVector;

	public static Vec2 MaxValue => maxVector;

	public static Vec2 MinValue => minVector;

	public static Vec2 Unitx => unitxVector;

	public static Vec2 Unity => unityVector;

	public static Vec2 NetMin => new Vec2(-10000f, -10000f);

	public static Vec2 NetMax => new Vec2(10000f, 10000f);

	public Vec2 normalized
	{
		get
		{
			float len = (float)Math.Sqrt(x * x + y * y);
			if (len != 0f)
			{
				float val = 1f / len;
				return new Vec2(x * val, y * val);
			}
			return Zero;
		}
	}

	public Vec2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vec2(float value)
	{
		x = value;
		y = value;
	}

	public Vec2(Vec2 vec)
	{
		x = vec.x;
		y = vec.y;
	}

	public static Vec2 Add(Vec2 value1, Vec2 value2)
	{
		value1.x += value2.x;
		value1.y += value2.y;
		return value1;
	}

	public static void Add(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = value1.x + value2.x;
		result.y = value1.y + value2.y;
	}

	public static Vec2 Barycentric(Vec2 value1, Vec2 value2, Vec2 value3, float amount1, float amount2)
	{
		return new Vec2(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
	}

	public static void Barycentric(ref Vec2 value1, ref Vec2 value2, ref Vec2 value3, float amount1, float amount2, out Vec2 result)
	{
		result = new Vec2(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
	}

	public static Vec2 CatmullRom(Vec2 value1, Vec2 value2, Vec2 value3, Vec2 value4, float amount)
	{
		return new Vec2(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
	}

	public static void CatmullRom(ref Vec2 value1, ref Vec2 value2, ref Vec2 value3, ref Vec2 value4, float amount, out Vec2 result)
	{
		result = new Vec2(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
	}

	public static Vec2 Clamp(Vec2 value1, Vec2 min, Vec2 max)
	{
		return new Vec2(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y));
	}

	public static void Clamp(ref Vec2 value1, ref Vec2 min, ref Vec2 max, out Vec2 result)
	{
		result = new Vec2(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y));
	}

	public static float Distance(Vec2 value1, Vec2 value2)
	{
		float num = value1.x - value2.x;
		float v2 = value1.y - value2.y;
		return (float)Math.Sqrt(num * num + v2 * v2);
	}

	public static void Distance(ref Vec2 value1, ref Vec2 value2, out float result)
	{
		float v1 = value1.x - value2.x;
		float v2 = value1.y - value2.y;
		result = (float)Math.Sqrt(v1 * v1 + v2 * v2);
	}

	public static float DistanceSquared(Vec2 value1, Vec2 value2)
	{
		float num = value1.x - value2.x;
		float v2 = value1.y - value2.y;
		return num * num + v2 * v2;
	}

	public static void DistanceSquared(ref Vec2 value1, ref Vec2 value2, out float result)
	{
		float v1 = value1.x - value2.x;
		float v2 = value1.y - value2.y;
		result = v1 * v1 + v2 * v2;
	}

	public static Vec2 Divide(Vec2 value1, Vec2 value2)
	{
		value1.x /= value2.x;
		value1.y /= value2.y;
		return value1;
	}

	public static void Divide(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = value1.x / value2.x;
		result.y = value1.y / value2.y;
	}

	public static Vec2 Divide(Vec2 value1, float divider)
	{
		float factor = 1f / divider;
		value1.x *= factor;
		value1.y *= factor;
		return value1;
	}

	public static void Divide(ref Vec2 value1, float divider, out Vec2 result)
	{
		float factor = 1f / divider;
		result.x = value1.x * factor;
		result.y = value1.y * factor;
	}

	public static float Dot(Vec2 value1, Vec2 value2)
	{
		return value1.x * value2.x + value1.y * value2.y;
	}

	public static void Dot(ref Vec2 value1, ref Vec2 value2, out float result)
	{
		result = value1.x * value2.x + value1.y * value2.y;
	}

	public override bool Equals(object obj)
	{
		if (obj is Vec2)
		{
			return Equals((Vec2)obj);
		}
		return false;
	}

	public bool Equals(Vec2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public static Vec2 Reflect(Vec2 vector, Vec2 normal)
	{
		float val = 2f * (vector.x * normal.x + vector.y * normal.y);
		Vec2 result = default(Vec2);
		result.x = vector.x - normal.x * val;
		result.y = vector.y - normal.y * val;
		return result;
	}

	public static void Reflect(ref Vec2 vector, ref Vec2 normal, out Vec2 result)
	{
		float val = 2f * (vector.x * normal.x + vector.y * normal.y);
		result.x = vector.x - normal.x * val;
		result.y = vector.y - normal.y * val;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode();
	}

	public static Vec2 Hermite(Vec2 value1, Vec2 tangent1, Vec2 value2, Vec2 tangent2, float amount)
	{
		Vec2 result = default(Vec2);
		Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
		return result;
	}

	public static void Hermite(ref Vec2 value1, ref Vec2 tangent1, ref Vec2 value2, ref Vec2 tangent2, float amount, out Vec2 result)
	{
		result.x = MathHelper.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
		result.y = MathHelper.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
	}

	public float Length()
	{
		return (float)Math.Sqrt(x * x + y * y);
	}

	public float LengthSquared()
	{
		return x * x + y * y;
	}

	public static Vec2 Lerp(Vec2 value1, Vec2 value2, float amount)
	{
		return DuckGame.Lerp.Vec2Smooth(value1, value2, amount);
	}

	public static void Lerp(ref Vec2 value1, ref Vec2 value2, float amount, out Vec2 result)
	{
		result = new Vec2(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount));
	}

	public static Vec2 Max(Vec2 value1, Vec2 value2)
	{
		return new Vec2((value1.x > value2.x) ? value1.x : value2.x, (value1.y > value2.y) ? value1.y : value2.y);
	}

	public static void Max(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = ((value1.x > value2.x) ? value1.x : value2.x);
		result.y = ((value1.y > value2.y) ? value1.y : value2.y);
	}

	public static Vec2 Min(Vec2 value1, Vec2 value2)
	{
		return new Vec2((value1.x < value2.x) ? value1.x : value2.x, (value1.y < value2.y) ? value1.y : value2.y);
	}

	public static void Min(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = ((value1.x < value2.x) ? value1.x : value2.x);
		result.y = ((value1.y < value2.y) ? value1.y : value2.y);
	}

	public static Vec2 Multiply(Vec2 value1, Vec2 value2)
	{
		value1.x *= value2.x;
		value1.y *= value2.y;
		return value1;
	}

	public static Vec2 Multiply(Vec2 value1, float scaleFactor)
	{
		value1.x *= scaleFactor;
		value1.y *= scaleFactor;
		return value1;
	}

	public static void Multiply(ref Vec2 value1, float scaleFactor, out Vec2 result)
	{
		result.x = value1.x * scaleFactor;
		result.y = value1.y * scaleFactor;
	}

	public static void Multiply(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = value1.x * value2.x;
		result.y = value1.y * value2.y;
	}

	public static Vec2 Negate(Vec2 value)
	{
		value.x = 0f - value.x;
		value.y = 0f - value.y;
		return value;
	}

	public static void Negate(ref Vec2 value, out Vec2 result)
	{
		result.x = 0f - value.x;
		result.y = 0f - value.y;
	}

	public void Normalize()
	{
		float len = (float)Math.Sqrt(x * x + y * y);
		if (len != 0f)
		{
			float val = 1f / len;
			x *= val;
			y *= val;
		}
	}

	public static Vec2 Normalize(Vec2 value)
	{
		float len = (float)Math.Sqrt(value.x * value.x + value.y * value.y);
		if (len != 0f)
		{
			float val = 1f / len;
			value.x *= val;
			value.y *= val;
		}
		return value;
	}

	public static void Normalize(ref Vec2 value, out Vec2 result)
	{
		float len = (float)Math.Sqrt(value.x * value.x + value.y * value.y);
		if (len != 0f)
		{
			float val = 1f / len;
			result.x = value.x * val;
			result.y = value.y * val;
		}
		else
		{
			result = Zero;
		}
	}

	public static Vec2 SmoothStep(Vec2 value1, Vec2 value2, float amount)
	{
		return new Vec2(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount));
	}

	public static void SmoothStep(ref Vec2 value1, ref Vec2 value2, float amount, out Vec2 result)
	{
		result = new Vec2(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount));
	}

	public static Vec2 Subtract(Vec2 value1, Vec2 value2)
	{
		value1.x -= value2.x;
		value1.y -= value2.y;
		return value1;
	}

	public static void Subtract(ref Vec2 value1, ref Vec2 value2, out Vec2 result)
	{
		result.x = value1.x - value2.x;
		result.y = value1.y - value2.y;
	}

	public static Vec2 Transform(Vec2 position, Matrix matrix)
	{
		Transform(ref position, ref matrix, out position);
		return position;
	}

	public static void Transform(ref Vec2 position, ref Matrix matrix, out Vec2 result)
	{
		result = new Vec2(position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41, position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42);
	}

	public static Vec2 Transform(Vec2 position, Quaternion quat)
	{
		Transform(ref position, ref quat, out position);
		return position;
	}

	public static void Transform(ref Vec2 position, ref Quaternion quat, out Vec2 result)
	{
		Quaternion v = new Quaternion(position.x, position.y, 0f, 0f);
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
			destination.x = position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41;
			destination.y = position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42;
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
		result = new Vec2(normal.x * matrix.M11 + normal.y * matrix.M21, normal.x * matrix.M12 + normal.y * matrix.M22);
	}

	public override string ToString()
	{
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		return string.Format(currentCulture, "{{x:{0} y:{1}}}", new object[2]
		{
			x.ToString(currentCulture),
			y.ToString(currentCulture)
		});
	}

	public static Vec2 operator -(Vec2 value)
	{
		value.x = 0f - value.x;
		value.y = 0f - value.y;
		return value;
	}

	public static bool operator ==(Vec2 value1, Vec2 value2)
	{
		if (value1.x == value2.x)
		{
			return value1.y == value2.y;
		}
		return false;
	}

	public static bool operator !=(Vec2 value1, Vec2 value2)
	{
		if (value1.x == value2.x)
		{
			return value1.y != value2.y;
		}
		return true;
	}

	public static Vec2 operator +(Vec2 value1, Vec2 value2)
	{
		value1.x += value2.x;
		value1.y += value2.y;
		return value1;
	}

	public static Vec2 operator -(Vec2 value1, Vec2 value2)
	{
		value1.x -= value2.x;
		value1.y -= value2.y;
		return value1;
	}

	public static Vec2 operator *(Vec2 value1, Vec2 value2)
	{
		value1.x *= value2.x;
		value1.y *= value2.y;
		return value1;
	}

	public static Vec2 operator *(Vec2 value, float scaleFactor)
	{
		value.x *= scaleFactor;
		value.y *= scaleFactor;
		return value;
	}

	public static Vec2 operator *(float scaleFactor, Vec2 value)
	{
		value.x *= scaleFactor;
		value.y *= scaleFactor;
		return value;
	}

	public static Vec2 operator /(Vec2 value1, Vec2 value2)
	{
		value1.x /= value2.x;
		value1.y /= value2.y;
		return value1;
	}

	public static Vec2 operator /(Vec2 value1, float divider)
	{
		float factor = 1f / divider;
		value1.x *= factor;
		value1.y *= factor;
		return value1;
	}

	public Vec2 Rotate(float radians, Vec2 pivot)
	{
		float cosRadians = (float)Math.Cos(radians);
		float sinRadians = (float)Math.Sin(radians);
		Vec2 translatedPoint = new Vec2
		{
			x = x - pivot.x,
			y = y - pivot.y
		};
		return new Vec2
		{
			x = translatedPoint.x * cosRadians - translatedPoint.y * sinRadians + pivot.x,
			y = translatedPoint.x * sinRadians + translatedPoint.y * cosRadians + pivot.y
		};
	}

	public static implicit operator Vector2(Vec2 vec)
	{
		return new Vector2(vec.x, vec.y);
	}

	public static implicit operator Vec2(Vector2 vec)
	{
		return new Vec2(vec.X, vec.Y);
	}
}
