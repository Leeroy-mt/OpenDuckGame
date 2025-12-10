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
        Graphics.DrawString("@JUMP@", new Vec2(base.x - 26f, base.y + 32f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vec2(base.x - 5f, base.y - 16f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vec2(base.x + 15f, base.y - 8f), Color.White * 0.5f);
        base.depth = 0.99f;
        graphic.color = dim;
        base.Draw();
    }
}
