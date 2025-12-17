using System;

namespace DuckGame;

[EditorGroup("Guns|Explosives")]
[BaggedProperty("isInDemo", true)]
public class Grenade : Gun
{
    public StateBinding _timerBinding = new StateBinding(nameof(_timer));

    public StateBinding _pinBinding = new StateBinding(nameof(_pin));

    private SpriteMap _sprite;

    public bool _pin = true;

    public float _timer = 1.2f;

    private Duck _cookThrower;

    private float _cookTimeOnThrow;

    public bool pullOnImpact;

    private bool _explosionCreated;

    private bool _localDidExplode;

    private bool _didBonus;

    private static int grenade;

    public int gr;

    public int _explodeFrames = -1;

    public Duck cookThrower => _cookThrower;

    public float cookTimeOnThrow => _cookTimeOnThrow;

    public Grenade(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 1;
        _ammoType = new ATShrapnel();
        _ammoType.penetration = 0.4f;
        _type = "gun";
        _sprite = new SpriteMap("grenade", 16, 16);
        graphic = _sprite;
        center = new Vec2(7f, 8f);
        collisionOffset = new Vec2(-4f, -5f);
        collisionSize = new Vec2(8f, 10f);
        base.bouncy = 0.4f;
        friction = 0.05f;
        _fireRumble = RumbleIntensity.Kick;
        _editorName = "Grenade";
        editorTooltip = "#1 Pull pin. #2 Throw grenade. Order of operations is important here.";
        _bio = "To cook grenade, pull pin and hold until feelings of terror run down your spine. Serves as many ducks as you can fit into a 3 meter radius.";
    }

    public override void Initialize()
    {
        gr = grenade;
        grenade++;
    }

    public override void OnNetworkBulletsFired(Vec2 pos)
    {
        _pin = false;
        _localDidExplode = true;
        if (!_explosionCreated)
        {
            Graphics.FlashScreen();
        }
        CreateExplosion(pos);
    }

    public void CreateExplosion(Vec2 pos)
    {
        if (!_explosionCreated)
        {
            float cx = pos.x;
            float cy = pos.y - 2f;
            Level.Add(new ExplosionPart(cx, cy));
            int num = 6;
            if (Graphics.effectsLevel < 2)
            {
                num = 3;
            }
            for (int i = 0; i < num; i++)
            {
                float dir = (float)i * 60f + Rando.Float(-10f, 10f);
                float dist = Rando.Float(12f, 20f);
                Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * (double)dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * (double)dist)));
            }
            _explosionCreated = true;
            SFX.Play("explode");
            RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
        }
    }

    public override void Update()
    {
        base.Update();
        if (!_pin)
        {
            _timer -= 0.01f;
            holsterable = false;
        }
        if (_timer < 0.5f && owner == null && !_didBonus)
        {
            _didBonus = true;
            if (Recorder.currentRecording != null)
            {
                Recorder.currentRecording.LogBonus();
            }
        }
        if (!_localDidExplode && _timer < 0f)
        {
            if (_explodeFrames < 0)
            {
                CreateExplosion(position);
                _explodeFrames = 4;
            }
            else
            {
                _explodeFrames--;
                if (_explodeFrames == 0)
                {
                    float cx = base.x;
                    float cy = base.y - 2f;
                    Graphics.FlashScreen();
                    if (base.isServerForObject)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            float dir = (float)i * 18f - 5f + Rando.Float(10f);
                            ATShrapnel shrap = new ATShrapnel();
                            shrap.range = 60f + Rando.Float(18f);
                            Bullet bullet = new Bullet(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir);
                            bullet.firedFrom = this;
                            firedBullets.Add(bullet);
                            Level.Add(bullet);
                        }
                        foreach (Window w in Level.CheckCircleAll<Window>(position, 40f))
                        {
                            if (Level.CheckLine<Block>(position, w.position, w) == null)
                            {
                                w.Destroy(new DTImpact(this));
                            }
                        }
                        bulletFireIndex += 20;
                        if (Network.isActive)
                        {
                            Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, rel: false, 4), NetMessagePriority.ReliableOrdered);
                            firedBullets.Clear();
                        }
                    }
                    Level.Remove(this);
                    base._destroyed = true;
                    _explodeFrames = -1;
                }
            }
        }
        if (base.prevOwner != null && _cookThrower == null)
        {
            _cookThrower = base.prevOwner as Duck;
            _cookTimeOnThrow = _timer;
        }
        _sprite.frame = ((!_pin) ? 1 : 0);
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (pullOnImpact)
        {
            OnPressAction();
        }
        base.OnSolidImpact(with, from);
    }

    public override void OnPressAction()
    {
        if (_pin)
        {
            _pin = false;
            Level.Add(new GrenadePin(base.x, base.y)
            {
                hSpeed = (float)(-offDir) * (1.5f + Rando.Float(0.5f)),
                vSpeed = -2f
            });
            if (base.duck != null)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
            }
            SFX.Play("pullPin");
        }
    }
}
