using System;

namespace DuckGame;

[EditorGroup("Stuff|Spikes")]
public class SawsDown : Saws
{
    private SpriteMap _sprite;

    public new bool up = true;

    public SawsDown(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("movingSpikes", 16, 21);
        _sprite.speed = 0.3f;
        graphic = _sprite;
        Center = new Vec2(8f, 14f);
        collisionOffset = new Vec2(-6f, -2f);
        collisionSize = new Vec2(12f, 4f);
        _editorName = "Saws Down";
        editorTooltip = "Deadly hazards, able to cut through even the strongest of boots";
        physicsMaterial = PhysicsMaterial.Metal;
        editorCycleType = typeof(SawsLeft);
        Angle = (float)Math.PI;
        base.editorOffset = new Vec2(0f, -6f);
        base.hugWalls = WallHug.Ceiling;
        _editorImageCenter = true;
        base.impactThreshold = 0.01f;
    }

    public override void Touch(MaterialThing with)
    {
        if (!with.destroyed)
        {
            with.Destroy(new DTImpale(this));
            with.vSpeed = 3f;
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
