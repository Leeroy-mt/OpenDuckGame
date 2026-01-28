using System;
using System.Collections.Generic;

namespace DuckGame;

public class SpawnAimer : Thing
{
    private float _moveSpeed;

    private float _thickness;

    private Color _color;

    private DuckPersona _persona;

    private Vec2 targetPos;

    private float distOut = 100f;

    private float distLen = 64f;

    private float sizeWaver;

    private float rot;

    private float streamAlpha = 1f;

    private SinWaveManualUpdate _sin = 0.25f;

    public Duck dugg;

    private float aimerScale = 1f;

    private List<Vec2> prevPos = new List<Vec2>();

    public SpawnAimer(float xpos, float ypos, int dir, float moveSpeed, Color color, DuckPersona person, float thickness)
        : base(xpos, ypos)
    {
        _moveSpeed = moveSpeed;
        _color = color;
        _thickness = thickness;
        offDir = (sbyte)dir;
        _persona = person;
        targetPos = new Vec2(xpos, ypos);
        if (_persona == Persona.Duck1)
        {
            Position = new Vec2(0f, 0f);
        }
        else if (_persona == Persona.Duck2)
        {
            Position = new Vec2(320f, 0f);
        }
        else if (_persona == Persona.Duck3)
        {
            Position = new Vec2(0f, 180f);
        }
        else if (_persona == Persona.Duck4)
        {
            Position = new Vec2(320f, 180f);
        }
        prevPos.Add(Position);
        base.layer = Layer.Foreground;
    }

    public override void Update()
    {
        _sin.Update();
        if (GameMode.started)
        {
            Level.Remove(this);
        }
        distOut = Lerp.FloatSmooth(distOut, 16f, 0.08f, 1.2f);
        distLen = Lerp.FloatSmooth(distLen, 10f, 0.08f, 1.2f);
        rot = Lerp.FloatSmooth(rot, 45f, 0.08f, 1.1f);
        if (Math.Abs(rot - 45f) < 20f)
        {
            streamAlpha -= 0.03f;
            if (streamAlpha < 0f)
            {
                streamAlpha = 0f;
            }
        }
        Level.current.camera.getMatrix();
        Vec2 pos = targetPos;
        aimerScale = base.layer.camera.width / Layer.HUD.width;
        Position = Lerp.Vec2Smooth(Position, pos, 0.2f);
        if ((Position - pos).Length() > 16f)
        {
            prevPos.Add(Position);
        }
        sizeWaver += 0.2f;
    }

    public override void Draw()
    {
        float rlDist = (distOut + (float)Math.Sin(sizeWaver) * 2f) * aimerScale;
        rlDist = distOut;
        if (Network.isActive && dugg != null && dugg.profile != null && dugg.profile.connection == DuckNetwork.localConnection)
        {
            rlDist += _sin.value * 2f;
        }
        _thickness = 2f;
        for (int i = 0; i < 4; i++)
        {
            float a = rot + (float)i * 90f;
            Vec2 dir = new Vec2((float)Math.Cos(Maths.DegToRad(a)), (float)(0.0 - Math.Sin(Maths.DegToRad(a))));
            Graphics.DrawLine(Position + dir * rlDist, Position + dir * (rlDist + distLen * aimerScale), _color * base.Alpha, _thickness * aimerScale, 0.9f);
            Graphics.DrawLine(Position + dir * (rlDist - 1f * aimerScale), Position + dir * (rlDist + 1f * aimerScale + distLen * aimerScale), Color.Black, (_thickness + 2f) * aimerScale, 0.8f);
        }
        if (!(streamAlpha > 0.01f))
        {
            return;
        }
        int index = 0;
        Vec2 posPrev = Vec2.Zero;
        bool hasPrev = false;
        foreach (Vec2 pos in prevPos)
        {
            if (hasPrev)
            {
                Vec2 dir2 = (posPrev - pos).Normalized;
                Graphics.DrawLine(posPrev - dir2, pos + dir2, _color * streamAlpha, (4f + (float)index * 2f) * aimerScale, 0.9f);
            }
            posPrev = pos;
            hasPrev = true;
            index++;
        }
    }
}
