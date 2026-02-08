using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialSign03 : TutorialSign
{
    public TutorialSign03(float xpos, float ypos)
        : base(xpos, ypos, "tutorial/jumpThrough", "Jump Through")
    {
    }

    public override void Draw()
    {
        Graphics.DrawString("@JUMP@", new Vector2(base.X + 40f, base.Y + 13f), Color.White * 0.5f);
        graphic.color = Color.White * 0.5f;
        base.Draw();
    }
}
