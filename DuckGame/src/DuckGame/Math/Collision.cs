using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public static class Collision
{
    public static void Initialize()
    {
    }

    public static bool Point(Vector2 point, Thing t)
    {
        if (point.X >= t.left && point.X <= t.right && point.Y >= t.top && point.Y <= t.bottom)
        {
            return true;
        }
        return false;
    }

    public static bool Point(Vector2 point, RectangleF r)
    {
        if (point.X >= r.Left && point.X <= r.Right && point.Y >= r.Top && point.Y <= r.Bottom)
        {
            return true;
        }
        return false;
    }

    public static bool Line(Vector2 point1, Vector2 point2, Thing t)
    {
        double a_rectangleMinX = t.left;
        double a_rectangleMinY = t.top;
        double a_rectangleMaxX = t.right;
        double a_rectangleMaxY = t.bottom;
        double minX = point1.X;
        double maxX = point2.X;
        if (point1.X > point2.X)
        {
            minX = point2.X;
            maxX = point1.X;
        }
        if (maxX > a_rectangleMaxX)
        {
            maxX = a_rectangleMaxX;
        }
        if (minX < a_rectangleMinX)
        {
            minX = a_rectangleMinX;
        }
        if (minX > maxX)
        {
            return false;
        }
        double minY = point1.Y;
        double maxY = point2.Y;
        double dx = point2.X - point1.X;
        if (Math.Abs(dx) > 1E-07)
        {
            double a = (double)(point2.Y - point1.Y) / dx;
            double b = (double)point1.Y - a * (double)point1.X;
            minY = a * minX + b;
            maxY = a * maxX + b;
        }
        if (minY > maxY)
        {
            double num = maxY;
            maxY = minY;
            minY = num;
        }
        if (maxY > a_rectangleMaxY)
        {
            maxY = a_rectangleMaxY;
        }
        if (minY < a_rectangleMinY)
        {
            minY = a_rectangleMinY;
        }
        if (minY > maxY)
        {
            return false;
        }
        return true;
    }

    public static bool Line(Vector2 point1, Vector2 point2, RectangleF rect)
    {
        double a_rectangleMinX = rect.X;
        double a_rectangleMinY = rect.Y;
        double a_rectangleMaxX = rect.X + rect.Width;
        double a_rectangleMaxY = rect.Y + rect.Height;
        double minX = point1.X;
        double maxX = point2.X;
        if (point1.X > point2.X)
        {
            minX = point2.X;
            maxX = point1.X;
        }
        if (maxX > a_rectangleMaxX)
        {
            maxX = a_rectangleMaxX;
        }
        if (minX < a_rectangleMinX)
        {
            minX = a_rectangleMinX;
        }
        if (minX > maxX)
        {
            return false;
        }
        double minY = point1.Y;
        double maxY = point2.Y;
        double dx = point2.X - point1.X;
        if (Math.Abs(dx) > 1E-07)
        {
            double a = (double)(point2.Y - point1.Y) / dx;
            double b = (double)point1.Y - a * (double)point1.X;
            minY = a * minX + b;
            maxY = a * maxX + b;
        }
        if (minY > maxY)
        {
            double num = maxY;
            maxY = minY;
            minY = num;
        }
        if (maxY > a_rectangleMaxY)
        {
            maxY = a_rectangleMaxY;
        }
        if (minY < a_rectangleMinY)
        {
            minY = a_rectangleMinY;
        }
        if (minY > maxY)
        {
            return false;
        }
        return true;
    }

    public static bool CCW(Vector2 A, Vector2 B, Vector2 C)
    {
        return (C.Y - A.Y) * (B.X - A.X) > (B.Y - A.Y) * (C.X - A.X);
    }

    public static bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        if (CCW(p1, p3, p4) != CCW(p2, p3, p4))
        {
            return CCW(p1, p2, p3) != CCW(p1, p2, p4);
        }
        return false;
    }

    public static bool Circle(Vector2 center, float radius, Thing t)
    {
        Vector2 closestPoint = center;
        if (center.X < t.left)
        {
            closestPoint.X = t.left;
        }
        else if (center.X > t.right)
        {
            closestPoint.X = t.right;
        }
        if (center.Y < t.top)
        {
            closestPoint.Y = t.top;
        }
        else if (center.Y > t.bottom)
        {
            closestPoint.Y = t.bottom;
        }
        Vector2 diff = closestPoint - center;
        if (diff.X * diff.X + diff.Y * diff.Y > radius * radius)
        {
            return false;
        }
        return true;
    }

    public static bool Circle(Vector2 center, float radius, RectangleF t)
    {
        Vector2 closestPoint = center;
        if (center.X < t.Left)
        {
            closestPoint.X = t.Left;
        }
        else if (center.X > t.Right)
        {
            closestPoint.X = t.Right;
        }
        if (center.Y < t.Top)
        {
            closestPoint.Y = t.Top;
        }
        else if (center.Y > t.Bottom)
        {
            closestPoint.Y = t.Bottom;
        }
        Vector2 diff = closestPoint - center;
        if (diff.X * diff.X + diff.Y * diff.Y > radius * radius)
        {
            return false;
        }
        return true;
    }

    public static bool Rect(Vector2 tl1, Vector2 br1, Thing t)
    {
        if (!(br1.Y < t.top) && !(tl1.Y > t.bottom) && !(tl1.X > t.right))
        {
            return !(br1.X < t.left);
        }
        return false;
    }

    public static bool Rect(Vector2 tl1, Vector2 br1, RectangleF t)
    {
        if (!(br1.Y < t.Y) && !(tl1.Y > t.Bottom) && !(tl1.X > t.Right))
        {
            return !(br1.X < t.X);
        }
        return false;
    }

    public static bool Rect(RectangleF r1, RectangleF r2)
    {
        if (!(r1.Y + r1.Height < r2.Y) && !(r1.Y > r2.Y + r2.Height) && !(r1.X > r2.X + r2.Width))
        {
            return !(r1.X + r1.Width < r2.X);
        }
        return false;
    }

    public static bool Rect(RectangleF r1, Vector4 r2)
    {
        if (!(r1.Y + r1.Height < r2.Y) && !(r1.Y > r2.Y + r2.W) && !(r1.X > r2.X + r2.Z))
        {
            return !(r1.X + r1.Width < r2.X);
        }
        return false;
    }

    public static Vector2 LineIntersectPoint(Vector2 line1V1, Vector2 line1V2, Vector2 line2V1, Vector2 line2V2)
    {
        float A1 = line1V2.Y - line1V1.Y;
        float B1 = line1V1.X - line1V2.X;
        float C1 = A1 * line1V1.X + B1 * line1V1.Y;
        float A2 = line2V2.Y - line2V1.Y;
        float B2 = line2V1.X - line2V2.X;
        float C2 = A2 * line2V1.X + B2 * line2V1.Y;
        float det = A1 * B2 - A2 * B1;
        if (det == 0f)
        {
            return Vector2.Zero;
        }
        float x = (B2 * C1 - B1 * C2) / det;
        float y = (A1 * C2 - A2 * C1) / det;
        return new Vector2(x, y);
    }

    public static Vector2 LinePoint(Vector2 point1, Vector2 point2, Thing thing)
    {
        Vector2 dif = point2 - point1;
        float[] p = new float[4]
        {
            0f - dif.X,
            dif.X,
            0f - dif.Y,
            dif.Y
        };
        float[] q = new float[4]
        {
            point1.X - thing.left,
            thing.right - point1.X,
            point1.Y - thing.top,
            thing.bottom - point1.Y
        };
        float u1 = float.NegativeInfinity;
        float u2 = float.PositiveInfinity;
        for (int i = 0; i < 4; i++)
        {
            if (p[i] == 0f)
            {
                if (q[i] < 0f)
                {
                    return Vector2.Zero;
                }
                continue;
            }
            float t = q[i] / p[i];
            if (p[i] < 0f && u1 < t)
            {
                u1 = t;
            }
            else if (p[i] > 0f && u2 > t)
            {
                u2 = t;
            }
        }
        if (u1 > u2 || u1 > 1f || u1 < 0f)
        {
            return Vector2.Zero;
        }
        return new Vector2(point1.X + u1 * dif.X, point1.Y + u1 * dif.Y);
    }

    public static Vector2 LinePoint(Vector2 point1, Vector2 point2, RectangleF rect)
    {
        Vector2 dif = point2 - point1;
        float[] p = new float[4]
        {
            0f - dif.X,
            dif.X,
            0f - dif.Y,
            dif.Y
        };
        float[] q = new float[4]
        {
            point1.X - rect.X,
            rect.X + rect.Width - point1.X,
            point1.Y - rect.Y,
            rect.Y + rect.Height - point1.Y
        };
        float u1 = float.NegativeInfinity;
        float u2 = float.PositiveInfinity;
        for (int i = 0; i < 4; i++)
        {
            if (p[i] == 0f)
            {
                if (q[i] < 0f)
                {
                    return Vector2.Zero;
                }
                continue;
            }
            float t = q[i] / p[i];
            if (p[i] < 0f && u1 < t)
            {
                u1 = t;
            }
            else if (p[i] > 0f && u2 > t)
            {
                u2 = t;
            }
        }
        if (u1 > u2 || u1 > 1f || u1 < 0f)
        {
            return Vector2.Zero;
        }
        return new Vector2(point1.X + u1 * dif.X, point1.Y + u1 * dif.Y);
    }
}
