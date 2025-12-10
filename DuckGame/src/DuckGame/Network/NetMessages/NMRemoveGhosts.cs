using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMRemoveGhosts : NetMessage
{
    public List<NetIndex16> remove = new List<NetIndex16>();

    public new byte levelIndex;

    protected GhostManager _manager;

    public NMRemoveGhosts()
    {
        manager = BelongsToManager.GhostManager;
    }

    public NMRemoveGhosts(GhostManager pManager)
    {
        levelIndex = DuckNetwork.levelIndex;
        manager = BelongsToManager.GhostManager;
        _manager = pManager;
    }

    public override void CopyTo(NetMessage pMessage)
    {
        (pMessage as NMRemoveGhosts).remove = remove;
        (pMessage as NMRemoveGhosts).levelIndex = levelIndex;
        base.CopyTo(pMessage);
    }

    protected override void OnSerialize()
    {
        byte numToSend = (byte)Math.Min(32, _manager._destroyedGhosts.Count + _manager._destroyResends.Count);
        _serializedData.Write(levelIndex);
        _serializedData.Write(numToSend);
        int i = 0;
        while (i < _manager._destroyResends.Count && numToSend != 0)
        {
            _serializedData.Write((ushort)(int)_manager._destroyResends[i]);
            remove.Add(_manager._destroyResends[i]);
            _manager._destroyResends.RemoveAt(i);
            numToSend--;
            i--;
            i++;
        }
        int i2 = 0;
        while (i2 < _manager._destroyedGhosts.Count && numToSend != 0)
        {
            _serializedData.Write((ushort)(int)_manager._destroyedGhosts[i2].ghostObjectIndex);
            remove.Add(_manager._destroyedGhosts[i2].ghostObjectIndex);
            _manager._destroyedGhosts.RemoveAt(i2);
            numToSend--;
            i2--;
            i2++;
        }
    }

    public override void OnDeserialize(BitBuffer pData)
    {
        levelIndex = pData.ReadByte();
        ushort num = pData.ReadByte();
        for (int i = 0; i < num; i++)
        {
            remove.Add(pData.ReadUShort());
        }
    }
}
