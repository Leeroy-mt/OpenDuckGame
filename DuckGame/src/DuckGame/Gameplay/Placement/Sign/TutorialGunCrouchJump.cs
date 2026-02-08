using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialGunCrouchJump : TutorialSign
{
    public TutorialGunCrouchJump(float xpos, float ypos)
        : base(xpos, ypos, "tutorial/guncrouchjump", "Gun Crouch Jump")
    {
    }

    public override void Draw()
    {
        Color dim = new Color(127, 127, 127);
        Graphics.DrawString("@SHOOT@", new Vector2(base.X - 16f, base.Y + 8f), Color.White * 0.5f);
        Graphics.DrawString("@JUMP@", new Vector2(base.X - 39f, base.Y + 8f), Color.White * 0.5f);
        base.Depth = 0.99f;
        graphic.color = dim;
        base.Draw();
    }
}
