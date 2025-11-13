namespace DuckGame;

public class UploadDialogue : ContextMenu
{
	private new string _text = "";

	private BitmapFont _font;

	private bool _hoverOk;

	private WorkshopItem _item;

	private int _uploadIndex;

	public UploadDialogue()
		: base(null)
	{
	}

	public override void Initialize()
	{
		base.layer = Layer.HUD;
		base.depth = 0.95f;
		float windowWidth = 300f;
		float windowHeight = 40f;
		Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
		new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
		position = topLeft + new Vec2(4f, 20f);
		itemSize = new Vec2(490f, 16f);
		_root = true;
		_font = new BitmapFont("biosFont", 8);
	}

	public void Open(string text, WorkshopItem pItem)
	{
		base.opened = true;
		_text = text;
		SFX.Play("openClick", 0.4f);
		_item = pItem;
		_uploadIndex = 0;
	}

	public void Close()
	{
		base.opened = false;
	}

	public override void Selected(ContextMenu item)
	{
	}

	public override void Update()
	{
		if (base.opened)
		{
			if (_opening)
			{
				_opening = false;
				_selectedIndex = 1;
			}
			float windowWidth = 300f;
			float windowHeight = 80f;
			Vec2 vec = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
			new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
			Vec2 okPos = vec + new Vec2(18f, 28f);
			Vec2 okSize = new Vec2(120f, 40f);
			_ = vec + new Vec2(160f, 28f);
			new Vec2(120f, 40f);
			if (Mouse.x > okPos.x && Mouse.x < okPos.x + okSize.x && Mouse.y > okPos.y && Mouse.y < okPos.y + okSize.y)
			{
				_hoverOk = true;
			}
			else
			{
				_hoverOk = false;
			}
			if (!Editor.tookInput && Input.Pressed("MENULEFT"))
			{
				_selectedIndex--;
			}
			else if (!Editor.tookInput && Input.Pressed("MENURIGHT"))
			{
				_selectedIndex++;
			}
			if (_selectedIndex < 0)
			{
				_selectedIndex = 0;
			}
			if (_selectedIndex > 1)
			{
				_selectedIndex = 1;
			}
		}
	}

	public override void Draw()
	{
		if (base.opened)
		{
			base.Draw();
			float windowWidth = 300f;
			float windowHeight = 60f;
			Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
			Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
			Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.depth, filled: false);
			Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.depth - 1);
			Graphics.DrawRect(topLeft + new Vec2(4f, 20f), bottomRight + new Vec2(-4f, -4f), new Color(10, 10, 10), base.depth + 1);
			Graphics.DrawRect(topLeft + new Vec2(2f, 2f), new Vec2(bottomRight.x - 2f, topLeft.y + 16f), new Color(70, 70, 70), base.depth + 1);
			Graphics.DrawString(_text, topLeft + new Vec2(5f, 5f), Color.White, base.depth + 2);
			_font.scale = new Vec2(1f, 1f);
			Vec2 barPos = topLeft + new Vec2(14f, 38f);
			Vec2 barSize = new Vec2(270f, 16f);
			TransferProgress p = _item.GetUploadProgress();
			float progress = (float)p.bytesDownloaded / (float)p.bytesTotal;
			Graphics.DrawRect(barPos, barPos + barSize * new Vec2(progress, 1f), _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
			if (p.bytesTotal == 0L)
			{
				_font.Draw("Waiting...", barPos.x, barPos.y - 12f, Color.White, base.depth + 3);
				return;
			}
			_font.Draw("Uploading " + p.bytesDownloaded + "/" + p.bytesTotal + "B", barPos.x, barPos.y - 12f, Color.White, base.depth + 3);
		}
	}
}
