using Steamworks;

public class Lobby
{
    #region Delegates & Events

    public delegate void UserStatusChangeDelegate(User? user, SteamLobbyUserStatusFlags flags, User? responsibleUser);

    public delegate void ChatMessageDelegate(User user, byte[] data);

    public event UserStatusChangeDelegate? UserStatusChange;

    public event ChatMessageDelegate? ChatMessage;

    #endregion

    public int randomID;

    #region Private Fields

    bool joinable;

    SteamLobbyType type;

    CSteamID id;

    #endregion

    #region Public Properties

    public bool Joinable
    {
        get => Id != 0
            && joinable;
        set
        {
            if (Id != 0 && Steam.Initialized)
            {
                SteamMatchmaking.SetLobbyJoinable(id, value);
                joinable = value;
            }
        }
    }
    public bool Processing { get; private set; }

    public int MaxMembers
    {
        get => Id != 0 && Steam.Initialized
            ? SteamMatchmaking.GetLobbyMemberLimit(id)
            : 0;
        set
        {
            if (Id != 0 && Steam.Initialized)
                SteamMatchmaking.SetLobbyMemberLimit(id, value);
        }
    }

    public ulong Id => id.m_SteamID;

    public string Name
    {
        get => Id != 0 && Steam.Initialized
            ? SteamMatchmaking.GetLobbyData(id, "name")
            : "";
        set
        {
            if (Id != 0 && Steam.Initialized)
                SteamMatchmaking.SetLobbyData(id, "name", value);
        }
    }

    public SteamLobbyJoinResult JoinResult { get; private set; }
    public SteamLobbyType Type
    {
        get => type;
        set
        {
            if (Id != 0 && Steam.Initialized)
            {
                SteamMatchmaking.SetLobbyType(id, (ELobbyType)value);
                type = value;
            }
        }
    }

    public User? Owner
    {
        get
        {
            try
            {
                if (Id != 0 && Steam.Initialized)
                    return User.GetUser(SteamMatchmaking.GetLobbyOwner(id));
            }
            catch { }

            return null;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            SteamMatchmaking.SetLobbyOwner((CSteamID)Id, (CSteamID)value.Id);
        }
    }

    public List<User?> Users => Id != 0 && Steam.Initialized
        ? SteamHelper.GetList(SteamMatchmaking.GetNumLobbyMembers(id), i => User.GetUser(SteamMatchmaking.GetLobbyMemberByIndex(id, i)))
        : [];

    #endregion

    #region Constructors

    public Lobby(ulong lobbyID)
        : this(new CSteamID(lobbyID)) { }

    public Lobby(SteamLobbyType lobbyTypeVal)
    {
        type = lobbyTypeVal;
        id = new CSteamID();
        joinable = true;

        Processing = true;
    }

    internal Lobby(CSteamID lobbyID)
    {
        type = SteamLobbyType.FriendsOnly;
        id = lobbyID;
        joinable = true;

        Processing = true;
    }

    #endregion

    #region Public Methods

    public void SetLobbyModsData(string value)
    {
        if (Id != 0 && Steam.Initialized)
            SteamMatchmaking.SetLobbyData(id, "mods", value);
    }

    public void OnProcessingComplete(ulong idVal, SteamLobbyJoinResult result)
    {
        id = new CSteamID(idVal);
        JoinResult = result;
        Processing = false;
    }

    public void OnUserStatusChange(User? user, SteamLobbyUserStatusFlags flags, User? responsibleUser)
    {
        UserStatusChange?.Invoke(user, flags, responsibleUser);
    }

    public void OnChatMessage(User user, byte[] data)
    {
        ChatMessage?.Invoke(user, data);
    }

    public void SetLobbyData(string name, string value)
    {
        if (Id != 0 && Steam.Initialized)
            SteamMatchmaking.SetLobbyData(id, name, value);
    }

    public string GetLobbyData(string name)
    {
        if (Id != 0 && Steam.Initialized)
            return SteamMatchmaking.GetLobbyData(id, name);
        return "";
    }

    #endregion
}