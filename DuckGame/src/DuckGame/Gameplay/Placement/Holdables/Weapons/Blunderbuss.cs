namespace DuckGame;

[EditorGroup("Guns|Shotguns")]
public class Blunderbuss : TampingWeapon
{
    public Blunderbuss(float xval, float yval)
        : base(xval, yval)
    {
        wideBarrel = true;
        ammo = 99;
        _ammoType = new ATShrapnel();
        _ammoType.range = 140f;
        _ammoType.rangeVariation = 40f;
        _ammoType.accuracy = 0.01f;
        _numBulletsPerFire = 4;
        _ammoType.penetration = 0.4f;
        _type = "gun";
        graphic = new Sprite("blunderbuss");
        center = new Vec2(19f, 5f);
        collisionOffset = new Vec2(-8f, -3f);
        collisionSize = new Vec2(16f, 7f);
        _barrelOffsetTL = new Vec2(34f, 4f);
        _fireSound = "shotgun";
        _kickForce = 2f;
        _fireRumble = RumbleIntensity.Light;
        _holdOffset = new Vec2(4f, 1f);
        editorTooltip = "Old-timey shotgun, takes approximately 150 years to reload.";
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
                MusketSmoke smoke = new MusketSmoke(base.barrelPosition.x - 16f + Rando.Float(32f), base.barrelPosition.y - 16f + Rando.Float(32f));
                smoke.depth = 0.9f + (float)i * 0.001f;
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
