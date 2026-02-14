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