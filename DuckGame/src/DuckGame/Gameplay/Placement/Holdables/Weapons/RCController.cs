namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("canSpawn", false)]
public class RCController : Gun
{
    public StateBinding _carBinding = new StateBinding(nameof(_car));

    public StateBinding _burningBinding = new StateBinding(nameof(_burning));

    public StateBinding _burnLifeBinding = new StateBinding(nameof(_burnLife));

    private SpriteMap _sprite;

    public bool _burning;

    public float _burnLife = 1f;

    public float _burnWait;

    public RCCar _car;

    private bool _pressed;

    private int _inc;

    private Thing lockedOwner;

    public RCController(float xval, float yval, RCCar car)
        : base(xval, yval)
    {
        ammo = 99;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        _sprite = new SpriteMap("rcController", 32, 32);
        graphic = _sprite;
        center = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-6f, -4f);
        collisionSize = new Vec2(12f, 9f);
        _barrelOffsetTL = new Vec2(26f, 14f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        flammable = 0.8f;
        _car = car;
        editorTooltip = "Allows you to drive a small car strapped with explosives. Every Duck's dream!";
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        if (_car == null && !(Level.current is Editor) && base.isServerForObject)
        {
            _car = new RCCar(base.x, base.y);
            _car._controller = this;
            Level.Add(_car);
        }
        _inc++;
        if (_inc > 14 && _car != null)
        {
            _inc = 0;
            if (_car.receivingSignal && !_car.destroyed)
            {
                Level.Add(new RCControlBolt(base.x, base.y, _car));
            }
        }
        if (lockedOwner != owner)
        {
            Release(lockedOwner);
        }
        if (base.isServerForObject)
        {
            if (_burning && _burnLife > 0f)
            {
                _burnWait -= 0.01f;
                if (_burnWait < 0f)
                {
                    Level.Add(SmallFire.New(8f, 0f, 0f, 0f, shortLife: false, this, canMultiply: false, this));
                    _burnWait = 1f;
                }
                _burnLife -= 0.002f;
                if (_burnLife <= 0f)
                {
                    _sprite.frame = 1;
                }
            }
            if (owner is Duck duckOwner)
            {
                if (_pressed && _car != null)
                {
                    if (_car.owner == null)
                    {
                        Fondle(_car);
                    }
                    _car.moveLeft = duckOwner.inputProfile.Down("LEFT");
                    _car.moveRight = duckOwner.inputProfile.Down("RIGHT");
                    _car.jump = duckOwner.inputProfile.Pressed("JUMP");
                    if (duckOwner.inputProfile.Pressed("GRAB"))
                    {
                        Fondle(_car);
                        _car.Destroy();
                    }
                }
                else
                {
                    _car.moveLeft = (_car.moveRight = (_car.jump = false));
                }
            }
            if (_car != null && _car.destroyed)
            {
                ammo = 0;
            }
        }
        base.Update();
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        _burning = true;
        return true;
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void OnPressAction()
    {
        _pressed = true;
        if (_car != null && !_car.destroyed)
        {
            if (owner is Duck duckOwner)
            {
                duckOwner.immobilized = true;
                duckOwner.remoteControl = true;
            }
            if (lockedOwner != null)
            {
                Release(lockedOwner);
            }
            lockedOwner = owner;
            _car.receivingSignal = true;
            if (Level.current.camera is FollowCam cam)
            {
                cam.Add(_car);
            }
        }
    }

    private void Release(Thing pOwner)
    {
        Duck duckOwner = pOwner as Duck;
        if (duckOwner == null)
        {
            duckOwner = base.prevOwner as Duck;
        }
        if (duckOwner != null)
        {
            duckOwner.immobilized = false;
            duckOwner.remoteControl = false;
        }
        _pressed = false;
        lockedOwner = null;
        if (_car != null)
        {
            _car.receivingSignal = false;
            if (Level.current.camera is FollowCam cam)
            {
                cam.Remove(_car);
            }
        }
    }

    public override void OnReleaseAction()
    {
        Release(owner);
    }

    public override void Fire()
    {
    }
}
