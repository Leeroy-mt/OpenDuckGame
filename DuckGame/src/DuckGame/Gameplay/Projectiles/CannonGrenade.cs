using System.Collections.Generic;

namespace DuckGame;

public class CannonGrenade : Grenade
{
    private List<Vec2> tail = new List<Vec2>();

    public CannonGrenade(float xval, float yval)
        : base(xval, yval)
    {
    }

    public override void Update()
    {
        _bouncy = 0.7f;
        gravMultiplier = 0.9f;
        frictionMult = 0.5f;
        base.Update();
    }

    public override void Draw()
    {
        tail.Add(position);
        if (tail.Count > 10)
        {
            tail.RemoveAt(0);
        }
        if (tail.Count > 1)
        {
            for (int i = 1; i < tail.Count; i++)
            {
                Graphics.DrawLine(tail[i - 1], tail[i], Color.White * ((float)i / (float)tail.Count) * 0.5f, 1f, 0.5f);
            }
        }
        base.Draw();
    }
}
