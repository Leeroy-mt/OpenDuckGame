namespace DuckGame;

public class JoinServer : Level, IConnectionScreen
{
    private bool _attemptedConnection;

    private ulong _lobbyID;

    private float _dots;

    private ulong _timeout;

    private bool _teamSelect;

    private string password = "";

    private bool _startedJoining;

    public JoinServer(ulong lobbyAddress)
    {
        _lobbyID = lobbyAddress;
        _centeredView = true;
        _teamSelect = Level.current is TeamSelect2;
    }

    public JoinServer(ulong lobbyAddress, string pPassword)
        : this(lobbyAddress)
    {
        password = pPassword;
    }

    public override void Initialize()
    {
        if (Network.isActive)
        {
            Level.current = new DisconnectFromGame(_lobbyID);
            return;
        }
        DuckNetwork.ClosePauseMenu();
        ConnectionStatusUI.Hide();
        SkipStart();
        DevConsole.Log(DCSection.NetCore, "!~----------------JoinServer Level Begins----------------~!");
        if (Profiles.active.Count == 0 || !_teamSelect)
        {
            foreach (Profile p in Profiles.active)
            {
                p.team.Leave(p);
            }
            if (Profiles.experienceProfile != null)
            {
                Profiles.experienceProfile.team = Teams.Player1;
                Profiles.experienceProfile.persona = Persona.Duck1;
            }
            else
            {
                Profiles.DefaultPlayer1.team = Teams.Player1;
                Profiles.DefaultPlayer1.persona = Persona.Duck1;
            }
        }
        TeamSelect2.FillMatchmakingProfiles();
        if (_lobbyID == 0L || NetworkDebugger.enabled)
        {
            DuckNetwork.joinPort = (int)_lobbyID;
            DuckNetwork.Join("joinTest");
        }
        else
        {
            DuckNetwork.Join(_lobbyID.ToString(), "localhost", password);
        }
        _attemptedConnection = true;
        _startedJoining = true;
        _timeout = 0uL;
        base.Initialize();
    }

    public override void Update()
    {
        if (_timeout++ > 1200)
        {
            Network.DisconnectClient(DuckNetwork.localConnection, new DuckNetErrorInfo(DuckNetError.ConnectionTimeout, "Connection timeout!"));
            Level.current = new ConnectionError("|RED|CONNECTION FAILED!");
            _timeout = 0uL;
        }
        base.Update();
    }

    public override void OnSessionEnded(DuckNetErrorInfo error)
    {
        if (_startedJoining)
        {
            if (error != null)
            {
                Level.current = new ConnectionError(error.message);
            }
            else
            {
                Level.current = new ConnectionError("|RED|CONNECTION FAILED!");
            }
        }
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
