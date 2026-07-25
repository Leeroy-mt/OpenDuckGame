using Microsoft.Xna.Framework;

namespace DuckGame;

public class MaterialRecolor : AutoEffect
{
    public Vector3 color;

    public MaterialRecolor(Vector3 col) : base(Content.Load<MTEffect>("Shaders/recolor"))
    {
        color = col;
    }

    public override void Update()
    {
    }

    public override void Apply()
    {
        Parameters["fcol"].SetValue(color);
        base.Apply();
    }
}
