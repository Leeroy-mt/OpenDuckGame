using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Stuff|Spikes")]
public class SawsRight : Saws
{
    private SpriteMap _sprite;

    public new bool up = true;

    public SawsRight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("movingSpikes", 16, 21);
        _sprite.speed = 0.3f;
        graphic = _sprite;
        Center = new Vector2(8f, 14f);
        collisionOffset = new Vector2(-2f, -6f);
        collisionSize = new Vector2(4f, 12f);
        _editorName = "Saws Right";
        editorTooltip = "Deadly hazards, able to cut through even the strongest of boots";
        physicsMaterial = PhysicsMaterial.Metal;
        editorCycleType = typeof(SawsDown);
        Angle = (float)Math.PI / 2f;
        base.editorOffset = new Vector2(-6f, 0f);
        base.hugWalls = WallHug.Left;
        _editorImageCenter = true;
        base.impactThreshold = 0.01f;
    }

    public override void Touch(MaterialThing with)
    {
        if (!with.destroyed)
        {
            with.Destroy(new DTImpale(this));
            with.hSpeed = 3f;
        }
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
