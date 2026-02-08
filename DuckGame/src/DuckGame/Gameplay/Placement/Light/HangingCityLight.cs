using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Normal)]
[BaggedProperty("isInDemo", true)]
public class HangingCityLight : Thing
{
    private SpriteThing _shade;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    public HangingCityLight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("hangingCityLight");
        Center = new Vector2(8f, 5f);
        _collisionSize = new Vector2(8f, 8f);
        _collisionOffset = new Vector2(-4f, -5f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Vector2 lightPos = new Vector2(base.X, base.Y);
            _occluders.Add(new LightOccluder(lightPos + new Vector2(-8f, 5f), lightPos + new Vector2(1f, -4f), new Color(0.4f, 0.4f, 0.4f)));
            _occluders.Add(new LightOccluder(lightPos + new Vector2(-1f, -4f), lightPos + new Vector2(8f, 5f), new Color(0.4f, 0.4f, 0.4f)));
            Level.Add(new PointLight(lightPos.X, lightPos.Y, new Color(247, 198, 120), 180f, _occluders));
        }
    }
}
