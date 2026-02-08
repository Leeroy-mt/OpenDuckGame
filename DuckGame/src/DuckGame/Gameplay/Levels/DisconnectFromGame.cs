using Microsoft.Xna.Framework;

namespace DuckGame;

public class DisconnectFromGame : Level, IConnectionScreen
{
    private float _dots;

    private bool _disconnected;

    private ulong joinAddress;

    private float wait = 1f;

    private bool _didDisconnect;

    public DisconnectFromGame()
    {
        _centeredView = true;
    }

    public DisconnectFromGame(ulong pAndJoin)
    {
        _centeredView = true;
        joinAddress = pAndJoin;
    }

    public override void Initialize()
    {
        DuckNetwork.ClosePauseMenu();
        if (Network.isActive)
        {
            _startCalled = true;
            DuckNetwork.Disconnect();
            Send.Message(new NMDisconnect(DuckNetError.ControlledDisconnect), NetMessagePriority.ReliableOrdered);
            ConnectionStatusUI.Hide();
        }
        base.Initialize();
    }

    public override void Update()
    {
        wait -= Maths.IncFrameTimer();
        if (wait < 0f)
        {
            if (!_didDisconnect)
            {
                _didDisconnect = true;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Disconnecting from game."));
                ConnectionStatusUI.Hide();
            }
            if (_disconnected)
            {
                if (joinAddress != 0L)
                {
                    Level.current = new JoinServer(joinAddress);
                }
                else
                {
                    Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
                }
                if (Graphics.fade <= 0f)
                {
                    Level.current = new TitleScreen();
                }
            }
        }
        base.Update();
    }

    public override void OnSessionEnded(DuckNetErrorInfo error)
    {
        _disconnected = true;
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
        string text = "Disconnecting";
        Graphics.DrawString(text + elipsis, new Vector2(Layer.HUD.width / 2f - Graphics.GetStringWidth(text) / 2f, Layer.HUD.height / 2f - 4f), Color.White);
    }
}
