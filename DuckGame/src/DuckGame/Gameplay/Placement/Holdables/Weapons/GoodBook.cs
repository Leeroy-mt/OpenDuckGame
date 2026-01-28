using System;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class GoodBook : Gun
{
    public StateBinding _raiseArmBinding = new StateBinding(nameof(_raiseArm));

    public StateBinding _timerBinding = new StateBinding(nameof(_timer));

    public StateBinding _netPreachBinding = new NetSoundBinding(nameof(_netPreach));

    public StateBinding _ringPulseBinding = new StateBinding(nameof(_ringPulse));

    public StateBinding _controlling1Binding = new StateBinding(nameof(controlling1));

    public StateBinding _controlling2Binding = new StateBinding(nameof(controlling2));

    public StateBinding _controlling3Binding = new StateBinding(nameof(controlling3));

    public StateBinding _controlling4Binding = new StateBinding(nameof(controlling4));

    public StateBinding _controlling5Binding = new StateBinding(nameof(controlling5));

    public StateBinding _controlling6Binding = new StateBinding(nameof(controlling6));

    public StateBinding _controlling7Binding = new StateBinding(nameof(controlling7));

    public StateBinding _controlling8Binding = new StateBinding(nameof(controlling8));

    public Duck[] controlling = new Duck[8];

    public Duck[] prevControlling = new Duck[8];

    public NetSoundEffect _netPreach = new NetSoundEffect("preach0", "preach1", "preach2", "preach3", "preach4", "preach5")
    {
        pitchVariationLow = -0.3f,
        pitchVariationHigh = -0.2f
    };

    private SpriteMap _sprite;

    public float _timer = 1.2f;

    public float _raiseArm;

    private Sprite _halo;

    private float _preachWait;

    private float _haloAlpha;

    private SinWave _haloWave = 0.05f;

    public float _ringPulse;

    public Duck controlling1
    {
        get
        {
            return controlling[0];
        }
        set
        {
            controlling[0] = value;
        }
    }

    public Duck controlling2
    {
        get
        {
            return controlling[1];
        }
        set
        {
            controlling[1] = value;
        }
    }

    public Duck controlling3
    {
        get
        {
            return controlling[2];
        }
        set
        {
            controlling[2] = value;
        }
    }

    public Duck controlling4
    {
        get
        {
            return controlling[3];
        }
        set
        {
            controlling[3] = value;
        }
    }

    public Duck controlling5
    {
        get
        {
            return controlling[4];
        }
        set
        {
            controlling[4] = value;
        }
    }

    public Duck controlling6
    {
        get
        {
            return controlling[5];
        }
        set
        {
            controlling[5] = value;
        }
    }

    public Duck controlling7
    {
        get
        {
            return controlling[6];
        }
        set
        {
            controlling[6] = value;
        }
    }

    public Duck controlling8
    {
        get
        {
            return controlling[7];
        }
        set
        {
            controlling[7] = value;
        }
    }

    public Duck prevControlling1
    {
        get
        {
            return prevControlling[0];
        }
        set
        {
            prevControlling[0] = value;
        }
    }

    public Duck prevControlling2
    {
        get
        {
            return prevControlling[1];
        }
        set
        {
            prevControlling[1] = value;
        }
    }

    public Duck prevControlling3
    {
        get
        {
            return prevControlling[2];
        }
        set
        {
            prevControlling[2] = value;
        }
    }

    public Duck prevControlling4
    {
        get
        {
            return prevControlling[3];
        }
        set
        {
            prevControlling[3] = value;
        }
    }

    public Duck prevControlling5
    {
        get
        {
            return prevControlling[4];
        }
        set
        {
            prevControlling[4] = value;
        }
    }

    public Duck prevControlling6
    {
        get
        {
            return prevControlling[5];
        }
        set
        {
            prevControlling[5] = value;
        }
    }

    public Duck prevControlling7
    {
        get
        {
            return prevControlling[6];
        }
        set
        {
            prevControlling[6] = value;
        }
    }

    public Duck prevControlling8
    {
        get
        {
            return prevControlling[7];
        }
        set
        {
            prevControlling[7] = value;
        }
    }

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

    public GoodBook(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 1;
        _ammoType = new ATShrapnel();
        _ammoType.penetration = 0.4f;
        _ammoType.range = 40f;
        _type = "gun";
        _sprite = new SpriteMap("goodBook", 17, 12);
        graphic = _sprite;
        Center = new Vec2(8f, 6f);
        collisionOffset = new Vec2(-5f, -4f);
        collisionSize = new Vec2(10f, 8f);
        _halo = new Sprite("halo");
        _halo.CenterOrigin();
        _holdOffset = new Vec2(3f, 4f);
        handOffset = new Vec2(1f, 1f);
        _hasTrigger = false;
        base.bouncy = 0.4f;
        friction = 0.05f;
        flammable = 1f;
        _editorName = "Good Book";
        editorTooltip = "Converts enemies to your side. Don't spoil the ending.";
        _bio = "A collection of words, maybe other ducks should hear them?";
        physicsMaterial = PhysicsMaterial.Wood;
        _netPreach.function = DoPreach;
        controlling = new Duck[8] { controlling1, controlling2, controlling3, controlling4, controlling5, controlling6, controlling7, controlling8 };
    }

    public void DoPreach()
    {
    }

    public void LoseControl()
    {
        for (int i = 0; i < 8; i++)
        {
            Duck d = controlling[i];
            if (d != null)
            {
                d.listenTime = 0;
                d.listening = false;
                controlling[i] = null;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (_owner != null && !_raised)
        {
            _sprite.frame = 1;
        }
        else
        {
            _sprite.frame = 0;
        }
        _raiseArm = Lerp.Float(_raiseArm, 0f, 0.05f);
        _preachWait = Lerp.Float(_preachWait, 0f, 0.06f);
        _ringPulse = Lerp.Float(_ringPulse, 0f, 0.05f);
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                for (int i = 0; i < 8; i++)
                {
                    Duck d = controlling[i];
                    if (d == null)
                    {
                        continue;
                    }
                    if (d.listenTime <= 0)
                    {
                        controlling[i] = null;
                        d.listening = false;
                        continue;
                    }
                    Fondle(d);
                    Fondle(d.holdObject);
                    foreach (Equipment e in d._equipment)
                    {
                        Fondle(e);
                    }
                    Fondle(d._ragdollInstance);
                    Fondle(d._trappedInstance);
                    Fondle(d._cookedInstance);
                }
            }
            else
            {
                for (int j = 0; j < 8; j++)
                {
                    Duck d2 = controlling[j];
                    if (d2 != null)
                    {
                        d2.listening = true;
                        d2.listenTime = 80;
                        d2.Fondle(d2.holdObject);
                        foreach (Equipment e2 in d2._equipment)
                        {
                            d2.Fondle(e2);
                        }
                        d2.Fondle(d2._ragdollInstance);
                        d2.Fondle(d2._trappedInstance);
                        d2.Fondle(d2._cookedInstance);
                    }
                    else if (prevControlling[j] != null)
                    {
                        prevControlling[j].listening = false;
                        prevControlling[j].listenTime = 0;
                    }
                }
            }
        }
        if (_triggerHeld && base.isServerForObject && base.duck != null && ((_preachWait <= 0f) & (base.duck.quack < 1)) && base.duck.grounded)
        {
            if (Network.isActive)
            {
                _netPreach.Play();
            }
            else
            {
                SFX.Play("preach" + Rando.Int(5), Rando.Float(0.8f, 1f), Rando.Float(-0.2f, -0.3f));
            }
            base.duck.quack = (byte)Rando.Int(12, 30);
            base.duck.profile.stats.timePreaching += (float)base.duck.quack / 0.1f * Maths.IncFrameTimer();
            _preachWait = Rando.Float(1.8f, 2.5f);
            _ringPulse = 1f;
            if (Rando.Int(1) == 0)
            {
                _raiseArm = Rando.Float(1.2f, 2f);
            }
            Ragdoll rag = Level.Nearest<Ragdoll>(base.X, base.Y, this);
            if (rag != null && rag.captureDuck != null && rag.captureDuck.dead && Level.CheckLine<Block>(base.duck.Position, rag.Position) == null && (rag.Position - base.duck.Position).Length() < _ammoType.range)
            {
                if (Network.isActive)
                {
                    Fondle(rag.captureDuck);
                    Fondle(rag);
                    Send.Message(new NMLayToRest(rag.captureDuck));
                }
                rag.captureDuck.LayToRest(base.duck.profile);
            }
            foreach (Duck d3 in Level.current.things[typeof(Duck)])
            {
                if (d3 is TargetDuck && (d3 as TargetDuck).stanceSetting == 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Level.Add(new MusketSmoke(d3.X - 5f + Rando.Float(10f), d3.Y + 6f - 3f + Rando.Float(6f) - (float)k * 1f)
                        {
                            move =
                            {
                                X = -0.2f + Rando.Float(0.4f),
                                Y = -0.2f + Rando.Float(0.4f)
                            }
                        });
                    }
                    SFX.Play("death");
                    Level.Add(new Tombstone(d3.X, d3.Y)
                    {
                        vSpeed = -2.5f
                    });
                    Level.Remove(d3);
                }
                if (d3 == base.duck || !d3.grounded || d3.holdObject is GoodBook || Level.CheckLine<Block>(base.duck.Position, d3.Position) != null || !((d3.Position - base.duck.Position).Length() < _ammoType.range))
                {
                    continue;
                }
                if (d3.dead)
                {
                    Fondle(d3);
                    d3.LayToRest(base.duck.profile);
                    continue;
                }
                Duck duckConverting = ((base.duck.converted != null) ? base.duck.converted : base.duck);
                Duck duckBeingConverted = ((d3.converted != null) ? d3.converted : d3);
                if (duckConverting == duckBeingConverted || duckConverting.profile.team == duckBeingConverted.profile.team)
                {
                    continue;
                }
                if (Network.isActive && d3.profile.networkIndex >= 0 && d3.profile.networkIndex < 8)
                {
                    controlling[d3.profile.networkIndex] = d3;
                }
                d3.listening = true;
                Fondle(d3);
                Fondle(d3.holdObject);
                foreach (Equipment e3 in d3._equipment)
                {
                    Fondle(e3);
                }
                Fondle(d3._ragdollInstance);
                Fondle(d3._trappedInstance);
                Fondle(d3._cookedInstance);
                d3.listenTime = 80;
                if (owner.X < d3.X)
                {
                    d3.offDir = -1;
                }
                else
                {
                    d3.offDir = 1;
                }
                d3.ThrowItem(throwWithForce: false);
                d3.conversionResistance -= 30;
                if (d3.conversionResistance <= 0)
                {
                    d3.ConvertDuck(duckConverting);
                    if (Network.isActive)
                    {
                        Send.Message(new NMConversion(d3, duckConverting));
                        controlling[d3.profile.networkIndex] = null;
                    }
                    d3.conversionResistance = 50;
                }
            }
        }
        _haloAlpha = Lerp.Float(_haloAlpha, (_triggerHeld && base.duck != null && base.duck.grounded) ? 1f : 0f, 0.05f);
        for (int l = 0; l < 8; l++)
        {
            prevControlling[l] = controlling[l];
        }
    }

    public override void OnPressAction()
    {
    }

    public override void Draw()
    {
        if (base.duck != null && !_raised && _raiseArm > 0f)
        {
            SpriteMap spriteArms = base.duck._spriteArms;
            bool flip = spriteArms.flipH;
            float a = spriteArms.Angle;
            spriteArms.flipH = offDir * -1 < 0;
            spriteArms.Angle = 0.7f * (float)offDir;
            Graphics.Draw(spriteArms, owner.X - (float)(5 * offDir), owner.Y + 3f + (float)(base.duck.crouch ? 3 : 0) + (float)(base.duck.sliding ? 3 : 0));
            spriteArms.Angle = a;
            spriteArms.flipH = flip;
            handOffset = new Vec2(9999f, 9999f);
        }
        else
        {
            handOffset = new Vec2(1f, 1f);
        }
        if (owner != null && _haloAlpha > 0.01f)
        {
            _halo.Alpha = _haloAlpha * 0.4f + (float)_haloWave * 0.1f;
            _halo.Depth = -0.2f;
            Sprite halo = _halo;
            float num = (_halo.ScaleY = 0.95f + (float)_haloWave * 0.05f);
            halo.ScaleX = num;
            _halo.Angle += 0.01f;
            Graphics.Draw(_halo, owner.X, owner.Y);
            if (_ringPulse > 0f)
            {
                int num3 = 16;
                Vec2 prev = Vec2.Zero;
                float range = _ammoType.range * 0.1f + (1f - _ringPulse) * (_ammoType.range * 0.9f);
                for (int i = 0; i < num3; i++)
                {
                    float val = Maths.DegToRad(360 / (num3 - 1) * i);
                    Vec2 cur = new Vec2((float)Math.Cos(val) * range, (0f - (float)Math.Sin(val)) * range);
                    if (i > 0)
                    {
                        Graphics.DrawLine(owner.Position + cur, owner.Position + prev, Color.White * (_ringPulse * 0.6f), _ringPulse * 10f);
                    }
                    prev = cur;
                }
            }
        }
        base.Draw();
    }
}
