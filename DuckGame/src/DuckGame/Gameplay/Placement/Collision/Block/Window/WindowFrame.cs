using Microsoft.Xna.Framework;

namespace DuckGame;

public class WindowFrame : Thing
{
    public float high;

    private bool floor;

    public WindowFrame(float xpos, float ypos, bool f)
        : base(xpos, ypos)
    {
        graphic = new Sprite("windowFrame");
        Center = new Vector2(5f, 26f);
        base.Depth = -0.95f;
        _editorCanModify = false;
        floor = f;
        if (floor)
        {
            graphic.AngleDegrees = -90f;
        }
    }

    public override void Draw()
    {
        graphic.Depth = base.Depth;
        if (floor)
        {
            Graphics.Draw(graphic, base.X + 14f, base.Y + 5f, new Rectangle(0f, graphic.height - 2, graphic.width, 2f));
            Graphics.Draw(graphic, base.X + 14f - high, base.Y + 5f, new Rectangle(0f, 0f, graphic.width, 3f));
        }
        else
        {
            Graphics.Draw(graphic, base.X - 5f, base.Y + 6f, new Rectangle(0f, graphic.height - 2, graphic.width, 2f));
            Graphics.Draw(graphic, base.X - 5f, base.Y + 6f - high, new Rectangle(0f, 0f, graphic.width, 3f));
        }
    }
}
