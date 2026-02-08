using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Pistols")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("isFatal", false)]
public class DuelingPistol : Gun
{
    public DuelingPistol(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 1;
        _ammoType = new ATShrapnel();
        _ammoType.range = 70f;
        _ammoType.accuracy = 0.5f;
        _ammoType.penetration = 0.4f;
        wideBarrel = true;
        _type = "gun";
        graphic = new Sprite("tinyGun");
        Center = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-6f, -4f);
        collisionSize = new Vector2(12f, 8f);
        _barrelOffsetTL = new Vector2(20f, 15f);
        _fireSound = "littleGun";
        _kickForce = 0f;
        _fireRumble = RumbleIntensity.Kick;
        editorTooltip = "The perfect weapon when a Duck has dishonored your family. One shot only.";
    }

    public static void ExplodeEffect(Vector2 position)
    {
        Level.Add(SmallSmoke.New(position.X, position.Y));
        Level.Add(SmallSmoke.New(position.X, position.Y));
        for (int i = 0; i < 8; i++)
        {
            Level.Add(Spark.New(position.X + Rando.Float(-3f, 3f), position.Y + Rando.Float(-3f, 3f), new Vector2(Rando.Float(-3f, 3f), 0f - Rando.Float(-3f, 3f)), 0.05f));
        }
        SFX.Play("shotgun", 1f, 0.3f);
    }

    public override void OnPressAction()
    {
        if (plugged && base.isServerForObject)
        {
            _kickForce = 3f;
            ApplyKick();
            ExplodeEffect(Position);
            if (Network.isActive)
            {
                Send.Message(new NMPistolExplode(Position));
            }
            if (base.duck != null)
            {
                base.duck.Swear();
            }
            Level.Remove(this);
        }
        else
        {
            base.OnPressAction();
        }
    }
}
