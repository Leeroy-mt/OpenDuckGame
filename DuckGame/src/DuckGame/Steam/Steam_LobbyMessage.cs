using System;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class Steam_LobbyMessage
{
    public const string M_CommunicationFailure = "COM_FAIL";

    public const string M_ImOuttaHere = "IM_OUTTAHERE";

#if FACEPUNCH
    public Friend from;
    public Friend context;
#else
    public User from;
    public User context;
#endif

    public string message;

    private static long kLobbyMessageID = 10968107910803936L;

#if FACEPUNCH
    public static void Send(string pMessage, Friend pContext)
#else
    public static void Send(string pMessage, User pContext)
#endif
    {
        BitBuffer message = new BitBuffer(allowPacking: false);
        message.Write(kLobbyMessageID);
#if FACEPUNCH
        if (pContext.Id != 0)
#else
        if (pContext != null)
#endif
            message.Write(pContext.Id);
        else
            message.Write(0uL);
        message.Write(pMessage);
#if FACEPUNCH
        Network.activeNetwork.core.lobby.SendChatBytes(message.buffer);
#else
        DGSteam.SendLobbyMessage(Network.activeNetwork.core.lobby, message.buffer, (uint)message.lengthInBytes);
#endif
    }

#if FACEPUNCH
    public static Steam_LobbyMessage Receive(Friend pFrom, byte[] pData)
    {
        try
        {
            Steam_LobbyMessage message = new Steam_LobbyMessage();
            BitBuffer b = new BitBuffer(pData, copyData: false);
            if (b.ReadLong() == kLobbyMessageID)
            {
                message.from = pFrom;
                ulong userID = b.ReadULong();
                if (userID != 0L)
                    message.context = new(userID);
                message.message = b.ReadString();
            }
            return message;
        }
        catch (Exception)
        {
            return null;
        }
    }
#else
    public static Steam_LobbyMessage Receive(User pFrom, byte[] pData)
    {
        try
        {
            Steam_LobbyMessage message = new Steam_LobbyMessage();
            BitBuffer b = new BitBuffer(pData, copyData: false);
            if (b.ReadLong() == kLobbyMessageID)
            {
                message.from = pFrom;
                ulong userID = b.ReadULong();
                if (userID != 0L)
                    message.context = User.GetUser(userID);
                message.message = b.ReadString();
            }
            return message;
        }
        catch (Exception)
        {
            return null;
        }
    }
#endif
}
