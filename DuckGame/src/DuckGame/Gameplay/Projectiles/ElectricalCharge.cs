using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class ElectricalCharge : Thing
{
    private List<Vector2> _prevPositions = new List<Vector2>();

    private Vector2 _travelVec;

    public ElectricalCharge(float xpos, float ypos, int off, Thing own)
        : base(xpos, ypos)
    {
        offDir = (sbyte)off;
        _travelVec = new Vector2((float)offDir * Rando.Float(6f, 10f), Rando.Float(-10f, 10f));
        owner = own;
    }

    public override void Update()
    {
        if (_prevPositions.Count == 0)
        {
            _prevPositions.Insert(0, Position);
        }
        Vector2 p = Position;
        Position += _travelVec;
        _travelVec = new Vector2((float)offDir * Rando.Float(6f, 10f), Rando.Float(-10f, 10f));
        _prevPositions.Insert(0, Position);
        base.Alpha -= 0.1f;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        foreach (IAmADuck d in Level.CheckLineAll<IAmADuck>(p, Position))
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
        Vector2 prev = Vector2.Zero;
        bool hasPrev = false;
        float a = 1f;
        foreach (Vector2 v in _prevPositions)
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
