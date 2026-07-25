using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MTSpriteBatchItemMetaData
{
#if NO_TEX2D
    public Texture2D texture;
#else
    public Tex2D texture;
#endif

    public float rotation;

    public Color color;

    public SpriteEffects effect;

    public float depth;

    public RectangleF tempRect;
}
