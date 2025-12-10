using System;

namespace DuckGame;

public class Steam_LobbyMessage
{
    public const string M_CommunicationFailure = "COM_FAIL";

    public const string M_ImOuttaHere = "IM_OUTTAHERE";

    public User from;

    public User context;

    public string message;

    private static long kLobbyMessageID = 10968107910803936L;

    public static void Send(string pMessage, User pContext)
    {
        BitBuffer message = new BitBuffer(allowPacking: false);
        message.Write(kLobbyMessageID);
        if (pContext != null)
        {
            message.Write(pContext.id);
        }
        else
        {
            message.Write(0uL);
        }
        message.Write(pMessage);
        Steam.SendLobbyMessage(Network.activeNetwork.core.lobby, message.buffer, (uint)message.lengthInBytes);
    }

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
                {
                    message.context = User.GetUser(userID);
                }
                message.message = b.ReadString();
            }
            return message;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
