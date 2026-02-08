using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Doors")]
[BaggedProperty("previewPriority", true)]
public class Key : Holdable
{
    private SpriteMap _sprite;

    public Key(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("key", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-7f, -4f);
        collisionSize = new Vector2(14f, 8f);
        base.Depth = -0.5f;
        thickness = 1f;
        weight = 3f;
        flammable = 0f;
        base.collideSounds.Add("metalRebound");
        physicsMaterial = PhysicsMaterial.Metal;
        editorTooltip = "For opening locked doors. You've heard of keys before, right?";
        holsterAngle = 90f;
        coolingFactor = 0.001f;
    }

    public override void Update()
    {
        _sprite.flipH = offDir < 0;
        if (owner != null)
        {
            Level.CheckLine<Door>(Position + new Vector2(-10f, 0f), Position + new Vector2(10f, 0f))?.UnlockDoor(this);
        }
        base.Update();
    }

    public override void Terminate()
    {
    }
}
