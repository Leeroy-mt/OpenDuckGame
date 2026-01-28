using System;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
public class CampingRifle : Gun
{
    public StateBinding _loadProgressBinding = new StateBinding(nameof(_loadProgress));

    public StateBinding _readyToFireBinding = new StateBinding(nameof(readyToFire));

    public sbyte _loadProgress = 100;

    public float _loadAnimation = 1f;

    public bool readyToFire;

    protected SpriteMap _loaderSprite;

    private SpriteMap _sprite;

    public bool burntOut;

    public CampingRifle(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATCampingBall();
        _type = "gun";
        _sprite = new SpriteMap("camping", 23, 15);
        _sprite.speed = 0f;
        graphic = _sprite;
        Center = new Vec2(11f, 7f);
        collisionOffset = new Vec2(-10f, -5f);
        collisionSize = new Vec2(20f, 12f);
        _barrelOffsetTL = new Vec2(22f, 6f);
        _fireSound = "shotgunFire2";
        _kickForce = 4f;
        _fireRumble = RumbleIntensity.Light;
        _numBulletsPerFire = 6;
        _manualLoad = true;
        flammable = 1f;
        _loaderSprite = new SpriteMap("camping_loader", 6, 4);
        _loaderSprite.Center = new Vec2(3f, 2f);
        _holdOffset = new Vec2(0f, -2f);
        _editorName = "Camping Gun";
        editorTooltip = "Designed to get campers into bed quickly.";
        loaded = false;
        _loadProgress = -1;
        _loadAnimation = 0f;
        isFatal = false;
        _clickSound = "campingEmpty";
        physicsMaterial = PhysicsMaterial.Plastic;
    }

    public override void Update()
    {
        if (!burntOut && burnt >= 1f)
        {
            _sprite = new SpriteMap("campingMelted", 23, 15);
            for (int i = 0; i < 4; i++)
            {
                Level.Add(SmallSmoke.New(Rando.Float(-4f, 4f), Rando.Float(-4f, 4f)));
            }
            _onFire = false;
            flammable = 0f;
            ammo = 0;
            graphic = _sprite;
            burntOut = true;
        }
        base.Update();
        if (_loadAnimation == -1f)
        {
            SFX.Play("click");
            _loadAnimation = 0f;
        }
        if (_loadAnimation >= 0f)
        {
            if (_loadProgress < 0)
            {
                if (_loadAnimation < 1f)
                {
                    _loadAnimation += 0.1f;
                }
                else
                {
                    _loadAnimation = 1f;
                }
            }
            else if (_loadAnimation < 0.5f)
            {
                _loadAnimation += 0.2f;
            }
            else
            {
                _loadAnimation = 0.5f;
            }
        }
        if (_loadProgress >= 0)
        {
            if (_loadProgress == 50 && base.isServerForObject)
            {
                Reload(shell: false);
                readyToFire = true;
            }
            if (_loadProgress < 100)
            {
                _loadProgress += 10;
            }
            else
            {
                _loadProgress = 100;
            }
        }
        if (!burntOut)
        {
            if (ammo == 4 || (bool)infinite)
            {
                _sprite.frame = 0;
            }
            else if (ammo == 3)
            {
                _sprite.frame = 1;
            }
            else if (ammo == 2)
            {
                _sprite.frame = 2;
            }
            else
            {
                _sprite.frame = 3;
            }
        }
    }

    public override void OnPressAction()
    {
        if (readyToFire)
        {
            if (ammo <= 0 || burntOut)
            {
                DoAmmoClick();
            }
            else
            {
                if (base.duck != null)
                {
                    RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
                }
                SFX.Play("campingThwoom");
                ApplyKick();
                Vec2 pos = Offset(base.barrelOffset);
                for (int i = 0; i < 6; i++)
                {
                    CampingSmoke smoke = new CampingSmoke(base.barrelPosition.X - 8f + Rando.Float(8f) + (float)offDir * 8f, base.barrelPosition.Y - 8f + Rando.Float(8f));
                    smoke.Depth = 0.9f + (float)i * 0.001f;
                    if (i < 3)
                    {
                        smoke.move -= base.barrelVector * Rando.Float(0.05f);
                    }
                    else
                    {
                        smoke.fly += base.barrelVector * (1f + Rando.Float(2.8f));
                    }
                    Level.Add(smoke);
                }
                if (!receivingPress)
                {
                    CampingBall n = new CampingBall(pos.X, pos.Y - 2f, base.duck);
                    Level.Add(n);
                    Fondle(n);
                    if (base.onFire)
                    {
                        n.LightOnFire();
                    }
                    if (owner != null)
                    {
                        n.responsibleProfile = owner.responsibleProfile;
                    }
                    n.clip.Add(owner as MaterialThing);
                    n.hSpeed = base.barrelVector.X * 10f;
                    n.vSpeed = base.barrelVector.Y * 7f - 0.75f;
                }
            }
            _loadProgress = -1;
            readyToFire = false;
            if (ammo == 1)
            {
                ammo = 0;
            }
        }
        else if (_loadProgress == -1)
        {
            _loadProgress = 0;
            _loadAnimation = -1f;
        }
    }

    public override void Draw()
    {
        base.Draw();
        Vec2 bOffset = new Vec2(13f, -2f);
        float offset = (float)Math.Sin(_loadAnimation * 3.14f) * 3f;
        Draw(_loaderSprite, new Vec2(bOffset.X - 8f - offset, bOffset.Y + 4f));
    }
}
