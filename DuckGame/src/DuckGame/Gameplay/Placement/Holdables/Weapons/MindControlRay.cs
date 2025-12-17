namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class MindControlRay : Gun
{
    public StateBinding _controlledDuckBinding = new StateBinding(nameof(_controlledDuck));

    private Duck _prevControlDuck;

    private SpriteMap _sprite;

    private SpriteMap _hat;

    private ActionTimer _beamTimer = 0.2f;

    public Duck _controlledDuck;

    private float _beamTime;

    private float _canConvert;

    private int _boltWait;

    private LoopingSound _beamSound = new LoopingSound("mindBeam");

    public Duck controlledDuck => _controlledDuck;

    public override NetworkConnection connection
    {
        get
        {
            return base.connection;
        }
        set
        {
            if (connection == DuckNetwork.localConnection && value != connection)
            {
                LoseControl();
            }
            base.connection = value;
        }
    }

    public MindControlRay(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 99;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        _sprite = new SpriteMap("mindControlGun", 16, 16);
        _sprite.frame = 2;
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-7f, -4f);
        collisionSize = new Vec2(14f, 10f);
        _hat = new SpriteMap("mindControlHelmet", 32, 32);
        _hat.center = new Vec2(16f, 16f);
        _barrelOffsetTL = new Vec2(18f, 8f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 1f;
        flammable = 0.8f;
        editorTooltip = "Best friend of Mind Control Steve. Takes control of enemy Ducks.";
    }

    public override void Terminate()
    {
        _beamSound.Kill();
        base.Terminate();
    }

    public override void Update()
    {
        if (owner != null)
        {
            _sprite.frame = 0;
        }
        else
        {
            _sprite.frame = 1;
        }
        immobilizeOwner = _controlledDuck != null;
        if (base.isServerForObject)
        {
            if (_beamTime > 1f || owner == null)
            {
                _beamTime = 0f;
                _triggerHeld = false;
                LoseControl();
            }
            if (_controlledDuck != null && owner is Duck d)
            {
                if (Network.isActive)
                {
                    _controlledDuck.mindControl = d.inputProfile;
                    d.Fondle(_controlledDuck);
                    d.Fondle(_controlledDuck.holdObject);
                    foreach (Equipment e in _controlledDuck._equipment)
                    {
                        d.Fondle(e);
                    }
                    d.Fondle(_controlledDuck._ragdollInstance);
                    d.Fondle(_controlledDuck._trappedInstance);
                    d.Fondle(_controlledDuck._cookedInstance);
                }
                if (d.inputProfile.Pressed("QUACK") || _controlledDuck.dead || _controlledDuck.HasEquipment(typeof(TinfoilHat)))
                {
                    _beamTime = 0f;
                    _triggerHeld = false;
                    LoseControl();
                    return;
                }
                _triggerHeld = true;
                if (_controlledDuck.x < d.x)
                {
                    d.offDir = -1;
                }
                else
                {
                    d.offDir = 1;
                }
            }
        }
        else
        {
            Duck d2 = owner as Duck;
            if (_controlledDuck != null && d2 != null)
            {
                _controlledDuck.mindControl = d2.inputProfile;
                d2.Fondle(_controlledDuck.holdObject);
                foreach (Equipment e2 in _controlledDuck._equipment)
                {
                    d2.Fondle(e2);
                }
                d2.Fondle(_controlledDuck._ragdollInstance);
                d2.Fondle(_controlledDuck._trappedInstance);
                d2.Fondle(_controlledDuck._cookedInstance);
            }
            if (_controlledDuck == null && _prevControlDuck != null)
            {
                _prevControlDuck.mindControl = null;
            }
            _prevControlDuck = _controlledDuck;
        }
        if (_triggerHeld && _controlledDuck != null)
        {
            _beamTime += 0.005f;
            _beamSound.pitch = Maths.NormalizeSection(_beamTime, 0.5f, 1f) * 0.6f;
        }
        else
        {
            _beamSound.pitch = 0f;
        }
        base.Update();
        if (_triggerHeld && _beamTimer.hit)
        {
            Vec2 pos = Offset(base.barrelOffset);
            Level.Add(new ControlWave(pos.x, pos.y, base.barrelAngle, this, base.isServerForObject));
            if (_controlledDuck != null)
            {
                _boltWait++;
                if (_boltWait > 2)
                {
                    Level.Add(new MindControlBolt(pos.x, pos.y, _controlledDuck));
                    _boltWait = 0;
                }
            }
            else
            {
                _boltWait = 0;
            }
        }
        _beamSound.lerpVolume = (_triggerHeld ? 0.55f : 0f);
        _beamSound.Update();
        if (_canConvert > 0f)
        {
            _canConvert -= 0.02f;
        }
        else
        {
            _canConvert = 0f;
        }
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        base.onFire = true;
        return true;
    }

    public override void Draw()
    {
        base.Draw();
        if (owner != null && owner is Duck d && !d.HasEquipment(typeof(Hat)))
        {
            _hat.alpha = d._sprite.alpha;
            _hat.flipH = d._sprite.flipH;
            _hat.depth = d.depth + 1;
            if (d._sprite.imageIndex > 11 && d._sprite.imageIndex < 14)
            {
                _hat.angleDegrees = (d._sprite.flipH ? 90 : (-90));
            }
            else
            {
                _hat.angleDegrees = 0f;
            }
            Vec2 offset = DuckRig.GetHatPoint(d._sprite.imageIndex);
            Graphics.Draw(_hat, d.x + offset.x * d._sprite.flipMultH, d.y + offset.y * d._sprite.flipMultV);
        }
    }

    public void ControlDuck(Duck d)
    {
        if (d == null || _canConvert > 0.01f || d.dead)
        {
            return;
        }
        LoseControl();
        if (!(owner is Duck own) || own == d)
        {
            return;
        }
        own.resetAction = true;
        d.profile.stats.timesMindControlled++;
        _controlledDuck = d;
        if (Network.isActive)
        {
            own.Fondle(d);
            own.Fondle(_controlledDuck.holdObject);
            foreach (Equipment e in _controlledDuck._equipment)
            {
                own.Fondle(e);
            }
            own.Fondle(_controlledDuck._ragdollInstance);
            own.Fondle(_controlledDuck._trappedInstance);
            own.Fondle(_controlledDuck._cookedInstance);
        }
        _controlledDuck.resetAction = true;
        _controlledDuck.mindControl = own.inputProfile;
        _controlledDuck.controlledBy = own;
        immobilizeOwner = true;
        SFX.Play("radioNoise", 0.8f);
        Event.Log(new MindControlEvent(base.responsibleProfile, d.profile));
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogBonus();
        }
    }

    public void LoseControl()
    {
        if (_controlledDuck != null)
        {
            Duck own = owner as Duck;
            if (own == null)
            {
                own = base.prevOwner as Duck;
            }
            if (own != null)
            {
                own.immobilized = false;
            }
            if (_controlledDuck != null)
            {
                _controlledDuck.mindControl = null;
                _controlledDuck.controlledBy = null;
            }
            _controlledDuck = null;
            _canConvert = 1f;
        }
    }

    public override void OnPressAction()
    {
        _beamTime = 0f;
        _beamTimer.SetToEnd();
    }

    public override void OnHoldAction()
    {
    }

    public override void Fire()
    {
    }
}
