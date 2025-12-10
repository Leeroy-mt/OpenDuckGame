using System.Collections.Generic;

namespace DuckGame;

public class GraphicList : Sprite
{
    private List<Sprite> _objects;

    public override int width
    {
        get
        {
            float left = 0f;
            float right = 0f;
            foreach (Sprite s in _objects)
            {
                if (s.x - s.centerx < left)
                {
                    left = s.x - s.centerx;
                }
                if (s.x - s.centerx + (float)s.width > right)
                {
                    right = s.x - s.centerx + (float)s.width;
                }
            }
            return (int)(right - left + 0.5f);
        }
    }

    public override int w => width;

    public override int height
    {
        get
        {
            float top = 0f;
            float bottom = 0f;
            foreach (Sprite s in _objects)
            {
                if (s.y - s.centery < top)
                {
                    top = s.x - s.centery;
                }
                if (s.y - s.centery + (float)s.height > bottom)
                {
                    bottom = s.y - s.centery + (float)s.width;
                }
            }
            return (int)(bottom - top + 0.5f);
        }
    }

    public override int h => height;

    public GraphicList(List<Sprite> list)
    {
        _objects = list;
    }

    public GraphicList()
    {
        _objects = new List<Sprite>();
    }

    public void Add(Sprite graphic)
    {
        _objects.Add(graphic);
    }

    public void Remove(Sprite graphic)
    {
        _objects.Remove(graphic);
    }

    public override void Draw()
    {
        foreach (Sprite g in _objects)
        {
            Vec2 pos = new Vec2(g.position);
            g.position -= center;
            g.position.x *= base.xscale;
            g.position.y *= base.yscale;
            g.position += position;
            float alph = g.alpha;
            g.alpha *= base.alpha;
            Vec2 scl = new Vec2(g.scale);
            g.xscale *= base.xscale;
            g.yscale *= base.yscale;
            float ang = g.angle;
            g.angle *= angle;
            bool flip = g.flipH;
            g.flipH = base.flipH;
            g.Draw();
            g.angle = ang;
            g.scale = scl;
            g.alpha = alph;
            g.position = pos;
            g.flipH = flip;
        }
    }
}
