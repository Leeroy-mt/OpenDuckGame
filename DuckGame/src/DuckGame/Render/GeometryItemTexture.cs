using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GeometryItemTexture
{
    public bool temporary;

    public float depth;

    public VertexPositionColorTexture[] vertices;

    public Texture2D texture;

    public int length;

    public int size = 512;

    public GeometryItemTexture()
    {
        vertices = new VertexPositionColorTexture[size];
    }

    public void Clear()
    {
        length = 0;
    }

    public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 tx1, Vector2 tx2, Vector2 tx3, Color c)
    {
        if (length + 3 >= size)
        {
            VertexPositionColorTexture[] newVerts = new VertexPositionColorTexture[size * 2];
            vertices.CopyTo(newVerts, 0);
            vertices = newVerts;
            size *= 2;
        }
        vertices[length].Position.X = p1.X;
        vertices[length].Position.Y = p1.Y;
        vertices[length].Position.Z = depth;
        vertices[length].TextureCoordinate.X = tx1.X;
        vertices[length].TextureCoordinate.Y = tx1.Y;
        vertices[length].Color = c;
        vertices[length + 1].Position.X = p2.X;
        vertices[length + 1].Position.Y = p2.Y;
        vertices[length + 1].Position.Z = depth;
        vertices[length + 1].TextureCoordinate.X = tx2.X;
        vertices[length + 1].TextureCoordinate.Y = tx2.Y;
        vertices[length + 1].Color = c;
        vertices[length + 2].Position.X = p3.X;
        vertices[length + 2].Position.Y = p3.Y;
        vertices[length + 2].Position.Z = depth;
        vertices[length + 2].TextureCoordinate.X = tx3.X;
        vertices[length + 2].TextureCoordinate.Y = tx3.Y;
        vertices[length + 2].Color = c;
        length += 3;
    }

    public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color c, Color c2, Color c3)
    {
        if (length + 3 >= size)
        {
            VertexPositionColorTexture[] newVerts = new VertexPositionColorTexture[size * 2];
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
