using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff")]
public class GreyBlock : Block
{
    public GreyBlock(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("greyBlock");
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -8f);
        collisionSize = new Vector2(16f, 16f);
        base.Depth = -0.5f;
        _editorName = "Grey Block";
        editorTooltip = "It's a featureless grey block.";
        thickness = 4f;
        physicsMaterial = PhysicsMaterial.Metal;
    }
}
