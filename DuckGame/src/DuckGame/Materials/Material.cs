using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Material
{
    public MTEffect effect { get; protected set; }

    #region Constructors

    public Material()
    {
    }

    public Material(string mat)
    {
        effect = Content.Load<MTEffect>(mat);
    }

    public Material(Effect e)
    {
        effect = e;
    }

    #endregion

    #region Public Methods

    public virtual void SetValue(string name, float value)
    {
        effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Vector2 value)
    {
        effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Vector3 value)
    {
        effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Color value)
    {
        effect.effect.Parameters[name]?.SetValue(value.ToVector4());
    }

    public virtual void SetValue(string name, RectangleF value)
    {
        effect.effect.Parameters[name]?.SetValue(value.ToVector4());
    }

    public virtual void SetValue(string name, Matrix value)
    {
        effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void SetValue(string name, Texture2D value)
    {
        effect.effect.Parameters[name]?.SetValue(value);
    }

    public virtual void Update()
    {
    }

    public virtual void Apply()
    {
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }

    #endregion

    public static implicit operator MTEffect(Material val)
    {
        return val.effect;
    }
}
