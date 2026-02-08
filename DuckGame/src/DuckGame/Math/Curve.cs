using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class Curve
{
    private static double[] kFactorialLookup;

    public static void System_Initialize()
    {
        CreateFactorialTable();
    }

    private static double factorial(int n)
    {
        if (n < 0)
        {
            throw new Exception("n is less than 0");
        }
        if (n > 32)
        {
            throw new Exception("n is greater than 32");
        }
        return kFactorialLookup[n];
    }

    private static void CreateFactorialTable()
    {
        kFactorialLookup = new double[33]
        {
            1.0, 1.0, 2.0, 6.0, 24.0, 120.0, 720.0, 5040.0, 40320.0, 362880.0,
            3628800.0, 39916800.0, 479001600.0, 6227020800.0, 87178291200.0, 1307674368000.0, 20922789888000.0, 355687428096000.0, 6402373705728000.0, 1.21645100408832E+17,
            2.43290200817664E+18, 5.109094217170944E+19, 1.1240007277776077E+21, 2.585201673888498E+22, 6.204484017332394E+23, 1.5511210043330986E+25, 4.0329146112660565E+26, 1.0888869450418352E+28, 3.0488834461171387E+29, 8.841761993739702E+30,
            2.6525285981219107E+32, 8.222838654177922E+33, 2.631308369336935E+35
        };
    }

    private static double Ni(int n, int i)
    {
        double num = factorial(n);
        double a2 = factorial(i);
        double a3 = factorial(n - i);
        return num / (a2 * a3);
    }

    private static double Bernstein(int n, int i, double t)
    {
        double ti = ((t != 0.0 || i != 0) ? Math.Pow(t, i) : 1.0);
        double tni = ((n != i || t != 1.0) ? Math.Pow(1.0 - t, n - i) : 1.0);
        return Ni(n, i) * ti * tni;
    }

    public static Vector2 Calculate(Vector2 start, Vector2 end, float lerp, float arcSizeMult = 1f)
    {
        Vector2 centerPos = (start + end) / 2f;
        if (end.X == start.X)
        {
            if (end.Y > start.Y)
            {
                centerPos.X = start.X - 6f * arcSizeMult;
            }
            else
            {
                centerPos.X = start.X + 6f * arcSizeMult;
            }
            arcSizeMult *= 0.2f;
        }
        if (end.Y > start.Y)
        {
            centerPos.Y = start.Y - 22f * arcSizeMult;
        }
        else
        {
            centerPos.Y = end.Y - 16f * arcSizeMult;
        }
        List<Vector2> curve = Bezier(8, start, centerPos, end);
        float curveLength = 0f;
        for (int i = 1; i < curve.Count; i++)
        {
            curveLength += (curve[i] - curve[i - 1]).Length();
        }
        _ = curveLength / (float)curve.Count;
        int curveIndex = (int)Math.Floor(lerp * (float)curve.Count) + 1;
        if (curveIndex >= curve.Count)
        {
            return end;
        }
        Vector2 prev = curve[curveIndex - 1];
        Vector2 next = curve[curveIndex];
        float partLerp = lerp % (1f / (float)curve.Count) * (float)curve.Count;
        end = prev + (next - prev) * partLerp;
        return end;
    }

    public static List<Vector2> Bezier(int cpts, params Vector2[] points)
    {
        int npts = points.Length;
        double t = 0.0;
        double step = 1.0 / (double)(cpts - 1);
        List<Vector2> output = new List<Vector2>();
        for (int i1 = 0; i1 != cpts; i1++)
        {
            Vector2 p = default(Vector2);
            if (1.0 - t < 5E-06)
            {
                t = 1.0;
            }
            for (int j = 0; j != npts; j++)
            {
                float basis = (float)Bernstein(npts - 1, j, t);
                p.X += basis * points[j].X;
                p.Y += basis * points[j].Y;
            }
            t += step;
            output.Add(p);
        }
        return output;
    }
}
