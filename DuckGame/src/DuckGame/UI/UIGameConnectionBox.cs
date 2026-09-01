namespace DuckGame;

public class UIGameConnectionBox : UIMatchmakingBox
{
    private UIMenu _openOnClose;

    private UIServerBrowser.LobbyData _connectLobby;

    private string _passwordAttempt;

    public UIGameConnectionBox(UIServerBrowser.LobbyData connect, UIMenu openOnClose, float xpos, float ypos, float wide = -1f, float high = -1f)
        : base(openOnClose, xpos, ypos, wide, high)
    {
        playMusic = false;
        _connectLobby = connect;
        _continueSearchOnFail = false;
        _caption = "JOINING";
    }

    public void SetPasswordAttempt(string pPassword)
    {
        _passwordAttempt = pPassword;
    }

    public override void Open()
    {
        base.Open();
        _tryConnectLobby = _connectLobby.lobby;
#if FACEPUNCH
        if (_connectLobby.lobby.Id == 0)
#else
        if (_connectLobby.lobby == null)
#endif
        {
            DuckNetwork.Join("", _connectLobby.lanAddress, _passwordAttempt);
        }
        else
        {
            DuckNetwork.Join(_tryConnectLobby.Id.ToString(), "localhost", _passwordAttempt);
        }
        ChangeState(MatchmakingState.Connecting);
        _newStatusList.Add("|DGGREEN|Connecting to game...");
    }

    protected override void UpdateAdditionalMatchmakingLogic()
    {
    }
}
