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
		Graphics.DrawString("@JUMP@", new Vec2(base.x + 40f, base.y + 13f), Color.White * 0.5f);
		graphic.color = Color.White * 0.5f;
		base.Draw();
	}
}
