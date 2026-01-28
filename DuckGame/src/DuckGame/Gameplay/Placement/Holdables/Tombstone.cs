namespace DuckGame;

[EditorGroup("Stuff|Props")]
public class Tombstone : Holdable, IPlatform
{
    private SpriteMap _sprite;

    public Tombstone(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("grave", 15, 16);
        graphic = _sprite;
        Center = new Vec2(7f, 8f);
        collisionOffset = new Vec2(-7f, -8f);
        collisionSize = new Vec2(15f, 15f);
        base.Depth = -0.5f;
        thickness = 4f;
        weight = 7f;
        flammable = 0f;
        base.collideSounds.Add("rockHitGround2");
        editorTooltip = "The saddest rock.";
    }
}
