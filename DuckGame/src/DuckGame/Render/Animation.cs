using System;

namespace DuckGame;

public struct Animation : IEquatable<Animation>
{
    public string name;

    public float speed;

    public int[] frames;

    public bool looping;

    public Animation(string nameVal, float speedVal, bool loopVal, int[] framesVal)
    {
        name = nameVal;
        speed = speedVal;
        frames = framesVal;
        looping = loopVal;
    }

    public static bool operator ==(Animation l, Animation r)
    {
        return l.Equals(r);
    }

    public static bool operator !=(Animation l, Animation r)
    {
        return !l.Equals(r);
    }

    public bool Equals(Animation other)
    {
        if (name == other.name && speed == other.speed && frames == other.frames && looping == other.looping)
        {
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return ((name.GetHashCode() * 19 + speed.GetHashCode()) * 19 + frames.GetHashCode()) * 19 + looping.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is Animation)
        {
            return Equals((Animation)obj);
        }
        return base.Equals(obj);
    }
}
