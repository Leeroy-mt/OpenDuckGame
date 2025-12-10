using System.Collections.Generic;

namespace DuckGame;

public class ElectricalCharge : Thing
{
    private List<Vec2> _prevPositions = new List<Vec2>();

    private Vec2 _travelVec;

    public ElectricalCharge(float xpos, float ypos, int off, Thing own)
        : base(xpos, ypos)
    {
        offDir = (sbyte)off;
        _travelVec = new Vec2((float)offDir * Rando.Float(6f, 10f), Rando.Float(-10f, 10f));
        owner = own;
    }

    public override void Update()
    {
        if (_prevPositions.Count == 0)
        {
            _prevPositions.Insert(0, position);
        }
        Vec2 p = position;
        position += _travelVec;
        _travelVec = new Vec2((float)offDir * Rando.Float(6f, 10f), Rando.Float(-10f, 10f));
        _prevPositions.Insert(0, position);
        base.alpha -= 0.1f;
        if (base.alpha < 0f)
        {
            Level.Remove(this);
        }
        foreach (IAmADuck d in Level.CheckLineAll<IAmADuck>(p, position))
        {
            if (d is MaterialThing realThing && d != owner.owner)
            {
                realThing.Zap(owner);
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        Vec2 prev = Vec2.Zero;
        bool hasPrev = false;
        float a = 1f;
        foreach (Vec2 v in _prevPositions)
        {
            if (!hasPrev)
            {
                hasPrev = true;
                prev = v;
            }
            else
            {
                Graphics.DrawLine(v, prev, Colors.DGYellow * a, 1f, 0.9f);
                a -= 0.25f;
            }
            if (a <= 0f)
            {
                break;
            }
        }
        base.Draw();
    }
}
