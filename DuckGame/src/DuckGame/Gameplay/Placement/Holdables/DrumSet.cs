namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("canSpawn", false)]
public class DrumSet : Holdable, IPlatform
{
    private BassDrum _bass;

    private Snare _snare;

    private HiHat _hat;

    private LowTom _lowTom;

    private CrashCymbal _crash;

    private MediumTom _medTom;

    private HighTom _highTom;

    public StateBinding _netBassDrumBinding = new NetSoundBinding(nameof(_netBassDrum));

    public StateBinding _netSnareBinding = new NetSoundBinding(nameof(_netSnare));

    public StateBinding _netHatBinding = new NetSoundBinding(nameof(_netHat));

    public StateBinding _netHatAlternateBinding = new NetSoundBinding(nameof(_netHatAlternate));

    public StateBinding _netLowTomBinding = new NetSoundBinding(nameof(_netLowTom));

    public StateBinding _netMediumTomBinding = new NetSoundBinding(nameof(_netMediumTom));

    public StateBinding _netHighTomBinding = new NetSoundBinding(nameof(_netHighTom));

    public StateBinding _netCrashBinding = new NetSoundBinding(nameof(_netCrash));

    public StateBinding _netThrowStickBinding = new NetSoundBinding(nameof(_netThrowStick));

    public NetSoundEffect _netBassDrum = new NetSoundEffect();

    public NetSoundEffect _netSnare = new NetSoundEffect();

    public NetSoundEffect _netHat = new NetSoundEffect();

    public NetSoundEffect _netHatAlternate = new NetSoundEffect();

    public NetSoundEffect _netLowTom = new NetSoundEffect();

    public NetSoundEffect _netMediumTom = new NetSoundEffect();

    public NetSoundEffect _netHighTom = new NetSoundEffect();

    public NetSoundEffect _netCrash = new NetSoundEffect();

    public NetSoundEffect _netThrowStick = new NetSoundEffect();

    private int hits;

    private int tick = 15;

    private int hitsSinceThrow;

    public override Vec2 netPosition
    {
        get
        {
            return Position;
        }
        set
        {
            Position = value;
        }
    }

    public DrumSet(float xpos, float ypos)
        : base(xpos, ypos)
    {
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -11f);
        collisionSize = new Vec2(16f, 14f);
        base.Depth = 0.5f;
        thickness = 0f;
        weight = 7f;
        _editorIcon = new Sprite("drumsIcon");
        _holdOffset = new Vec2(1f, 7f);
        flammable = 0.3f;
        base.collideSounds.Add("rockHitGround2");
        handOffset = new Vec2(0f, -9999f);
        holsterable = false;
        tapeable = false;
        base.hugWalls = WallHug.Floor;
        editorTooltip = "Stress got you down? It's time to jam!";
    }

    public void ThrowStick()
    {
        if (Rando.Float(1f) >= 0.5f)
        {
            Level.Add(new DrumStick(base.X - 5f, base.Y - 8f));
        }
        else
        {
            Level.Add(new DrumStick(base.X + 5f, base.Y - 8f));
        }
    }

    public override void Initialize()
    {
        _bass = new BassDrum(base.X, base.Y);
        Level.Add(_bass);
        _snare = new Snare(base.X, base.Y);
        Level.Add(_snare);
        _hat = new HiHat(base.X, base.Y);
        Level.Add(_hat);
        _lowTom = new LowTom(base.X, base.Y);
        Level.Add(_lowTom);
        _crash = new CrashCymbal(base.X, base.Y);
        Level.Add(_crash);
        _medTom = new MediumTom(base.X, base.Y);
        Level.Add(_medTom);
        _highTom = new HighTom(base.X, base.Y);
        Level.Add(_highTom);
        _bass.Position = Position;
        _bass.Depth = base.Depth + 1;
        _snare.Position = Position + new Vec2(10f, -7f);
        _snare.Depth = base.Depth;
        _hat.Depth = base.Depth - 1;
        _hat.Position = Position + new Vec2(13f, -11f);
        _lowTom.Depth = base.Depth - 1;
        _lowTom.Position = Position + new Vec2(-9f, -5f);
        _crash.Depth = base.Depth;
        _crash.Position = Position + new Vec2(-15f, -15f);
        _medTom.Depth = base.Depth + 3;
        _medTom.Position = Position + new Vec2(-8f, -12f);
        _highTom.Depth = base.Depth + 3;
        _highTom.Position = Position + new Vec2(7f, -12f);
        _netBassDrum.function = _bass.Hit;
        _netSnare.function = _snare.Hit;
        _netHat.function = _hat.Hit;
        _netHatAlternate.function = _hat.AlternateHit;
        _netLowTom.function = _lowTom.Hit;
        _netMediumTom.function = _medTom.Hit;
        _netHighTom.function = _highTom.Hit;
        _netCrash.function = _crash.Hit;
        _netThrowStick.function = ThrowStick;
    }

    public override void Terminate()
    {
        Level.Remove(_bass);
        Level.Remove(_snare);
        Level.Remove(_hat);
        Level.Remove(_lowTom);
        Level.Remove(_medTom);
        Level.Remove(_highTom);
        Level.Remove(_crash);
    }

    public override void Update()
    {
        tick--;
        if (tick <= 0)
        {
            tick = 15;
            hits--;
        }
        if (hits < 0)
        {
            hits = 0;
        }
        if (owner == null || base.held)
        {
            base.Depth = 0.5f;
        }
        if (owner != null && base.held)
        {
            owner.vSpeed = 0f;
            owner.hSpeed = 0f;
            if (base.isServerForObject)
            {
                int num = hits;
                if (base.duck.inputProfile.Pressed("UP"))
                {
                    if (Network.isActive)
                    {
                        _netCrash.Play();
                    }
                    else
                    {
                        _crash.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("SHOOT"))
                {
                    if (Network.isActive)
                    {
                        _netSnare.Play();
                    }
                    else
                    {
                        _snare.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("RIGHT"))
                {
                    if (Network.isActive)
                    {
                        _netHighTom.Play();
                    }
                    else
                    {
                        _highTom.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("DOWN"))
                {
                    if (Network.isActive)
                    {
                        _netMediumTom.Play();
                    }
                    else
                    {
                        _medTom.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("LEFT"))
                {
                    if (Network.isActive)
                    {
                        _netLowTom.Play();
                    }
                    else
                    {
                        _lowTom.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("JUMP"))
                {
                    if (Network.isActive)
                    {
                        _netHat.Play();
                    }
                    else
                    {
                        _hat.Hit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("LTRIGGER"))
                {
                    if (Network.isActive)
                    {
                        _netHat.Play();
                    }
                    else
                    {
                        _hat.AlternateHit();
                    }
                    hits++;
                }
                if (base.duck.inputProfile.Pressed("RAGDOLL") || base.duck.inputProfile.Pressed("STRAFE"))
                {
                    if (Network.isActive)
                    {
                        _netBassDrum.Play();
                    }
                    else
                    {
                        _bass.Hit();
                    }
                    hits++;
                }
                if (num != hits)
                {
                    if (base.duck != null)
                    {
                        RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
                    }
                    hitsSinceThrow++;
                    if ((float)hits * 0.02f > Rando.Float(1f) && Rando.Float(1f) > 0.95f && hitsSinceThrow > 10)
                    {
                        if (Network.isActive)
                        {
                            _netThrowStick.Play();
                        }
                        else
                        {
                            ThrowStick();
                        }
                        hitsSinceThrow = 0;
                    }
                }
            }
        }
        _bass.Position = Position;
        _bass.Depth = base.Depth + 1;
        _snare.Position = Position + new Vec2(10f, -7f);
        _snare.Depth = base.Depth;
        _hat.Depth = base.Depth - 1;
        _hat.Position = Position + new Vec2(13f, -11f);
        _lowTom.Depth = base.Depth - 1;
        _lowTom.Position = Position + new Vec2(-9f, -5f);
        _crash.Depth = base.Depth;
        _crash.Position = Position + new Vec2(-15f, -15f);
        _medTom.Depth = base.Depth + 3;
        _medTom.Position = Position + new Vec2(-8f, -12f);
        _highTom.Depth = base.Depth + 3;
        _highTom.Position = Position + new Vec2(7f, -12f);
    }
}
