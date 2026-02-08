using Microsoft.Xna.Framework;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
[BaggedProperty("isFatal", false)]
public class WaterRifle : Gun
{
    private FluidStream _stream;

    private ConstantSound _sound = new ConstantSound("demoBlaster");

    private new int _wait;

    public WaterRifle(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 9;
        _ammoType = new AT9mm();
        _type = "gun";
        graphic = new Sprite("waterGun");
        Center = new Vector2(11f, 7f);
        collisionOffset = new Vector2(-11f, -6f);
        collisionSize = new Vector2(23f, 13f);
        _barrelOffsetTL = new Vector2(24f, 6f);
        _fireSound = "pistolFire";
        _kickForce = 3f;
        _holdOffset = new Vector2(-1f, 0f);
        loseAccuracy = 0.1f;
        maxAccuracyLost = 0.6f;
        _bio = "";
        _editorName = "Water Blaster";
        physicsMaterial = PhysicsMaterial.Metal;
        _stream = new FluidStream(base.X, base.Y, new Vector2(1f, 0f), 2f);
        isFatal = false;
    }

    public override void Initialize()
    {
        Level.Add(_stream);
    }

    public override void Terminate()
    {
        Level.Remove(_stream);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnPressAction()
    {
    }

    public override void OnHoldAction()
    {
        _wait++;
        if (_wait == 3)
        {
            _stream.sprayAngle = base.barrelVector * 2f;
            _stream.Position = base.barrelPosition;
            FluidData dat = Fluid.Water;
            dat.amount = 0.01f;
            _stream.Feed(dat);
            _wait = 0;
        }
    }
}
