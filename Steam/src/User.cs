using Steamworks;

namespace Steam;

public class User
{
    #region Public Fields

    public virtual ulong Id => _id.m_SteamID;

    public virtual string Name
    {
        get => Id != 0 && DGSteam.Initialized
            ? SteamFriends.GetFriendPersonaName(_id)
            : "";
    }

    public virtual byte[]? AvatarSmall
    {
        get => Id != 0 && DGSteam.Initialized
            ? avatarDataSmall ??= SteamHelper.GetImageRGBA(SteamFriends.GetSmallFriendAvatar(_id))
            : null;
    }

    public virtual byte[]? AvatarMedium
    {
        get => Id != 0 && DGSteam.Initialized
            ? avatarDataMedium ??= SteamHelper.GetImageRGBA(SteamFriends.GetMediumFriendAvatar(_id))
            : null;
    }

    public virtual bool InGame
    {
        get => Id != 0
            && DGSteam.Initialized
            && SteamFriends.GetFriendGamePlayed(_id, out _);
    }

    public virtual bool InCurrentGame
    {
        get => Id != 0
            && DGSteam.Initialized
            && SteamFriends.GetFriendGamePlayed(_id, out var game)
            && game.m_gameID.AppID() == SteamUtils.GetAppID();
    }

    protected virtual bool InLobby
    {
        get => Id != 0
            && DGSteam.Initialized
            && SteamFriends.GetFriendGamePlayed(_id, out var game)
            && game.m_steamIDLobby.m_SteamID != 0;
    }

    public virtual bool InCurrentLobby
    {
        get => Id != 0
            && DGSteam.Lobby != null
            && DGSteam.Initialized
            && SteamFriends.GetFriendGamePlayed(_id, out var game)
            && game.m_steamIDLobby.m_SteamID != DGSteam.Lobby.Id;
    }

    public virtual UserInfo Info => new()
    {
        InGame = InGame,
        InCurrentGame = InCurrentGame,
        InLobby = InLobby,
        InMyLobby = InCurrentLobby,
        State = State,
        Relationship = Relationship
    };

    public virtual SteamUserState State
    {
        get => Id != 0 && DGSteam.Initialized
            ? (SteamUserState)SteamFriends.GetFriendPersonaState(_id)
            : SteamUserState.Offline;
    }

    public virtual FriendRelationship Relationship
    {
        get => Id != 0 && DGSteam.Initialized
            ? (FriendRelationship)SteamFriends.GetFriendRelationship(_id)
            : FriendRelationship.None;
    }

    #endregion

    #region Private Fields

    CSteamID _id;

    byte[]? avatarDataSmall;
    byte[]? avatarDataMedium;

    static Dictionary<ulong, User>? users;

    #endregion

    #region Constructors

    User(ulong id)
        : this(new CSteamID(id)) { }

    internal User(CSteamID id)
    {
        _id = id;
    }

    #endregion

    public static User? GetUser(ulong id)
    {
        if (id == 0)
            return null;

        users ??= [];
        using Lock _lock = new(users);

        if (!users.TryGetValue(id, out User? user))
        {
            user = new User(id);
            users[id] = user;
        }

        return user;
    }

    internal static User? GetUser(CSteamID id)
    {
        return GetUser(id.m_SteamID);
    }
}