namespace DuckGame;

public class DisconnectError : Level
{
    private Profile _profile;

    public DisconnectError(Profile who)
    {
        _profile = who;
        _centeredView = true;
    }

    public override void Initialize()
    {
        DuckNetwork.ClosePauseMenu();
        ConnectionStatusUI.Hide();
        HUD.AddCornerMessage(HUDCorner.BottomRight, "@START@CONTINUE");
        _startCalled = true;
        base.Initialize();
    }

    public override void Update()
    {
        if (Input.Pressed("START"))
        {
            Level.current = new TitleScreen();
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_profile != null)
        {
            string text = " |RED|" + _profile.name + " has disconnected.";
            Graphics.DrawString(text, new Vec2(Layer.HUD.camera.width / 2f - Graphics.GetStringWidth(text) / 2f, Layer.HUD.camera.height / 2f), Color.White);
        }
        else
        {
            string text2 = " |RED|The host has disconnected.";
            Graphics.DrawString(text2, new Vec2(Layer.HUD.camera.width / 2f - Graphics.GetStringWidth(text2) / 2f, Layer.HUD.camera.height / 2f), Color.White);
        }
    }
}
