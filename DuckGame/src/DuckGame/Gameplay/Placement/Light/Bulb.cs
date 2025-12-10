using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class Bulb : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public Bulb(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("bulb");
        center = new Vec2(8f, 4f);
        _collisionSize = new Vec2(4f, 6f);
        _collisionOffset = new Vec2(-2f, -4f);
        base.depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Level.Add(new PointLight(base.x, base.y, new Color(155, 125, 100), 80f, _occluders));
            _shade = new SpriteThing(base.x, base.y, new Sprite("bulb"));
            _shade.center = center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }
}
