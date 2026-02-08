using Microsoft.Xna.Framework;
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
        Center = new Vector2(8f, 4f);
        _collisionSize = new Vector2(4f, 6f);
        _collisionOffset = new Vector2(-2f, -4f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Level.Add(new PointLight(base.X, base.Y, new Color(155, 125, 100), 80f, _occluders));
            _shade = new SpriteThing(base.X, base.Y, new Sprite("bulb"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }
}
