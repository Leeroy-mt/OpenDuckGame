namespace DuckGame;

[EditorGroup("Arcade|Targets", EditorItemType.ArcadeNew)]
[BaggedProperty("canSpawn", false)]
[BaggedProperty("isOnlineCapable", false)]
[BaggedProperty("previewPriority", true)]
public class GoodyNew : Goody
{
	public EditorProperty<int> Order = new EditorProperty<int>(0, null, -1f, 256f, 1f, "RANDOM");

	public EditorProperty<int> Style;

	public EditorProperty<int> Style_Group;

	private SpriteMap _sprite;

	public override void EditorPropertyChanged(object property)
	{
		UpdateFrame();
	}

	public GoodyNew(float xpos, float ypos)
		: base(xpos, ypos, new SpriteMap("challenge/goody", 16, 16))
	{
		_sprite = graphic as SpriteMap;
		Style = new EditorProperty<int>(0, this, -1f, 3f, 1f, "RANDOM");
		Style_Group = new EditorProperty<int>(0, this, -1f, 4f, 1f, "RANDOM");
		Order._tooltip = "All Targets/Goodies with smaller Order numbers must be destroyed/collected before this goody appears.";
		_editorName = "Goody";
		_contextMenuFilter.Add("Sequence");
		base.sequence._resetLikelyhood = false;
	}

	public void UpdateFrame()
	{
		int style = Style.value;
		int group = Style_Group.value;
		if (style == -1)
		{
			style = Rando.Int(0, 3);
		}
		if (group == -1)
		{
			group = Rando.Int(0, 4);
		}
		if (style > 3)
		{
			style = 3;
		}
		if (group > 4)
		{
			group = 4;
		}
		_sprite.frame = style + group * 4;
	}

	public override void Initialize()
	{
		base.sequence.order = Order.value;
		if (base.sequence.order == -1)
		{
			base.sequence.order = Rando.Int(256);
		}
		base.sequence.waitTillOrder = true;
		UpdateFrame();
		base.Initialize();
	}
}
