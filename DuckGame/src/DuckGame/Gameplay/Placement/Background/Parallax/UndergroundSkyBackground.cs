using Microsoft.Xna.Framework;

namespace DuckGame;

public class UndergroundSkyBackground : BackgroundUpdater
{
    private float _speedMult;

    private bool _moving;

    public UndergroundSkyBackground(float xpos, float ypos, bool moving = false, float speedMult = 1f)
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
            _parallax = new ParallaxBackground("background/underground", 0f, 0f, 5);
            float speed = 0.9f * _speedMult;
            float backDist = 0.99f;
            _parallax.AddZone(0, backDist, speed);
            _parallax.AddZone(1, backDist, speed);
            _parallax.AddZone(2, backDist, speed);
            _parallax.AddZone(3, backDist, speed);
            _parallax.AddZone(4, backDist, speed);
            _parallax.AddZone(5, backDist, speed);
            _parallax.AddZone(6, backDist, speed);
            _parallax.AddZone(7, backDist, speed);
            _parallax.AddZone(8, backDist, speed);
            _parallax.AddZone(9, backDist, speed);
            _parallax.AddZone(10, backDist, speed);
            Level.Add(_parallax);
            _parallax.X -= 340f;
            _parallax.restrictBottom = false;
            _parallax.Depth = -0.9f;
            _parallax.layer = new Layer("PARALLAX3", 115, new Camera(0f, 0f, 320f, 200f));
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
