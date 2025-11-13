using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DuckGame;

public class GameMode
{
	private static GameModeCore _core = new GameModeCore();

	public static List<Profile> lastWinners = new List<Profile>();

	protected UIComponent _pauseGroup;

	protected UIMenu _pauseMenu;

	protected UIMenu _confirmMenu;

	protected UIMenu _confirmBlacklistMenu;

	protected UIMenu _confirmReturnToLobby;

	protected MenuBoolean _returnToLobby = new MenuBoolean();

	protected bool _matchOver;

	protected bool _paused;

	protected MenuBoolean _quit = new MenuBoolean();

	protected float _wait;

	protected float _roundEndWait = 1f;

	protected bool _doScore;

	protected bool _addedPoints;

	protected bool _endedHighlights;

	protected bool _switchedLevel;

	protected bool _roundHadWinner;

	protected float _waitFade = 1f;

	protected float _waitSpawn = 1.8f;

	protected float _waitAfterSpawn;

	protected int _waitAfterSpawnDings;

	protected float _fontFade = 1f;

	protected List<Duck> _pendingSpawns;

	protected BitmapFont _font;

	protected bool _editorTestMode;

	protected bool _validityTest;

	private Stopwatch _watch;

	private long frames;

	public bool skippedLevel;

	protected float _pulse;

	public static GameModeCore core
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

	public static int roundsBetweenIntermission
	{
		get
		{
			return _core.roundsBetweenIntermission;
		}
		set
		{
			_core.roundsBetweenIntermission = value;
		}
	}

	public static int winsPerSet
	{
		get
		{
			return _core.winsPerSet;
		}
		set
		{
			_core.winsPerSet = value;
		}
	}

	protected static bool _started
	{
		get
		{
			return _core._started;
		}
		set
		{
			_core._started = value;
		}
	}

	public static bool started => _started;

	public static bool getReady
	{
		get
		{
			return _core.getReady;
		}
		set
		{
			_core.getReady = value;
		}
	}

	protected static int _numMatchesPlayed
	{
		get
		{
			return _core._numMatchesPlayed;
		}
		set
		{
			_core._numMatchesPlayed = value;
		}
	}

	public static int numMatchesPlayed
	{
		get
		{
			return _numMatchesPlayed;
		}
		set
		{
			_numMatchesPlayed = value;
		}
	}

	public static bool showdown
	{
		get
		{
			return _core.showdown;
		}
		set
		{
			_core.showdown = value;
		}
	}

	public static string previousLevel
	{
		get
		{
			return _core.previousLevel;
		}
		set
		{
			_core.previousLevel = value;
		}
	}

	protected static string _currentMusic
	{
		get
		{
			return _core._currentMusic;
		}
		set
		{
			_core._currentMusic = value;
		}
	}

	public static bool firstDead
	{
		get
		{
			return _core.firstDead;
		}
		set
		{
			_core.firstDead = value;
		}
	}

	public static bool playedGame
	{
		get
		{
			return _core.playedGame;
		}
		set
		{
			_core.playedGame = value;
		}
	}

	public bool matchOver => _matchOver;

	public List<Duck> pendingSpawns
	{
		get
		{
			return _pendingSpawns;
		}
		set
		{
			_pendingSpawns = value;
		}
	}

	public GameMode(bool validityTest = false, bool editorTestMode = false)
	{
		_validityTest = validityTest;
		_editorTestMode = editorTestMode;
	}

	public static void Subscribe()
	{
		if (Level.current is GameLevel && (Level.current as GameLevel).data != null && (Level.current as GameLevel).data.metaData.workshopID != 0L)
		{
			WorkshopItem item = WorkshopItem.GetItem((Level.current as GameLevel).data.metaData.workshopID);
			if ((item.stateFlags & WorkshopItemState.Subscribed) != WorkshopItemState.None)
			{
				Steam.WorkshopUnsubscribe(item.id);
			}
			else
			{
				Steam.WorkshopSubscribe(item.id);
			}
		}
	}

	public static void View()
	{
		if (Level.current is GameLevel && (Level.current as GameLevel).data != null && (Level.current as GameLevel).data.metaData.workshopID != 0L)
		{
			Steam.OverlayOpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + (Level.current as GameLevel).data.metaData.workshopID);
		}
	}

	public static void Blacklist()
	{
		if (Level.current is GameLevel && (Level.current as GameLevel).data != null && (Level.current as GameLevel).data.metaData.workshopID != 0L)
		{
			Global.data.blacklist.Add((Level.current as GameLevel).data.metaData.workshopID);
		}
		Skip();
	}

	public static void Skip()
	{
		if (Level.current is GameLevel)
		{
			(Level.current as GameLevel).SkipMatch();
		}
	}

	public void DoInitialize()
	{
		_started = false;
		firstDead = false;
		playedGame = true;
		if (!_editorTestMode)
		{
			Highlights.StartRound();
		}
		_font = new BitmapFont("biosFont", 8);
		if (Network.isServer)
		{
			string play = Music.RandomTrack("InGame", Music.currentSong);
			Music.LoadAlternateSong(play);
			Music.CancelLooping();
			if (Network.isActive)
			{
				Send.Message(new NMSwitchMusic(play));
			}
		}
		Initialize();
		if (Network.isActive)
		{
			getReady = false;
		}
		else
		{
			getReady = true;
		}
	}

	private void CreatePauseGroup()
	{
		if (_pauseGroup != null)
		{
			Level.Remove(_pauseGroup);
		}
		if (_editorTestMode)
		{
			_pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
			_pauseMenu = new UIMenu("@LWING@PAUSE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
			_confirmMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
			UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
			pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
			pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
			pauseBox.leftSection.Add(new UIText(" ", Color.White, UIAlign.Left));
			pauseBox.leftSection.Add(new UIMenuItem("|DGRED|QUIT", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit), UIAlign.Left));
			pauseBox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
			_pauseMenu.Add(pauseBox);
			_pauseMenu.Close();
			_pauseGroup.Add(_pauseMenu, doAnchor: false);
			Options.AddMenus(_pauseGroup);
			Options.openOnClose = _pauseMenu;
			_confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_confirmMenu, _pauseMenu), UIAlign.Left, default(Color), backButton: true));
			_confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit)));
			_confirmMenu.Close();
			_pauseGroup.Add(_confirmMenu, doAnchor: false);
			_pauseGroup.isPauseMenu = true;
			Level.Add(_pauseGroup);
			_pauseGroup.Update();
			_pauseGroup.Update();
			_pauseGroup.Close();
			_confirmMenu.Update();
			return;
		}
		_pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
		_pauseMenu = new UIMenu("@LWING@PAUSE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
		_confirmMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
		_confirmBlacklistMenu = new UIMenu("AVOID LEVEL?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
		_confirmReturnToLobby = new UIMenu("RETURN TO LOBBY?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@BACK @SELECT@SELECT");
		UIDivider pauseBox2 = new UIDivider(vert: true, 0.8f);
		pauseBox2.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
		pauseBox2.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
		pauseBox2.leftSection.Add(new UIText(" ", Color.White, UIAlign.Left));
		pauseBox2.leftSection.Add(new UIMenuItem("|DGRED|BACK TO LOBBY", new UIMenuActionOpenMenu(_pauseMenu, _confirmReturnToLobby), UIAlign.Left));
		pauseBox2.leftSection.Add(new UIMenuItem("|DGRED|QUIT", new UIMenuActionOpenMenu(_pauseMenu, _confirmMenu), UIAlign.Left));
		_confirmReturnToLobby.Add(new UIText("YOUR CURRENT GAME", Color.White));
		_confirmReturnToLobby.Add(new UIText("WILL BE CANCELLED.", Color.White));
		_confirmReturnToLobby.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_confirmReturnToLobby, _pauseMenu), UIAlign.Left, default(Color), backButton: true));
		_confirmReturnToLobby.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _returnToLobby)));
		_confirmReturnToLobby.Close();
		_pauseGroup.Add(_confirmReturnToLobby, doAnchor: false);
		if (Level.current.isCustomLevel || Level.current is RandomLevel)
		{
			pauseBox2.leftSection.Add(new UIText(" ", Color.White));
			if ((Level.current as GameLevel).data.metaData.workshopID != 0L)
			{
				WorkshopItem item = WorkshopItem.GetItem((Level.current as GameLevel).data.metaData.workshopID);
				if (item != null)
				{
					pauseBox2.leftSection.Add(new UIMenuItem("@STEAMICON@|DGGREEN|VIEW", new UIMenuActionCallFunction(View), UIAlign.Left));
					if ((item.stateFlags & WorkshopItemState.Subscribed) != WorkshopItemState.None)
					{
						pauseBox2.leftSection.Add(new UIMenuItem("@STEAMICON@|DGRED|UNSUBSCRIBE", new UIMenuActionCloseMenuCallFunction(_pauseGroup, Subscribe), UIAlign.Left));
					}
					else
					{
						pauseBox2.leftSection.Add(new UIMenuItem("@STEAMICON@|DGGREEN|SUBSCRIBE", new UIMenuActionCloseMenuCallFunction(_pauseGroup, Subscribe), UIAlign.Left));
						pauseBox2.leftSection.Add(new UIMenuItem("@blacklist@|DGRED|NEVER AGAIN", new UIMenuActionOpenMenu(_pauseMenu, _confirmBlacklistMenu), UIAlign.Left));
					}
				}
			}
			if (!_matchOver && Network.isServer)
			{
				pauseBox2.leftSection.Add(new UIMenuItem("@SKIPSPIN@|DGRED|SKIP", new UIMenuActionCloseMenuCallFunction(_pauseGroup, Skip), UIAlign.Left));
			}
		}
		pauseBox2.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
		_pauseMenu.Add(pauseBox2);
		_pauseMenu.Close();
		_pauseGroup.Add(_pauseMenu, doAnchor: false);
		Options.AddMenus(_pauseGroup);
		Options.openOnClose = _pauseMenu;
		_confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_confirmMenu, _pauseMenu), UIAlign.Left, default(Color), backButton: true));
		_confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit)));
		_confirmMenu.Close();
		_pauseGroup.Add(_confirmMenu, doAnchor: false);
		_confirmBlacklistMenu.Add(new UIText("", Color.White, UIAlign.Center, -4f)
		{
			scale = new Vec2(0.5f)
		});
		_confirmBlacklistMenu.Add(new UIText("Are you sure you want to avoid", Color.White, UIAlign.Center, -4f)
		{
			scale = new Vec2(0.5f)
		});
		_confirmBlacklistMenu.Add(new UIText("this level in the future?", Color.White, UIAlign.Center, -4f)
		{
			scale = new Vec2(0.5f)
		});
		_confirmBlacklistMenu.Add(new UIText("", Color.White, UIAlign.Center, -4f)
		{
			scale = new Vec2(0.5f)
		});
		_confirmBlacklistMenu.Add(new UIMenuItem("|DGRED|@blacklist@YES!", new UIMenuActionCloseMenuCallFunction(_pauseGroup, Blacklist)));
		_confirmBlacklistMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_confirmBlacklistMenu, _pauseMenu), UIAlign.Center, default(Color), backButton: true));
		_confirmBlacklistMenu.Close();
		_pauseGroup.Add(_confirmBlacklistMenu, doAnchor: false);
		Level.Add(_pauseGroup);
		_pauseGroup.isPauseMenu = true;
		_pauseGroup.Close();
		_pauseGroup.Update();
		_pauseGroup.Update();
		_confirmBlacklistMenu.Update();
		_confirmMenu.Update();
		_pauseMenu.Update();
		_confirmReturnToLobby.Update();
		_confirmBlacklistMenu.Update();
	}

	protected virtual void Initialize()
	{
		Graphics.fade = 1f;
	}

	public void DoStart()
	{
		if (!_editorTestMode)
		{
			Deathmatch.levelsSinceRandom++;
			Deathmatch.levelsSinceWorkshop++;
			Deathmatch.levelsSinceCustom++;
			foreach (Profile item in Profiles.active)
			{
				item.stats.timesSpawned++;
			}
			Global.data.timesSpawned.valueInt++;
		}
		Start();
		_started = true;
	}

	protected virtual void Start()
	{
	}

	public void DoUpdate()
	{
		if (_validityTest)
		{
			if (_watch == null)
			{
				_watch = new Stopwatch();
			}
			_watch.Start();
		}
		if (Graphics.fade > 0.9f && Input.Pressed("START") && !Network.isActive)
		{
			if (_watch != null)
			{
				_watch.Stop();
			}
			CreatePauseGroup();
			_pauseGroup.DoUpdate();
			_pauseGroup.DoUpdate();
			_pauseGroup.DoUpdate();
			_pauseMenu.DoUpdate();
			_pauseGroup.Open();
			_pauseMenu.Open();
			MonoMain.pauseMenu = _pauseGroup;
			if (!_paused)
			{
				if (!_validityTest)
				{
					Music.Pause();
				}
				SFX.Play("pause", 0.6f);
				_paused = true;
			}
			return;
		}
		if (_paused && MonoMain.pauseMenu == null)
		{
			_paused = false;
			SFX.Play("resume", 0.6f);
			if (!_validityTest)
			{
				Music.Resume();
			}
		}
		if (_returnToLobby.value && !Network.isActive)
		{
			Level.current = new TeamSelect2(pReturningFromGame: true);
			_returnToLobby.value = false;
		}
		if (_quit.value)
		{
			if (_editorTestMode)
			{
				Level.current = DuckGameTestArea.currentEditor;
				return;
			}
			if (_validityTest)
			{
				Level.current = DeathmatchTestDialogue.currentEditor;
				return;
			}
			if (Network.isActive)
			{
				Level.current = new DisconnectFromGame();
				return;
			}
			Graphics.fade -= 0.04f;
			if (Graphics.fade < 0.01f)
			{
				Level.current = new TitleScreen();
			}
			return;
		}
		Graphics.fade = 1f;
		if (!_validityTest && Music.finished)
		{
			if (Music.pendingSong != null)
			{
				Music.SwitchSongs();
			}
			else
			{
				PlayMusic();
			}
		}
		if (Music.finished)
		{
			_wait -= 0.0006f;
		}
		_waitFade -= 0.04f;
		if (_waitFade > 0f || !getReady || (Network.isActive && Network.isClient && !Level.current.transferCompleteCalled))
		{
			return;
		}
		_waitSpawn -= 0.06f;
		if (_waitSpawn <= 0f)
		{
			if (Network.isServer && _pendingSpawns == null)
			{
				_pendingSpawns = AssignSpawns();
			}
			if (_pendingSpawns != null && _pendingSpawns.Count > 0)
			{
				_waitSpawn = 1.1f;
				if (_pendingSpawns.Count == 1)
				{
					_waitSpawn = 2f;
				}
				Duck spawn = _pendingSpawns[0];
				spawn.respawnPos = spawn.position;
				spawn.localSpawnVisible = true;
				_pendingSpawns.RemoveAt(0);
				Vec3 col = spawn.profile.persona.color;
				Level.Add(new SpawnLine(spawn.x, spawn.y, 0, 0f, new Color((int)col.x, (int)col.y, (int)col.z), 32f));
				Level.Add(new SpawnLine(spawn.x, spawn.y, 0, -4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
				Level.Add(new SpawnLine(spawn.x, spawn.y, 0, 4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
				Level.Add(new SpawnAimer(spawn.x, spawn.y, 0, 4f, new Color((int)col.x, (int)col.y, (int)col.z), spawn.persona, 4f)
				{
					dugg = spawn
				});
				SFX.Play("pullPin", 0.7f);
				if (spawn.isServerForObject && !_editorTestMode)
				{
					if (!Network.isActive && spawn.profile.team.name == "ZEKE")
					{
						Ragdoll ragdoll = new Ragdoll(spawn.x, spawn.y, null, slide: false, 0f, 0, Vec2.Zero);
						Level.Add(ragdoll);
						ragdoll.RunInit();
						ragdoll.MakeZekeBear();
					}
					if (Party.HasPerk(spawn.profile, PartyPerks.Present) || (TeamSelect2.Enabled("WINPRES") && lastWinners.Contains(spawn.profile)))
					{
						Present p = new Present(spawn.x, spawn.y);
						Level.Add(p);
						spawn.GiveHoldable(p);
					}
					if (Party.HasPerk(spawn.profile, PartyPerks.Jetpack) || TeamSelect2.Enabled("JETTY"))
					{
						Jetpack j = new Jetpack(spawn.x, spawn.y);
						Level.Add(j);
						spawn.Equip(j);
					}
					if (TeamSelect2.Enabled("HELMY"))
					{
						Helmet j2 = new Helmet(spawn.x, spawn.y);
						Level.Add(j2);
						spawn.Equip(j2);
					}
					if (TeamSelect2.Enabled("SHOESTAR"))
					{
						Boots j3 = new Boots(spawn.x, spawn.y);
						Level.Add(j3);
						spawn.Equip(j3);
					}
					if (DevConsole.fancyMode)
					{
						FancyShoes j4 = new FancyShoes(spawn.x, spawn.y);
						Level.Add(j4);
						spawn.Equip(j4);
					}
					if (TeamSelect2.Enabled("DILLY"))
					{
						DuelingPistol j5 = new DuelingPistol(spawn.x, spawn.y);
						Level.Add(j5);
						spawn.GiveHoldable(j5);
					}
					if (TeamSelect2.Enabled("COOLBOOK"))
					{
						GoodBook j6 = new GoodBook(spawn.x, spawn.y);
						Level.Add(j6);
						spawn.GiveHoldable(j6);
					}
					if (Party.HasPerk(spawn.profile, PartyPerks.Armor))
					{
						Helmet j7 = new Helmet(spawn.x, spawn.y);
						Level.Add(j7);
						spawn.Equip(j7);
						ChestPlate h = new ChestPlate(spawn.x, spawn.y);
						Level.Add(h);
						spawn.Equip(h);
					}
					if (Party.HasPerk(spawn.profile, PartyPerks.Pistol))
					{
						Pistol j8 = new Pistol(spawn.x, spawn.y);
						Level.Add(j8);
						spawn.GiveHoldable(j8);
					}
					if (Party.HasPerk(spawn.profile, PartyPerks.NetGun))
					{
						NetGun j9 = new NetGun(spawn.x, spawn.y);
						Level.Add(j9);
						spawn.GiveHoldable(j9);
					}
					if (TeamSelect2.QUACK3)
					{
						Helmet j10 = new Helmet(spawn.x, spawn.y);
						Level.Add(j10);
						spawn.Equip(j10);
						ChestPlate h2 = new ChestPlate(spawn.x, spawn.y);
						Level.Add(h2);
						spawn.Equip(h2);
						Holster holst = new Holster(spawn.x, spawn.y);
						Level.Add(holst);
						spawn.Equip(holst);
						if (spawn.profile.carryOverObject != null)
						{
							Level.Add(spawn.profile.carryOverObject);
							holst.SetContainedObject(spawn.profile.carryOverObject);
						}
						else
						{
							DuelingPistol p2 = new DuelingPistol(spawn.x, spawn.y);
							Level.Add(p2);
							holst.SetContainedObject(p2);
						}
					}
				}
			}
			else if (!_started)
			{
				_waitAfterSpawn -= 0.05f;
				if (_waitAfterSpawn <= 0f)
				{
					_waitAfterSpawnDings++;
					if (_waitAfterSpawnDings > 2)
					{
						Party.Clear();
						DoStart();
						SFX.Play("ding");
						Event.Log(new RoundStartEvent());
						lastWinners.Clear();
						if (Level.current is GameLevel)
						{
							(Level.current as GameLevel).MatchStart();
						}
						foreach (Duck d in Level.current.things[typeof(Duck)])
						{
							if (d.profile.localPlayer)
							{
								d.connection = DuckNetwork.localConnection;
							}
							d.immobilized = false;
						}
					}
					else
					{
						SFX.Play("preStartDing");
					}
					_waitSpawn = 1.1f;
				}
			}
			else
			{
				_fontFade -= 0.1f;
				if (_fontFade < 0f)
				{
					_fontFade = 0f;
				}
			}
		}
		if (Network.isClient)
		{
			return;
		}
		if (_started)
		{
			Update();
		}
		if (_matchOver)
		{
			_roundEndWait -= 0.005f;
		}
		if (skippedLevel)
		{
			_roundEndWait = -1f;
		}
		if (_roundEndWait < 0.5f && !_addedPoints && !skippedLevel)
		{
			DoAddPoints();
		}
		if (_roundEndWait < 0.1f && !_endedHighlights)
		{
			_endedHighlights = true;
			if (!_editorTestMode)
			{
				Highlights.FinishRound();
			}
		}
		if (!(_roundEndWait < 0f) || _switchedLevel)
		{
			return;
		}
		bool draw = false;
		if (!Network.isActive && !skippedLevel)
		{
			int highestScore = 0;
			List<Team> winers = Teams.winning;
			if (winers.Count > 0)
			{
				highestScore = winers[0].score;
				if (Teams.active.Count > 1)
				{
					Global.WinLevel(winers[0]);
				}
			}
			else
			{
				draw = true;
				if (Teams.active.Count > 1)
				{
					Global.WinLevel(null);
				}
			}
			if (!_editorTestMode && highestScore > 4)
			{
				foreach (Team t in Teams.active)
				{
					if (t.score == highestScore)
					{
						continue;
					}
					if (t.score < 1)
					{
						foreach (Profile activeProfile in t.activeProfiles)
						{
							Party.AddRandomPerk(activeProfile);
						}
					}
					else if (t.score < 2 && Rando.Float(1f) > 0.3f)
					{
						foreach (Profile activeProfile2 in t.activeProfiles)
						{
							Party.AddRandomPerk(activeProfile2);
						}
					}
					else if (t.score < 5 && Rando.Float(1f) > 0.6f)
					{
						foreach (Profile activeProfile3 in t.activeProfiles)
						{
							Party.AddRandomPerk(activeProfile3);
						}
					}
					else if (t.score < 7 && Rando.Float(1f) > 0.85f)
					{
						foreach (Profile activeProfile4 in t.activeProfiles)
						{
							Party.AddRandomPerk(activeProfile4);
						}
					}
					else
					{
						if (t.score >= highestScore || !(Rando.Float(1f) > 0.9f))
						{
							continue;
						}
						foreach (Profile activeProfile5 in t.activeProfiles)
						{
							Party.AddRandomPerk(activeProfile5);
						}
					}
				}
			}
		}
		Level newLevel = GetNextLevel();
		previousLevel = newLevel.level;
		if (!skippedLevel)
		{
			if (Teams.active.Count > 1)
			{
				if (!_editorTestMode)
				{
					Global.data.levelsPlayed++;
				}
				if (draw && Network.isServer)
				{
					if (!_editorTestMode)
					{
						Global.data.littleDraws.valueInt++;
					}
					if (Network.isActive)
					{
						Send.Message(new NMAssignDraw());
					}
				}
			}
			if (Network.isServer)
			{
				List<int> scores = new List<int>();
				foreach (Profile p3 in DuckNetwork.profiles)
				{
					p3.ready = true;
					if (p3.team != null)
					{
						scores.Add(p3.team.score);
						if (p3.connection != null)
						{
							p3.ready = false;
						}
					}
					else
					{
						scores.Add(0);
					}
				}
				Send.Message(new NMTransferScores(scores));
				RunPostRound(_editorTestMode);
			}
		}
		if (_validityTest && _watch != null)
		{
			_ = _watch.ElapsedMilliseconds;
			if ((float)frames / ((float)_watch.ElapsedMilliseconds / 1000f) < 30f)
			{
				DeathmatchTestDialogue.success = false;
				DeathmatchTestDialogue.tooSlow = true;
			}
			else
			{
				DeathmatchTestDialogue.success = true;
			}
			Level.current = DeathmatchTestDialogue.currentEditor;
			return;
		}
		if (TeamSelect2.QUACK3)
		{
			foreach (Profile item in Profiles.active)
			{
				item.carryOverObject = null;
			}
			foreach (Duck d2 in Level.current.things[typeof(Duck)])
			{
				if (d2.GetEquipment(typeof(Holster)) is Holster { containedObject: not null } h3)
				{
					d2.profile.carryOverObject = h3.containedObject;
				}
			}
		}
		if (!DuckNetwork.TryPeacefulResolution())
		{
			if (_doScore && !skippedLevel)
			{
				_doScore = false;
				if (showdown)
				{
					if (_roundHadWinner)
					{
						showdown = false;
						Level.current = new RockScoreboard(newLevel, ScoreBoardMode.ShowWinner);
						if (!_editorTestMode)
						{
							Global.data.drawsPlayed.valueInt++;
						}
						if (Network.isActive)
						{
							Send.Message(new NMDrawBroken());
						}
					}
					else
					{
						_endedHighlights = false;
						Level.current = newLevel;
					}
				}
				else
				{
					Level.current = new RockIntro(newLevel);
					_doScore = false;
				}
			}
			else
			{
				_endedHighlights = false;
				if (TeamSelect2.partyMode && !skippedLevel)
				{
					Level.current = new DrinkRoom(newLevel);
				}
				else
				{
					Level.current = newLevel;
				}
			}
		}
		_switchedLevel = true;
	}

	public static void RunPostRound(bool testMode)
	{
		if (Level.current == null || testMode)
		{
			return;
		}
		if (!Global.HasAchievement("mine"))
		{
			foreach (Mine item in Level.current.things[typeof(Mine)])
			{
				foreach (KeyValuePair<Duck, float> pair in item.ducksOnMine)
				{
					if (!pair.Key.dead && pair.Value > 5f && pair.Key.profile != null && (!Network.isActive || pair.Key.profile.connection == DuckNetwork.localConnection))
					{
						Global.GiveAchievement("mine");
						break;
					}
				}
				if (Global.HasAchievement("mine"))
				{
					break;
				}
			}
		}
		if (!Global.HasAchievement("book"))
		{
			int converted = 0;
			foreach (Duck d in Level.current.things[typeof(Duck)])
			{
				if (d.converted != null && d.converted.profile != null && (!Network.isActive || d.converted.profile.connection == DuckNetwork.localConnection))
				{
					converted++;
				}
			}
			if (converted > 2)
			{
				Global.GiveAchievement("book");
			}
		}
		if (Teams.active.Count > 1 && Profiles.experienceProfile != null && (!Network.isActive || (DuckNetwork.localProfile != null && DuckNetwork.localProfile.slotType != SlotType.Spectator)))
		{
			DuckNetwork.GiveXP("Rounds", 1, Rando.Int(6, 7), 4, 350, 650);
			if (Profiles.experienceProfile.roundsSinceXP > 10)
			{
				DuckNetwork.GiveXP("Participation", 0, 75, 4, 75, 75, 75);
			}
			Profiles.experienceProfile.roundsSinceXP++;
		}
	}

	protected virtual void Update()
	{
	}

	public List<Duck> PrepareSpawns()
	{
		_pendingSpawns = AssignSpawns();
		return _pendingSpawns;
	}

	protected virtual List<Duck> AssignSpawns()
	{
		return new List<Duck>();
	}

	protected virtual void PlayMusic()
	{
		if (!_validityTest)
		{
			string text = Music.RandomTrack("InGame", _currentMusic);
			Music.Play(text, looping: false);
			_currentMusic = text;
		}
	}

	protected virtual Level GetNextLevel()
	{
		if (_editorTestMode)
		{
			return new GameLevel((Level.current as GameLevel).levelInputString, 0, validityTest: false, editorTestMode: true);
		}
		return new GameLevel(Deathmatch.RandomLevelString(previousLevel));
	}

	protected void DoAddPoints()
	{
		_addedPoints = true;
		Event.Log(new RoundEndEvent());
		Highlights.highlightRatingMultiplier = 0f;
		if (AddPoints().Count > 0)
		{
			_roundHadWinner = true;
			SFX.Play("scoreDing", 0.9f);
		}
		if (!skippedLevel && !_editorTestMode)
		{
			_numMatchesPlayed++;
			if (_numMatchesPlayed >= roundsBetweenIntermission || showdown)
			{
				_numMatchesPlayed = 0;
				_doScore = true;
			}
		}
	}

	protected virtual List<Profile> AddPoints()
	{
		return new List<Profile>();
	}

	public void SkipMatch()
	{
		skippedLevel = true;
		EndMatch();
	}

	protected void EndMatch()
	{
		_matchOver = true;
	}

	public virtual void PostDrawLayer(Layer layer)
	{
		frames++;
		if (layer != Layer.HUD)
		{
			return;
		}
		if (_waitAfterSpawnDings > 0 && _fontFade > 0.01f)
		{
			_font.scale = new Vec2(2f, 2f);
			_font.alpha = _fontFade;
			string s = "GET";
			if (_waitAfterSpawnDings == 2)
			{
				s = "READY";
			}
			else if (_waitAfterSpawnDings == 3)
			{
				s = "";
			}
			float wide = _font.GetWidth(s);
			_font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f, Layer.HUD.camera.height / 2f - _font.height / 2f, Color.White);
		}
		if (_validityTest && _waitAfterSpawnDings > 0 && _fontFade < 0.5f)
		{
			_pulse += 0.08f;
			string s2 = "WIN THE MATCH";
			float wide2 = _font.GetWidth(s2);
			_font.alpha = ((float)Math.Sin(_pulse) + 1f) / 2f;
			_font.Draw(s2, Layer.HUD.camera.width / 2f - wide2 / 2f, Layer.HUD.camera.height - _font.height - 16f, Color.Red);
		}
	}
}
