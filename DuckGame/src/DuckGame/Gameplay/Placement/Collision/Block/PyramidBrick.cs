using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Pyramid", EditorItemType.Pyramid)]
public class PyramidBrick : Block
{
    public PyramidBrick(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("pyramidBrick");
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -8f);
        collisionSize = new Vector2(16f, 16f);
        base.Depth = -0.5f;
        _editorName = "Pyramid Block";
        thickness = 4f;
        physicsMaterial = PhysicsMaterial.Metal;
    }
}
