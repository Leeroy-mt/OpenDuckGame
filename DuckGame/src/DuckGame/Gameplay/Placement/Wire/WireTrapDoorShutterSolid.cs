using Microsoft.Xna.Framework;

namespace DuckGame;

public class WireTrapDoorShutterSolid : Block, IShutter
{
    private WireTrapDoor _button;

    private bool _open;

    private Vector2 _colSize;

    private SpriteMap _sprite;

    public void UpdateSprite()
    {
        _sprite.frame = (int)_button.length - 1 + (int)_button.color * 4;
        _colSize = new Vector2(7 + 16 * (int)_button.length, 12f);
        collisionSize = _colSize;
        UpdateOpenState();
    }

    public WireTrapDoorShutterSolid(float xpos, float ypos, WireTrapDoor b)
        : base(xpos, ypos)
    {
        _button = b;
        collisionSize = new Vector2(39f, 12f);
        collisionOffset = new Vector2(-3f, -3f);
        Center = new Vector2(3f, 3f);
        _sprite = new SpriteMap("wireTrapDoorArmBig", 71, 13);
        graphic = _sprite;
    }

    public override void Initialize()
    {
        UpdateSprite();
        base.Initialize();
    }

    private void UpdateOpenState()
    {
        if (base.AngleDegrees == 0f)
        {
            collisionSize = _colSize;
            _open = false;
        }
        else
        {
            if (base.AngleDegrees == 0f)
            {
                return;
            }
            if (!_open)
            {
                if (!(Level.current is Editor))
                {
                    foreach (PhysicsObject item in Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vector2(0f, -8f), base.bottomRight))
                    {
                        item.sleeping = false;
                    }
                }
                _open = true;
            }
            collisionSize = new Vector2(1f, 1f);
        }
    }

    public override void Update()
    {
        UpdateOpenState();
        base.Update();
    }

    public override void Draw()
    {
        graphic.flipH = offDir < 0;
        base.Draw();
    }
}
