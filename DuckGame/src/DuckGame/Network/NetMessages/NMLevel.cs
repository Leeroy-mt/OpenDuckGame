using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class NMLevel : NMEvent
{
    public string level;

    public new byte levelIndex;

    public int seed;

    public bool needsChecksum;

    public uint checksum;

    private Level _level;

    private bool _connectionFailure;

    public NMLevel()
    {
        manager = BelongsToManager.EventManager;
    }

    public NMLevel(Level pLevel)
    {
        manager = BelongsToManager.EventManager;
        _level = pLevel;
        level = pLevel.networkIdentifier;
        levelIndex = (pLevel.networkIndex = DuckNetwork.levelIndex);
        if (pLevel is GameLevel)
        {
            GameLevel lev = pLevel as GameLevel;
            if (lev.clientLevel)
            {
                needsChecksum = true;
                checksum = 0u;
                pLevel.waitingOnNewData = true;
            }
            else if (lev.customLevel)
            {
                needsChecksum = true;
                checksum = lev.checksum;
                DuckNetwork.compressedLevelData = new MemoryStream(lev.compressedData, 0, lev.compressedData.Length, writable: false, publiclyVisible: true);
                DuckNetwork.compressedLevelName = lev.displayName;
            }
        }
        if (pLevel is XMLLevel)
        {
            seed = (pLevel as XMLLevel).seed;
        }
        _level.levelMessages.Add(this);
    }

    public override void CopyTo(NetMessage pMessage)
    {
        _level.levelMessages.Add(pMessage as NMLevel);
        (pMessage as NMLevel).levelIndex = levelIndex;
        (pMessage as NMLevel).level = level;
        (pMessage as NMLevel)._level = _level;
        (pMessage as NMLevel).needsChecksum = needsChecksum;
        (pMessage as NMLevel).seed = seed;
        base.CopyTo(pMessage);
    }

    public override bool MessageIsCompleted()
    {
        if (_connectionFailure)
        {
            return false;
        }
        if (_level.initialized || DuckNetwork.levelIndex != levelIndex)
        {
            DevConsole.Log(DCSection.DuckNet, "|DGORANGE|Loading new level.. (" + levelIndex + ").");
            return true;
        }
        return false;
    }

    public bool ChecksumsFinished()
    {
        if (!needsChecksum)
        {
            return true;
        }
        foreach (Profile p in DuckNetwork.profiles)
        {
            if (p.connection != null && p.connection != DuckNetwork.localConnection && !_level.HasChecksumReply(p.connection))
            {
                return false;
            }
        }
        return true;
    }

    public bool OnLevelLoaded()
    {
        if (DuckNetwork.levelIndex == levelIndex && ChecksumsFinished())
        {
            Level.current.things.RefreshState();
            GhostManager.context.RefreshGhosts();
            if (base.connection != null)
            {
                Send.Message(new NMLevelDataBegin(_level.networkIndex), base.connection);
                GhostManager.context.UpdateGhostSync(base.connection, pDelta: false, pSendMessages: true, NetMessagePriority.ReliableOrdered);
                Send.Message(new NMLevelData(_level), base.connection);
                _level.SendLevelData(base.connection);
            }
            return true;
        }
        return false;
    }

    public override void Activate()
    {
        if (DuckNetwork.status != DuckNetStatus.Connected)
        {
            if (!_connectionFailure)
            {
                Network.DisconnectClient(DuckNetwork.localConnection, new DuckNetErrorInfo(DuckNetError.ConnectionTimeout, "Game started during connection."));
                _connectionFailure = true;
            }
            return;
        }
        Network.ContextSwitch(levelIndex);
        if (level == "@TEAMSELECT")
        {
            _level = new TeamSelect2(pReturningFromGame: true);
        }
        else if (level == "@ROCKINTRO")
        {
            GameMode.numMatchesPlayed = 0;
            _level = new RockIntro(null);
        }
        else if (level == "@ROCKTHROW|SHOWSCORE")
        {
            _level = new RockScoreboard();
        }
        else if (level == "@ROCKTHROW|SHOWWINNER")
        {
            _level = new RockScoreboard(null, ScoreBoardMode.ShowWinner);
        }
        else if (level == "@ROCKTHROW|SHOWEND")
        {
            Graphics.fade = 0f;
            _level = new RockScoreboard(null, ScoreBoardMode.ShowWinner, afterHighlights: true);
        }
        else
        {
            if (needsChecksum)
            {
                GameLevel newLev = new GameLevel(level, seed);
                if (level.EndsWith(".client"))
                {
                    int clientIndex = 0;
                    try
                    {
                        clientIndex = Convert.ToInt32(level[0].ToString() ?? "");
                    }
                    catch (Exception)
                    {
                    }
                    int myIndex = DuckNetwork.GetProfiles(DuckNetwork.localConnection).First().networkIndex;
                    if (clientIndex == myIndex)
                    {
                        string customLevel = Deathmatch.RandomLevelString("", "deathmatch", forceCustom: true);
                        if (customLevel != "")
                        {
                            customLevel = customLevel.Substring(0, customLevel.Length - 7);
                            LevelData lev = Content.GetLevel(customLevel);
                            uint checksum = lev.GetChecksum();
                            byte[] compressedData = XMLLevel.GetCompressedLevelData(lev, customLevel);
                            DuckNetwork.compressedLevelData = new MemoryStream(compressedData, 0, compressedData.Length, writable: true, publiclyVisible: true);
                            DuckNetwork.levelIndex = levelIndex;
                            Send.Message(new NMRequestLevelChecksum(customLevel, checksum, levelIndex));
                            newLev.data = lev;
                            newLev.waitingOnNewData = false;
                            DuckNetwork.compressedLevelName = newLev.displayName;
                        }
                    }
                    else
                    {
                        newLev.waitingOnNewData = true;
                        newLev.networkIndex = levelIndex;
                    }
                    _level = newLev;
                }
                else
                {
                    List<LevelData> allLevels = Content.GetAllLevels(level);
                    LevelData dat = null;
                    foreach (LevelData l in allLevels)
                    {
                        if (l.GetChecksum() == this.checksum)
                        {
                            dat = l;
                            break;
                        }
                    }
                    if (dat == null || NetworkDebugger.enabled)
                    {
                        DuckNetwork.core.levelTransferSession++;
                        DuckNetwork.core.compressedLevelData = null;
                        DuckNetwork.core.levelTransferSize = 0;
                        DuckNetwork.core.levelTransferProgress = 0;
                        Send.Message(new NMClientNeedsLevelData(levelIndex, DuckNetwork.core.levelTransferSession), base.connection);
                        newLev.waitingOnNewData = true;
                        newLev.networkIndex = levelIndex;
                    }
                    else
                    {
                        newLev.data = dat;
                        Send.Message(new NMLevelFileReady(levelIndex), base.connection);
                    }
                    _level = newLev;
                }
            }
            else
            {
                _level = new GameLevel(level, seed);
            }
            if (_level != null && _level is XMLLevel)
            {
                (_level as XMLLevel).seed = seed;
            }
        }
        if (Network.InLobby() || (Level.current is RockScoreboard && !(_level is RockScoreboard)))
        {
            Music.Stop();
        }
        Level.current = _level;
        _level.transferCompleteCalled = false;
        _level.networkIndex = levelIndex;
    }

    public override string ToString()
    {
        string s = base.ToString();
        if (level != null)
        {
            s = s + "(" + level + ", " + seed + ")";
        }
        return s;
    }
}
