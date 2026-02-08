using Microsoft.Xna.Framework;
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
        Center = new Vector2(7f, 5f);
        _collisionSize = new Vector2(10f, 10f);
        _collisionOffset = new Vector2(-3f, -4f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Floor;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            if (flipHorizontal)
            {
                Vector2 lightPos = new Vector2(base.X - 1f, base.Y - 1f);
                _occluders.Add(new LightOccluder(lightPos + new Vector2(2f, 4f), lightPos + new Vector2(2f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos + new Vector2(2f, 2f), lightPos + new Vector2(-6f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos.X + 1f, lightPos.Y + 1f, new Color(247, 198, 150), 170f, _occluders));
            }
            else
            {
                Vector2 lightPos2 = new Vector2(base.X + 1f, base.Y - 1f);
                _occluders.Add(new LightOccluder(lightPos2 + new Vector2(-2f, 4f), lightPos2 + new Vector2(-2f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos2 + new Vector2(-2f, 2f), lightPos2 + new Vector2(6f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos2.X - 1f, lightPos2.Y + 1f, new Color(247, 198, 150), 170f, _occluders));
            }
        }
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        base.Draw();
    }
}
