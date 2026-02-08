using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Pyramid", EditorItemType.Pyramid)]
[BaggedProperty("isInDemo", true)]
public class PyramidLightRoof : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    private Block myBlock;

    private PointLight light;

    private bool did;

    public PyramidLightRoof(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("pyramidRoofLight");
        Center = new Vector2(7f, 5f);
        _collisionSize = new Vector2(14f, 6f);
        _collisionOffset = new Vector2(-7f, -3f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, -3f), Position + new Vector2(-15f, 4f), new Color(1f, 0.9f, 0.8f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(15f, -3f), Position + new Vector2(15f, 4f), new Color(1f, 0.9f, 0.8f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, -2f), Position + new Vector2(15f, -2f), new Color(1f, 0.9f, 0.8f)));
            light = new PointLight(base.X, base.Y - 1f, PyramidWallLight.lightColor, 110f, _occluders, strangeFalloff: true);
            Level.Add(light);
            _shade = new SpriteThing(base.X, base.Y, new Sprite("pyramidRoofLightShade"));
            _shade.Center = Center;
            _shade.layer = Layer.Foreground;
            Level.Add(_shade);
        }
    }

    public override void Update()
    {
        if (!did)
        {
            myBlock = Level.CheckPoint<Block>(new Vector2(base.X, base.Y - 8f));
            did = true;
        }
        if (myBlock != null && myBlock.removeFromLevel)
        {
            Level.Remove(this);
            Level.Remove(light);
            Level.Remove(_shade);
        }
        base.Update();
    }
}
