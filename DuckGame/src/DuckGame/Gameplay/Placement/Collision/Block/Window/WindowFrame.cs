namespace DuckGame;

public class WindowFrame : Thing
{
    public float high;

    private bool floor;

    public WindowFrame(float xpos, float ypos, bool f)
        : base(xpos, ypos)
    {
        graphic = new Sprite("windowFrame");
        center = new Vec2(5f, 26f);
        base.depth = -0.95f;
        _editorCanModify = false;
        floor = f;
        if (floor)
        {
            graphic.angleDegrees = -90f;
        }
    }

    public override void Draw()
    {
        graphic.depth = base.depth;
        if (floor)
        {
            Graphics.Draw(graphic, base.x + 14f, base.y + 5f, new Rectangle(0f, graphic.height - 2, graphic.width, 2f));
            Graphics.Draw(graphic, base.x + 14f - high, base.y + 5f, new Rectangle(0f, 0f, graphic.width, 3f));
        }
        else
        {
            Graphics.Draw(graphic, base.x - 5f, base.y + 6f, new Rectangle(0f, graphic.height - 2, graphic.width, 2f));
            Graphics.Draw(graphic, base.x - 5f, base.y + 6f - high, new Rectangle(0f, 0f, graphic.width, 3f));
        }
    }
}
