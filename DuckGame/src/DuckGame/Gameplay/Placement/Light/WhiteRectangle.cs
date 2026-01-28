namespace DuckGame;

public class WhiteRectangle : Thing
{
    public Vec2 size;

    public Sprite gradientLine;

    public Sprite edge;

    public Sprite edgeVert;

    public bool water;

    public WhiteRectangle(float xpos, float ypos, float wide, float high, bool waterVal = false)
        : base(xpos, ypos)
    {
        size = new Vec2(wide, high);
        base.layer = Layer.Lighting;
        gradientLine = new Sprite("lavaGlowLine");
        edge = new Sprite("lavaGlowEdge");
        edgeVert = new Sprite("lavaGlowEdgeVert");
        edge.Center = new Vec2(32f, 32f);
        water = waterVal;
        if (water)
        {
            edge = new Sprite("waterLighting");
        }
    }

    public override void Draw()
    {
        if (water)
        {
            Graphics.DrawTexturedLine(edge.texture, Position + new Vec2(0f, size.Y / 2f), Position + new Vec2(size.X, size.Y / 2f), new Color(255, 255, 255, 0), size.Y / 20f, 1f);
            return;
        }
        Graphics.DrawRect(Position, Position + size, new Color(255, 255, 255, 0), 1f);
        Graphics.DrawTexturedLine(gradientLine.texture, Position + new Vec2(0f, 0f), Position + new Vec2(size.X, 0f), new Color(255, 255, 255, 0), 1f, 1f);
        Graphics.DrawTexturedLine(gradientLine.texture, Position + new Vec2(0f, size.Y + 1f), Position + new Vec2(size.X, size.Y + 1f), new Color(255, 255, 255, 0), 1f, 1f);
        Graphics.DrawTexturedLine(gradientLine.texture, Position + new Vec2(0f, 0f), Position + new Vec2(0f, size.Y), new Color(255, 255, 255, 0), 0.5f, 1f);
        Graphics.DrawTexturedLine(gradientLine.texture, Position + new Vec2(size.X - 1f, 0f), Position + new Vec2(size.X - 1f, size.Y), new Color(255, 255, 255, 0), 0.5f, 1f);
        edgeVert.ScaleX = 0.5f;
        edge.ScaleX = 0.5f;
        edge.flipH = false;
        edge.color = new Color(255, 255, 255, 0);
        edgeVert.color = new Color(255, 255, 255, 0);
        Graphics.Draw(edge, base.X, base.Y);
        edge.flipH = true;
        Graphics.Draw(edge, base.X + size.X, base.Y);
        edgeVert.flipH = true;
        Graphics.Draw(edgeVert, base.X + size.X + 16f, base.Y + size.Y);
        edgeVert.flipH = false;
        Graphics.Draw(edgeVert, base.X - 16f, base.Y + size.Y);
        base.Draw();
    }
}
