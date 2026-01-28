using System.Linq;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class SnowGenerator : Thing
{
    private static bool initGen;

    private float snowWait = 1f;

    public SnowGenerator(float x, float y)
        : base(x, y)
    {
        _editorName = "Snow Machine";
        graphic = new Sprite("snowGenerator");
        Center = new Vec2(8f, 8f);
        base.Depth = 0.55f;
        _visibleInGame = false;
        snowWait = Rando.Float(4f);
        editorTooltip = "Let it snow!";
        solid = false;
        _collisionSize = new Vec2(0f, 0f);
        maxPlaceable = 32;
    }

    public override void Initialize()
    {
        initGen = true;
        base.Initialize();
    }

    public override void Update()
    {
        if (initGen)
        {
            int num = 0;
            foreach (SnowGenerator g in Level.current.things[typeof(SnowGenerator)].ToList())
            {
                if (num < maxPlaceable)
                {
                    num++;
                }
                else
                {
                    Level.current.RemoveThing(g);
                }
            }
            initGen = false;
        }
        snowWait -= Maths.IncFrameTimer();
        if (snowWait <= 0f)
        {
            snowWait = Rando.Float(2f, 4f);
            Level.Add(new SnowFallParticle(base.X + Rando.Float(-8f, 8f), base.Y + Rando.Float(-8f, 8f), new Vec2(0f, 0f)));
        }
        base.Update();
    }
}
