using System;
using System.Globalization;

namespace DuckGame;

public class Main : MonoMain
{
	public static bool isDemo = false;

	public static DuckGameEditor editor;

	public static string lastLevel = "";

	public static string SpecialCode = "";

	public static string SpecialCode2 = "";

	public static int codeNumber = 0;

	private BitmapFont _font;

	public static ulong connectID = 0uL;

	public static bool foundPurchaseInfo = false;

	public static float price = 10f;

	public static string currencyType = "USD";

	public static bool stopForever = false;

	public static bool _gotHook = false;

	private bool didHash;

	public bool joinedLobby;

	public static string GetPriceString()
	{
		return "|GREEN|" + price.ToString("0.00", CultureInfo.InvariantCulture) + " " + currencyType + "|WHITE|";
	}

	public static void SetPurchaseDetails(float p, string ct)
	{
		price = p;
		currencyType = ct;
		foundPurchaseInfo = true;
	}

	public static void ResetMatchStuff()
	{
		DevConsole.Log(DCSection.General, "ResetMatchStuff()");
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
				{
					continue;
				}
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
			{
				Profiles.Save(Profiles.experienceProfile);
			}
			if (Profiles.all != null)
			{
				foreach (Profile item in Profiles.all)
				{
					item?.RecordPreviousStats();
				}
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
		{
			return;
		}
		foreach (Profile p in Profiles.all)
		{
			if (p != null)
			{
				p.wins = 0;
			}
		}
	}

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
		{
			item.RecordPreviousStats();
		}
		editor = new DuckGameEditor();
		Input.devicesChanged = false;
		TeamSelect2.ControllerLayoutsChanged();
		SetPurchaseDetails(9.99f, "USD");
		if (connectID != 0L)
		{
			SpecialCode = "Joining lobby on startup (" + connectID + ")";
			NCSteam.PrepareProfilesForJoin();
			NCSteam.inviteLobbyID = connectID;
			Level.current = new JoinServer(connectID, MonoMain.lobbyPassword);
		}
		else if (Level.current == null)
		{
			if (MonoMain.networkDebugger)
			{
				Level.core.currentLevel = new NetworkDebugger(null, Layer.core);
				Layer.core = new LayerCore();
				Layer.core.InitializeLayers();
				Level.core.nextLevel = null;
				Level.current.DoInitialize();
				Level.core.currentLevel.lowestPoint = 100000f;
			}
			else if (MonoMain.startInEditor)
			{
				Level.current = editor;
			}
			else if (MonoMain.noIntro)
			{
				Level.current = new TitleScreen();
			}
			else
			{
				Level.current = new BIOSScreen();
			}
		}
		_font = new BitmapFont("biosFont", 8);
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
		{
			FireManager.Update();
		}
		DamageManager.Update();
		if (!Network.isActive)
		{
			NetRand.generator = Rando.generator;
		}
		if (joinedLobby || !Program.testServer || Network.isActive || !Steam.lobbySearchComplete)
		{
			return;
		}
		if (Steam.lobbySearchResult != null)
		{
			Network.JoinServer("", 0, Steam.lobbySearchResult.id.ToString());
			joinedLobby = true;
			return;
		}
		User friend = Steam.friends.Find((User x) => x.name == "superjoebob");
		if (friend != null)
		{
			Steam.SearchForLobby(friend);
		}
	}

	protected override void OnDraw()
	{
	}
}
