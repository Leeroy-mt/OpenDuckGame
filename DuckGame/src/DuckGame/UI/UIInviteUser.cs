#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class UIInviteUser
{
    public bool inGame;

    public bool inDuckGame;

    public bool inMyLobby;

    public bool triedInvite;

    public string name;

#if FACEPUNCH
    public FriendState state;
#else
    public SteamUserState state;
#endif

    public Sprite sprite;

#if FACEPUNCH
    public Friend user;
#else
    public User user;
#endif
}
