namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class SnowDrift : MaterialThing
{
    public EditorProperty<int> style;

    public bool kill;

    private bool melt;

    public override void EditorPropertyChanged(object property)
    {
        if ((int)style == -1)
        {
            (graphic as SpriteMap).frame = Rando.Int(3);
        }
        else
        {
            (graphic as SpriteMap).frame = style.value;
        }
    }

    public SnowDrift(float xpos, float ypos, int dir)
        : base(xpos, ypos)
    {
        style = new EditorProperty<int>(-1, this, -1f, 3f, 1f);
        graphic = new SpriteMap("drifts", 16, 17);
        if ((int)style == -1)
        {
            (graphic as SpriteMap).frame = Rando.Int(3);
        }
        base.hugWalls = WallHug.Floor;
        center = new Vec2(8f, 14f);
        collisionSize = new Vec2(14f, 4f);
        collisionOffset = new Vec2(-7f, -2f);
        base.layer = Layer.Blocks;
        base.depth = 0.5f;
        editorTooltip = "The safest drift of all!";
    }

    public override void Update()
    {
        if (kill)
        {
            base.alpha -= 0.012f;
            base.yscale -= 0.15f;
            base.xscale += 0.12f;
            base.y += 0.44f;
        }
        if (melt)
        {
            base.alpha -= 0.0036000002f;
            base.yscale -= 0.045f;
            base.xscale += 0.036000002f;
            base.y += 0.16f;
        }
        if (base.yscale < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (!kill && with.impactPowerV > 2f)
        {
            float vPower = with.impactPowerV;
            float hPower = with.impactDirectionH;
            if (vPower > 6f)
            {
                vPower = 6f;
            }
            if (hPower > 6f)
            {
                hPower = 6f;
            }
            for (int i = 0; i < 12; i++)
            {
                float mul = 1f;
                if (i < 10)
                {
                    mul = 0.7f;
                }
                Level.Add(new SnowFallParticle(base.x + Rando.Float(-8f, 8f), base.y + Rando.Float(-6f, 0f), new Vec2(hPower * mul * 0.1f + Rando.Float(-0.2f * (vPower * mul), 0.2f * (vPower * mul)), (0f - Rando.Float(0.8f, 1.5f)) * (vPower * mul * 0.15f)), i < 6));
            }
            kill = true;
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
