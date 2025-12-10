using System;

namespace DuckGame;

public static class Collision
{
    public static void Initialize()
    {
    }

    public static bool Point(Vec2 point, Thing t)
    {
        if (point.x >= t.left && point.x <= t.right && point.y >= t.top && point.y <= t.bottom)
        {
            return true;
        }
        return false;
    }

    public static bool Point(Vec2 point, Rectangle r)
    {
        if (point.x >= r.Left && point.x <= r.Right && point.y >= r.Top && point.y <= r.Bottom)
        {
            return true;
        }
        return false;
    }

    public static bool Line(Vec2 point1, Vec2 point2, Thing t)
    {
        double a_rectangleMinX = t.left;
        double a_rectangleMinY = t.top;
        double a_rectangleMaxX = t.right;
        double a_rectangleMaxY = t.bottom;
        double minX = point1.x;
        double maxX = point2.x;
        if (point1.x > point2.x)
        {
            minX = point2.x;
            maxX = point1.x;
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
        double minY = point1.y;
        double maxY = point2.y;
        double dx = point2.x - point1.x;
        if (Math.Abs(dx) > 1E-07)
        {
            double a = (double)(point2.y - point1.y) / dx;
            double b = (double)point1.y - a * (double)point1.x;
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

    public static bool Line(Vec2 point1, Vec2 point2, Rectangle rect)
    {
        double a_rectangleMinX = rect.x;
        double a_rectangleMinY = rect.y;
        double a_rectangleMaxX = rect.x + rect.width;
        double a_rectangleMaxY = rect.y + rect.height;
        double minX = point1.x;
        double maxX = point2.x;
        if (point1.x > point2.x)
        {
            minX = point2.x;
            maxX = point1.x;
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
        double minY = point1.y;
        double maxY = point2.y;
        double dx = point2.x - point1.x;
        if (Math.Abs(dx) > 1E-07)
        {
            double a = (double)(point2.y - point1.y) / dx;
            double b = (double)point1.y - a * (double)point1.x;
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

    public static bool CCW(Vec2 A, Vec2 B, Vec2 C)
    {
        return (C.y - A.y) * (B.x - A.x) > (B.y - A.y) * (C.x - A.x);
    }

    public static bool LineIntersect(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4)
    {
        if (CCW(p1, p3, p4) != CCW(p2, p3, p4))
        {
            return CCW(p1, p2, p3) != CCW(p1, p2, p4);
        }
        return false;
    }

    public static bool Circle(Vec2 center, float radius, Thing t)
    {
        Vec2 closestPoint = new Vec2(center);
        if (center.x < t.left)
        {
            closestPoint.x = t.left;
        }
        else if (center.x > t.right)
        {
            closestPoint.x = t.right;
        }
        if (center.y < t.top)
        {
            closestPoint.y = t.top;
        }
        else if (center.y > t.bottom)
        {
            closestPoint.y = t.bottom;
        }
        Vec2 diff = closestPoint - center;
        if (diff.x * diff.x + diff.y * diff.y > radius * radius)
        {
            return false;
        }
        return true;
    }

    public static bool Circle(Vec2 center, float radius, Rectangle t)
    {
        Vec2 closestPoint = new Vec2(center);
        if (center.x < t.Left)
        {
            closestPoint.x = t.Left;
        }
        else if (center.x > t.Right)
        {
            closestPoint.x = t.Right;
        }
        if (center.y < t.Top)
        {
            closestPoint.y = t.Top;
        }
        else if (center.y > t.Bottom)
        {
            closestPoint.y = t.Bottom;
        }
        Vec2 diff = closestPoint - center;
        if (diff.x * diff.x + diff.y * diff.y > radius * radius)
        {
            return false;
        }
        return true;
    }

    public static bool Rect(Vec2 tl1, Vec2 br1, Thing t)
    {
        if (!(br1.y < t.top) && !(tl1.y > t.bottom) && !(tl1.x > t.right))
        {
            return !(br1.x < t.left);
        }
        return false;
    }

    public static bool Rect(Vec2 tl1, Vec2 br1, Rectangle t)
    {
        if (!(br1.y < t.y) && !(tl1.y > t.Bottom) && !(tl1.x > t.Right))
        {
            return !(br1.x < t.x);
        }
        return false;
    }

    public static bool Rect(Rectangle r1, Rectangle r2)
    {
        if (!(r1.y + r1.height < r2.y) && !(r1.y > r2.y + r2.height) && !(r1.x > r2.x + r2.width))
        {
            return !(r1.x + r1.width < r2.x);
        }
        return false;
    }

    public static bool Rect(Rectangle r1, Vec4 r2)
    {
        if (!(r1.y + r1.height < r2.y) && !(r1.y > r2.y + r2.w) && !(r1.x > r2.x + r2.z))
        {
            return !(r1.x + r1.width < r2.x);
        }
        return false;
    }

    public static Vec2 LineIntersectPoint(Vec2 line1V1, Vec2 line1V2, Vec2 line2V1, Vec2 line2V2)
    {
        float A1 = line1V2.y - line1V1.y;
        float B1 = line1V1.x - line1V2.x;
        float C1 = A1 * line1V1.x + B1 * line1V1.y;
        float A2 = line2V2.y - line2V1.y;
        float B2 = line2V1.x - line2V2.x;
        float C2 = A2 * line2V1.x + B2 * line2V1.y;
        float det = A1 * B2 - A2 * B1;
        if (det == 0f)
        {
            return Vec2.Zero;
        }
        float x = (B2 * C1 - B1 * C2) / det;
        float y = (A1 * C2 - A2 * C1) / det;
        return new Vec2(x, y);
    }

    public static Vec2 LinePoint(Vec2 point1, Vec2 point2, Thing thing)
    {
        Vec2 dif = point2 - point1;
        float[] p = new float[4]
        {
            0f - dif.x,
            dif.x,
            0f - dif.y,
            dif.y
        };
        float[] q = new float[4]
        {
            point1.x - thing.left,
            thing.right - point1.x,
            point1.y - thing.top,
            thing.bottom - point1.y
        };
        float u1 = float.NegativeInfinity;
        float u2 = float.PositiveInfinity;
        for (int i = 0; i < 4; i++)
        {
            if (p[i] == 0f)
            {
                if (q[i] < 0f)
                {
                    return Vec2.Zero;
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
            return Vec2.Zero;
        }
        return new Vec2(point1.x + u1 * dif.x, point1.y + u1 * dif.y);
    }

    public static Vec2 LinePoint(Vec2 point1, Vec2 point2, Rectangle rect)
    {
        Vec2 dif = point2 - point1;
        float[] p = new float[4]
        {
            0f - dif.x,
            dif.x,
            0f - dif.y,
            dif.y
        };
        float[] q = new float[4]
        {
            point1.x - rect.x,
            rect.x + rect.width - point1.x,
            point1.y - rect.y,
            rect.y + rect.height - point1.y
        };
        float u1 = float.NegativeInfinity;
        float u2 = float.PositiveInfinity;
        for (int i = 0; i < 4; i++)
        {
            if (p[i] == 0f)
            {
                if (q[i] < 0f)
                {
                    return Vec2.Zero;
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
            return Vec2.Zero;
        }
        return new Vec2(point1.x + u1 * dif.x, point1.y + u1 * dif.y);
    }
}
