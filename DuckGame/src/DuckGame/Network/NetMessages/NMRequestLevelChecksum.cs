using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMRequestLevelChecksum : NMConditionalEvent
{
    public string level;

    public uint checksum;

    public new int levelIndex;

    public NMRequestLevelChecksum()
    {
    }

    public NMRequestLevelChecksum(string pLevel, uint pChecksum, int pLevelIndex)
    {
        level = pLevel;
        checksum = pChecksum;
        levelIndex = pLevelIndex;
    }

    public override bool Update()
    {
        if (Level.current.networkIndex < levelIndex)
        {
            return Math.Abs(Level.current.networkIndex - levelIndex) > 100;
        }
        return true;
    }

    public override void Activate()
    {
        if (!(Level.current is GameLevel) || Level.current.networkIndex != levelIndex)
        {
            return;
        }
        List<LevelData> allLevels = Content.GetAllLevels(level);
        LevelData dat = null;
        foreach (LevelData l in allLevels)
        {
            if (l.GetChecksum() == checksum)
            {
                dat = l;
                break;
            }
        }
        dat = null;
        (Level.current as XMLLevel)._level = level;
        if (dat == null)
        {
            DuckNetwork.core.levelTransferSession++;
            DuckNetwork.core.compressedLevelData = null;
            DuckNetwork.core.levelTransferSize = 0;
            DuckNetwork.core.levelTransferProgress = 0;
            Send.Message(new NMClientNeedsLevelData(Level.current.networkIndex, DuckNetwork.core.levelTransferSession), base.connection);
        }
        else
        {
            (Level.current as GameLevel).data = dat;
            Level.current.waitingOnNewData = false;
        }
    }
}
