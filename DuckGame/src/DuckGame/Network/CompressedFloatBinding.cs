using System;

namespace DuckGame;

public class CompressedFloatBinding : StateBinding
{
    private float _range = 1f;

    public override Type type => typeof(int);

    public override int intValue => GetCompressedFloat(base.getTyped<float>());

    public int GetCompressedFloat(float val)
    {
        int maxVal = (int)BitBuffer.GetMaxValue(_bits) / 2;
        if (base.isRotation)
        {
            if (val < 0f)
            {
                _ = _range / 2f;
                val = val % (0f - _range) + _range;
            }
            val = val % _range / _range;
        }
        else
        {
            val = Maths.Clamp(val, 0f - _range, _range) / _range;
        }
        return (int)Math.Round(val * (float)maxVal);
    }

    public override object GetNetValue()
    {
        return GetCompressedFloat(base.getTyped<float>());
    }

    public override object ReadNetValue(object val)
    {
        int num = (int)val;
        long maxVal = BitBuffer.GetMaxValue(_bits) / 2;
        return (float)num / (float)maxVal * _range;
    }

    public override object ReadNetValue(BitBuffer pData)
    {
        int num = (int)pData.ReadBits(type, bits);
        long maxVal = BitBuffer.GetMaxValue(_bits) / 2;
        return (float)num / (float)maxVal * _range;
    }

    public CompressedFloatBinding(string field, float range = 1f, int bits = 16, bool isRot = false, bool doLerp = false)
        : base(field, bits, isRot)
    {
        _range = range;
        if (isRot)
        {
            _range = (float)Math.PI * 2f;
        }
        _lerp = doLerp;
    }

    public CompressedFloatBinding(string field, float range, int bits, bool isRot)
        : base(field, bits, isRot)
    {
        _range = range;
        if (isRot)
        {
            _range = (float)Math.PI * 2f;
        }
    }

    public CompressedFloatBinding(GhostPriority p, string field, float range = 1f, int bits = 16, bool isRot = false, bool doLerp = false)
        : base(field, bits, isRot)
    {
        _range = range;
        if (isRot)
        {
            _range = (float)Math.PI * 2f;
        }
        _priority = p;
        _lerp = doLerp;
    }
}
