using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Pyramid", EditorItemType.Pyramid)]
[BaggedProperty("isInDemo", true)]
public class PyramidBLight : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    private SpriteMap _sprite;

    public PyramidBLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("pyramidBackgroundLight", 14, 12);
        _sprite.AddAnimation("go", 0.2f, true, 0, 1, 2, 3, 4);
        _sprite.SetAnimation("go");
        graphic = _sprite;
        Center = new Vector2(7f, 8f);
        _collisionSize = new Vector2(8f, 8f);
        _collisionOffset = new Vector2(-4f, -4f);
        base.Depth = -0.9f;
        base.Alpha = 0.7f;
        base.layer = Layer.Game;
        placementLayerOverride = Layer.Blocks;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Level.Add(new PointLight(base.X, base.Y - 1f, PyramidWallLight.lightColor, 120f, null, strangeFalloff: true));
        }
    }
}
