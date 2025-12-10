using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Normal)]
[BaggedProperty("isInDemo", true)]
public class TroubleLight : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public TroubleLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("troubleLight");
        center = new Vec2(7f, 5f);
        _collisionSize = new Vec2(10f, 10f);
        _collisionOffset = new Vec2(-3f, -4f);
        base.depth = 0.9f;
        base.hugWalls = WallHug.Floor;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            if (flipHorizontal)
            {
                Vec2 lightPos = new Vec2(base.x - 1f, base.y - 1f);
                _occluders.Add(new LightOccluder(lightPos + new Vec2(2f, 4f), lightPos + new Vec2(2f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos + new Vec2(2f, 2f), lightPos + new Vec2(-6f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos.x + 1f, lightPos.y + 1f, new Color(247, 198, 150), 170f, _occluders));
            }
            else
            {
                Vec2 lightPos2 = new Vec2(base.x + 1f, base.y - 1f);
                _occluders.Add(new LightOccluder(lightPos2 + new Vec2(-2f, 4f), lightPos2 + new Vec2(-2f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos2 + new Vec2(-2f, 2f), lightPos2 + new Vec2(6f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos2.x - 1f, lightPos2.y + 1f, new Color(247, 198, 150), 170f, _occluders));
            }
        }
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        base.Draw();
    }
}
