using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Rifles")]
[BaggedProperty("isInDemo", true)]
public class Musket : TampingWeapon
{
    public Musket(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 99;
        _ammoType = new ATShrapnel();
        _ammoType.range = 470f;
        _ammoType.rangeVariation = 70f;
        _ammoType.accuracy = 0.2f;
        _type = "gun";
        graphic = new Sprite("musket");
        Center = new Vector2(19f, 5f);
        collisionOffset = new Vector2(-8f, -3f);
        collisionSize = new Vector2(16f, 7f);
        _barrelOffsetTL = new Vector2(38f, 3f);
        _fireSound = "shotgun";
        _kickForce = 2f;
        _fireRumble = RumbleIntensity.Light;
        _holdOffset = new Vector2(3f, 0f);
        editorTooltip = "Old-timey rifle, takes approximately 150 years to reload.";
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnPressAction()
    {
        if (_tamped)
        {
            base.OnPressAction();
            int num = 0;
            for (int i = 0; i < 14; i++)
            {
                MusketSmoke smoke = new MusketSmoke(base.barrelPosition.X - 16f + Rando.Float(32f), base.barrelPosition.Y - 16f + Rando.Float(32f));
                smoke.Depth = 0.9f + (float)i * 0.001f;
                if (num < 6)
                {
                    smoke.move -= base.barrelVector * Rando.Float(0.1f);
                }
                if (num > 5 && num < 10)
                {
                    smoke.fly += base.barrelVector * (2f + Rando.Float(7.8f));
                }
                Level.Add(smoke);
                num++;
            }
            _tampInc = 0f;
            _tampTime = (infinite.value ? 0.5f : 0f);
            _tamped = false;
        }
        else if (!_raised && owner is Duck { grounded: not false } duckOwner)
        {
            duckOwner.immobilized = true;
            duckOwner.sliding = false;
            _rotating = true;
        }
    }
}
