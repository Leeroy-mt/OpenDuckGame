using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Normal)]
[BaggedProperty("isInDemo", true)]
public class StreetLight : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public StreetLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("streetLight");
        Center = new Vector2(6f, 54f);
        _collisionSize = new Vector2(8f, 8f);
        _collisionOffset = new Vector2(-4f, -2f);
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
                Vector2 lightPos = new Vector2(base.X - 16f, base.Y - 32f - 20f);
                _occluders.Add(new LightOccluder(lightPos + new Vector2(8f, 5f), lightPos + new Vector2(-1f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos + new Vector2(1f, -4f), lightPos + new Vector2(-8f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos.X, lightPos.Y + 1f, new Color(247, 198, 120), 200f, _occluders));
            }
            else
            {
                Vector2 lightPos2 = new Vector2(base.X + 16f, base.Y - 32f - 20f);
                _occluders.Add(new LightOccluder(lightPos2 + new Vector2(-8f, 5f), lightPos2 + new Vector2(1f, -4f), new Color(0.4f, 0.4f, 0.4f)));
                _occluders.Add(new LightOccluder(lightPos2 + new Vector2(-1f, -4f), lightPos2 + new Vector2(8f, 5f), new Color(0.4f, 0.4f, 0.4f)));
                Level.Add(new PointLight(lightPos2.X, lightPos2.Y + 1f, new Color(247, 198, 120), 200f, _occluders));
            }
        }
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        base.Draw();
    }
}
