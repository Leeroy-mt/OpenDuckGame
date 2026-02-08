using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class WallLightLeft : Thing
{
    private PointLight _light;

    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public WallLightLeft(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("wallLight");
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(5f, 16f);
        _collisionOffset = new Vector2(-7f, -8f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Left;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(base.topLeft + new Vector2(-2f, 0f), base.topRight, new Color(1f, 0.8f, 0.8f)));
            _occluders.Add(new LightOccluder(base.bottomLeft + new Vector2(-2f, 0f), base.bottomRight, new Color(1f, 0.8f, 0.8f)));
            _light = new PointLight(base.X - 5f, base.Y, new Color(255, 255, 190), 100f, _occluders);
            Level.Add(_light);
            _shade = new SpriteThing(base.X, base.Y, new Sprite("wallLight"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }

    public override void Update()
    {
        _light.visible = visible;
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }
}
