using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class OfficeLight : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public OfficeLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("officeLight");
        center = new Vec2(16f, 3f);
        _collisionSize = new Vec2(30f, 6f);
        _collisionOffset = new Vec2(-15f, -3f);
        base.depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(position + new Vec2(-15f, -3f), position + new Vec2(-15f, 4f), new Color(1f, 1f, 1f)));
            _occluders.Add(new LightOccluder(position + new Vec2(15f, -3f), position + new Vec2(15f, 4f), new Color(1f, 1f, 1f)));
            _occluders.Add(new LightOccluder(position + new Vec2(-15f, -2f), position + new Vec2(15f, -2f), new Color(1f, 1f, 1f)));
            Level.Add(new PointLight(base.x, base.y - 1f, new Color(255, 255, 255), 100f, _occluders));
            _shade = new SpriteThing(base.x, base.y, new Sprite("officeLight"));
            _shade.center = center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }
}
