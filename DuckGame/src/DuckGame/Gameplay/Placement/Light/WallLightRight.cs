using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class WallLightRight : Thing
{
    private PointLight _light;

    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public WallLightRight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("wallLight");
        Center = new Vec2(8f, 8f);
        _collisionSize = new Vec2(5f, 16f);
        _collisionOffset = new Vec2(2f, -8f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Right;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(base.topLeft, base.topRight + new Vec2(2f, 0f), new Color(1f, 0.8f, 0.8f)));
            _occluders.Add(new LightOccluder(base.bottomLeft, base.bottomRight + new Vec2(2f, 0f), new Color(1f, 0.8f, 0.8f)));
            _light = new PointLight(base.X + 5f, base.Y, new Color(255, 255, 190), 100f, _occluders);
            Level.Add(_light);
            _shade = new SpriteThing(base.X, base.Y, new Sprite("wallLight"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            _shade.flipHorizontal = true;
            Level.Add(_shade);
        }
    }

    public override void Update()
    {
        _light.visible = visible;
        _shade.visible = visible;
        base.Update();
    }

    public override void Draw()
    {
        graphic.flipH = true;
        base.Draw();
    }
}
