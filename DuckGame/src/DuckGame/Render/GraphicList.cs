using Microsoft.Xna.Framework;
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
                if (s.X - s.CenterX < left)
                {
                    left = s.X - s.CenterX;
                }
                if (s.X - s.CenterX + (float)s.width > right)
                {
                    right = s.X - s.CenterX + (float)s.width;
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
                if (s.Y - s.CenterY < top)
                {
                    top = s.X - s.CenterY;
                }
                if (s.Y - s.CenterY + (float)s.height > bottom)
                {
                    bottom = s.Y - s.CenterY + (float)s.width;
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
            Vector2 pos = g.Position;
            g.Position -= Center;
            g.X *= ScaleX;
            g.Y *= ScaleY;
            g.Position += Position;
            float alph = g.Alpha;
            g.Alpha *= Alpha;
            Vector2 scl = g.Scale;
            g.ScaleX *= ScaleX;
            g.ScaleY *= ScaleY;
            float ang = g.Angle;
            g.Angle *= Angle;
            bool flip = g.flipH;
            g.flipH = base.flipH;
            g.Draw();
            g.Angle = ang;
            g.Scale = scl;
            g.Alpha = alph;
            g.Position = pos;
            g.flipH = flip;
        }
    }
}
