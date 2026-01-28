using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isInDemo", true)]
public class ArcadeLight : Thing
{
    private PointLight _light;

    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public ArcadeLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("arcadeLight");
        Center = new Vec2(9f, 24f);
        _collisionSize = new Vec2(16f, 24f);
        _collisionOffset = new Vec2(-8f, -22f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(Position + new Vec2(-8f, 2f), Position + new Vec2(-8f, -8f), new Color(1f, 0.7f, 0.7f)));
            _occluders.Add(new LightOccluder(Position + new Vec2(10f, 2f), Position + new Vec2(10f, -8f), new Color(1f, 0.7f, 0.7f)));
            _occluders.Add(new LightOccluder(Position + new Vec2(-8f, -7f), Position + new Vec2(10f, -7f), new Color(1f, 0.7f, 0.7f)));
            _light = new PointLight(base.X + 1f, base.Y - 6f, new Color(255, 255, 190), 130f, _occluders);
            Level.Add(_light);
            _shade = new SpriteThing(base.X, base.Y, new Sprite("arcadeLight"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }

    public override void Update()
    {
        _light.visible = visible;
        _shade.visible = visible;
        base.Update();
    }
}
