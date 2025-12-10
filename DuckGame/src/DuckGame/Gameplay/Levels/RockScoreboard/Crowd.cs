using System;
using System.Collections.Generic;

namespace DuckGame;

public class Crowd : Thing
{
    private class HelperChairDistanceSorter : IComparer<CrowdDuck>
    {
        int IComparer<CrowdDuck>.Compare(CrowdDuck a, CrowdDuck b)
        {
            return a.distVal - b.distVal;
        }
    }

    private static CrowdCore _core = new CrowdCore();

    public static int crowdSeed = 0;

    private static HelperChairDistanceSorter helperChairDistanceSorter = new HelperChairDistanceSorter();

    private static Dictionary<Profile, FanNum> fanList = new Dictionary<Profile, FanNum>();

    private static int extraFans;

    public static CrowdCore core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
    }

    public static Mood mood
    {
        get
        {
            return _core._mood;
        }
        set
        {
            _core._newMood = value;
        }
    }

    public static List<List<CrowdDuck>> _members => _core._members;

    public static int fansUsed
    {
        get
        {
            return _core.fansUsed;
        }
        set
        {
            _core.fansUsed = value;
        }
    }

    public static int totalFans
    {
        get
        {
            int fans = extraFans;
            foreach (KeyValuePair<Profile, FanNum> fan in fanList)
            {
                fans += fan.Value.totalFans;
            }
            return fans;
        }
    }

    public BitBuffer NetSerialize()
    {
        BitBuffer data = new BitBuffer();
        int emptyChairs = 0;
        foreach (List<CrowdDuck> member in _members)
        {
            foreach (CrowdDuck duck in member)
            {
                if (duck.empty)
                {
                    emptyChairs++;
                    continue;
                }
                if (emptyChairs > 0)
                {
                    data.Write(val: false);
                    data.Write((byte)emptyChairs);
                    emptyChairs = 0;
                }
                data.Write(val: true);
                data.WritePacked(duck.duckColor, 2);
                data.WritePacked(((duck.lastLoyalty != null) ? duck.lastLoyalty.networkIndex : (-1)) + 1, 3);
                data.WritePacked(((duck.loyalty != null) ? duck.loyalty.networkIndex : (-1)) + 1, 3);
                if (duck.loyalty != null || duck.lastLoyalty != null)
                {
                    data.Write(duck.loyal);
                }
            }
        }
        if (emptyChairs > 0)
        {
            data.Write(val: false);
            data.Write((byte)emptyChairs);
            emptyChairs = 0;
        }
        data.Write(crowdSeed);
        return data;
    }

    public void NetDeserialize(BitBuffer data)
    {
        foreach (List<CrowdDuck> member in _members)
        {
            foreach (CrowdDuck item in member)
            {
                Level.Remove(item);
            }
        }
        _members.Clear();
        int emptyChairs = 0;
        float ypos = -18f;
        float yOff = 2f;
        float zpos = -30f;
        for (int row = 0; row < 4; row++)
        {
            _members.Add(new List<CrowdDuck>());
            for (int i = 0; i < 45; i++)
            {
                Profile loyalty = null;
                Profile lastLoyalty = null;
                bool isLoyal = false;
                int color = 0;
                bool chairIsEmpty = false;
                if (emptyChairs == 0)
                {
                    if (!data.ReadBool())
                    {
                        emptyChairs = data.ReadByte();
                    }
                    else
                    {
                        color = (int)data.ReadBits(typeof(int), 2);
                        int lastLoyaltyIndex = (int)data.ReadBits(typeof(int), 3);
                        int loyaltyIndex = (int)data.ReadBits(typeof(int), 3);
                        if (lastLoyaltyIndex > 0 || loyaltyIndex > 0)
                        {
                            if (loyaltyIndex > 0)
                            {
                                loyalty = DuckNetwork.profiles[loyaltyIndex - 1];
                            }
                            if (lastLoyaltyIndex > 0)
                            {
                                lastLoyalty = DuckNetwork.profiles[lastLoyaltyIndex - 1];
                            }
                            isLoyal = data.ReadBool();
                        }
                    }
                }
                if (emptyChairs > 0)
                {
                    chairIsEmpty = true;
                    emptyChairs--;
                }
                int facing = ((i >= 9) ? ((i < 16) ? 1 : 2) : 0);
                CrowdDuck crowd = new CrowdDuck(-30 + i * 30 + 3, yOff + ypos, zpos, facing, row, i, (!chairIsEmpty) ? 1 : 0, loyalty, lastLoyalty, isLoyal, color);
                _members[row].Add(crowd);
            }
            zpos -= 20f;
            yOff -= 11f;
        }
        crowdSeed = data.ReadInt();
        foreach (List<CrowdDuck> member2 in _members)
        {
            foreach (CrowdDuck item2 in member2)
            {
                Level.Add(item2);
            }
        }
        InitSigns();
    }

    public static void GoHome()
    {
        _members.Clear();
        fansUsed = 0;
    }

    public static void ThrowHats(Profile p)
    {
        foreach (List<CrowdDuck> member in _members)
        {
            foreach (CrowdDuck item in member)
            {
                item.ThrowHat(p);
            }
        }
    }

    public static void InitializeCrowd()
    {
        if (Network.isClient)
        {
            return;
        }
        if (_members.Count == 0)
        {
            fanList.Clear();
            foreach (Profile p in Profiles.active)
            {
                if (p.slotType != SlotType.Spectator)
                {
                    fanList[p] = new FanNum
                    {
                        profile = p,
                        loyalFans = p.stats.loyalFans,
                        unloyalFans = p.stats.unloyalFans
                    };
                }
            }
            _ = Profile.totalFansThisGame;
            int max = (int)(20f + (float)Profile.totalFansThisGame * 0.1f);
            if (max > 36)
            {
                max = 36;
            }
            if (max < 0)
            {
                max = 0;
            }
            extraFans = Rando.Int(max / 2, max);
            float ypos = -18f;
            _members.Add(new List<CrowdDuck>());
            List<int> chairs = new List<int>();
            for (int i = 0; i < 45; i++)
            {
                chairs.Add(i);
            }
            chairs.Shuffle();
            foreach (int i2 in chairs)
            {
                int facing = ((i2 >= 9) ? ((i2 < 16) ? 1 : 2) : 0);
                CrowdDuck crowd = new CrowdDuck(-30 + i2 * 30 + 3, 2f + ypos, -30f, facing, 0, i2);
                _members[0].Add(crowd);
            }
            _members[0].Sort(helperChairDistanceSorter);
            _members.Add(new List<CrowdDuck>());
            chairs = new List<int>();
            for (int j = 0; j < 45; j++)
            {
                chairs.Add(j);
            }
            chairs.Shuffle();
            foreach (int i3 in chairs)
            {
                int facing2 = ((i3 >= 9) ? ((i3 < 16) ? 1 : 2) : 0);
                CrowdDuck crowd2 = new CrowdDuck(-30 + i3 * 30 + 3, -9f + ypos, -50f, facing2, 1, i3);
                _members[1].Add(crowd2);
            }
            _members[1].Sort(helperChairDistanceSorter);
            _members.Add(new List<CrowdDuck>());
            chairs = new List<int>();
            for (int k = 0; k < 45; k++)
            {
                chairs.Add(k);
            }
            chairs.Shuffle();
            foreach (int i4 in chairs)
            {
                int facing3 = ((i4 >= 9) ? ((i4 < 16) ? 1 : 2) : 0);
                CrowdDuck crowd3 = new CrowdDuck(-30 + i4 * 30 + 3, -20f + ypos, -70f, facing3, 2, i4);
                _members[2].Add(crowd3);
            }
            _members[2].Sort(helperChairDistanceSorter);
            _members.Add(new List<CrowdDuck>());
            chairs = new List<int>();
            for (int l = 0; l < 45; l++)
            {
                chairs.Add(l);
            }
            chairs.Shuffle();
            foreach (int i5 in chairs)
            {
                int facing4 = ((i5 >= 9) ? ((i5 < 16) ? 1 : 2) : 0);
                CrowdDuck crowd4 = new CrowdDuck(-30 + i5 * 30 + 3, -31f + ypos, -90f, facing4, 3, i5);
                _members[3].Add(crowd4);
            }
            _members[3].Sort(helperChairDistanceSorter);
        }
        if (Level.current is RockScoreboard)
        {
            foreach (List<CrowdDuck> member in _members)
            {
                foreach (CrowdDuck item in member)
                {
                    item.ClearActions();
                    Level.Add(item);
                }
            }
        }
        crowdSeed = Rando.Int(1999999);
        InitSigns();
    }

    public static void UpdateFans()
    {
        float totalScore = 0f;
        float lowestScore = 999f;
        float highestScore = -999f;
        List<float> scores = new List<float>();
        List<Profile> profiles = new List<Profile>();
        int ct = 0;
        foreach (Profile p in Profiles.active)
        {
            if (p.slotType != SlotType.Spectator)
            {
                ct++;
                float score = p.endOfRoundStats.CalculateProfileScore();
                if (score < lowestScore)
                {
                    lowestScore = score;
                }
                else if (score > highestScore)
                {
                    highestScore = score;
                }
                totalScore += score;
                scores.Add(score);
                profiles.Add(p);
            }
        }
        float averageScore = totalScore / (float)ct;
        foreach (List<CrowdDuck> member in _members)
        {
            foreach (CrowdDuck duck in member)
            {
                if (duck.empty)
                {
                    continue;
                }
                for (int i = 0; i < scores.Count; i++)
                {
                    float gainChance = scores[i] - averageScore;
                    if (gainChance > 0.5f)
                    {
                        gainChance = 0.5f;
                    }
                    if (gainChance < -0.5f)
                    {
                        gainChance = -0.5f;
                    }
                    duck.TryChangingAllegiance(profiles[i], gainChance);
                }
            }
        }
    }

    public static bool HasFansLeft()
    {
        foreach (KeyValuePair<Profile, FanNum> fan in fanList)
        {
            if (fan.Value.totalFans > 0)
            {
                return true;
            }
        }
        return extraFans > 0;
    }

    public static FanNum GetFan()
    {
        if (extraFans > 0 && Rando.Float(1f) > 0.5f)
        {
            extraFans--;
            return null;
        }
        List<FanNum> availableProfiles = new List<FanNum>();
        foreach (KeyValuePair<Profile, FanNum> pair in fanList)
        {
            if (pair.Value.totalFans > 0)
            {
                availableProfiles.Add(pair.Value);
            }
        }
        if (availableProfiles.Count == 0)
        {
            return null;
        }
        FanNum num = null;
        while (true)
        {
            num = availableProfiles[Rando.Int(availableProfiles.Count - 1)];
            if (availableProfiles.Count == 1)
            {
                break;
            }
            if (!((float)Math.Min(num.loyalFans, 100) / 100f * 0.5f + Rando.Float(0.5f) >= Rando.Float(1f)))
            {
                availableProfiles.Remove(num);
            }
        }
        Profile p = num.profile;
        if (num.loyalFans > 0 && ((num.unloyalFans == 0) | (Rando.Float(1f) > 0.3f)))
        {
            num.loyalFans--;
            return new FanNum
            {
                profile = p,
                loyalFans = 1
            };
        }
        num.unloyalFans--;
        return new FanNum
        {
            profile = p,
            unloyalFans = 1
        };
    }

    private static void InitSigns()
    {
        Random r = Rando.generator;
        Rando.generator = new Random(crowdSeed);
        for (int i = 0; i < 4; i++)
        {
            string text = "DUCK GAME";
            if (Rando.Int(10000) == 1)
            {
                text = "LOL";
            }
            else if (Rando.Int(100) == 1)
            {
                text = "WE LOVE IT";
            }
            else if (Rando.Int(20) == 1)
            {
                text = "LETS ROCK";
            }
            else if (Rando.Int(1000000) == 1)
            {
                text = "www.wonthelp.info";
            }
            Profile p = null;
            if (Rando.Float(1f) > 0.5f)
            {
                List<Team> winning = Teams.winning;
                if (winning.Count > 0)
                {
                    Team winteam = winning[Rando.Int(winning.Count - 1)];
                    Profile winpro = winteam.activeProfiles[Rando.Int(winteam.activeProfiles.Count - 1)];
                    text = ((!Profiles.IsDefault(winpro)) ? winpro.nameUI : winpro.team.name);
                    p = winpro;
                }
            }
            List<CrowdDuck> row = GetAvailableRow(text.Length, p, i);
            if (!(Rando.Float(1f) > 0.96f) || row == null)
            {
                continue;
            }
            int curLetter = 0;
            foreach (CrowdDuck item in row)
            {
                item.SetLetter(text.Substring(curLetter, 1), curLetter, hate: false, p);
                curLetter++;
            }
        }
        Rando.generator = r;
    }

    public override void Initialize()
    {
        base.Initialize();
        InitializeCrowd();
    }

    private static List<CrowdDuck> GetAvailableRow(int num, Profile p, int rowOnly = -1)
    {
        List<List<CrowdDuck>> rows = new List<List<CrowdDuck>>();
        int iRow = 0;
        if (rowOnly != -1)
        {
            iRow = rowOnly;
        }
        for (; iRow < ((rowOnly != -1) ? (rowOnly + 1) : 4); iRow++)
        {
            List<CrowdDuck> curRow = new List<CrowdDuck>();
            foreach (CrowdDuck d in _members[iRow])
            {
                if (!d.empty && !d.busy && (p == null || d.loyalty == p))
                {
                    curRow.Add(d);
                    continue;
                }
                if (curRow.Count >= num)
                {
                    rows.Add(curRow);
                }
                curRow = new List<CrowdDuck>();
            }
            if (curRow.Count >= num)
            {
                rows.Add(curRow);
            }
        }
        if (rows.Count > 0)
        {
            List<CrowdDuck> randomRow = rows[Rando.Int(rows.Count - 1)];
            if (randomRow.Count > num)
            {
                randomRow = randomRow.GetRange(Rando.Int(randomRow.Count - num), num);
            }
            return randomRow;
        }
        return null;
    }

    public override void Update()
    {
        if (_core._newMood != _core._mood)
        {
            _core._moodWait -= 0.15f;
            if (_core._moodWait < 0f)
            {
                _core._mood = _core._newMood;
                _core._moodWait = 1f;
            }
        }
    }
}
