using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class ParallaxZone
{
    public float distance;

    public float speed;

    public float scroll;

    public float wrapMul = 1f;

    public bool moving;

    public bool visible = true;

    private List<Sprite> _sprites = new List<Sprite>();

    private List<Thing> _things = new List<Thing>();

    private int _ypos;

    public ParallaxZone(float d, float s, bool m, bool vis = true, int ypos = 0)
    {
        distance = d;
        speed = s;
        moving = m;
        visible = vis;
        _ypos = 0;
    }

    public void Update(float mul)
    {
        if (moving)
        {
            mul = 1f;
        }
        scroll += (1f - distance) * speed * mul;
    }

    public void RenderSprites(Vector2 position)
    {
        float dep = 0.4f + (float)_ypos * 0.01f;
        foreach (Sprite s in _sprites)
        {
            s.Position += position;
            s.X += scroll;
            if (s.X < -200 * wrapMul)
            {
                s.X += 500 * wrapMul;
            }
            if (s.X > 450 * wrapMul)
            {
                s.X -= 500 * wrapMul;
            }
            s.Depth = dep;
            Graphics.Draw(s, s.X, s.Y);
            dep += 0.001f;
            s.X -= scroll;
            s.Position -= position;
        }
        foreach (Thing s2 in _things)
        {
            s2.Position += position;
            s2.X += scroll;
            if (s2.X < -200)
            {
                s2.X += 500;
            }
            if (s2.X > 450)
            {
                s2.X -= 500;
            }
            s2.Depth = dep;
            s2.Update();
            s2.Draw();
            s2.X -= scroll;
            s2.Position -= position;
        }
    }

    public void AddSprite(Sprite s)
    {
        _sprites.Add(s);
    }

    public void AddThing(Thing s)
    {
        _things.Add(s);
    }
}
