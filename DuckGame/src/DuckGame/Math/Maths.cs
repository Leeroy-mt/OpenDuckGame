using System;

namespace DuckGame;

public class Maths
{
    public const float PI = (float)Math.PI;

    public static uint MaxFloatToInt = 16777216u;

    public static float FramesToTravel(float distance, float acceleration, float startSpeed)
    {
        return ((float)Math.Sqrt(Math.Pow(2f * startSpeed + acceleration, 2.0) + (double)(8f * acceleration * distance)) - 2f * startSpeed - acceleration) / (2f * acceleration);
    }

    public static float DistanceTravelled(int frames, float acceleration, float startSpeed)
    {
        return 0.5f * acceleration * (float)((frames + 1) * frames) + (float)frames * startSpeed;
    }

    public static float TicksToSeconds(int ticks)
    {
        return (float)ticks / 60f;
    }

    public static int SecondsToTicks(float secs)
    {
        return (int)Math.Round(secs * 60f);
    }

    public static float IncFrameTimer()
    {
        return 1f / 60f;
    }

    public static Vec2 RoundToPixel(Vec2 pos)
    {
        pos.X = (float)Math.Round(pos.X / 1f) * 1f;
        pos.Y = (float)Math.Round(pos.Y / 1f) * 1f;
        return pos;
    }

    public static float FastSin(float rads)
    {
        if (rads < -(float)Math.PI)
        {
            rads += (float)Math.PI * 2f;
        }
        else if (rads > (float)Math.PI)
        {
            rads -= (float)Math.PI * 2f;
        }
        if (rads < 0f)
        {
            return 4f / (float)Math.PI * rads + 0.40528473f * rads * rads;
        }
        return 4f / (float)Math.PI * rads - 0.40528473f * rads * rads;
    }

    public static float FastCos(float rads)
    {
        if (rads < -(float)Math.PI)
        {
            rads += (float)Math.PI * 2f;
        }
        else if (rads > (float)Math.PI)
        {
            rads -= (float)Math.PI * 2f;
        }
        rads += (float)Math.PI / 2f;
        if (rads > (float)Math.PI)
        {
            rads -= (float)Math.PI * 2f;
        }
        if (rads < 0f)
        {
            return 4f / (float)Math.PI * rads + 0.40528473f * rads * rads;
        }
        return 4f / (float)Math.PI * rads - 0.40528473f * rads * rads;
    }

    public static float LerpTowards(float current, float to, float amount)
    {
        if (to > current)
        {
            current += amount;
            if (to < current)
            {
                current = to;
            }
        }
        else if (to < current)
        {
            current -= amount;
            if (to > current)
            {
                current = to;
            }
        }
        return current;
    }

    public static float Ratio(int val1, int val2)
    {
        if ((float)val2 == 0f)
        {
            return val1;
        }
        return (float)val1 / (float)val2;
    }

    public static float NormalizeSection(float value, float sectionMin, float sectionMax)
    {
        return Clamp(Clamp(value - sectionMin, 0f, sectionMax) / (sectionMax - sectionMin), 0f, 1f);
    }

    public static float CountDown(float value, float amount, float min = 0f)
    {
        value = ((!(value > min)) ? min : (value - amount));
        return value;
    }

    public static int CountDown(int value, int amount, int min = 0)
    {
        value = ((value <= min) ? min : (value - amount));
        return value;
    }

    public static float CountUp(float value, float amount, float max = 1f)
    {
        value = ((!(value < max)) ? max : (value + amount));
        return value;
    }

    public static bool Intersects(Vec2 a1, Vec2 a2, Vec2 b1, Vec2 b2, out Vec2 intersection)
    {
        intersection = Vec2.Zero;
        Vec2 b3 = a2 - a1;
        Vec2 d = b2 - b1;
        float bDotDPerp = b3.X * d.Y - b3.Y * d.X;
        if (bDotDPerp == 0f)
        {
            return false;
        }
        Vec2 c = b1 - a1;
        float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
        if (t < 0f || t > 1f)
        {
            return false;
        }
        float u = (c.X * b3.Y - c.Y * b3.X) / bDotDPerp;
        if (u < 0f || u > 1f)
        {
            return false;
        }
        intersection = a1 + t * b3;
        return true;
    }

    public static float DegToRad(float deg)
    {
        return deg * ((float)Math.PI / 180f);
    }

    public static float RadToDeg(float rad)
    {
        return rad * (180f / (float)Math.PI);
    }

    public static float PointDirection(Vec2 p1, Vec2 p2)
    {
        return RadToDeg((float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X));
    }

    public static float PointDirectionRad(Vec2 p1, Vec2 p2)
    {
        return (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);
    }

    public static float PointDirection2(Vec2 p1, Vec2 p2)
    {
        return (float)Math.Atan2(p2.Y, p2.X) - (float)Math.Atan2(p1.Y, p1.X);
    }

    public static float PointDirection(float x1, float y1, float x2, float y2)
    {
        return RadToDeg((float)Math.Atan2(y1 - y2, x2 - x1));
    }

    public static float Clamp(float val, float min, float max)
    {
        return Math.Min(Math.Max(val, min), max);
    }

    public static int Clamp(int val, int min, int max)
    {
        return Math.Min(Math.Max(val, min), max);
    }

    public static int Int(bool val)
    {
        if (!val)
        {
            return 0;
        }
        return 1;
    }

    public static Vec2 AngleToVec(float radians)
    {
        return new Vec2(float.Cos(radians), -float.Sin(radians));
    }

    public static Vec2 Snap(Vec2 pPosition, float xSnap, float ySnap)
    {
        pPosition.X = (float)(int)Math.Floor(pPosition.X / xSnap) * xSnap;
        pPosition.Y = (float)(int)Math.Floor(pPosition.Y / ySnap) * ySnap;
        return pPosition;
    }

    public static Vec2 SnapRound(Vec2 pPosition, float xSnap, float ySnap)
    {
        pPosition.X = (float)(int)Math.Round(pPosition.X / xSnap) * xSnap;
        pPosition.Y = (float)(int)Math.Round(pPosition.Y / ySnap) * ySnap;
        return pPosition;
    }

    public static float Snap(float pValue, float pSnap)
    {
        pValue = (float)(int)Math.Floor(pValue / pSnap) * pSnap;
        return pValue;
    }

    public static int Hash(string val)
    {
        byte[] dataToHash = new byte[val.Length * 2];
        Buffer.BlockCopy(val.ToCharArray(), 0, dataToHash, 0, dataToHash.Length);
        int dataLength = dataToHash.Length;
        if (dataLength == 0)
        {
            return 0;
        }
        uint hash = Convert.ToUInt32(dataLength);
        int remainingBytes = dataLength & 3;
        int numberOfLoops = dataLength >> 2;
        int currentIndex = 0;
        while (numberOfLoops > 0)
        {
            hash += BitConverter.ToUInt16(dataToHash, currentIndex);
            uint tmp = (uint)(BitConverter.ToUInt16(dataToHash, currentIndex + 2) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            hash += hash >> 11;
            currentIndex += 4;
            numberOfLoops--;
        }
        switch (remainingBytes)
        {
            case 3:
                hash += BitConverter.ToUInt16(dataToHash, currentIndex);
                hash ^= hash << 16;
                hash ^= (uint)(dataToHash[currentIndex + 2] << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += BitConverter.ToUInt16(dataToHash, currentIndex);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += dataToHash[currentIndex];
                hash ^= hash << 10;
                hash += hash >> 1;
                break;
        }
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        return (int)(hash + (hash >> 6));
    }
}
