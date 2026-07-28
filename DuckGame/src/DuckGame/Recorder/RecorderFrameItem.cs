using Microsoft.Xna.Framework;

namespace DuckGame;

/* Rectangle Features (Current):
 * - Texture Index (mapped textures) (2 bytes)
 * - TopLeft & BottomRight (only rectangles) (16 bytes)
 * - Rotation (4 bytes)
 * - Color (mono-color) (4 bytes)
 * - Texture Coordinates (2D) (8 bytes)
 * - Depth (4 bytes)
 * - Total: 38 bytes per rectangle
 */

/* Triangle Features:
 * - Texture Index (2 bytes)
 * - 3 Compact Vertices (36 bytes)
 * - Color (mono-color) (4 bytes)
 * - Depth (mono-depth) (4 bytes)
 * - Total: 46 bytes per triangle (92 per quad)
 */

/* Quad Features:
 * - Texture Index (2 bytes)
 * - 4 Compact Vertices (48 bytes)
 * - Color (mono-color) (4 bytes)
 * - Depth (mono-depth) (4 bytes)
 * - Total: 58 bytes per quad
 */

/*
 * struct CompactVertex (12 bytes)
 * {
 *    public Vector2 Position; (8 bytes)
 *    public short TextureX, TextureY; (4 bytes)
 * }
 */

public struct RecorderFrameItem
{
    public short texture;

    public Vector2 topLeft;

    public Vector2 bottomRight;

    public float rotation;

    public Color color;

    public short texX;

    public short texY;

    public short texW;

    public short texH;

    public float depth;

    public void SetData(short textureVal, Vector2 topLeftVal, Vector2 bottomRightVal, float rotationVal, Color colorVal, short texXVal, short texYVal, short texWVal, short texHVal, float depthVal)
    {
        texture = textureVal;
        topLeft = topLeftVal;
        bottomRight = bottomRightVal;
        rotation = rotationVal;
        color = colorVal;
        texX = texXVal;
        texY = texYVal;
        texW = texWVal;
        texH = texHVal;
        depth = depthVal;
    }
}
