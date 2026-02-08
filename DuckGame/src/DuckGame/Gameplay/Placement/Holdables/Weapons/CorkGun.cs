using Microsoft.Xna.Framework;

namespace DuckGame;

public class CorkGun : Gun
{
    public CorkObject corkObject;

    private SpriteMap _sprite;

    private int _firedCork;

    public float windingVelocity;

    public CorkGun(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 100000;
        _ammoType = new ATCork();
        _type = "gun";
        _sprite = new SpriteMap("corkGun", 13, 10);
        graphic = _sprite;
        Center = new Vector2(6f, 4f);
        collisionOffset = new Vector2(-6f, -4f);
        collisionSize = new Vector2(12f, 8f);
        _barrelOffsetTL = new Vector2(10f, 3f);
        _fireSound = "corkFire";
        _kickForce = 1f;
        _fireRumble = RumbleIntensity.Kick;
    }

    public override void Update()
    {
        if (windingVelocity > 1f)
        {
            windingVelocity = 1f;
        }
        windingVelocity = Lerp.FloatSmooth(windingVelocity, 0f, 0.05f);
        if (corkObject != null)
        {
            float num = corkObject.WindUp(windingVelocity);
            if (num < 10f)
            {
                Level.Remove(corkObject);
                ammo = 1;
                windingVelocity = 0f;
                corkObject = null;
                _firedCork = 0;
                base.Scale = new Vector2(1.5f, 1.5f);
            }
            if (num < 16f)
            {
                windingVelocity = 1f;
            }
        }
        base.Scale = Lerp.Vec2Smooth(base.Scale, Vector2.One, 0.1f);
        _sprite.frame = ((_firedCork != 0) ? 1 : 0);
        base.Update();
    }

    public override void OnPressAction()
    {
        if (_firedCork == 0)
        {
            _firedCork = 1;
            base.OnPressAction();
        }
    }

    public override void OnHoldAction()
    {
        if (_firedCork == 2)
        {
            windingVelocity += 0.12f;
        }
        base.OnHoldAction();
    }

    public override void OnReleaseAction()
    {
        if (_firedCork == 1)
        {
            _firedCork = 2;
        }
        base.OnReleaseAction();
    }
}
