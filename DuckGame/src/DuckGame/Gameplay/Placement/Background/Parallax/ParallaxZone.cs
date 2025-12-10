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

    public void RenderSprites(Vec2 position)
    {
        float dep = 0.4f + (float)_ypos * 0.01f;
        foreach (Sprite s in _sprites)
        {
            s.position += position;
            s.position.x += scroll;
            if (s.position.x < -200f * wrapMul)
            {
                s.position.x += 500f * wrapMul;
            }
            if (s.position.x > 450f * wrapMul)
            {
                s.position.x -= 500f * wrapMul;
            }
            s.depth = dep;
            Graphics.Draw(s, s.x, s.y);
            dep += 0.001f;
            s.position.x -= scroll;
            s.position -= position;
        }
        foreach (Thing s2 in _things)
        {
            s2.position += position;
            s2.position.x += scroll;
            if (s2.position.x < -200f)
            {
                s2.position.x += 500f;
            }
            if (s2.position.x > 450f)
            {
                s2.position.x -= 500f;
            }
            s2.depth = dep;
            s2.Update();
            s2.Draw();
            s2.position.x -= scroll;
            s2.position -= position;
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
