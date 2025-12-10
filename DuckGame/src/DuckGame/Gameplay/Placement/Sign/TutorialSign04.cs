namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialSign04 : TutorialSign
{
    public TutorialSign04(float xpos, float ypos)
        : base(xpos, ypos, "tutorial/fallThrough", "Fall Through")
    {
    }

    public override void Draw()
    {
        Graphics.DrawString("@JUMP@", new Vec2(base.x + 7f, base.y - 30f), Color.White * 0.5f);
        Graphics.DrawString("@DOWN@", new Vec2(base.x - 18f, base.y - 30f), Color.White * 0.5f);
        graphic.color = Color.White * 0.5f;
        base.Draw();
    }
}
