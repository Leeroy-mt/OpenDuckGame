using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Material
{
    protected MTEffect _effect;

    public MTEffect effect => _effect;

    public Material()
    {
    }

    public Material(string mat)
    {
        _effect = Content.Load<MTEffect>(mat);
    }

    public Material(Effect e)
    {
        _effect = e;
    }

    public virtual void SetValue(string name, float value)
    {
        _effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Vector2 value)
    {
        _effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Vector3 value)
    {
        _effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Color value)
    {
        _effect.effect.Parameters[name]?.SetValue(value.ToVector4());
    }

    public virtual void SetValue(string name, Rectangle value)
    {
        _effect.effect.Parameters[name]?.SetValue(value.ToVector4());
    }

    public virtual void SetValue(string name, Matrix value)
    {
        _effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Texture2D value)
    {
        _effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void Update()
    {
    }

    public virtual void Apply()
    {
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }

    public static implicit operator MTEffect(Material val)
    {
        return val.effect;
    }
}
