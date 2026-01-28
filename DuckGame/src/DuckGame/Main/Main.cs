using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace DuckGame;

public class Main : MonoMain
{
    #region Public Events

    public event EventHandler<GameTime> UpdateCalled;

    public event EventHandler<GameTime> DrawCalled;

    #endregion

    #region Public Fields

    public static bool isDemo;

    public static bool foundPurchaseInfo;

    public static bool stopForever;

    public static bool _gotHook;

    public static int codeNumber;

    public static float price = 10;

    public static ulong connectID;

    public static string lastLevel = "";

    public static string SpecialCode = "";

    public static string SpecialCode2 = "";

    public static string currencyType = "USD";

    public static DuckGameEditor editor;

    public bool joinedLobby;

    #endregion

    #region Public Methods

    public static string GetPriceString() =>
        $"|GREEN|{price.ToString("0.00", CultureInfo.InvariantCulture)} {currencyType}|WHITE|";

    public static void SetPurchaseDetails(float p, string ct)
    {
        price = p;
        currencyType = ct;
        foundPurchaseInfo = true;
    }

    public static void ResetMatchStuff()
    {
        DevConsole.Log(DCSection.General, $"{nameof(ResetMatchStuff)}()");
        DuckFile.BeginDataCommit();
        PurpleBlock.Reset();
        Highlights.ClearHighlights();
        Crowd.GoHome();
        GameMode.lastWinners.Clear();
        Deathmatch.levelsSinceRandom = 0;
        Deathmatch.levelsSinceCustom = 0;
        GameMode.numMatchesPlayed = 0;
        GameMode.showdown = false;
        RockWeather.Reset();
        Music.Reset();
        if (!Program.crashed)
        {
            foreach (Team t in Teams.all)
            {
                int prevScoreboardScore = (t.score = 0);
                t.prevScoreboardScore = prevScoreboardScore;
                if (t.activeProfiles.Count <= 0)
                    continue;
                foreach (Profile p in t.activeProfiles)
                {
                    ProfileStats stats = p.stats;
                    DateTime lastPlayed = (p.stats.lastPlayed = DateTime.Now);
                    stats.lastPlayed = lastPlayed;
                    p.RecordPreviousStats();
                    Profiles.Save(p);
                }
            }
            if (Profiles.experienceProfile != null)
                Profiles.Save(Profiles.experienceProfile);
            if (Profiles.all != null)
            {
                foreach (Profile item in Profiles.all)
                    item?.RecordPreviousStats();
            }
            Global.Save();
            Options.Save();
        }
        Crowd.InitializeCrowd();
        DuckFile.EndDataCommit();
        DuckFile.FlagForBackup();
    }

    public static void ResetGameStuff()
    {
        if (Profiles.all == null)
            return;
        foreach (Profile p in Profiles.all)
            p?.wins = 0;
    }

    #endregion

    #region Protected Methods

    protected override void OnStart()
    {
        Options.Initialize();
        Teams.PostInitialize();
        Unlocks.Initialize();
        ConnectionStatusUI.Initialize();
        Unlocks.CalculateTreeValues();
        Profiles.Initialize();
        Challenges.InitializeChallengeData();
        ProfilesCore.TryAutomerge();
        Dialogue.Initialize();
        DuckTitle.Initialize();
        News.Initialize();
        Script.Initialize();
        DuckNews.Initialize();
        VirtualBackground.InitializeBack();
        AmmoType.InitializeTypes();
        DestroyType.InitializeTypes();
        VirtualTransition.Initialize();
        Unlockables.Initialize();
        UIInviteMenu.Initialize();
        LevelGenerator.Initialize();
        DuckFile.InitializeMojis();
        ResetMatchStuff();
        DuckFile._flaggedForBackup = false;
        foreach (Profile item in Profiles.active)
            item.RecordPreviousStats();
        editor = new DuckGameEditor();
        Input.devicesChanged = false;
        TeamSelect2.ControllerLayoutsChanged();
        SetPurchaseDetails(9.99f, "USD");
        if (connectID != 0L)
        {
            SpecialCode = $"Joining lobby on startup ({connectID})";
            NCSteam.PrepareProfilesForJoin();
            NCSteam.inviteLobbyID = connectID;
            Level.current = new JoinServer(connectID, lobbyPassword);
        }
        else if (Level.current == null)
        {
            if (networkDebugger)
            {
                Level.core.currentLevel = new NetworkDebugger(null, Layer.core);
                Layer.core = new LayerCore();
                Layer.core.InitializeLayers();
                Level.core.nextLevel = null;
                Level.current.DoInitialize();
                Level.core.currentLevel.lowestPoint = 100000f;
            }
            else if (startInEditor)
                Level.current = editor;
            else if (noIntro)
                Level.current = new TitleScreen();
            else
                Level.current = new BIOSScreen();
        }
        ModLoader.Start();
    }

    protected override void OnUpdate()
    {
        if (DevConsole.startupCommands.Count > 0)
        {
            DevConsole.RunCommand(DevConsole.startupCommands[0]);
            DevConsole.startupCommands.RemoveAt(0);
        }
        isDemo = false;
        RockWeather.TickWeather();
        RandomLevelDownloader.Update();
        if (!NetworkDebugger.enabled)
            FireManager.Update();
        DamageManager.Update();
        if (!Network.isActive)
            NetRand.generator = Rando.generator;

    }

    protected override void OnDraw()
    {
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        UpdateCalled?.Invoke(this, gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        DrawCalled?.Invoke(this, gameTime);
    }

    #endregion
}