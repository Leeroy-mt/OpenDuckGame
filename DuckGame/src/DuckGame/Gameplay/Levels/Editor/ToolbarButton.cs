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
            graphic.Scale = new Vec2(0.5f, 0.5f);
        }
        else
        {
            graphic = new SpriteMap("iconSheet", 16, 16)
            {
                frame = image
            };
        }
        hoverText = ht;
        base.Depth = 0.88f;
    }

    public override void Update()
    {
        Rectangle plotRect = new Rectangle(Position.X, Position.Y, 16f, 16f);
        if (Editor.inputMode == EditorInput.Mouse)
        {
            if (Mouse.x > base.X && Mouse.x < base.X + 16f && Mouse.y > base.Y && Mouse.y < base.Y + 16f)
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
        Graphics.DrawRect(Position, Position + new Vec2(16f, 16f), _hover ? new Color(170, 170, 170) : new Color(70, 70, 70), 0.87f);
        graphic.Position = Position;
        graphic.Depth = 0.88f;
        graphic.Draw();
    }
}
