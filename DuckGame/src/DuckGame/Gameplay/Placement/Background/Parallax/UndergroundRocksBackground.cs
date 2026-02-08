using Microsoft.Xna.Framework;

namespace DuckGame;

public class UndergroundRocksBackground : BackgroundUpdater
{
    private float _speedMult;

    private bool _moving;

    public UndergroundRocksBackground(float xpos, float ypos, bool moving = false, float speedMult = 1f)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 1
        };
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _speedMult = speedMult;
        _moving = moving;
        visible = false;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Level.current.backgroundColor = new Color(0, 0, 0);
            _parallax = new ParallaxBackground("background/rocksUnderground", 0f, 0f, 3);
            float speed = 0.8f * _speedMult;
            float rockDist = 0.8f;
            for (int i = 0; i < 10; i++)
            {
                _parallax.AddZone(i, rockDist, speed);
            }
            _parallax.AddZone(10, 0.85f, speed);
            _parallax.AddZone(11, 0.9f, speed);
            _parallax.AddZone(19, 0.9f, speed);
            _parallax.AddZone(20, 0.85f, speed);
            _parallax.restrictBottom = false;
            for (int j = 0; j < 11; j++)
            {
                _parallax.AddZone(21 + j, rockDist, speed);
            }
            _parallax.Depth = -0.9f;
            _parallax.layer = new Layer("PARALLAX2", 110, new Camera(0f, 0f, 320f, 200f));
            _parallax.layer.aspectReliesOnGameLayer = true;
            _parallax.layer.allowTallAspect = true;
            overrideBaseScissorCall = true;
            Layer.Add(_parallax.layer);
            Level.Add(_parallax);
        }
    }

    public override void Terminate()
    {
        Level.Remove(_parallax);
    }
}
