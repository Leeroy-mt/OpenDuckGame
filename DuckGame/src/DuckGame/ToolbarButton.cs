namespace DuckGame;

public class ToolbarButton : Thing
{
	private new ContextToolbarItem _owner;

	private bool _hover;

	public string hoverText = "";

	public bool hover
	{
		get
		{
			return _hover;
		}
		set
		{
			_hover = value;
		}
	}

	public ToolbarButton(ContextToolbarItem owner, int image, string ht)
	{
		_owner = owner;
		base.layer = Editor.objectMenuLayer;
		if (image == 99)
		{
			graphic = new Sprite("steamIcon");
			graphic.scale = new Vec2(0.5f, 0.5f);
		}
		else
		{
			graphic = new SpriteMap("iconSheet", 16, 16)
			{
				frame = image
			};
		}
		hoverText = ht;
		base.depth = 0.88f;
	}

	public override void Update()
	{
		Rectangle plotRect = new Rectangle(position.x, position.y, 16f, 16f);
		if (Editor.inputMode == EditorInput.Mouse)
		{
			if (Mouse.x > base.x && Mouse.x < base.x + 16f && Mouse.y > base.y && Mouse.y < base.y + 16f)
			{
				_owner.toolBarToolTip = hoverText;
				_hover = true;
				if (Mouse.left == InputState.Pressed)
				{
					_owner.toolBarToolTip = null;
					_ = _level;
					Editor.clickedMenu = true;
					_owner.ButtonPressed(this);
				}
			}
			else
			{
				_hover = false;
			}
		}
		else if (Editor.inputMode == EditorInput.Touch && TouchScreen.GetTap().Check(plotRect, base.layer.camera))
		{
			_hover = true;
			_ = _level;
			Editor.clickedMenu = true;
			_owner.ButtonPressed(this);
		}
		else if (_hover)
		{
			_owner.toolBarToolTip = hoverText;
		}
	}

	public override void Draw()
	{
		Graphics.DrawRect(position, position + new Vec2(16f, 16f), _hover ? new Color(170, 170, 170) : new Color(70, 70, 70), 0.87f);
		graphic.position = position;
		graphic.depth = 0.88f;
		graphic.Draw();
	}
}
