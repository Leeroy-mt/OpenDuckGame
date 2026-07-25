using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public struct DrawCall
{
#if NO_TEX2D
    public Texture2D texture;
#else
    public Tex2D texture;
#endif

    public Vector2 position;

    public RectangleF? sourceRect;

    public Color color;

    public float rotation;

    public Vector2 origin;

    public Vector2 scale;

    public SpriteEffects effects;

    public float depth;

    public Effect material;
}
