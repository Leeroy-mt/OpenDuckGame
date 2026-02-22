using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class ProfilesCore
{
    public List<Profile> defaultProfileMappings = new List<Profile> { null, null, null, null, null, null, null, null };

    public List<Profile> _profiles;

    private Profile _experienceProfile;

    public Team EnvironmentTeam = new Team("Environment", "hats/noHat", demo: true);

    public Profile EnvironmentProfile;

    public bool initialized;

    private static int numExperienceProfiles;

    public IEnumerable<Profile> all
    {
        get
        {
            if (DuckNetwork.active)
            {
                return DuckNetwork.profiles;
            }
            return _profiles;
        }
    }

    public List<Profile> allCustomProfiles
    {
        get
        {
            List<Profile> p = new List<Profile>();
            for (int i = 8; i < _profiles.Count; i++)
            {
                if (_profiles[i].steamID == 0L || _profiles[i] == Profiles.experienceProfile)
                {
                    p.Add(_profiles[i]);
                }
            }
            return p;
        }
    }

    public IEnumerable<Profile> universalProfileList
    {
        get
        {
            List<Profile> list = new List<Profile>(_profiles);
            list.AddRange(DuckNetwork.profiles);
            return list;
        }
    }

    public Profile DefaultExperienceProfile => _experienceProfile;

    public Profile DefaultPlayer1
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[0];
            }
            return all.ElementAt(0);
        }
    }

    public Profile DefaultPlayer2
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[1];
            }
            return all.ElementAt(1);
        }
    }

    public Profile DefaultPlayer3
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[2];
            }
            return all.ElementAt(2);
        }
    }

    public Profile DefaultPlayer4
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[3];
            }
            return all.ElementAt(3);
        }
    }

    public Profile DefaultPlayer5
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[4];
            }
            return all.ElementAt(4);
        }
    }

    public Profile DefaultPlayer6
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[5];
            }
            return all.ElementAt(5);
        }
    }

    public Profile DefaultPlayer7
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[6];
            }
            return all.ElementAt(6);
        }
    }

    public Profile DefaultPlayer8
    {
        get
        {
            if (!Network.isActive)
            {
                return Profile.defaultProfileMappings[7];
            }
            return all.ElementAt(7);
        }
    }

    public List<Profile> active
    {
        get
        {
            List<Profile> profiles = new List<Profile>();
            foreach (Profile p in Profiles.all ?? profiles)
            {
                if (p.team != null)
                {
                    profiles.Add(p);
                }
            }
            return profiles;
        }
    }

    public List<Profile> activeNonSpectators
    {
        get
        {
            List<Profile> profiles = new List<Profile>();
            foreach (Profile p in Profiles.all)
            {
                if (p.team != null && p.slotType != SlotType.Spectator)
                {
                    profiles.Add(p);
                }
            }
            return profiles;
        }
    }

    public ProfilesCore()
    {
        EnvironmentProfile = new Profile("Environment", InputProfile.Get("Blank"), EnvironmentTeam, Persona.Duck1);
    }

    public int DefaultProfileNumber(Profile p)
    {
        return _profiles.IndexOf(p);
    }

    public void Initialize()
    {
        _profiles = new List<Profile>
        {
            new Profile("Player1", InputProfile.Get("MPPlayer1"), Teams.Player1, Persona.Duck1, network: false, "PLAYER1", pDefaultProfile: true),
            new Profile("Player2", InputProfile.Get("MPPlayer2"), Teams.Player2, Persona.Duck2, network: false, "PLAYER2", pDefaultProfile: true),
            new Profile("Player3", InputProfile.Get("MPPlayer3"), Teams.Player3, Persona.Duck3, network: false, "PLAYER3", pDefaultProfile: true),
            new Profile("Player4", InputProfile.Get("MPPlayer4"), Teams.Player4, Persona.Duck4, network: false, "PLAYER4", pDefaultProfile: true),
            new Profile("Player5", InputProfile.Get("MPPlayer5"), Teams.Player5, Persona.Duck5, network: false, "PLAYER5", pDefaultProfile: true),
            new Profile("Player6", InputProfile.Get("MPPlayer6"), Teams.Player6, Persona.Duck6, network: false, "PLAYER6", pDefaultProfile: true),
            new Profile("Player7", InputProfile.Get("MPPlayer7"), Teams.Player7, Persona.Duck7, network: false, "PLAYER7", pDefaultProfile: true),
            new Profile("Player8", InputProfile.Get("MPPlayer8"), Teams.Player8, Persona.Duck8, network: false, "PLAYER8", pDefaultProfile: true)
        };
        Profile.defaultProfileMappings[0] = _profiles[0];
        Profile.defaultProfileMappings[1] = _profiles[1];
        Profile.defaultProfileMappings[2] = _profiles[2];
        Profile.defaultProfileMappings[3] = _profiles[3];
        Profile.defaultProfileMappings[4] = _profiles[4];
        Profile.defaultProfileMappings[5] = _profiles[5];
        Profile.defaultProfileMappings[6] = _profiles[6];
        Profile.defaultProfileMappings[7] = _profiles[7];
        Profile.loading = true;
        DevConsole.Log(DCSection.General, "Loading profiles from (" + DuckFile.profileDirectory + ")");
        string[] files = DuckFile.GetFiles(DuckFile.profileDirectory);
        DevConsole.Log(DCSection.General, "Found (" + files.Count() + ") profiles.");
        List<Profile> steamProfiles = new List<Profile>();
        string[] array = files;
        foreach (string file in array)
        {
            if (file.Contains("__backup_"))
            {
                continue;
            }
            DuckXML doc = DuckFile.LoadDuckXML(file);
            if (doc == null || doc.invalid || doc.Element("Profile") == null)
            {
                continue;
            }
            string name = doc.Element("Profile").Element("Name").Value;
            DXMLNode steamIDElement = doc.Element("Profile").Element("SteamID");
            ulong steamIDValue = 0uL;
            try
            {
                if (steamIDElement != null)
                {
                    try
                    {
                        steamIDValue = Change.ToUInt64(steamIDElement.Value.Trim());
                    }
                    catch (Exception)
                    {
                        steamIDValue = 0uL;
                    }
                    if (steamIDValue != 0L && Path.GetFileNameWithoutExtension(file) != steamIDValue.ToString())
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
            }
            bool needAdd = false;
            Profile p = _profiles.FirstOrDefault((Profile profile) => profile.name == name);
            if (p == null || !Profiles.IsDefault(p))
            {
                p = new Profile("");
                p.fileName = file;
                needAdd = true;
            }
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile().Load(" + file + ")");
            }
            IEnumerable<DXMLNode> root = doc.Elements("Profile");
            if (root != null)
            {
                foreach (DXMLNode element in root.Elements())
                {
                    if (element.Name == "ID" && !Profiles.IsDefault(p))
                    {
                        p.SetID(element.Value);
                    }
                    else if (element.Name == "Name")
                    {
                        p.name = element.Value;
                    }
                    else if (element.Name == "Mood")
                    {
                        p.funslider = Change.ToSingle(element.Value);
                    }
                    else if (element.Name == "PreferredColor" && !Profiles.IsDefault(p))
                    {
                        p.preferredColor = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "NS")
                    {
                        p.numSandwiches = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "MF")
                    {
                        p.milkFill = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "LML")
                    {
                        p.littleManLevel = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "NLM")
                    {
                        p.numLittleMen = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "LMB")
                    {
                        p.littleManBucks = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "RSXP")
                    {
                        p.roundsSinceXP = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimesMet")
                    {
                        p.timesMetVincent = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimesMet2")
                    {
                        p.timesMetVincentSale = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimesMet3")
                    {
                        p.timesMetVincentSell = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimesMet4")
                    {
                        p.timesMetVincentImport = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimesMet5")
                    {
                        p.timesMetVincentHint = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "TimeOfDay")
                    {
                        p.timeOfDay = Change.ToSingle(element.Value);
                    }
                    else if (element.Name == "CD")
                    {
                        p.currentDay = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "Punished")
                    {
                        p.punished = Change.ToInt32(element.Value);
                    }
                    else if (element.Name == "XtraPoints")
                    {
                        p.xp = Change.ToInt32(element.Value);
                        if (MonoMain.logFileOperations)
                        {
                            DevConsole.Log(DCSection.General, ("Profile(" + name != null) ? name : (").loadXP(" + p.xp + ")"));
                        }
                    }
                    else if (element.Name == "FurniPositions")
                    {
                        p.furniturePositionData = BitBuffer.FromString(element.Value);
                        p.prevFurniPositionData = p.furniturePositionData.ToString();
                    }
                    else if (element.Name == "Fowner")
                    {
                        p.furnitureOwnershipData = BitBuffer.FromString(element.Value);
                    }
                    else if (element.Name == "SteamID")
                    {
                        try
                        {
                            p.steamID = Change.ToUInt64(element.Value.Trim());
                        }
                        catch (Exception)
                        {
                            p.steamID = 0uL;
                        }
                        if (p.steamID != 0L && !(Path.GetFileNameWithoutExtension(file) != p.steamID.ToString()))
                        {
                            steamProfiles.Add(p);
                        }
                    }
                    else if (element.Name == "LastKnownName")
                    {
                        p.lastKnownName = element.Value;
                    }
                    else if (element.Name == "Stats")
                    {
                        p.stats.Deserialize(element);
                    }
                    else if (element.Name == "Unlocks")
                    {
                        string[] val = element.Value.Split('|');
                        int numToRead = Math.Min(val.Length, 100);
                        for (int i2 = 0; i2 < numToRead; i2++)
                        {
                            if (val[i2] != "" && !p.unlocks.Contains(val[i2]))
                            {
                                p.unlocks.Add(val[i2]);
                            }
                        }
                    }
                    else if (element.Name == "Tickets")
                    {
                        p.ticketCount = Convert.ToInt32(element.Value);
                    }
                    else if (element.Name == "ChallengeData")
                    {
                        try
                        {
                            byte[] data = Editor.StringToBytes(element.Value);
                            BitBuffer b = new BitBuffer(data, copyData: false);
                            b.lengthInBytes = data.Length;
                            while (b.position < b.lengthInBytes)
                            {
                                ChallengeSaveData s = ChallengeSaveData.FromBuffer(b.ReadBitBuffer(allowPacking: false));
                                if (s.trophy != TrophyType.Baseline || s.bestTime != 0)
                                {
                                    s.profileID = p.id;
                                    if (s.trophy == TrophyType.Developer)
                                    {
                                        Options.Data.gotDevMedal = true;
                                    }
                                    p.challengeData.Add(s.challenge, s);
                                }
                            }
                        }
                        catch (Exception ex4)
                        {
                            DevConsole.Log(DCSection.General, "Profile (" + file + ") failed to load ChallengeData:" + ex4.Message);
                        }
                    }
                    else
                    {
                        if (!(element.Name == "Mappings") || MonoMain.defaultControls)
                        {
                            continue;
                        }
                        p.inputMappingOverrides.Clear();
                        foreach (DXMLNode map in element.Elements())
                        {
                            if (!(map.Name == "InputMapping"))
                            {
                                continue;
                            }
                            DeviceInputMapping mapping = new DeviceInputMapping();
                            mapping.Deserialize(map);
                            try
                            {
                                DeviceInputMapping defaultMapping = Input.GetDefaultMapping(mapping.deviceName, mapping.deviceGUID);
                                if (defaultMapping != null)
                                {
                                    foreach (KeyValuePair<string, int> pair in defaultMapping.map)
                                    {
                                        if (!mapping.map.ContainsKey(pair.Key))
                                        {
                                            mapping.MapInput(pair.Key, pair.Value);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            p.inputMappingOverrides.Add(mapping);
                        }
                    }
                }
            }
            if (needAdd)
            {
                _profiles.Add(p);
            }
        }
        Profile experience_profile = null;
        ulong experienceProfileID = 0uL;
        numExperienceProfiles = 0;
        if (Steam.user == null)
        {
            experience_profile = Profiles.DefaultPlayer1;
        }
        else
        {
            if (Steam.user != null && Steam.user.id != 0L)
            {
                Options.Data.lastSteamID = Steam.user.id;
            }
            experienceProfileID = Options.Data.lastSteamID;
            foreach (Profile pro in Profiles.all)
            {
                if (pro.steamID != 0L)
                {
                    numExperienceProfiles++;
                }
                if (pro.steamID == experienceProfileID)
                {
                    experience_profile = pro;
                }
            }
        }
        if (numExperienceProfiles == 0)
        {
            Options.Data.defaultAccountMerged = true;
            Options.Data.didAutoMerge = true;
        }
        string temporary_account_delete = null;
        if (experience_profile == null)
        {
            if (experienceProfileID != 0L)
            {
                Profile temporaryAccount = _profiles.FirstOrDefault((Profile x) => x.name == "experience_profile" && x.id == "replace_with_steam");
                if (temporaryAccount != null)
                {
                    foreach (KeyValuePair<string, ChallengeSaveData> challengeDatum in temporaryAccount.challengeData)
                    {
                        challengeDatum.Value.profileID = experienceProfileID.ToString();
                    }
                    temporaryAccount.name = experienceProfileID.ToString();
                    temporaryAccount.SetID(experienceProfileID.ToString());
                    temporaryAccount.steamID = experienceProfileID;
                    experience_profile = temporaryAccount;
                    temporary_account_delete = temporaryAccount.fileName;
                }
                else
                {
                    experience_profile = new Profile(experienceProfileID.ToString(), null, null, null, network: false, experienceProfileID.ToString());
                    experience_profile.steamID = experienceProfileID;
                    numExperienceProfiles++;
                }
            }
            else
            {
                experience_profile = new Profile("experience_profile", null, null, null, network: false, "replace_with_steam");
            }
            if (!Profiles.all.Contains(experience_profile))
            {
                Profiles.Add(experience_profile);
            }
            Save(experience_profile);
            if (temporary_account_delete != null)
            {
                DuckFile.Delete(temporary_account_delete);
            }
        }
        _experienceProfile = experience_profile;
        _experienceProfile.defaultTeam = Teams.Player1;
        Profile.defaultProfileMappings[0] = _experienceProfile;
        if (Options.legacyPreferredColor > 0)
        {
            _experienceProfile.preferredColor = Options.legacyPreferredColor;
        }
        byte flippers = Profile.CalculateLocalFlippers();
        foreach (Profile p2 in _profiles)
        {
            p2.flippers = flippers;
            p2.ticketCount = Challenges.GetTicketCount(p2);
            if (p2.ticketCount < 0)
            {
                p2.ticketCount = 0;
            }
        }
        Profile.loading = false;
        initialized = true;
    }

    public static int CouldAutomerge()
    {
        if (numExperienceProfiles == 1)
        {
            bool hadData1 = false;
            foreach (KeyValuePair<string, ChallengeData> pair in Challenges.challenges)
            {
                if (Profiles.experienceProfile.GetSaveData(pair.Key).trophy != TrophyType.Baseline)
                {
                    hadData1 = true;
                    break;
                }
            }
            bool hadData2 = false;
            foreach (KeyValuePair<string, ChallengeData> pair2 in Challenges.challenges)
            {
                if (Profiles.DefaultPlayer1.GetSaveData(pair2.Key).trophy != TrophyType.Baseline)
                {
                    hadData2 = true;
                    break;
                }
            }
            if ((hadData1 || hadData2) && !(hadData1 && hadData2))
            {
                if (!hadData2)
                {
                    return 2;
                }
                return 1;
            }
        }
        return 0;
    }

    public static void TryAutomerge()
    {
        int could = CouldAutomerge();
        if (could > 0 && !Options.Data.didAutoMerge)
        {
            Options.MergeDefault(could == 1, pShowDialog: false);
            Options.Data.didAutoMerge = true;
        }
    }

    public List<ProfileStatRank> GetEndOfRoundStatRankings(StatInfo stat)
    {
        List<ProfileStatRank> rankings = new List<ProfileStatRank>();
        foreach (Profile p in active)
        {
            float val = p.endOfRoundStats.GetStatCalculation(stat);
            bool found = false;
            for (int i = 0; i < rankings.Count; i++)
            {
                if (val > rankings[i].value)
                {
                    rankings.Insert(i, new ProfileStatRank(stat, val, p));
                    found = true;
                    break;
                }
                if (val == rankings[i].value)
                {
                    rankings[i].profiles.Add(p);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                rankings.Add(new ProfileStatRank(stat, val, p));
            }
        }
        return rankings;
    }

    public bool IsDefault(Profile p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.linkedProfile != null)
        {
            return IsDefault(p.linkedProfile);
        }
        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            if (_profiles[i] == p)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsExperience(Profile p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.linkedProfile != null)
        {
            return IsExperience(p.linkedProfile);
        }
        return p == Profiles.experienceProfile;
    }

    public bool IsDefaultName(string p)
    {
        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            if (_profiles[i].name == p)
            {
                return true;
            }
        }
        return false;
    }

    public void Add(Profile p)
    {
        _profiles.Add(p);
        Save(p);
    }

    public void Remove(Profile p)
    {
        _profiles.Remove(p);
    }

    public void Delete(Profile p)
    {
        _profiles.Remove(p);
        DuckFile.Delete(GetFileName(p));
    }

    public string GetFileName(Profile p)
    {
        if (p == EnvironmentProfile)
        {
            return null;
        }
        if (p.linkedProfile != null)
        {
            return GetFileName(p.linkedProfile);
        }
        if (p.isNetworkProfile)
        {
            return null;
        }
        if (p.fileName != null)
        {
            return p.fileName;
        }
        string formattedName = p.name;
        if (p.steamID != 0L)
        {
            if (Steam.user == null || p.steamID != DG.localID)
            {
                return null;
            }
            formattedName = p.steamID.ToString();
        }
        return DuckFile.profileDirectory + DuckFile.ReplaceInvalidCharacters(formattedName) + ".pro";
    }

    public void Save(Profile p, string pPrepend = null)
    {
        if (NetworkDebugger.enabled || p == EnvironmentProfile)
        {
            return;
        }
        if (p.linkedProfile != null)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile.Save() saving linkedProfile");
            }
            Save(p.linkedProfile, pPrepend);
        }
        else
        {
            if (p.isNetworkProfile)
            {
                return;
            }
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile.Save(" + p.name + ")");
            }
            DuckXML doc = new DuckXML();
            DXMLNode profile = new DXMLNode("Profile");
            DXMLNode name = new DXMLNode("Name", p.formattedName);
            profile.Add(name);
            DXMLNode id = new DXMLNode("ID", p.id);
            profile.Add(id);
            DXMLNode mood = new DXMLNode("Mood", p.funslider);
            profile.Add(mood);
            DXMLNode color = new DXMLNode("PreferredColor", p.preferredColor);
            profile.Add(color);
            DXMLNode numsan = new DXMLNode("NS", p.numSandwiches);
            profile.Add(numsan);
            DXMLNode milkfill = new DXMLNode("MF", p.milkFill);
            profile.Add(milkfill);
            DXMLNode manlev = new DXMLNode("LML", p.littleManLevel);
            profile.Add(manlev);
            DXMLNode nummen = new DXMLNode("NLM", p.numLittleMen);
            profile.Add(nummen);
            DXMLNode rnds = new DXMLNode("RSXP", p.roundsSinceXP);
            profile.Add(rnds);
            DXMLNode manbucks = new DXMLNode("LMB", p.littleManBucks);
            profile.Add(manbucks);
            DXMLNode vincent = new DXMLNode("TimesMet", p.timesMetVincent);
            profile.Add(vincent);
            DXMLNode vincent2 = new DXMLNode("TimesMet2", p.timesMetVincentSale);
            profile.Add(vincent2);
            DXMLNode vincent3 = new DXMLNode("TimesMet3", p.timesMetVincentSell);
            profile.Add(vincent3);
            DXMLNode vincent4 = new DXMLNode("TimesMet4", p.timesMetVincentImport);
            profile.Add(vincent4);
            DXMLNode vincent5 = new DXMLNode("TimesMet5", p.timesMetVincentHint);
            profile.Add(vincent5);
            DXMLNode timeOfDay = new DXMLNode("TimeOfDay", p.timeOfDay);
            profile.Add(timeOfDay);
            DXMLNode currentDay = new DXMLNode("CD", p.currentDay);
            profile.Add(currentDay);
            DXMLNode punished = new DXMLNode("Punished", p.punished);
            profile.Add(punished);
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile(" + p.name + ").xp = " + p.xp);
            }
            DXMLNode xtra = new DXMLNode("XtraPoints", p.xp);
            profile.Add(xtra);
            string fur = p.furniturePositionData.ToString();
            DXMLNode furpos = new DXMLNode("FurniPositions", fur);
            profile.Add(furpos);
            DXMLNode fowner = new DXMLNode("Fowner", p.furnitureOwnershipData.ToString());
            profile.Add(fowner);
            DXMLNode steamer = new DXMLNode("SteamID", p.steamID);
            profile.Add(steamer);
            if (p.steamID != 0L && Steam.user != null && p.steamID == Steam.user.id)
            {
                DXMLNode lastName = new DXMLNode("LastKnownName", Steam.user.name);
                profile.Add(lastName);
            }
            profile.Add(p.stats.Serialize());
            string unlockString = "";
            foreach (string thing in p.unlocks)
            {
                unlockString = unlockString + thing + "|";
            }
            if (unlockString.Length > 0)
            {
                unlockString = unlockString.Substring(0, unlockString.Length - 1);
            }
            DXMLNode unlocks = new DXMLNode("Unlocks", unlockString);
            profile.Add(unlocks);
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile(" + p.name + ").ticketCount = " + p.ticketCount);
            }
            DXMLNode tickets = new DXMLNode("Tickets", p.ticketCount);
            profile.Add(tickets);
            DXMLNode inputMappingData = new DXMLNode("Mappings");
            foreach (DeviceInputMapping map in p.inputMappingOverrides)
            {
                inputMappingData.Add(map.Serialize());
            }
            profile.Add(inputMappingData);
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Profile.SavingChallengeData(" + p.name + ")");
            }
            BitBuffer fullData = new BitBuffer(allowPacking: false);
            foreach (KeyValuePair<string, ChallengeSaveData> pair in p.challengeData)
            {
                try
                {
                    BitBuffer b = pair.Value.ToBuffer();
                    fullData.Write(b);
                }
                catch (Exception)
                {
                }
            }
            if (fullData.position > 0)
            {
                if (MonoMain.logFileOperations)
                {
                    DevConsole.Log(DCSection.General, "Profile.SavingChallengeData(" + p.name + ") (found data to save)");
                }
                DXMLNode challengeData = new DXMLNode("ChallengeData", Editor.BytesToString(fullData.data));
                profile.Add(challengeData);
            }
            doc.Add(profile);
            p.fileName = DuckFile.profileDirectory + DuckFile.ReplaceInvalidCharacters(p.formattedName) + ".pro";
            if (pPrepend != null)
            {
                DuckFile.SaveDuckXML(doc, DuckFile.profileDirectory + pPrepend + DuckFile.ReplaceInvalidCharacters(p.formattedName) + ".pro");
            }
            else
            {
                DuckFile.SaveDuckXML(doc, p.fileName);
            }
        }
    }
}
