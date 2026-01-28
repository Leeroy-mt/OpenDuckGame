using System.Collections.Generic;

namespace DuckGame;

public class HostTable : Thing
{
    private class MemberData
    {
        public bool quack;

        public Vec2 tongueLerp;

        public Vec2 tongueSlowLerp;

        public Vec2 tilt;

        public Vec2 bob;

        public Duck duck;

        public DuckAI ai;

        public int beverage;

        public float beverageLerp;
    }

    private Sprite _crown;

    private Sprite _chair;

    private SpriteMap _beverage;

    private LevelCore _fakeLevelCore;

    public static bool loop;

    private List<Profile> spectators = new List<Profile>();

    private Dictionary<Profile, MemberData> _data = new Dictionary<Profile, MemberData>();

    private List<Profile> remove = new List<Profile>();

    public HostTable(float pX, float pY)
        : base(pX, pY, new Sprite("hostTable"))
    {
        base.layer = Layer.HUD;
        graphic.CenterOrigin();
        Center = new Vec2(graphic.w / 2, 0f);
        _chair = new Sprite("hostChair");
        _beverage = new SpriteMap("beverages", 16, 18);
        _chair.CenterOrigin();
        _crown = new Sprite("hostCrown");
        _crown.CenterOrigin();
        _fakeLevelCore = new LevelCore();
        _fakeLevelCore.currentLevel = new Level();
    }

    public override void Update()
    {
        Thing.skipLayerAdding = true;
        loop = true;
        bool networkActive = Network.activeNetwork._networkActive;
        Network.activeNetwork._networkActive = false;
        LevelCore oldCore = Level.core;
        Level.core = _fakeLevelCore;
        foreach (Profile p in DuckNetwork.profiles)
        {
            if (p.slotType == SlotType.Spectator && p.connection != null && !spectators.Contains(p) && p.team != null)
            {
                spectators.Add(p);
            }
        }
        foreach (Profile p2 in spectators)
        {
            if (p2.connection == null || p2.slotType != SlotType.Spectator || p2.team == null)
            {
                remove.Add(p2);
                continue;
            }
            MemberData m = GetData(p2);
            if (m.duck == null)
            {
                InputProfile input = p2.inputProfile;
                m.duck = new Duck(100f, 0f, p2);
                m.duck.enablePhysics = false;
                Level.Add(m.duck);
                m.duck.mindControl = (m.ai = new DuckAI());
                m.duck.derpMindControl = false;
                m.duck.Depth = base.Depth - 20;
                m.ai.virtualDevice = new VirtualInput(0);
                m.ai.virtualQuack = true;
                m.duck.connection = p2.connection;
                p2.duck = null;
                p2.inputProfile = input;
            }
            bool quack = p2.netData.Get("quack", pDefault: false);
            if (quack && !m.quack)
            {
                m.ai.Press("QUACK");
            }
            else if (quack && m.quack)
            {
                m.ai.HoldDown("QUACK");
            }
            else if (!quack && m.quack)
            {
                m.ai.Release("QUACK");
            }
            m.ai.virtualDevice.rightStick = p2.netData.Get("spectatorTongue", Vec2.Zero);
            m.ai.virtualDevice.leftTrigger = p2.netData.Get("quackPitch", 0f);
            if (p2.team.hasHat && m.duck.hat == null)
            {
                TeamHat t = new TeamHat(0f, 0f, p2.team);
                Level.Add(t);
                m.duck.Equip(t, makeSound: false);
            }
            m.quack = quack;
        }
        foreach (Profile p3 in remove)
        {
            MemberData m2 = GetData(p3);
            if (m2.duck != null)
            {
                if (m2.duck.hat != null)
                {
                    Level.Remove(m2.duck.hat);
                }
                Level.Remove(m2.duck);
                _data.Remove(p3);
            }
            spectators.Remove(p3);
        }
        remove.Clear();
        Level.current.things.RefreshState();
        Level.current.UpdateThings();
        Level.core = oldCore;
        Network.activeNetwork._networkActive = networkActive;
        loop = false;
        Thing.skipLayerAdding = false;
    }

    private MemberData GetData(Profile pProfile)
    {
        MemberData m = null;
        if (!_data.TryGetValue(pProfile, out m))
        {
            m = (_data[pProfile] = new MemberData());
        }
        return m;
    }

    public override void Draw()
    {
        bool networkActive = Network.activeNetwork._networkActive;
        Network.activeNetwork._networkActive = false;
        LevelCore oldCore = Level.core;
        Level.core = _fakeLevelCore;
        foreach (Thing t in Level.current.things)
        {
            if (t is Duck)
            {
                (t as Duck).UpdateConnectionIndicators();
                (t as Duck).DrawConnectionIndicators();
            }
            t.DoDraw();
        }
        float wide = spectators.Count * 22;
        float drawX = base.X - wide / 2f + 10f;
        int idx = 0;
        foreach (Profile p in spectators)
        {
            MemberData m = GetData(p);
            sbyte beverage = p.netData.Get("spectatorBeverage", (sbyte)(-1));
            if (m.beverage != beverage)
            {
                m.beverageLerp = Lerp.FloatSmooth(m.beverageLerp, 1f, 0.2f, 1.1f);
                if (m.beverageLerp >= 1f)
                {
                    m.beverage = beverage;
                }
            }
            else
            {
                m.beverageLerp = Lerp.FloatSmooth(m.beverageLerp, 0f, 0.2f, 1.1f);
                if (m.beverageLerp < 0.05f)
                {
                    m.beverageLerp = 0f;
                }
            }
            bool flip = idx >= spectators.Count / 2;
            if (p.netData.Get("spectatorFlip", pDefault: false))
            {
                flip = !flip;
            }
            if (m.beverage != -1)
            {
                _beverage.frame = m.beverage;
                float randoOffset = (float)(idx * idx) * 5.4041653f % 1f * 7f;
                int yOff = 1;
                if (idx == 1 || idx == 2)
                {
                    yOff = 0;
                }
                Graphics.Draw(_beverage, drawX - randoOffset + (float)((idx >= spectators.Count / 2) ? 5 : (-16)), base.Y - 15f + 16f * m.beverageLerp + (float)yOff, (m.beverageLerp < 0.05f) ? (base.Depth + 1) : (base.Depth - 1));
            }
            if (p == DuckNetwork.hostProfile)
            {
                Graphics.Draw(_crown, drawX, base.Y + 2f, base.Depth + 2);
            }
            Vec2 chairPos = new Vec2(drawX, base.Y - 2f);
            chairPos += new Vec2(m.tilt.X, (0f - m.tilt.Y) * 0.25f) * 4f;
            Vec2 drawPos = chairPos;
            Vec2 bob = m.bob;
            if (bob.Y < 0f)
            {
                bob.Y *= 1.6f;
            }
            drawPos += new Vec2(bob.X, (0f - bob.Y) * 1.5f) * 4f;
            m.tilt = Lerp.Vec2Smooth(m.tilt, p.netData.Get("spectatorTilt", Vec2.Zero), 0.15f);
            m.bob = Lerp.Vec2Smooth(m.bob, p.netData.Get("spectatorBob", Vec2.Zero), 0.15f);
            p.netData.Get("quack", pDefault: false);
            if (m.duck != null)
            {
                m.duck.Position = drawPos + new Vec2(0f, -5f);
                m.duck.offDir = (sbyte)((!flip) ? 1 : (-1));
            }
            _chair.flipH = flip;
            Graphics.Draw(_chair, chairPos.X - (float)(flip ? (-4) : 4), chairPos.Y - 4f, base.Depth - 40);
            drawX += wide / (float)spectators.Count;
            idx++;
        }
        Level.core = oldCore;
        Network.activeNetwork._networkActive = networkActive;
        if (spectators.Count > 0)
        {
            base.Draw();
        }
    }
}
