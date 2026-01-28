namespace DuckGame;

public abstract class Equipment : Holdable
{
    public StateBinding _equippedBinding = new StateBinding(nameof(netEquippedDuck));

    public StateBinding _equipIndexBinding = new StateBinding(nameof(equipIndex));

    public NetIndex4 equipIndex = new NetIndex4(0);

    public NetIndex4 localEquipIndex = new NetIndex4(0);

    protected bool _hasEquippedCollision;

    protected Vec2 _equippedCollisionOffset;

    protected Vec2 _equippedCollisionSize;

    protected bool _jumpMod;

    protected Vec2 _offset;

    protected bool _autoOffset = true;

    private Vec2 _colSize = Vec2.Zero;

    private float _equipmentHealth = 1f;

    public float autoEquipTime;

    public bool _prevEquipped;

    protected Vec2 _wearOffset = Vec2.Zero;

    public bool wearable = true;

    protected bool _isArmor;

    protected float _equippedThickness = 0.1f;

    private bool _appliedEquippedCollision;

    private Vec2 _unequippedCollisionSize;

    private Vec2 _unequippedCollisionOffset;

    public Duck netEquippedDuck
    {
        get
        {
            return _equippedDuck;
        }
        set
        {
            if (_equippedDuck != value && _equippedDuck != null)
            {
                _equippedDuck.Unequip(this, forceNetwork: true);
            }
            if (_equippedDuck != value)
            {
                value?.Equip(this, makeSound: false, forceNetwork: true);
            }
            _equippedDuck = value;
        }
    }

    public override Vec2 collisionOffset
    {
        get
        {
            if (!_hasEquippedCollision || _equippedDuck == null)
            {
                return _collisionOffset;
            }
            return _equippedCollisionOffset;
        }
        set
        {
            _collisionOffset = value;
        }
    }

    public override Vec2 collisionSize
    {
        get
        {
            if (!_hasEquippedCollision || _equippedDuck == null)
            {
                return _collisionSize;
            }
            return _equippedCollisionSize;
        }
        set
        {
            _collisionSize = value;
        }
    }

    public bool jumpMod => _jumpMod;

    public Vec2 wearOffset
    {
        get
        {
            return _wearOffset;
        }
        set
        {
            _wearOffset = value;
        }
    }

    public bool isArmor => _isArmor;

    public Equipment(float xpos, float ypos)
        : base(xpos, ypos)
    {
        weight = 2f;
        thickness = 0.1f;
    }

    public override void Terminate()
    {
        if (_equippedDuck != null)
        {
            _equippedDuck.Unequip(this, forceNetwork: true);
        }
        base.Terminate();
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (type is DTImpale || type is DTImpact)
        {
            SFX.Play("smallDestroy", 0.8f, Rando.Float(-0.1f, 0.1f));
            for (int i = 0; i < 3; i++)
            {
                Level.Add(SmallSmoke.New(base.X + Rando.Float(-2f, 2f), base.Y + Rando.Float(-2f, 2f)));
            }
        }
        return true;
    }

    public void PositionOnOwner()
    {
        if (_equippedDuck == null)
        {
            return;
        }
        Duck d = base.duck;
        if (d != null && d.skeleton != null)
        {
            DuckBone bone = base.duck.skeleton.upperTorso;
            if (this is Hat || this is ChokeCollar)
            {
                bone = base.duck.skeleton.head;
            }
            else if (this is Boots)
            {
                offDir = owner.offDir;
                bone = base.duck.skeleton.lowerTorso;
            }
            offDir = owner.offDir;
            Position = bone.position;
            Angle = ((offDir > 0) ? (0f - bone.orientation) : bone.orientation);
            Vec2 off = _wearOffset;
            if (this is TeamHat)
            {
                off -= (this as TeamHat).hatOffset;
            }
            Position += new Vec2(off.X * (float)offDir, off.Y).Rotate(Angle, Vec2.Zero);
        }
    }

    public override void Update()
    {
        if (autoEquipTime > 0f)
        {
            autoEquipTime -= 0.016f;
        }
        else
        {
            autoEquipTime = 0f;
        }
        if (base.isServerForObject)
        {
            if (_equipmentHealth <= 0f && _equippedDuck != null && base.duck != null)
            {
                base.duck.KnockOffEquipment(this);
                if (Network.isActive)
                {
                    NetSoundEffect.Play("equipmentTing");
                }
            }
            _equipmentHealth = Lerp.Float(_equipmentHealth, 1f, 0.003f);
        }
        UpdateEquippedCollision();
        if (destroyed)
        {
            base.Alpha -= 0.1f;
            if (base.Alpha < 0f)
            {
                Level.Remove(this);
            }
        }
        if (localEquipIndex < equipIndex)
        {
            for (int i = 0; i < 2; i++)
            {
                Level.Add(SmallSmoke.New(base.X + Rando.Float(-2f, 2f), base.Y + Rando.Float(-2f, 2f)));
            }
            SFX.Play("equip", 0.8f);
            localEquipIndex = equipIndex;
        }
        _prevEquipped = _equippedDuck != null;
        thickness = ((_equippedDuck != null) ? _equippedThickness : 0.1f);
        if (Network.isActive && _equippedDuck != null && base.duck != null && !base.duck.HasEquipment(this))
        {
            base.duck.Equip(this, makeSound: false);
        }
        PositionOnOwner();
        base.Update();
    }

    public new void Hurt(float amount)
    {
        _equipmentHealth -= amount;
    }

    private void UpdateEquippedCollision()
    {
        if (_hasEquippedCollision && _equippedDuck != null)
        {
            if (!_appliedEquippedCollision)
            {
                _unequippedCollisionSize = _collisionSize;
                _unequippedCollisionOffset = _collisionOffset;
            }
            collisionSize = _equippedCollisionSize;
            collisionOffset = _equippedCollisionOffset;
            _appliedEquippedCollision = true;
        }
        else if (_appliedEquippedCollision)
        {
            collisionSize = _unequippedCollisionSize;
            collisionOffset = _equippedCollisionOffset;
            _appliedEquippedCollision = false;
        }
    }

    public virtual void Equip(Duck d)
    {
        if (_equippedDuck == null)
        {
            owner = d;
            solid = false;
            _equippedDuck = d;
        }
        UpdateEquippedCollision();
    }

    public virtual void UnEquip()
    {
        if (_equippedDuck != null)
        {
            owner = null;
            solid = true;
            _equippedDuck = null;
        }
        UpdateEquippedCollision();
    }

    public override void PressAction()
    {
        if (_equippedDuck == null && base.held)
        {
            if (owner is Duck d)
            {
                RumbleManager.AddRumbleEvent(d.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.None));
                d.ThrowItem();
                d.Equip(this);
            }
        }
        else
        {
            base.PressAction();
        }
    }

    public override void Draw()
    {
        PositionOnOwner();
        base.Draw();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (_equippedDuck == null && autoEquipTime > 0f)
        {
            Duck d = with as Duck;
            if (d == null && with is FeatherVolume)
            {
                d = (with as FeatherVolume).duckOwner;
            }
            if (d != null)
            {
                d.Equip(this);
                if (this is ChokeCollar)
                {
                    (this as ChokeCollar).ball.hSpeed = 0f;
                    (this as ChokeCollar).ball.vSpeed = 0f;
                }
            }
        }
        base.OnSoftImpact(with, from);
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (_equippedDuck == null || bullet.owner == base.duck || !bullet.isLocal)
        {
            return false;
        }
        if (_isArmor && base.duck != null)
        {
            if (bullet.isLocal)
            {
                base.duck.KnockOffEquipment(this, ting: true, bullet);
                Thing.Fondle(this, DuckNetwork.localConnection);
            }
            if (bullet.isLocal && Network.isActive)
            {
                NetSoundEffect.Play("equipmentTing");
            }
            bullet.hitArmor = true;
            Level.Add(MetalRebound.New(hitPos.X, hitPos.Y, (bullet.travelDirNormalized.X > 0f) ? 1 : (-1)));
            for (int i = 0; i < 6; i++)
            {
                Level.Add(Spark.New(base.X, base.Y, bullet.travelDirNormalized));
            }
            return base.Hit(bullet, hitPos);
        }
        return base.Hit(bullet, hitPos);
    }
}
