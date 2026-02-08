using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public struct DrawCall
{
    public Tex2D texture;

    public Vector2 position;

    public Rectangle? sourceRect;

    public Color color;

    public float rotation;

    public Vector2 origin;

    public Vector2 scale;

    public SpriteEffects effects;

    public float depth;

    public Material material;
}
