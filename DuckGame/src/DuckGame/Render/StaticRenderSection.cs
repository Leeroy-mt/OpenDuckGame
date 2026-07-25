using Microsoft.Xna.Framework;
using System.Collections.Generic;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class StaticRenderSection
{
#if NO_TEX2D
    public XnaRenderTarget2D target;
#else
    public RenderTarget2D target;
#endif

    public List<Thing> things = new List<Thing>();

    public Vector2 position;
}
