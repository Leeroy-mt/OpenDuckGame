using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class DeathmatchLevel : XMLLevel, IHaveAVirtualTransition
{
    protected static bool _started = false;

    public static bool playedGame = false;

    protected FollowCam _followCam;

    private Vec2 _p1;

    private Vec2 _p2;

    protected List<Duck> _pendingSpawns;

    protected float _waitSpawn = 1.8f;

    private float _waitFade = 1f;

    private float _waitAfterSpawn;

    private int _waitAfterSpawnDings;

    private BitmapFont _font;

    private float _fontFade = 1f;

    protected Deathmatch _deathmatch;

    public static bool firstDead = false;

    private bool _deathmatchStarted;

    private bool _didPlay;

    public static bool newLevel = true;

    private bool didStart;

    public static bool started
    {
        get
        {
            return _started;
        }
        set
        {
            _started = value;
        }
    }

    public FollowCam followCam => _followCam;

    public DeathmatchLevel(string level)
        : base(level)
    {
        _followCam = new FollowCam();
        _followCam.lerpMult = 1.2f;
        base.camera = _followCam;
        _started = false;
    }

    public override void Terminate()
    {
        base.Terminate();
        foreach (Profile item in Profiles.active)
        {
            item.duck = null;
        }
    }

    public override void Initialize()
    {
        firstDead = false;
        playedGame = true;
        foreach (Profile item in Profiles.active)
        {
            item.duck = null;
        }
        _font = new BitmapFont("biosFont", 8);
        base.Initialize();
        if (!Network.isActive)
        {
            StartDeathmatch();
        }
        _deathmatch = new Deathmatch(this);
        AddThing(_deathmatch);
        _pendingSpawns = _deathmatch.SpawnPlayers(recordStats: true);
        _pendingSpawns = _pendingSpawns.OrderBy((Duck sp) => sp.x).ToList();
        foreach (Duck duck in _pendingSpawns)
        {
            followCam.Add(duck);
        }
        followCam.Adjust();
        _things.RefreshState();
        _p1 = new Vec2(9999f, -9999f);
        _p2 = Vec2.Zero;
        int numDucks = 0;
        foreach (Duck duck2 in base.things[typeof(Duck)])
        {
            if (duck2.x < _p1.x)
            {
                _p1 = duck2.position;
            }
            _p2 += duck2.position;
            numDucks++;
        }
        _p2 /= (float)numDucks;
    }

    public void StartDeathmatch()
    {
        _deathmatchStarted = true;
        Music.LoadAlternateSong(Music.RandomTrack("InGame", Music.currentSong));
        Music.CancelLooping();
    }

    public override void Update()
    {
        if (!_deathmatchStarted)
        {
            return;
        }
        if (!didStart)
        {
            Graphics.fade = 1f;
            didStart = true;
        }
        if (_deathmatch != null && Music.finished)
        {
            if (_didPlay)
            {
                Music.Play(Music.currentSong, looping: false);
            }
            else
            {
                if (Music.pendingSong != null)
                {
                    Music.SwitchSongs();
                }
                else
                {
                    _deathmatch.PlayMusic();
                }
                _didPlay = true;
            }
        }
        _waitFade -= 0.04f;
        if (_waitFade > 0f)
        {
            return;
        }
        _waitSpawn -= 0.06f;
        if (!(_waitSpawn <= 0f))
        {
            return;
        }
        if (_pendingSpawns != null && _pendingSpawns.Count > 0)
        {
            _waitSpawn = 1.1f;
            if (_pendingSpawns.Count == 1)
            {
                _waitSpawn = 2f;
            }
            Duck spawn = _pendingSpawns[0];
            spawn.visible = true;
            AddThing(spawn);
            _pendingSpawns.RemoveAt(0);
            if (Network.isServer && Network.isActive)
            {
                Send.Message(new NMSpawnDuck(spawn.netProfileIndex));
            }
            Vec3 col = spawn.profile.persona.color;
            Level.Add(new SpawnLine(spawn.x, spawn.y, 0, 0f, new Color((int)col.x, (int)col.y, (int)col.z), 32f));
            Level.Add(new SpawnLine(spawn.x, spawn.y, 0, -4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
            Level.Add(new SpawnLine(spawn.x, spawn.y, 0, 4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
            Level.Add(new SpawnAimer(spawn.x, spawn.y, 0, 4f, new Color((int)col.x, (int)col.y, (int)col.z), spawn.persona, 4f));
            SFX.Play("pullPin", 0.7f);
            if (Party.HasPerk(spawn.profile, PartyPerks.Present) || (TeamSelect2.Enabled("WINPRES") && Deathmatch.lastWinners.Contains(spawn.profile)))
            {
                Present p = new Present(spawn.x, spawn.y);
                Level.Add(p);
                spawn.GiveHoldable(p);
            }
            if (Party.HasPerk(spawn.profile, PartyPerks.Jetpack) || TeamSelect2.Enabled("JETTY"))
            {
                Jetpack j = new Jetpack(spawn.x, spawn.y);
                Level.Add(j);
                spawn.Equip(j);
            }
            if (TeamSelect2.Enabled("HELMY"))
            {
                Helmet j2 = new Helmet(spawn.x, spawn.y);
                Level.Add(j2);
                spawn.Equip(j2);
            }
            if (TeamSelect2.Enabled("SHOESTAR"))
            {
                Boots j3 = new Boots(spawn.x, spawn.y);
                Level.Add(j3);
                spawn.Equip(j3);
            }
            if (Party.HasPerk(spawn.profile, PartyPerks.Armor))
            {
                Helmet j4 = new Helmet(spawn.x, spawn.y);
                Level.Add(j4);
                spawn.Equip(j4);
                ChestPlate h = new ChestPlate(spawn.x, spawn.y);
                Level.Add(h);
                spawn.Equip(h);
            }
            if (Party.HasPerk(spawn.profile, PartyPerks.Pistol))
            {
                Pistol j5 = new Pistol(spawn.x, spawn.y);
                Level.Add(j5);
                spawn.GiveHoldable(j5);
            }
            if (Party.HasPerk(spawn.profile, PartyPerks.NetGun))
            {
                NetGun j6 = new NetGun(spawn.x, spawn.y);
                Level.Add(j6);
                spawn.GiveHoldable(j6);
            }
        }
        else if (!_started)
        {
            _waitAfterSpawn -= 0.05f;
            if (_waitAfterSpawn <= 0f)
            {
                if (Network.isServer && Network.isActive && _waitAfterSpawnDings == 0)
                {
                    Send.Message(new NMGetReady());
                }
                _waitAfterSpawnDings++;
                if (_waitAfterSpawnDings > 2)
                {
                    Party.Clear();
                    _started = true;
                    SFX.Play("ding");
                    Event.Log(new RoundStartEvent());
                }
                else
                {
                    SFX.Play("preStartDing");
                }
                _waitSpawn = 1.1f;
            }
        }
        else
        {
            _fontFade -= 0.1f;
            if (_fontFade < 0f)
            {
                _fontFade = 0f;
            }
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD && _waitAfterSpawnDings > 0 && _fontFade > 0.01f)
        {
            _font.scale = new Vec2(2f, 2f);
            _font.alpha = _fontFade;
            string s = "GET";
            if (_waitAfterSpawnDings == 2)
            {
                s = "READY";
            }
            else if (_waitAfterSpawnDings == 3)
            {
                s = "";
            }
            float wide = _font.GetWidth(s);
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f, Layer.HUD.camera.height / 2f - _font.height / 2f, Color.White);
        }
        base.PostDrawLayer(layer);
    }
}
