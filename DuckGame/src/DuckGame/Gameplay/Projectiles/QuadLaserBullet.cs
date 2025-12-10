namespace DuckGame;

public class QuadLaserBullet : Thing, ITeleport
{
    public StateBinding _positionBinding = new CompressedVec2Binding("position", int.MaxValue, isvelocity: false, doLerp: true);

    public StateBinding _travelBinding = new CompressedVec2Binding("travel", 20);

    private Vec2 _travel;

    private SinWaveManualUpdate _wave = 0.5f;

    private SinWaveManualUpdate _wave2 = 1f;

    public int safeFrames;

    public Duck safeDuck;

    public float timeAlive;

    public Vec2 travel
    {
        get
        {
            return _travel;
        }
        set
        {
            _travel = value;
        }
    }

    public QuadLaserBullet(float xpos, float ypos, Vec2 travel)
        : base(xpos, ypos)
    {
        _travel = travel;
        collisionOffset = new Vec2(-1f, -1f);
        _collisionSize = new Vec2(2f, 2f);
    }

    public override void Update()
    {
        _wave.Update();
        _wave2.Update();
        timeAlive += 0.016f;
        position += _travel * 0.5f;
        if (base.isServerForObject && (base.x > Level.current.bottomRight.x + 200f || base.x < Level.current.topLeft.x - 200f))
        {
            Level.Remove(this);
        }
        foreach (MaterialThing t in Level.CheckRectAll<MaterialThing>(base.topLeft, base.bottomRight))
        {
            if ((safeFrames > 0 && t == safeDuck) || !t.isServerForObject)
            {
                continue;
            }
            bool wasDestroyed = t.destroyed;
            t.Destroy(new DTIncinerate(this));
            if (t.destroyed && !wasDestroyed)
            {
                if (Recorder.currentRecording != null)
                {
                    Recorder.currentRecording.LogAction(2);
                }
                if (t is Duck && !(t as Duck).dead)
                {
                    Recorder.currentRecording.LogBonus();
                }
            }
        }
        if (safeFrames > 0)
        {
            safeFrames--;
        }
        base.Update();
    }

    public override void Draw()
    {
        Graphics.DrawRect(position + new Vec2(-4f, -4f), position + new Vec2(4f, 4f), new Color(255 - (int)(_wave.normalized * 90f), 137 + (int)(_wave.normalized * 50f), 31 + (int)(_wave.normalized * 30f)), base.depth);
        Graphics.DrawRect(position + new Vec2(-4f, -4f), position + new Vec2(4f, 4f), new Color(255, 224 - (int)(_wave2.normalized * 150f), 90 + (int)(_wave2.normalized * 50f)), base.depth + 1, filled: false);
        base.Draw();
    }
}
