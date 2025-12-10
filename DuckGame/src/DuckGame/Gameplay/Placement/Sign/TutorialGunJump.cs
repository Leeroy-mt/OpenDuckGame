namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialGunJump : TutorialSign
{
    public TutorialGunJump(float xpos, float ypos)
        : base(xpos, ypos, "tutorial/gunjump", "Gun Jump")
    {
    }

    public override void Draw()
    {
        Color dim = new Color(127, 127, 127);
        Graphics.DrawString("@SHOOT@", new Vec2(base.x - 16f, base.y + 8f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vec2(base.x - 39f, base.y + 8f), Color.White * 0.5f);
        base.depth = 0.99f;
        graphic.color = dim;
        base.Draw();
    }
}
