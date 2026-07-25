using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialRecolor : Material
{
    public Vector3 color;

    public MaterialRecolor(Vector3 col) : base(Content.Load<Effect>("Shaders/recolor"))
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
