namespace DuckGame;

public class MessageDialogue : ContextMenu
{
	private new string _text = "";

	private string[] _description;

	public bool result;

	private BitmapFont _font;

	private ContextMenu _ownerMenu;

	public ContextMenu confirmItem;

	public string contextString;

	private bool _hoverOk;

	private bool _hoverCancel;

	public float windowYOffsetAdd;

	private float bottomRightPos;

	public bool okayOnly;

	public MessageDialogue(ContextMenu pOwnerMenu)
		: base(null)
	{
		_ownerMenu = pOwnerMenu;
	}

	public MessageDialogue()
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
		_font.allowBigSprites = true;
	}

	public void Open(string text, string startingText = "", string pDescription = null)
	{
		base.opened = true;
		_text = text;
		if (pDescription == null)
		{
			_description = null;
		}
		else
		{
			_description = pDescription.Split('\n');
		}
		SFX.Play("openClick", 0.4f);
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
		if (!base.opened)
		{
			return;
		}
		if (_opening)
		{
			_opening = false;
			_selectedIndex = 1;
		}
		if (Keyboard.Pressed(Keys.Enter))
		{
			result = true;
			CompleteDialogue();
		}
		if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed || Input.Pressed("CANCEL"))
		{
			result = false;
			CompleteDialogue();
		}
		float windowWidth = 300f;
		float windowHeight = 80f;
		Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f + windowYOffsetAdd);
		Vec2 okPos = new Vec2(topLeft.x + 18f, bottomRightPos - 50f);
		Vec2 okSize = new Vec2(120f, 40f);
		float middle = (base.layer.width / 2f + windowWidth / 2f - topLeft.x) / 2f;
		if (okayOnly)
		{
			okPos = new Vec2(topLeft.x + middle - okSize.x / 2f, bottomRightPos - 50f);
		}
		Vec2 cancelPos = new Vec2(topLeft.x + 160f, bottomRightPos - 50f);
		Vec2 cancelSize = new Vec2(120f, 40f);
		Rectangle rOKButton = new Rectangle(okPos.x, okPos.y, okSize.x, okSize.y);
		Rectangle rCancelButton = new Rectangle(cancelPos.x, cancelPos.y, cancelSize.x, cancelSize.y);
		bool bTouchSelectedOK = false;
		bool bTouchSelectedCancel = false;
		if (Editor.inputMode == EditorInput.Mouse)
		{
			if (Mouse.x > okPos.x && Mouse.x < okPos.x + okSize.x && Mouse.y > okPos.y && Mouse.y < okPos.y + okSize.y)
			{
				_hoverOk = true;
			}
			else
			{
				_hoverOk = false;
			}
			if (!okayOnly)
			{
				if (Mouse.x > cancelPos.x && Mouse.x < cancelPos.x + cancelSize.x && Mouse.y > cancelPos.y && Mouse.y < cancelPos.y + cancelSize.y)
				{
					_hoverCancel = true;
				}
				else
				{
					_hoverCancel = false;
				}
			}
		}
		else if (Editor.inputMode == EditorInput.Touch)
		{
			if (!okayOnly)
			{
				if (TouchScreen.GetTap().Check(rOKButton, Layer.HUD.camera))
				{
					_selectedIndex = 0;
					_hoverOk = true;
					bTouchSelectedOK = true;
				}
				else
				{
					_hoverOk = false;
				}
				if (TouchScreen.GetTap().Check(rCancelButton, Layer.HUD.camera))
				{
					_selectedIndex = 1;
					_hoverCancel = true;
					bTouchSelectedCancel = true;
				}
				else
				{
					_hoverCancel = false;
				}
			}
		}
		else
		{
			_hoverOk = (_hoverCancel = false);
		}
		if (!_hoverOk && !_hoverCancel)
		{
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
			if (Editor.inputMode == EditorInput.Gamepad)
			{
				if (okayOnly)
				{
					_hoverOk = true;
				}
				else
				{
					_hoverOk = (_hoverCancel = false);
					if (_selectedIndex == 0)
					{
						_hoverOk = true;
					}
					else
					{
						_hoverCancel = true;
					}
				}
			}
		}
		if (!Editor.tookInput && _hoverOk && ((Editor.inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed) || Input.Pressed("SELECT") || bTouchSelectedOK))
		{
			result = true;
			CompleteDialogue();
		}
		if (!Editor.tookInput && _hoverCancel && ((Editor.inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed) || Input.Pressed("SELECT") || bTouchSelectedCancel))
		{
			result = false;
			CompleteDialogue();
		}
	}

	private void CompleteDialogue()
	{
		base.opened = false;
		Editor.tookInput = true;
		Editor.lockInput = null;
		if (_ownerMenu == null)
		{
			if (Level.current is Editor)
			{
				(Level.current as Editor).CompleteDialogue(confirmItem);
			}
		}
		else if (confirmItem != null && _ownerMenu != null && result)
		{
			_ownerMenu.Selected(confirmItem);
		}
	}

	public override void Draw()
	{
		if (!base.opened)
		{
			return;
		}
		base.Draw();
		Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Layer.HUD.width, Layer.HUD.height), Color.Black * 0.5f, base.depth - 2);
		float windowWidth = 300f;
		float windowHeight = 80f;
		Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f + windowYOffsetAdd);
		Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f + windowYOffsetAdd);
		float middle = (bottomRight.x - topLeft.x) / 2f;
		if (_description != null)
		{
			int topOffset = 18;
			string[] description = _description;
			foreach (string obj in description)
			{
				float wide = Graphics.GetStringWidth(obj);
				Graphics.DrawString(obj, topLeft + new Vec2(middle - wide / 2f, 5 + topOffset), Color.White, base.depth + 2);
				topOffset += 8;
				bottomRight.y += 8f;
			}
		}
		Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.depth, filled: false);
		Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.depth - 1);
		Graphics.DrawRect(topLeft + new Vec2(4f, 20f), bottomRight + new Vec2(-4f, -4f), new Color(10, 10, 10), base.depth + 1);
		Graphics.DrawRect(topLeft + new Vec2(2f, 2f), new Vec2(bottomRight.x - 2f, topLeft.y + 16f), new Color(70, 70, 70), base.depth + 1);
		float titleWidth = Graphics.GetStringWidth(_text);
		Graphics.DrawString(_text, topLeft + new Vec2(middle - titleWidth / 2f, 5f), Color.White, base.depth + 2);
		_font.scale = new Vec2(2f, 2f);
		if (okayOnly)
		{
			Vec2 okSize = new Vec2(120f, 40f);
			Vec2 okPos = new Vec2(base.x + middle - okSize.x / 2f - 2f, bottomRight.y - 50f);
			Graphics.DrawRect(okPos, okPos + okSize, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
			_font.Draw("OK", okPos.x + okSize.x / 2f - _font.GetWidth("OK") / 2f, okPos.y + 12f, Color.White, base.depth + 3);
		}
		else
		{
			Vec2 okPos2 = new Vec2(topLeft.x + 18f, bottomRight.y - 50f);
			Vec2 okSize2 = new Vec2(120f, 40f);
			Graphics.DrawRect(okPos2, okPos2 + okSize2, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
			_font.Draw("OK", okPos2.x + okSize2.x / 2f - _font.GetWidth("OK") / 2f, okPos2.y + 12f, Color.White, base.depth + 3);
			Vec2 cancelPos = new Vec2(topLeft.x + 160f, bottomRight.y - 50f);
			Vec2 cancelSize = new Vec2(120f, 40f);
			Graphics.DrawRect(cancelPos, cancelPos + cancelSize, _hoverCancel ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
			_font.Draw("CANCEL", cancelPos.x + cancelSize.x / 2f - _font.GetWidth("CANCEL") / 2f, cancelPos.y + 12f, Color.White, base.depth + 3);
		}
		bottomRightPos = bottomRight.y;
	}
}
