namespace DuckGame;

public class ConnectingScreen : Level
{
    private float _dots;

    public ConnectingScreen()
    {
        _centeredView = true;
    }

    public override void Initialize()
    {
        DuckNetwork.ClosePauseMenu();
        ConnectionStatusUI.Hide();
        base.Initialize();
    }

    public override void Draw()
    {
        _dots += 0.01f;
        if (_dots > 1f)
        {
            _dots = 0f;
        }
        string elipsis = "";
        for (int i = 0; i < 3; i++)
        {
            if (_dots * 4f > (float)(i + 1))
            {
                elipsis += ".";
            }
        }
        string text = "Connecting";
        Graphics.DrawString(text + elipsis, new Vec2(Layer.HUD.width / 2f - Graphics.GetStringWidth(text) / 2f, Layer.HUD.height / 2f - 4f), Color.White);
    }
}
