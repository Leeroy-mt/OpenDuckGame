using System;
using System.Text;

namespace DuckGame;

[Serializable]
public struct Quaternion : IEquatable<Quaternion>
{
	public float x;

	public float y;

	public float z;

	public float w;

	private static Quaternion identity = new Quaternion(0f, 0f, 0f, 1f);

	public static Quaternion Identity => identity;

	public Quaternion(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public Quaternion(Vec3 vectorPart, float scalarPart)
	{
		x = vectorPart.x;
		y = vectorPart.y;
		z = vectorPart.z;
		w = scalarPart;
	}

	public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
	{
		quaternion1.x += quaternion2.x;
		quaternion1.y += quaternion2.y;
		quaternion1.z += quaternion2.z;
		quaternion1.w += quaternion2.w;
		return quaternion1;
	}

	public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		result.w = quaternion1.w + quaternion2.w;
		result.x = quaternion1.x + quaternion2.x;
		result.y = quaternion1.y + quaternion2.y;
		result.z = quaternion1.z + quaternion2.z;
	}

	public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.x = value2.x * value1.w + value1.x * value2.w + value2.y * value1.z - value2.z * value1.y;
		quaternion.y = value2.y * value1.w + value1.y * value2.w + value2.z * value1.x - value2.x * value1.z;
		quaternion.z = value2.z * value1.w + value1.z * value2.w + value2.x * value1.y - value2.y * value1.x;
		quaternion.w = value2.w * value1.w - (value2.x * value1.x + value2.y * value1.y) + value2.z * value1.z;
		return quaternion;
	}

	public void Conjugate()
	{
		x = 0f - x;
		y = 0f - y;
		z = 0f - z;
	}

	public static Quaternion Conjugate(Quaternion value)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.x = 0f - value.x;
		quaternion.y = 0f - value.y;
		quaternion.z = 0f - value.z;
		quaternion.w = value.w;
		return quaternion;
	}

	public static void Conjugate(ref Quaternion value, out Quaternion result)
	{
		result.x = 0f - value.x;
		result.y = 0f - value.y;
		result.z = 0f - value.z;
		result.w = value.w;
	}

	public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
	{
		result.x = value2.x * value1.w + value1.x * value2.w + value2.y * value1.z - value2.z * value1.y;
		result.y = value2.y * value1.w + value1.y * value2.w + value2.z * value1.x - value2.x * value1.z;
		result.z = value2.z * value1.w + value1.z * value2.w + value2.x * value1.y - value2.y * value1.x;
		result.w = value2.w * value1.w - (value2.x * value1.x + value2.y * value1.y) + value2.z * value1.z;
	}

	public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.x = (float)Math.Cos(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) + (float)Math.Sin(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
		quaternion.y = (float)Math.Sin(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) - (float)Math.Cos(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
		quaternion.z = (float)Math.Cos(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f) - (float)Math.Sin(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f);
		quaternion.w = (float)Math.Cos(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) + (float)Math.Sin(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
		return quaternion;
	}

	public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
	{
		result.x = (float)Math.Cos(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) + (float)Math.Sin(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
		result.y = (float)Math.Sin(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) - (float)Math.Cos(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
		result.z = (float)Math.Cos(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f) - (float)Math.Sin(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f);
		result.w = (float)Math.Cos(yaw * 0.5f) * (float)Math.Cos(pitch * 0.5f) * (float)Math.Cos(roll * 0.5f) + (float)Math.Sin(yaw * 0.5f) * (float)Math.Sin(pitch * 0.5f) * (float)Math.Sin(roll * 0.5f);
	}

	public static Quaternion CreateFromAxisAngle(Vec3 axis, float angle)
	{
		float sin_a = (float)Math.Sin(angle / 2f);
		return new Quaternion(axis.x * sin_a, axis.y * sin_a, axis.z * sin_a, (float)Math.Cos(angle / 2f));
	}

	public static void CreateFromAxisAngle(ref Vec3 axis, float angle, out Quaternion result)
	{
		float sin_a = (float)Math.Sin(angle / 2f);
		result.x = axis.x * sin_a;
		result.y = axis.y * sin_a;
		result.z = axis.z * sin_a;
		result.w = (float)Math.Cos(angle / 2f);
	}

	public static Quaternion CreateFromRotationMatrix(Matrix matrix)
	{
		Quaternion result = default(Quaternion);
		if (matrix.M11 + matrix.M22 + matrix.M33 > 0f)
		{
			float M1 = (float)Math.Sqrt(matrix.M11 + matrix.M22 + matrix.M33 + 1f);
			result.w = M1 * 0.5f;
			M1 = 0.5f / M1;
			result.x = (matrix.M23 - matrix.M32) * M1;
			result.y = (matrix.M31 - matrix.M13) * M1;
			result.z = (matrix.M12 - matrix.M21) * M1;
			return result;
		}
		if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			float M2 = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			float M3 = 0.5f / M2;
			result.x = 0.5f * M2;
			result.y = (matrix.M12 + matrix.M21) * M3;
			result.z = (matrix.M13 + matrix.M31) * M3;
			result.w = (matrix.M23 - matrix.M32) * M3;
			return result;
		}
		if (matrix.M22 > matrix.M33)
		{
			float M4 = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			float M5 = 0.5f / M4;
			result.x = (matrix.M21 + matrix.M12) * M5;
			result.y = 0.5f * M4;
			result.z = (matrix.M32 + matrix.M23) * M5;
			result.w = (matrix.M31 - matrix.M13) * M5;
			return result;
		}
		float M6 = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
		float M7 = 0.5f / M6;
		result.x = (matrix.M31 + matrix.M13) * M7;
		result.y = (matrix.M32 + matrix.M23) * M7;
		result.z = 0.5f * M6;
		result.w = (matrix.M12 - matrix.M21) * M7;
		return result;
	}

	public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
	{
		if (matrix.M11 + matrix.M22 + matrix.M33 > 0f)
		{
			float M1 = (float)Math.Sqrt(matrix.M11 + matrix.M22 + matrix.M33 + 1f);
			result.w = M1 * 0.5f;
			M1 = 0.5f / M1;
			result.x = (matrix.M23 - matrix.M32) * M1;
			result.y = (matrix.M31 - matrix.M13) * M1;
			result.z = (matrix.M12 - matrix.M21) * M1;
		}
		else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			float M2 = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			float M3 = 0.5f / M2;
			result.x = 0.5f * M2;
			result.y = (matrix.M12 + matrix.M21) * M3;
			result.z = (matrix.M13 + matrix.M31) * M3;
			result.w = (matrix.M23 - matrix.M32) * M3;
		}
		else if (matrix.M22 > matrix.M33)
		{
			float M4 = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			float M5 = 0.5f / M4;
			result.x = (matrix.M21 + matrix.M12) * M5;
			result.y = 0.5f * M4;
			result.z = (matrix.M32 + matrix.M23) * M5;
			result.w = (matrix.M31 - matrix.M13) * M5;
		}
		else
		{
			float M6 = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
			float M7 = 0.5f / M6;
			result.x = (matrix.M31 + matrix.M13) * M7;
			result.y = (matrix.M32 + matrix.M23) * M7;
			result.z = 0.5f * M6;
			result.w = (matrix.M12 - matrix.M21) * M7;
		}
	}

	public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
	{
		float w5 = 1f / (quaternion2.x * quaternion2.x + quaternion2.y * quaternion2.y + quaternion2.z * quaternion2.z + quaternion2.w * quaternion2.w);
		float w6 = (0f - quaternion2.x) * w5;
		float w7 = (0f - quaternion2.y) * w5;
		float w8 = (0f - quaternion2.z) * w5;
		float w9 = quaternion2.w * w5;
		Quaternion result = default(Quaternion);
		result.x = quaternion1.x * w9 + w6 * quaternion1.w + (quaternion1.y * w8 - quaternion1.z * w7);
		result.y = quaternion1.y * w9 + w7 * quaternion1.w + (quaternion1.z * w6 - quaternion1.x * w8);
		result.z = quaternion1.z * w9 + w8 * quaternion1.w + (quaternion1.x * w7 - quaternion1.y * w6);
		result.w = quaternion1.w * quaternion2.w * w5 - (quaternion1.x * w6 + quaternion1.y * w7 + quaternion1.z * w8);
		return result;
	}

	public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		float w5 = 1f / (quaternion2.x * quaternion2.x + quaternion2.y * quaternion2.y + quaternion2.z * quaternion2.z + quaternion2.w * quaternion2.w);
		float w6 = (0f - quaternion2.x) * w5;
		float w7 = (0f - quaternion2.y) * w5;
		float w8 = (0f - quaternion2.z) * w5;
		float w9 = quaternion2.w * w5;
		result.x = quaternion1.x * w9 + w6 * quaternion1.w + (quaternion1.y * w8 - quaternion1.z * w7);
		result.y = quaternion1.y * w9 + w7 * quaternion1.w + (quaternion1.z * w6 - quaternion1.x * w8);
		result.z = quaternion1.z * w9 + w8 * quaternion1.w + (quaternion1.x * w7 - quaternion1.y * w6);
		result.w = quaternion1.w * quaternion2.w * w5 - (quaternion1.x * w6 + quaternion1.y * w7 + quaternion1.z * w8);
	}

	public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
	{
		return quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;
	}

	public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
	{
		result = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Quaternion))
		{
			return false;
		}
		return this == (Quaternion)obj;
	}

	public bool Equals(Quaternion other)
	{
		if (x == other.x && y == other.y && z == other.z)
		{
			return w == other.w;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode() + z.GetHashCode() + w.GetHashCode();
	}

	public static Quaternion Inverse(Quaternion quaternion)
	{
		float m1 = 1f / (quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
		Quaternion result = default(Quaternion);
		result.x = (0f - quaternion.x) * m1;
		result.y = (0f - quaternion.y) * m1;
		result.z = (0f - quaternion.z) * m1;
		result.w = quaternion.w * m1;
		return result;
	}

	public static void Inverse(ref Quaternion quaternion, out Quaternion result)
	{
		float m1 = 1f / (quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
		result.x = (0f - quaternion.x) * m1;
		result.y = (0f - quaternion.y) * m1;
		result.z = (0f - quaternion.z) * m1;
		result.w = quaternion.w * m1;
	}

	public float Length()
	{
		return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
	}

	public float LengthSquared()
	{
		return x * x + y * y + z * z + w * w;
	}

	public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
	{
		float f2 = 1f - amount;
		Quaternion result = default(Quaternion);
		if (quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w >= 0f)
		{
			result.x = f2 * quaternion1.x + amount * quaternion2.x;
			result.y = f2 * quaternion1.y + amount * quaternion2.y;
			result.z = f2 * quaternion1.z + amount * quaternion2.z;
			result.w = f2 * quaternion1.w + amount * quaternion2.w;
		}
		else
		{
			result.x = f2 * quaternion1.x - amount * quaternion2.x;
			result.y = f2 * quaternion1.y - amount * quaternion2.y;
			result.z = f2 * quaternion1.z - amount * quaternion2.z;
			result.w = f2 * quaternion1.w - amount * quaternion2.w;
		}
		float f4 = result.x * result.x + result.y * result.y + result.z * result.z + result.w * result.w;
		float f5 = 1f / (float)Math.Sqrt(f4);
		result.x *= f5;
		result.y *= f5;
		result.z *= f5;
		result.w *= f5;
		return result;
	}

	public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
	{
		float m2 = 1f - amount;
		if (quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w >= 0f)
		{
			result.x = m2 * quaternion1.x + amount * quaternion2.x;
			result.y = m2 * quaternion1.y + amount * quaternion2.y;
			result.z = m2 * quaternion1.z + amount * quaternion2.z;
			result.w = m2 * quaternion1.w + amount * quaternion2.w;
		}
		else
		{
			result.x = m2 * quaternion1.x - amount * quaternion2.x;
			result.y = m2 * quaternion1.y - amount * quaternion2.y;
			result.z = m2 * quaternion1.z - amount * quaternion2.z;
			result.w = m2 * quaternion1.w - amount * quaternion2.w;
		}
		float m4 = result.x * result.x + result.y * result.y + result.z * result.z + result.w * result.w;
		float m5 = 1f / (float)Math.Sqrt(m4);
		result.x *= m5;
		result.y *= m5;
		result.z *= m5;
		result.w *= m5;
	}

	public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
	{
		float q4 = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;
		bool flag = false;
		if (q4 < 0f)
		{
			flag = true;
			q4 = 0f - q4;
		}
		float q5;
		float q6;
		if (q4 > 0.999999f)
		{
			q5 = 1f - amount;
			q6 = (flag ? (0f - amount) : amount);
		}
		else
		{
			float q7 = (float)Math.Acos(q4);
			float q8 = (float)(1.0 / Math.Sin(q7));
			q5 = (float)Math.Sin((1f - amount) * q7) * q8;
			q6 = (flag ? ((float)(0.0 - Math.Sin(amount * q7)) * q8) : ((float)Math.Sin(amount * q7) * q8));
		}
		Quaternion result = default(Quaternion);
		result.x = q5 * quaternion1.x + q6 * quaternion2.x;
		result.y = q5 * quaternion1.y + q6 * quaternion2.y;
		result.z = q5 * quaternion1.z + q6 * quaternion2.z;
		result.w = q5 * quaternion1.w + q6 * quaternion2.w;
		return result;
	}

	public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
	{
		float q4 = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;
		bool flag = false;
		if (q4 < 0f)
		{
			flag = true;
			q4 = 0f - q4;
		}
		float q5;
		float q6;
		if (q4 > 0.999999f)
		{
			q5 = 1f - amount;
			q6 = (flag ? (0f - amount) : amount);
		}
		else
		{
			float q7 = (float)Math.Acos(q4);
			float q8 = (float)(1.0 / Math.Sin(q7));
			q5 = (float)Math.Sin((1f - amount) * q7) * q8;
			q6 = (flag ? ((float)(0.0 - Math.Sin(amount * q7)) * q8) : ((float)Math.Sin(amount * q7) * q8));
		}
		result.x = q5 * quaternion1.x + q6 * quaternion2.x;
		result.y = q5 * quaternion1.y + q6 * quaternion2.y;
		result.z = q5 * quaternion1.z + q6 * quaternion2.z;
		result.w = q5 * quaternion1.w + q6 * quaternion2.w;
	}

	public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
	{
		quaternion1.x -= quaternion2.x;
		quaternion1.y -= quaternion2.y;
		quaternion1.z -= quaternion2.z;
		quaternion1.w -= quaternion2.w;
		return quaternion1;
	}

	public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		result.x = quaternion1.x - quaternion2.x;
		result.y = quaternion1.y - quaternion2.y;
		result.z = quaternion1.z - quaternion2.z;
		result.w = quaternion1.w - quaternion2.w;
	}

	public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
	{
		float f12 = quaternion1.y * quaternion2.z - quaternion1.z * quaternion2.y;
		float f13 = quaternion1.z * quaternion2.x - quaternion1.x * quaternion2.z;
		float f14 = quaternion1.x * quaternion2.y - quaternion1.y * quaternion2.x;
		float f15 = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z;
		Quaternion result = default(Quaternion);
		result.x = quaternion1.x * quaternion2.w + quaternion2.x * quaternion1.w + f12;
		result.y = quaternion1.y * quaternion2.w + quaternion2.y * quaternion1.w + f13;
		result.z = quaternion1.z * quaternion2.w + quaternion2.z * quaternion1.w + f14;
		result.w = quaternion1.w * quaternion2.w - f15;
		return result;
	}

	public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
	{
		quaternion1.x *= scaleFactor;
		quaternion1.y *= scaleFactor;
		quaternion1.z *= scaleFactor;
		quaternion1.w *= scaleFactor;
		return quaternion1;
	}

	public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
	{
		result.x = quaternion1.x * scaleFactor;
		result.y = quaternion1.y * scaleFactor;
		result.z = quaternion1.z * scaleFactor;
		result.w = quaternion1.w * scaleFactor;
	}

	public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		float f12 = quaternion1.y * quaternion2.z - quaternion1.z * quaternion2.y;
		float f13 = quaternion1.z * quaternion2.x - quaternion1.x * quaternion2.z;
		float f14 = quaternion1.x * quaternion2.y - quaternion1.y * quaternion2.x;
		float f15 = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z;
		result.x = quaternion1.x * quaternion2.w + quaternion2.x * quaternion1.w + f12;
		result.y = quaternion1.y * quaternion2.w + quaternion2.y * quaternion1.w + f13;
		result.z = quaternion1.z * quaternion2.w + quaternion2.z * quaternion1.w + f14;
		result.w = quaternion1.w * quaternion2.w - f15;
	}

	public static Quaternion Negate(Quaternion quaternion)
	{
		Quaternion result = default(Quaternion);
		result.x = 0f - quaternion.x;
		result.y = 0f - quaternion.y;
		result.z = 0f - quaternion.z;
		result.w = 0f - quaternion.w;
		return result;
	}

	public static void Negate(ref Quaternion quaternion, out Quaternion result)
	{
		result.x = 0f - quaternion.x;
		result.y = 0f - quaternion.y;
		result.z = 0f - quaternion.z;
		result.w = 0f - quaternion.w;
	}

	public void Normalize()
	{
		float f1 = 1f / (float)Math.Sqrt(x * x + y * y + z * z + w * w);
		x *= f1;
		y *= f1;
		z *= f1;
		w *= f1;
	}

	public static Quaternion Normalize(Quaternion quaternion)
	{
		float f1 = 1f / (float)Math.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
		Quaternion result = default(Quaternion);
		result.x = quaternion.x * f1;
		result.y = quaternion.y * f1;
		result.z = quaternion.z * f1;
		result.w = quaternion.w * f1;
		return result;
	}

	public static void Normalize(ref Quaternion quaternion, out Quaternion result)
	{
		float f1 = 1f / (float)Math.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
		result.x = quaternion.x * f1;
		result.y = quaternion.y * f1;
		result.z = quaternion.z * f1;
		result.w = quaternion.w * f1;
	}

	public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
	{
		quaternion1.x += quaternion2.x;
		quaternion1.y += quaternion2.y;
		quaternion1.z += quaternion2.z;
		quaternion1.w += quaternion2.w;
		return quaternion1;
	}

	public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
	{
		float w5 = 1f / (quaternion2.x * quaternion2.x + quaternion2.y * quaternion2.y + quaternion2.z * quaternion2.z + quaternion2.w * quaternion2.w);
		float w6 = (0f - quaternion2.x) * w5;
		float w7 = (0f - quaternion2.y) * w5;
		float w8 = (0f - quaternion2.z) * w5;
		float w9 = quaternion2.w * w5;
		Quaternion result = default(Quaternion);
		result.x = quaternion1.x * w9 + w6 * quaternion1.w + (quaternion1.y * w8 - quaternion1.z * w7);
		result.y = quaternion1.y * w9 + w7 * quaternion1.w + (quaternion1.z * w6 - quaternion1.x * w8);
		result.z = quaternion1.z * w9 + w8 * quaternion1.w + (quaternion1.x * w7 - quaternion1.y * w6);
		result.w = quaternion1.w * quaternion2.w * w5 - (quaternion1.x * w6 + quaternion1.y * w7 + quaternion1.z * w8);
		return result;
	}

	public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.x == quaternion2.x && quaternion1.y == quaternion2.y && quaternion1.z == quaternion2.z)
		{
			return quaternion1.w == quaternion2.w;
		}
		return false;
	}

	public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.x == quaternion2.x && quaternion1.y == quaternion2.y && quaternion1.z == quaternion2.z)
		{
			return quaternion1.w != quaternion2.w;
		}
		return true;
	}

	public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
	{
		float f12 = quaternion1.y * quaternion2.z - quaternion1.z * quaternion2.y;
		float f13 = quaternion1.z * quaternion2.x - quaternion1.x * quaternion2.z;
		float f14 = quaternion1.x * quaternion2.y - quaternion1.y * quaternion2.x;
		float f15 = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z;
		Quaternion result = default(Quaternion);
		result.x = quaternion1.x * quaternion2.w + quaternion2.x * quaternion1.w + f12;
		result.y = quaternion1.y * quaternion2.w + quaternion2.y * quaternion1.w + f13;
		result.z = quaternion1.z * quaternion2.w + quaternion2.z * quaternion1.w + f14;
		result.w = quaternion1.w * quaternion2.w - f15;
		return result;
	}

	public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
	{
		quaternion1.x *= scaleFactor;
		quaternion1.y *= scaleFactor;
		quaternion1.z *= scaleFactor;
		quaternion1.w *= scaleFactor;
		return quaternion1;
	}

	public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
	{
		quaternion1.x -= quaternion2.x;
		quaternion1.y -= quaternion2.y;
		quaternion1.z -= quaternion2.z;
		quaternion1.w -= quaternion2.w;
		return quaternion1;
	}

	public static Quaternion operator -(Quaternion quaternion)
	{
		quaternion.x = 0f - quaternion.x;
		quaternion.y = 0f - quaternion.y;
		quaternion.z = 0f - quaternion.z;
		quaternion.w = 0f - quaternion.w;
		return quaternion;
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
}
