using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialRoundBox : Material
{
    public float Radius;

    public Rectangle Rectangle;

    public MaterialRoundBox()
    {
        _effect = Content.Load<MTEffect>("Shaders\\roundBoxSdf");
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] is Texture2D texture)
        {
            var r = Radius;
            SetValue("radii", new Rectangle(r, r, r, r));
            SetValue("rect", Rectangle);
            SetValue("smoothing", 2.5f);
        }

        foreach (var pass in _effect.effect.CurrentTechnique.Passes)
            pass.Apply();
    }
}
