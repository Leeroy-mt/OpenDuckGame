using System;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class SnowPile : MaterialThing
{
    public bool kill;

    private bool melt;

    private Predicate<MaterialThing> _collisionPred;

    public SnowPile(float xpos, float ypos, int dir)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("bigDrift", 32, 32);
        base.hugWalls = WallHug.Floor;
        center = new Vec2(12f, 24f);
        collisionSize = new Vec2(24f, 10f);
        collisionOffset = new Vec2(-12f, -2f);
        base.layer = Layer.Game;
        base.depth = 0.85f;
        editorTooltip = "A nice, big, fluffy sneaky snow pile.";
    }

    public override void Update()
    {
        if (_collisionPred == null)
        {
            _collisionPred = (MaterialThing thing) => thing == null || !Collision.Rect(base.topLeft, base.bottomRight, thing);
        }
        base.clip.RemoveWhere(_collisionPred);
        if (melt)
        {
            base.alpha -= 0.0012f;
            base.yscale -= 0.015000001f;
            base.y += 0.13f;
        }
        if (base.yscale < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (!kill && (with.impactPowerV > 2f || with.impactPowerH > 2f))
        {
            base.clip.Add(with);
            float vPower = with.impactPowerV;
            float hPower = 0f - with.impactDirectionH;
            if (vPower > 6f)
            {
                vPower = 6f;
            }
            if (hPower > 6f)
            {
                hPower = 6f;
            }
            if (vPower < 2f)
            {
                vPower = 2f;
            }
            for (int i = 0; i < 20; i++)
            {
                float mul = 1f;
                if (i < 10)
                {
                    mul = 0.7f;
                }
                Level.Add(new SnowFallParticle(base.x + Rando.Float(-9f, 9f), base.y + 7f + Rando.Float(-16f, 0f), new Vec2(hPower * mul * 0.1f + Rando.Float(-0.2f * (vPower * mul), 0.2f * (vPower * mul)), (0f - Rando.Float(0.8f, 1.5f)) * (vPower * mul * 0.15f)), i < 6));
            }
        }
        base.OnSoftImpact(with, from);
    }

    public override void HeatUp(Vec2 location)
    {
        melt = true;
        base.HeatUp(location);
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        base.Draw();
    }
}
