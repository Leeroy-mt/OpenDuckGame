using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class StaticRenderSection
{
    public RenderTarget2D target;

    public List<Thing> things = new List<Thing>();

    public Vector2 position;
}
