using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Machine Guns")]
[BaggedProperty("isSuperWeapon", true)]
public class AK47 : Gun
{
    public AK47(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 30;
        _ammoType = new ATHighCalMachinegun();
        _type = "gun";
        graphic = new Sprite("ak47");
        Center = new Vector2(16f, 15f);
        collisionOffset = new Vector2(-8f, -3f);
        collisionSize = new Vector2(18f, 10f);
        _barrelOffsetTL = new Vector2(32f, 14f);
        _fireSound = "deepMachineGun2";
        _fullAuto = true;
        _fireWait = 1.2f;
        _kickForce = 3.5f;
        _fireRumble = RumbleIntensity.Kick;
        loseAccuracy = 0.2f;
        maxAccuracyLost = 0.8f;
        editorTooltip = "Go-to weapon of all your favorite Duck Action Heroes.";
    }
}
