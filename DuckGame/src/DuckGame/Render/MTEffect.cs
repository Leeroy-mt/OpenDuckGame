using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MTEffect
{
    #region Private Fields

    string _effectName;
    Effect _base;

    #endregion

    #region Public Properties

    public short EffectIndex { get; set; }

    public string effectName => _effectName;

    public Effect effect => _base;

    #endregion

    public MTEffect(Effect tex, string cureffectName, short cureffectIndex = 0)
    {
        _base = tex;
        _effectName = cureffectName;
        EffectIndex = cureffectIndex;
    }

    public static implicit operator Effect(MTEffect tex)
    {
        return tex?._base;
    }

    public static implicit operator MTEffect(Effect tex)
    {
        return tex is null ? null : Content.GetMTEffect(tex);
    }
}

public class AutoEffect(Effect effect) : Effect(effect)
{
    public virtual void Update()
    {
    }

    public virtual void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
            pass.Apply();
    }

    public void SetValue(string name, bool value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, bool[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, float value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, float[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, int value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, int[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Matrix value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Matrix[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Quaternion value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Quaternion[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, string value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Texture value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector2 value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector2[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector3 value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector3[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector4 value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Vector4[] value)
    {
        Parameters[name]?.SetValue(value);
    }

    public void SetValue(string name, Color value)
    {
        Parameters[name]?.SetValue(value.ToVector4());
    }

    public void SetValue(string name, RectangleF value)
    {
        Parameters[name]?.SetValue(value.ToVector4());
    }
}