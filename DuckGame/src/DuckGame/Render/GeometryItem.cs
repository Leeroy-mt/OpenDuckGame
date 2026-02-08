using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GeometryItem
{
    public bool temporary;

    public float depth;

    public VertexPositionColor[] vertices;

    public int length;

    public int size = 512;

    public Material material;

    public GeometryItem()
    {
        vertices = new VertexPositionColor[size];
    }

    public void Clear()
    {
        length = 0;
    }

    public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color c)
    {
        if (length + 3 >= size)
        {
            VertexPositionColor[] newVerts = new VertexPositionColor[size * 2];
            vertices.CopyTo(newVerts, 0);
            vertices = newVerts;
            size *= 2;
        }
        vertices[length].Position.X = p1.X;
        vertices[length].Position.Y = p1.Y;
        vertices[length].Position.Z = depth;
        vertices[length].Color = c;
        vertices[length + 1].Position.X = p2.X;
        vertices[length + 1].Position.Y = p2.Y;
        vertices[length + 1].Position.Z = depth;
        vertices[length + 1].Color = c;
        vertices[length + 2].Position.X = p3.X;
        vertices[length + 2].Position.Y = p3.Y;
        vertices[length + 2].Position.Z = depth;
        vertices[length + 2].Color = c;
        length += 3;
    }

    public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color c, Color c2, Color c3)
    {
        if (length + 3 >= size)
        {
            VertexPositionColor[] newVerts = new VertexPositionColor[size * 2];
            vertices.CopyTo(newVerts, 0);
            vertices = newVerts;
            size *= 2;
        }
        vertices[length].Position.X = p1.X;
        vertices[length].Position.Y = p1.Y;
        vertices[length].Position.Z = depth;
        vertices[length].Color = c;
        vertices[length + 1].Position.X = p2.X;
        vertices[length + 1].Position.Y = p2.Y;
        vertices[length + 1].Position.Z = depth;
        vertices[length + 1].Color = c2;
        vertices[length + 2].Position.X = p3.X;
        vertices[length + 2].Position.Y = p3.Y;
        vertices[length + 2].Position.Z = depth;
        vertices[length + 2].Color = c3;
        length += 3;
    }
}
