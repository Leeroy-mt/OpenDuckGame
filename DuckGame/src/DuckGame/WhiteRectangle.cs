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
		edge.center = new Vec2(32f, 32f);
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
			Graphics.DrawTexturedLine(edge.texture, position + new Vec2(0f, size.y / 2f), position + new Vec2(size.x, size.y / 2f), new Color(255, 255, 255, 0), size.y / 20f, 1f);
			return;
		}
		Graphics.DrawRect(position, position + size, new Color(255, 255, 255, 0), 1f);
		Graphics.DrawTexturedLine(gradientLine.texture, position + new Vec2(0f, 0f), position + new Vec2(size.x, 0f), new Color(255, 255, 255, 0), 1f, 1f);
		Graphics.DrawTexturedLine(gradientLine.texture, position + new Vec2(0f, size.y + 1f), position + new Vec2(size.x, size.y + 1f), new Color(255, 255, 255, 0), 1f, 1f);
		Graphics.DrawTexturedLine(gradientLine.texture, position + new Vec2(0f, 0f), position + new Vec2(0f, size.y), new Color(255, 255, 255, 0), 0.5f, 1f);
		Graphics.DrawTexturedLine(gradientLine.texture, position + new Vec2(size.x - 1f, 0f), position + new Vec2(size.x - 1f, size.y), new Color(255, 255, 255, 0), 0.5f, 1f);
		edgeVert.xscale = 0.5f;
		edge.xscale = 0.5f;
		edge.flipH = false;
		edge.color = new Color(255, 255, 255, 0);
		edgeVert.color = new Color(255, 255, 255, 0);
		Graphics.Draw(edge, base.x, base.y);
		edge.flipH = true;
		Graphics.Draw(edge, base.x + size.x, base.y);
		edgeVert.flipH = true;
		Graphics.Draw(edgeVert, base.x + size.x + 16f, base.y + size.y);
		edgeVert.flipH = false;
		Graphics.Draw(edgeVert, base.x - 16f, base.y + size.y);
		base.Draw();
	}
}
