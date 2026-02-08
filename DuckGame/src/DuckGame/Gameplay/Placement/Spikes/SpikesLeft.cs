using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Stuff|Spikes")]
[BaggedProperty("previewPriority", false)]
public class SpikesLeft : Spikes
{
    private SpriteMap _sprite;

    public SpikesLeft(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("spikes", 16, 19);
        _sprite.speed = 0.1f;
        graphic = _sprite;
        Center = new Vector2(8f, 14f);
        collisionOffset = new Vector2(-3f, -7f);
        collisionSize = new Vector2(5f, 13f);
        _editorName = "Spikes Left";
        editorTooltip = "Pointy and dangerous.";
        physicsMaterial = PhysicsMaterial.Metal;
        editorCycleType = typeof(Spikes);
        Angle = -(float)Math.PI / 2f;
        up = false;
        base.editorOffset = new Vector2(6f, 0f);
        base.hugWalls = WallHug.Right;
        _killImpact = ImpactedFrom.Left;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }
}
