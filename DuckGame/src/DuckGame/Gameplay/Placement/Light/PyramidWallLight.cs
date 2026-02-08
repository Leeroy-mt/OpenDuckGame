using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Pyramid", EditorItemType.Pyramid)]
[BaggedProperty("isInDemo", true)]
public class PyramidWallLight : Thing
{
    public static Color lightColor = new Color(byte.MaxValue, (byte)200, (byte)150);

    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    private SpriteMap _sprite;

    private Vector2 lightPos;

    public PyramidWallLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("pyramidWallLight", 14, 12);
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
        base.hugWalls = WallHug.Left | WallHug.Right;
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        if (DevConsole.showCollision)
        {
            Graphics.DrawCircle(lightPos, 2f, Color.Blue);
            foreach (LightOccluder o in _occluders)
            {
                Graphics.DrawLine(o.p1, o.p2, Color.Red, 1f, 1f);
            }
        }
        base.Draw();
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, 3f), Position + new Vector2(-15f, -4f), new Color(0.95f, 0.9f, 0.85f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(15f, 3f), Position + new Vector2(15f, -4f), new Color(0.95f, 0.9f, 0.85f)));
            _occluders.Add(new LightOccluder(Position + new Vector2(-15f, 2f), Position + new Vector2(15f, 2f), new Color(0.95f, 0.9f, 0.85f)));
            if (flipHorizontal)
            {
                lightPos = new Vector2(base.X, base.Y);
                Level.Add(new PointLight(lightPos.X, lightPos.Y, lightColor, 120f, _occluders, strangeFalloff: true));
            }
            else
            {
                lightPos = new Vector2(base.X, base.Y);
                Level.Add(new PointLight(lightPos.X, lightPos.Y, lightColor, 120f, _occluders, strangeFalloff: true));
            }
        }
    }
}
