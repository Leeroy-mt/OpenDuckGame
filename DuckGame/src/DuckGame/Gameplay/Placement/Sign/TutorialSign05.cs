using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialSign05 : TutorialSign
{
    public TutorialSign05(float xpos, float ypos)
        : base(xpos, ypos, "tutorial/fly", "Fly")
    {
    }

    public override void Draw()
    {
        Color dim = new Color(127, 127, 127);
        Graphics.DrawString("@JUMP@", new Vector2(base.X - 26f, base.Y + 32f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vector2(base.X - 5f, base.Y - 16f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vector2(base.X + 15f, base.Y - 8f), Color.White * 0.5f);
        base.Depth = 0.99f;
        graphic.color = dim;
        base.Draw();
    }
}
