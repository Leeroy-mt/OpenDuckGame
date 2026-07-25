using Steamworks;

public class OfflineSelfUser : User
{
    #region Public Properties

    public override bool InGame => true;
    public override bool InCurrentGame => true;
    public override bool InCurrentLobby => false;

    public override ulong Id => 0;

    public override string Name => "UNKNOWN";

    public override SteamUserState State => SteamUserState.Offline;
    public override FriendRelationship Relationship => FriendRelationship.None;

    public override byte[] AvatarSmall => null!;
    public override byte[] AvatarMedium => null!;

    #endregion

    protected override bool InLobby => false;

    internal OfflineSelfUser()
        : base(new CSteamID())
    {
    }
}
