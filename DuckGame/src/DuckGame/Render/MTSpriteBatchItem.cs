using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MTSpriteBatchItem
{
    public MTSpriteBatchItemMetaData MetaData;

    public bool inPool = true;

    public Texture2D Texture;

    public Material Material;

    public float Depth;

    public VertexPositionColorTexture vertexTL;

    public VertexPositionColorTexture vertexTR;

    public VertexPositionColorTexture vertexBL;

    public VertexPositionColorTexture vertexBR;

    public MTSpriteBatchItem()
    {
        vertexTL = default(VertexPositionColorTexture);
        vertexTR = default(VertexPositionColorTexture);
        vertexBL = default(VertexPositionColorTexture);
        vertexBR = default(VertexPositionColorTexture);
    }

    public void Set(float x, float y, float w, float h, Color color, Vec2 texCoordTL, Vec2 texCoordBR)
    {
        vertexTL.Position.X = x;
        vertexTL.Position.Y = y;
        vertexTL.Position.Z = Depth;
        vertexTL.Color = color;
        vertexTL.TextureCoordinate.X = texCoordTL.X;
        vertexTL.TextureCoordinate.Y = texCoordTL.Y;
        vertexTR.Position.X = x + w;
        vertexTR.Position.Y = y;
        vertexTR.Position.Z = Depth;
        vertexTR.Color = color;
        vertexTR.TextureCoordinate.X = texCoordBR.X;
        vertexTR.TextureCoordinate.Y = texCoordTL.Y;
        vertexBL.Position.X = x;
        vertexBL.Position.Y = y + h;
        vertexBL.Position.Z = Depth;
        vertexBL.Color = color;
        vertexBL.TextureCoordinate.X = texCoordTL.X;
        vertexBL.TextureCoordinate.Y = texCoordBR.Y;
        vertexBR.Position.X = x + w;
        vertexBR.Position.Y = y + h;
        vertexBR.Position.Z = Depth;
        vertexBR.Color = color;
        vertexBR.TextureCoordinate.X = texCoordBR.X;
        vertexBR.TextureCoordinate.Y = texCoordBR.Y;
    }

    public void Set(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, Vec2 t1, Vec2 t2, Vec2 t3, Vec2 t4, Color c)
    {
        vertexTL.Position.X = p1.X;
        vertexTL.Position.Y = p1.Y;
        vertexTL.Position.Z = Depth;
        vertexTL.Color = c;
        vertexTL.TextureCoordinate.X = t1.X;
        vertexTL.TextureCoordinate.Y = t1.Y;
        vertexTR.Position.X = p2.X;
        vertexTR.Position.Y = p2.Y;
        vertexTR.Position.Z = Depth;
        vertexTR.Color = c;
        vertexTR.TextureCoordinate.X = t2.X;
        vertexTR.TextureCoordinate.Y = t2.Y;
        vertexBL.Position.X = p3.X;
        vertexBL.Position.Y = p3.Y;
        vertexBL.Position.Z = Depth;
        vertexBL.Color = c;
        vertexBL.TextureCoordinate.X = t3.X;
        vertexBL.TextureCoordinate.Y = t3.Y;
        vertexBR.Position.X = p4.X;
        vertexBR.Position.Y = p4.Y;
        vertexBR.Position.Z = Depth;
        vertexBR.Color = c;
        vertexBR.TextureCoordinate.X = t4.X;
        vertexBR.TextureCoordinate.Y = t4.Y;
    }

    public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vec2 texCoordTL, Vec2 texCoordBR)
    {
        vertexTL.Position.X = x + dx * cos - dy * sin;
        vertexTL.Position.Y = y + dx * sin + dy * cos;
        vertexTL.Position.Z = Depth;
        vertexTL.Color = color;
        vertexTL.TextureCoordinate.X = texCoordTL.X;
        vertexTL.TextureCoordinate.Y = texCoordTL.Y;
        vertexTR.Position.X = x + (dx + w) * cos - dy * sin;
        vertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
        vertexTR.Position.Z = Depth;
        vertexTR.Color = color;
        vertexTR.TextureCoordinate.X = texCoordBR.X;
        vertexTR.TextureCoordinate.Y = texCoordTL.Y;
        vertexBL.Position.X = x + dx * cos - (dy + h) * sin;
        vertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
        vertexBL.Position.Z = Depth;
        vertexBL.Color = color;
        vertexBL.TextureCoordinate.X = texCoordTL.X;
        vertexBL.TextureCoordinate.Y = texCoordBR.Y;
        vertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
        vertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
        vertexBR.Position.Z = Depth;
        vertexBR.Color = color;
        vertexBR.TextureCoordinate.X = texCoordBR.X;
        vertexBR.TextureCoordinate.Y = texCoordBR.Y;
    }
}
