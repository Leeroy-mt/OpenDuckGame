using Microsoft.Xna.Framework;

namespace DuckGame;

public class QuadLaserBullet : Thing, ITeleport
{
    public StateBinding _positionBinding = new CompressedVec2Binding(nameof(Position), int.MaxValue, isvelocity: false, doLerp: true);

    public StateBinding _travelBinding = new CompressedVec2Binding(nameof(travel), 20);

    private Vector2 _travel;

    private SinWaveManualUpdate _wave = 0.5f;

    private SinWaveManualUpdate _wave2 = 1f;

    public int safeFrames;

    public Duck safeDuck;

    public float timeAlive;

    public Vector2 travel
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

    public QuadLaserBullet(float xpos, float ypos, Vector2 travel)
        : base(xpos, ypos)
    {
        _travel = travel;
        collisionOffset = new Vector2(-1f, -1f);
        _collisionSize = new Vector2(2f, 2f);
    }

    public override void Update()
    {
        _wave.Update();
        _wave2.Update();
        timeAlive += 0.016f;
        Position += _travel * 0.5f;
        if (base.isServerForObject && (base.X > Level.current.bottomRight.X + 200f || base.X < Level.current.topLeft.X - 200f))
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
        Graphics.DrawRect(Position + new Vector2(-4f, -4f), Position + new Vector2(4f, 4f), new Color(255 - (int)(_wave.normalized * 90f), 137 + (int)(_wave.normalized * 50f), 31 + (int)(_wave.normalized * 30f)), base.Depth);
        Graphics.DrawRect(Position + new Vector2(-4f, -4f), Position + new Vector2(4f, 4f), new Color(255, 224 - (int)(_wave2.normalized * 150f), 90 + (int)(_wave2.normalized * 50f)), base.Depth + 1, filled: false);
        base.Draw();
    }
}
