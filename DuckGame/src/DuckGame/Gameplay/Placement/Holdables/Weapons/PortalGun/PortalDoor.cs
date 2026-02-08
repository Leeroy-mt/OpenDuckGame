using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class PortalDoor
{
    public Vector2 point1;

    public Vector2 point2;

    public Vector2 center;

    public bool horizontal;

    public bool isLeft;

    public Rectangle rect;

    public Layer layer;

    public List<Block> collision = new List<Block>();

    public float top => point1.Y;

    public float left => point1.X;

    public float bottom => point2.Y;

    public float right => point2.X;

    public void Update()
    {
        Vector2 position = new Vector2(center.X, center.Y);
        Matrix m = Level.current.camera.getMatrix();
        Vector2 vec = Vector2.Transform(position, m);
        int xScissor = (int)vec.X;
        if (xScissor < 0)
        {
            xScissor = 0;
        }
        if (xScissor > Graphics.width)
        {
            xScissor = Graphics.width;
        }
        int yScissor = (int)vec.Y;
        if (yScissor < 0)
        {
            yScissor = 0;
        }
        if (yScissor > Graphics.height)
        {
            yScissor = Graphics.height;
        }
        if (horizontal)
        {
            if (isLeft)
            {
                layer.scissor = new Rectangle(0f, yScissor, Graphics.width, Graphics.height - yScissor);
            }
            else
            {
                layer.scissor = new Rectangle(0f, 0f, Graphics.width, yScissor);
            }
        }
        else if (isLeft)
        {
            layer.scissor = new Rectangle(xScissor, 0f, Graphics.width - xScissor, Graphics.height);
        }
        else
        {
            layer.scissor = new Rectangle(0f, 0f, xScissor, Graphics.height);
        }
    }

    public void Draw()
    {
        Graphics.DrawLine(point1, point2, Color.Orange, 2f, 1f);
    }
}
