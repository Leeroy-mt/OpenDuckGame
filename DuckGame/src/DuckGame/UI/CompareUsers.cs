using System.Collections.Generic;

namespace DuckGame;

public class CompareUsers : IComparer<UIInviteUser>
{
    public int Compare(UIInviteUser h1, UIInviteUser h2)
    {
        if (h1 == h2)
        {
            return 0;
        }
        int priority = 0;
        if (h1.inDuckGame)
        {
            priority -= 4;
        }
        else if (h1.inGame)
        {
            priority++;
        }
        if (h2.inDuckGame)
        {
            priority += 4;
        }
        else if (h2.inGame)
        {
            priority--;
        }
        if (h1.inMyLobby)
        {
            priority--;
        }
        if (h2.inMyLobby)
        {
            priority++;
        }
        if (h1.triedInvite)
        {
            priority -= 8;
        }
        if (h2.triedInvite)
        {
            priority += 8;
        }
        if (h1.state == SteamUserState.Online)
        {
            priority -= 2;
        }
        if (h2.state == SteamUserState.Online)
        {
            priority += 2;
        }
        return priority;
    }
}
