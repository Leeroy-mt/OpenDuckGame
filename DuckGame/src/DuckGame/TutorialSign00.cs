namespace DuckGame;

[EditorGroup("Details|Signs|Tutorial", EditorItemType.PowerUser)]
public class TutorialSign00 : TutorialSign
{
	public TutorialSign00(float xpos, float ypos)
		: base(xpos, ypos, "tutorial/jump", "Jump")
	{
	}

	public override void Draw()
	{
		Graphics.DrawString("@JUMP@", new Vec2(base.x - 24f, base.y + 36f), Color.White * 0.5f);
		graphic.color = Color.White * 0.5f;
		base.Draw();
	}
}
