using System;

namespace DuckGame;

public class CompressedVec2Binding : StateBinding
{
    private int _range;

    public override int bits => 32;

    public override Type type => typeof(int);

    public override int intValue => GetCompressedVec2((Vec2)classValue, _range);

    public override object GetNetValue()
    {
        return GetCompressedVec2(base.getTyped<Vec2>(), _range);
    }

    public static int GetCompressedVec2(Vec2 val, int range = int.MaxValue)
    {
        if (Math.Abs(val.X) < 1E-07f)
        {
            val.X = 0f;
        }
        if (Math.Abs(val.Y) < 1E-07f)
        {
            val.Y = 0f;
        }
        if (range != int.MaxValue)
        {
            float rangeMult = 32767 / range;
            val.X = Maths.Clamp(val.X, -range, range) * rangeMult;
            val.Y = Maths.Clamp(val.Y, -range, range) * rangeMult;
        }
        short num = (short)Maths.Clamp((int)Math.Round(val.X), -32768, 32767);
        short yVal = (short)Maths.Clamp((int)Math.Round(val.Y), -32768, 32767);
        return (int)(((ulong)(ushort)num << 16) | (ushort)yVal);
    }

    public override object ReadNetValue(object val)
    {
        return GetUncompressedVec2((int)val, _range);
    }

    public override object ReadNetValue(BitBuffer pData)
    {
        return GetUncompressedVec2((int)pData.ReadBits(type, bits), _range);
    }

    public static Vec2 GetUncompressedVec2(int val, int range = int.MaxValue)
    {
        short yVal = (short)(val & 0xFFFF);
        short xVal = (short)((val >> 16) & 0xFFFF);
        Vec2 vecVal = new Vec2(xVal, yVal);
        if (range != int.MaxValue)
        {
            float rangeMult = 32767 / range;
            vecVal.X /= rangeMult;
            vecVal.Y /= rangeMult;
        }
        return vecVal;
    }

    public CompressedVec2Binding(string field, int range = int.MaxValue, bool isvelocity = false, bool doLerp = false)
        : base(field, -1, rot: false, isvelocity)
    {
        _range = range;
        _lerp = doLerp;
    }

    public CompressedVec2Binding(GhostPriority p, string field, int range = int.MaxValue, bool isvelocity = false, bool doLerp = false)
        : base(field, -1, rot: false, isvelocity)
    {
        _range = range;
        _priority = p;
        _lerp = doLerp;
    }

    public CompressedVec2Binding(string field, int range, bool doLerp)
        : base(field)
    {
        _range = range;
        _lerp = doLerp;
    }

    public CompressedVec2Binding(string field, int range)
        : base(field)
    {
        _range = range;
    }
}
