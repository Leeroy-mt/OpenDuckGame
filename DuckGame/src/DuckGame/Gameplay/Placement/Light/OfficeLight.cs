using Microsoft.Xna.Framework;
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
        Center = new Vector2(16f, 3f);
        _collisionSize = new Vector2(30f, 6f);
        _collisionOffset = new Vector2(-15f, -3f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, -3f), Position + new Vector2(-15f, 4f), new Color(1f, 1f, 1f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(15f, -3f), Position + new Vector2(15f, 4f), new Color(1f, 1f, 1f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, -2f), Position + new Vector2(15f, -2f), new Color(1f, 1f, 1f)));
            Level.Add(new PointLight(base.X, base.Y - 1f, new Color(255, 255, 255), 100f, _occluders));
            _shade = new SpriteThing(base.X, base.Y, new Sprite("officeLight"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }
}
