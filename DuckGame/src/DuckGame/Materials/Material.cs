using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Material(Effect effect) : Effect(effect)
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