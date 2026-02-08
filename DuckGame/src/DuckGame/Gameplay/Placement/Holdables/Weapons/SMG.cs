using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Machine Guns")]
public class SMG : Gun
{
    public SMG(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 30;
        _ammoType = new AT9mm();
        _ammoType.range = 110f;
        _ammoType.accuracy = 0.6f;
        _type = "gun";
        graphic = new Sprite("smg");
        Center = new Vector2(8f, 4f);
        collisionOffset = new Vector2(-8f, -4f);
        collisionSize = new Vector2(16f, 8f);
        _barrelOffsetTL = new Vector2(17f, 2f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        _fireRumble = RumbleIntensity.Kick;
        _holdOffset = new Vector2(-1f, 0f);
        loseAccuracy = 0.2f;
        maxAccuracyLost = 0.8f;
        editorTooltip = "Rapid-fire bullet-spitting machine. Great for making artisanal swiss cheese.";
    }
}
