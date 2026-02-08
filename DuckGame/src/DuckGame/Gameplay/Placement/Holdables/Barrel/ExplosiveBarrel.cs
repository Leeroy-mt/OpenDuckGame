using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Props|Barrels")]
public class ExplosiveBarrel : DemoCrate, IPlatform
{
    public ExplosiveBarrel(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("explosiveBarrel");
        Center = new Vector2(7f, 8f);
        collisionOffset = new Vector2(-7f, -8f);
        collisionSize = new Vector2(14f, 16f);
        base.Depth = -0.1f;
        _editorName = "Barrel (Explosive)";
        editorTooltip = "Nobody knows what's in these things or why everyone just leaves them around.";
        thickness = 4f;
        weight = 10f;
        physicsMaterial = PhysicsMaterial.Metal;
        base.collideSounds.Clear();
        base.collideSounds.Add("barrelThud");
        _holdOffset = new Vector2(1f, 0f);
        flammable = 0.3f;
        _placementCost += 10;
        baseExplosionRange = 70f;
    }

    public override void DoBlockDestruction()
    {
    }
}
