namespace DuckGame;

public class ArmatureLogo : Level
{
    private BitmapFont _font;

    private Sprite _logo;

    private float _wait = 1f;

    private bool _fading;

    public override void Initialize()
    {
        _font = new BitmapFont("biosFont", 8);
        _logo = new Sprite("logo_armature");
        Graphics.fade = 0f;
    }

    public override void Update()
    {
        if (!_fading)
        {
            if (Graphics.fade < 1f)
            {
                Graphics.fade += 0.013f;
            }
            else
            {
                Graphics.fade = 1f;
            }
        }
        else if (Graphics.fade > 0f)
        {
            Graphics.fade -= 0.013f;
        }
        else
        {
            Graphics.fade = 0f;
            if (MonoMain.startInEditor)
            {
                Level.current = Main.editor;
            }
            else
            {
                Level.current = new TitleScreen();
            }
        }
        _wait -= 0.006f;
        if (_wait < 0f || Input.Pressed("START") || Input.Pressed("SELECT"))
        {
            _fading = true;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Game)
        {
            float scalar = 0.25f;
            _logo.scale = new Vec2(scalar, scalar);
            Graphics.Draw(_logo, 160f - (float)(_logo.width / 2) * scalar, 90f - (float)(_logo.height / 2) * scalar);
        }
    }
}
