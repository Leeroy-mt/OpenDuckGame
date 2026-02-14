using Microsoft.Xna.Framework;

namespace DuckGame;

public class MaterialRecolor : Material
{
    public Vector3 color;

    public MaterialRecolor(Vector3 col)
    {
        color = col;
        _effect = Content.Load<MTEffect>("Shaders/recolor");
    }

    public override void Update()
    {
    }

    public override void Apply()
    {
        _effect.effect.Parameters["fcol"].SetValue(color);
        base.Apply();
    }
}
