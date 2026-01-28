namespace DuckGame;

[EditorGroup("Blocks|Snow")]
[BaggedProperty("isInDemo", false)]
public class SnowTileset : AutoBlock
{
    protected string meltedTileset = "snowTileset";

    protected string frozenTileset = "snowIceTileset";

    private float melt;

    private bool melted = true;

    private long lastHitFrame;

    public SnowTileset(float x, float y, string tset = "snowTileset")
        : base(x, y, tset)
    {
        _editorName = "Snow";
        physicsMaterial = PhysicsMaterial.Metal;
        verticalWidthThick = 15f;
        verticalWidth = 14f;
        horizontalHeight = 15f;
        _tileset = "snowTileset";
        _sprite = new SpriteMap("snowTileset", 16, 16);
        graphic = _sprite;
        _sprite.frame = 40;
        cold = true;
        willHeat = true;
        _impactThreshold = -1f;
        meltedTileset = tset;
    }

    public override void Initialize()
    {
        if (base.level != null)
        {
            base.level.cold = true;
        }
        base.Initialize();
    }

    public override void HeatUp(Vec2 location)
    {
        if (!melted)
        {
            melt += 0.05f;
            if (melt > 1f)
            {
                Melt();
            }
        }
        base.HeatUp(location);
    }

    public void Melt()
    {
        Melt(Network.isServer, pNetMessage: false);
    }

    public void Melt(bool pServer, bool pNetMessage)
    {
        if (!melted && (pServer || pNetMessage))
        {
            melted = true;
            _sprite = new SpriteMap(meltedTileset, 16, 16);
            _sprite.frame = (graphic as SpriteMap).frame;
            graphic = _sprite;
            DoPositioning();
            if (Network.isActive && !pNetMessage)
            {
                Send.Message(new NMMeltTile(Position));
            }
        }
    }

    public void Freeze()
    {
        Freeze(Network.isServer, pNetMessage: false);
    }

    public void Freeze(bool pServer, bool pNetMessage)
    {
        if (melted && (pServer || pNetMessage))
        {
            melted = false;
            _sprite = new SpriteMap(frozenTileset, 16, 16);
            _sprite.frame = (graphic as SpriteMap).frame;
            graphic = _sprite;
            DoPositioning();
            melt = 0f;
            if (!melted && Network.isActive && !pNetMessage)
            {
                Send.Message(new NMFreezeTile(Position));
            }
        }
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (!melted)
        {
            if (with is PhysicsObject)
            {
                (with as PhysicsObject).specialFrictionMod = 0.16f;
                (with as PhysicsObject).modFric = true;
            }
        }
        else if (Graphics.frame - lastHitFrame > 5 && with.totalImpactPower > 2.5f && with.impactPowerV > 0.5f)
        {
            lastHitFrame = Graphics.frame;
            int num = (int)(with.totalImpactPower * 0.5f);
            if (num > 4)
            {
                num = 4;
            }
            if (num < 2)
            {
                num = 2;
            }
            switch (from)
            {
                case ImpactedFrom.Right:
                    {
                        for (int l = 0; l < num; l++)
                        {
                            Level.Add(new SnowFallParticle(base.left - Rando.Float(0f, 1f), with.Y + Rando.Float(-6f, 6f), new Vec2(0f - Rando.Float(0.3f, 1f), Rando.Float(-0.5f, 0.5f))));
                        }
                        break;
                    }
                case ImpactedFrom.Left:
                    {
                        for (int j = 0; j < num; j++)
                        {
                            Level.Add(new SnowFallParticle(base.right - Rando.Float(0f, 1f), with.Y + Rando.Float(-6f, 6f), new Vec2(Rando.Float(0.3f, 1f), Rando.Float(-0.5f, 0.5f))));
                        }
                        break;
                    }
                case ImpactedFrom.Bottom:
                    {
                        for (int k = 0; k < num; k++)
                        {
                            Level.Add(new SnowFallParticle(with.X + Rando.Float(-6f, 6f), base.top - Rando.Float(0f, 1f), new Vec2(Rando.Float(-0.5f, 0.5f), 0f - Rando.Float(0.3f, 1f))));
                        }
                        break;
                    }
                case ImpactedFrom.Top:
                    {
                        for (int i = 0; i < num; i++)
                        {
                            Level.Add(new SnowFallParticle(with.X + Rando.Float(-6f, 6f), base.bottom + Rando.Float(0f, 1f), new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0.3f, 1f))));
                        }
                        break;
                    }
            }
        }
        base.OnSoftImpact(with, from);
    }
}
