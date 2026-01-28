namespace DuckGame;

[EditorGroup("Background|Parallax|custom", EditorItemType.Custom)]
public class CustomParallax : BackgroundUpdater
{
    public bool didInit;

    public static string customParallax
    {
        get
        {
            return Custom.data[CustomType.Parallax][0];
        }
        set
        {
            Custom.data[CustomType.Parallax][0] = value;
            Custom.Clear(CustomType.Block, value);
        }
    }

    public CustomParallax(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 6
        };
        Center = new Vec2(8f, 8f);
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "Custom Parallax";
    }

    public override void Initialize()
    {
        didInit = true;
        if (Level.current is Editor)
        {
            return;
        }
        backgroundColor = new Color(25, 38, 41);
        Level.current.backgroundColor = backgroundColor;
        CustomTileData data = Custom.GetData(0, CustomType.Parallax);
        if (data != null && data.texture != null)
        {
            _parallax = new ParallaxBackground(data.texture);
            for (int i = 0; i < 40; i++)
            {
                _parallax.AddZone(i, 0f, 0f, moving: true);
            }
            Level.Add(_parallax);
        }
        else
        {
            _parallax = new ParallaxBackground("background/office", 0f, 0f, 3);
            Level.Add(_parallax);
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Terminate()
    {
        Level.Remove(_parallax);
    }

    public override ContextMenu GetContextMenu()
    {
        EditorGroupMenu editorGroupMenu = new EditorGroupMenu(null, root: true);
        editorGroupMenu.AddItem(new ContextFile("style", null, new FieldBinding(this, "customParallax"), ContextFileType.Parallax));
        return editorGroupMenu;
    }
}
