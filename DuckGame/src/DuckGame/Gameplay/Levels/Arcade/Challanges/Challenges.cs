using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class Challenges
{
    #region Public Fields

    public static int valueBronze = 15;

    public static int valueSilver = 5;

    public static int valueGold = 5;

    public static int valuePlatinum = 12;

    #endregion

    #region Private Fields

    static Dictionary<string, ChallengeData> _challenges = [];

    static List<ChallengeData> _challengesInArcade;

    #endregion

    #region Public Properties

    public static List<ChallengeData> challengesInArcade
    {
        get
        {
            if (_challengesInArcade == null)
            {
                _challengesInArcade = [];
                ArcadeLevel arcade = new(Content.GetLevelID("arcade"))
                {
                    bareInitialize = true
                };
                arcade.InitializeMachines();

                if (arcade != null)
                {
                    foreach (ArcadeMachine m in arcade._challenges)
                    {
                        if (m is ImportMachine)
                            continue;

                        foreach (string challenge in m.data.challenges)
                        {
                            if (GetChallenge(challenge) is ChallengeData d)
                                challengesInArcade.Add(d);
                        }
                    }

                    foreach (ChallengeData dat in GetAllChancyChallenges(_challengesInArcade))
                        _challengesInArcade.Add(dat);
                }
            }
            return _challengesInArcade;
        }
    }

    public static Dictionary<string, ChallengeData> challenges => _challenges;

    #endregion

    #region Public Methods

    public static ChallengeData LoadChallengeData(string pLevel)
    {
        if (pLevel == null)
            return null;

        if (_challenges.TryGetValue(pLevel, out var data))
            return data;

        LevelData doc = Content.GetLevel(pLevel)
                     ?? DuckFile.LoadLevel(pLevel);

        if (doc != null)
        {
            var guid = doc.metaData.guid;
            foreach (BinaryClassChunk elly in doc.objects.objects)
            {
                var typeString = elly.GetProperty<string>("type");
                ChallengeMode m = null;
                try
                {
                    if (typeString != null && typeString.Contains("DuckGame.ChallengeMode,"))
                        m = Thing.LoadThing(elly, chance: false) as ChallengeMode;
                    else if (typeString != null && typeString.Contains("DuckGame.ChallengeModeNew,"))
                        m = Thing.LoadThing(elly, chance: false) as ChallengeModeNew;

                    if (m != null)
                    {
                        m.challenge.fileName = pLevel;
                        m.challenge.levelID = guid;
                        m.challenge.preview = doc.previewData.preview;
                        _challenges.Add(doc.metaData.guid, m.challenge);
                        return m.challenge;
                    }
                }
                catch
                {
                }
            }
        }
        return null;
    }

    public static void Initialize()
    {
        foreach (var challenge in Content.GetLevels("challenge", LevelLocation.Content, pRecursive: true, pOnline: false, pEightPlayer: false))
            LoadChallengeData(challenge);
    }

    public static void LoadElementsFromNode(string pName, DXMLNode pNode)
    {
        foreach (DXMLNode e in pNode.Elements("challengeSaveData"))
        {
            ChallengeSaveData dat = new();
            dat.LegacyDeserialize(e);

            if (dat.trophy == TrophyType.Developer)
                Options.Data.gotDevMedal = true;

            dat.challenge = pName;
            var profile = Profiles.Get(dat.profileID);
            if (profile != null && !profile.challengeData.ContainsKey(pName))
            {
                profile.challengeData.Add(pName, dat);
                dat.profileID = profile.id;
            }
        }
    }

    public static void InitializeChallengeData()
    {
        var files = DuckFile.GetFiles(DuckFile.challengeDirectory);
        foreach (string save in files)
        {
            DuckXML doc = DuckFile.LoadDuckXML(save);
            if (doc != null)
            {
                string name = Path.GetFileNameWithoutExtension(save);
                var dataNode = doc.Element("Data");
                if (dataNode != null)
                    LoadElementsFromNode(name, dataNode);
            }
        }
    }

    public static int GetNumTrophies(Profile p)
    {
        var trophies = 0;
        foreach (var challenge in _challenges)
        {
            var dat = p.GetSaveData(challenge.Value.levelID, canBeNull: true);
            if (dat != null && dat.trophy != TrophyType.Baseline)
                trophies++;
        }
        return trophies;
    }

    public static ChallengeSaveData GetSaveData(string guid, Profile p, bool canBeNull = false)
    {
        return p.GetSaveData(guid, canBeNull);
    }

    public static List<ChallengeSaveData> GetAllSaveData(Profile p)
    {
        List<ChallengeSaveData> datList = [];
        foreach (var challengeDatum in p.challengeData)
            datList.Add(challengeDatum.Value);
        return datList;
    }

    public static List<ChallengeSaveData> GetAllSaveData()
    {
        List<ChallengeSaveData> datList = [];
        foreach (Profile item in Profiles.all)
        {
            foreach (KeyValuePair<string, ChallengeSaveData> challengeDatum in item.challengeData)
                datList.Add(challengeDatum.Value);
        }
        return datList;
    }

    public static ChallengeData GetChallenge(string name)
    {
        if (!_challenges.TryGetValue(name, out var dat))
            return LoadChallengeData(name);
        return dat;
    }

    public static List<ChallengeData> GetEligibleChancyChallenges(Profile p)
    {
        List<ChallengeData> data = [];
        foreach (var d in challenges)
        {
            if (d.Value.requirement == "" || !d.Value.CheckRequirement(p))
                continue;

            if (d.Value.prevchal != "")
            {
                var dat = GetChallenge(d.Value.prevchal);
                var save = p.GetSaveData(dat.levelID, canBeNull: true);

                if (save != null && save.trophy > TrophyType.Baseline)
                    data.Add(d.Value);
            }
            else
            {
                data.Add(d.Value);
            }
        }
        return data;
    }

    public static List<ChallengeData> GetAllChancyChallenges(List<ChallengeData> available = null)
    {
        List<ChallengeData> data = [];
        foreach (var d in challenges)
        {
            if (d.Value.requirement != "" &&
                (d.Value.prevchal == null
                || d.Value.prevchal == ""
                || available == null
                || available.FirstOrDefault(x => x != null && x.fileName == d.Value.prevchal) != null)
                )
                data.Add(d.Value);
        }
        return data;
    }

    public static List<ChallengeData> GetEligibleIncompleteChancyChallenges(Profile p)
    {
        var eligibleChancyChallenges = GetEligibleChancyChallenges(p);
        List<ChallengeData> incomplete = [];
        foreach (var d in eligibleChancyChallenges)
        {
            var s = p.GetSaveData(d.levelID, canBeNull: true);
            if (s == null || s.trophy < TrophyType.Bronze)
                incomplete.Add(d);
        }
        return incomplete;
    }

    public static float GetChallengeSkillIndex()
    {
        var val = 0;
        var max = 0;
        List<ChallengeData> challengesInArcade = [];
        var arcade = Level.current as ArcadeLevel ?? ArcadeLevel.currentArcade;
        if (arcade != null)
        {
            foreach (var challenge in arcade._challenges)
            {
                foreach (var s in challenge.data.challenges)
                    challengesInArcade.Add(GetChallenge(s));
            }

            foreach (var dat in GetAllChancyChallenges(challengesInArcade))
                challengesInArcade.Add(dat);

            foreach (var dat2 in _challenges)
            {
                if (challengesInArcade.Contains(dat2.Value))
                {
                    max += 4;
                    var saveDat = Profiles.active[0].GetSaveData(dat2.Value.levelID, canBeNull: true);
                    if (saveDat != null)
                        val = (int)(val + saveDat.trophy);
                }
            }

            return val / (float)max;
        }

        return 0;
    }

    public static int GetTicketCount(Profile p)
    {
        var max = 0;
        foreach (var dat in _challenges)
        {
            if (!challengesInArcade.Contains(dat.Value))
                continue;

            var saveDat = p.GetSaveData(dat.Value.levelID, canBeNull: true);
            if (saveDat != null)
            {
                if (saveDat.trophy >= TrophyType.Bronze)
                    max += valueBronze;

                if (saveDat.trophy >= TrophyType.Silver)
                    max += valueSilver;

                if (saveDat.trophy >= TrophyType.Gold)
                    max += valueGold;

                if (saveDat.trophy >= TrophyType.Platinum)
                    max += valuePlatinum;
            }
        }

        foreach (var dat2 in Unlocks.GetUnlocks(UnlockType.Any))
        {
            if (dat2.ProfileUnlocked(p))
                max -= dat2.cost;
        }

        return max;
    }

    #endregion
}