using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class DuckGameTestArea : DeathmatchLevel
{
	private Editor _editor;

	protected int _seed;

	protected RandomLevelData _center;

	protected LevGenType _genType;

	private string _levelValue;

	public static Editor currentEditor;

	private MenuBoolean _capture = new MenuBoolean();

	private MenuBoolean _quit = new MenuBoolean();

	private MenuBoolean _restart = new MenuBoolean();

	private MenuBoolean _startTestMode = new MenuBoolean();

	private UIComponent _pauseGroup;

	private UIMenu _pauseMenu;

	private UIMenu _confirmMenu;

	private UIMenu _testMode;

	public int numPlayers = 4;

	private bool _paused;

	private int wait;

	public DuckGameTestArea(Editor editor, string level, int seed = 0, RandomLevelData center = null, LevGenType genType = LevGenType.Any)
		: base(level)
	{
		_editor = editor;
		DeathmatchLevel._started = true;
		_followCam.lerpMult = 1.1f;
		_seed = seed;
		_center = center;
		_genType = genType;
		_levelValue = level;
		currentEditor = editor;
	}

	public override void Initialize()
	{
		_pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
		_pauseMenu = new UIMenu("@LWING@PAUSE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE  @SELECT@SELECT");
		_confirmMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK  @SELECT@SELECT");
		_testMode = new UIMenu("TEST MODE", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK  @SELECT@SELECT");
		UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
		pauseBox.leftSection.Add(new UIMenuItem("RESTART", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _restart), UIAlign.Left));
		pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
		pauseBox.leftSection.Add(new UIMenuItem("TEST MODE", new UIMenuActionOpenMenu(_pauseMenu, _testMode), UIAlign.Left));
		pauseBox.leftSection.Add(new UIText("", Color.White));
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
		_testMode.Add(new UIMenuItemNumber("PLAYERS", null, new FieldBinding(this, "numPlayers", 2f, 8f, 1f)));
		_testMode.Add(new UIMenuItem("START", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _startTestMode)));
		_testMode.SetBackFunction(new UIMenuActionOpenMenu(_testMode, _pauseMenu));
		_testMode.Close();
		_pauseGroup.Add(_testMode, doAnchor: false);
		_pauseGroup.isPauseMenu = true;
		_pauseGroup.Close();
		_pauseGroup.Update();
		_pauseGroup.Update();
		Level.Add(_pauseGroup);
		if (_level == "RANDOM")
		{
			LevelGenerator.MakeLevel(_center, (_center.left && _center.right) ? true : false, _seed, _genType, Editor._procTilesWide, Editor._procTilesHigh, Editor._procXPos, Editor._procYPos).LoadParts(0f, 0f, this, _seed);
			List<SpawnPoint> spawns = new List<SpawnPoint>();
			foreach (SpawnPoint spawn in base.things[typeof(SpawnPoint)])
			{
				spawns.Add(spawn);
			}
			List<SpawnPoint> chosenSpawns = new List<SpawnPoint>();
			for (int i = 0; i < 4; i++)
			{
				if (chosenSpawns.Count == 0)
				{
					chosenSpawns.Add(spawns.ElementAt(Rando.Int(spawns.Count - 1)));
					continue;
				}
				IOrderedEnumerable<SpawnPoint> ordered = spawns.OrderByDescending(delegate(SpawnPoint x)
				{
					int num2 = 9999999;
					foreach (SpawnPoint item in chosenSpawns)
					{
						num2 = (int)Math.Min((item.position - x.position).length, num2);
					}
					return num2;
				});
				chosenSpawns.Add(ordered.First());
			}
			foreach (SpawnPoint s in spawns)
			{
				if (!chosenSpawns.Contains(s))
				{
					Level.Remove(s);
				}
			}
			Level.Add(new PyramidBackground(0f, 0f)
			{
				visible = false
			});
		}
		else
		{
			_level = _level.Replace(Directory.GetCurrentDirectory() + "\\", "");
			LevelData dat = DuckFile.LoadLevel(_level);
			if (dat != null)
			{
				foreach (BinaryClassChunk @object in dat.objects.objects)
				{
					Thing t = Thing.LoadThing(@object);
					if (t != null)
					{
						if (!t.visibleInGame)
						{
							t.visible = false;
						}
						AddThing(t);
					}
				}
			}
		}
		base.things.RefreshState();
		foreach (Profile p in Profiles.active)
		{
			if (p.team != null)
			{
				p.team.Leave(p);
			}
		}
		int num = 4;
		if (base.things[typeof(EightPlayer)].Count() > 0)
		{
			num = 8;
		}
		for (int i2 = 0; i2 < num; i2++)
		{
			Profiles.defaultProfiles[i2].team = Teams.allStock[i2];
			Profiles.defaultProfiles[i2].persona = Profiles.defaultProfiles[i2].defaultPersona;
			Profiles.defaultProfiles[i2].UpdatePersona();
			Input.ApplyDefaultMapping(Profiles.defaultProfiles[i2].inputProfile, Profiles.defaultProfiles[i2]);
		}
		foreach (Duck d in new Deathmatch(this).SpawnPlayers(recordStats: false))
		{
			Level.Add(d);
			base.followCam.Add(d);
		}
	}

	public void PauseLogic()
	{
		if (Input.Pressed("START"))
		{
			_pauseGroup.Open();
			_pauseMenu.Open();
			MonoMain.pauseMenu = _pauseGroup;
			if (!_paused)
			{
				SFX.Play("pause", 0.6f);
				_paused = true;
			}
		}
		else if (_paused && MonoMain.pauseMenu == null)
		{
			_paused = false;
			SFX.Play("resume", 0.6f);
			DeathmatchLevel._started = false;
		}
	}

	public override void Update()
	{
		if (_startTestMode.value)
		{
			foreach (Profile item in Profiles.active)
			{
				item.team = null;
			}
			if (numPlayers > 7)
			{
				Profiles.DefaultPlayer8.team = Teams.Player8;
				if (Profiles.DefaultPlayer8.inputProfile == null)
				{
					Profiles.DefaultPlayer8.inputProfile = InputProfile.DefaultPlayer8;
				}
			}
			if (numPlayers > 6)
			{
				Profiles.DefaultPlayer7.team = Teams.Player7;
				if (Profiles.DefaultPlayer7.inputProfile == null)
				{
					Profiles.DefaultPlayer7.inputProfile = InputProfile.DefaultPlayer7;
				}
			}
			if (numPlayers > 5)
			{
				Profiles.DefaultPlayer6.team = Teams.Player6;
				if (Profiles.DefaultPlayer6.inputProfile == null)
				{
					Profiles.DefaultPlayer6.inputProfile = InputProfile.DefaultPlayer6;
				}
			}
			if (numPlayers > 4)
			{
				Profiles.DefaultPlayer5.team = Teams.Player5;
				if (Profiles.DefaultPlayer5.inputProfile == null)
				{
					Profiles.DefaultPlayer5.inputProfile = InputProfile.DefaultPlayer5;
				}
			}
			if (numPlayers > 3)
			{
				Profiles.DefaultPlayer4.team = Teams.Player4;
				if (Profiles.DefaultPlayer4.inputProfile == null)
				{
					Profiles.DefaultPlayer4.inputProfile = InputProfile.DefaultPlayer4;
				}
			}
			if (numPlayers > 2)
			{
				Profiles.DefaultPlayer3.team = Teams.Player3;
				if (Profiles.DefaultPlayer3.inputProfile == null)
				{
					Profiles.DefaultPlayer3.inputProfile = InputProfile.DefaultPlayer3;
				}
			}
			if (numPlayers > 1)
			{
				Profiles.DefaultPlayer2.team = Teams.Player2;
				if (Profiles.DefaultPlayer2.inputProfile == null)
				{
					Profiles.DefaultPlayer2.inputProfile = InputProfile.DefaultPlayer2;
				}
			}
			if (numPlayers > 0)
			{
				Profiles.experienceProfile.team = Teams.Player1;
			}
			EditorTestLevel l = null;
			if (base.things[typeof(EditorTestLevel)].Count() > 0)
			{
				l = base.things[typeof(EditorTestLevel)].First() as EditorTestLevel;
			}
			Level.current = new GameLevel(_levelValue, 0, validityTest: false, editorTestMode: true);
			if (l != null)
			{
				Level.current.AddThing(l);
			}
			return;
		}
		if (_restart.value)
		{
			transitionSpeedMultiplier = 2f;
			EditorTestLevel l2 = null;
			if (base.things[typeof(EditorTestLevel)].Count() > 0)
			{
				l2 = base.things[typeof(EditorTestLevel)].First() as EditorTestLevel;
			}
			Level.current = new DuckGameTestArea(_editor, _levelValue, _seed, _center, _genType);
			Level.current.transitionSpeedMultiplier = 2f;
			if (l2 != null)
			{
				Level.current.AddThing(l2);
			}
			return;
		}
		if (_level == "RANDOM")
		{
			if (wait < 4)
			{
				wait++;
			}
			if (wait == 4)
			{
				wait++;
				foreach (AutoBlock item2 in base.things[typeof(AutoBlock)])
				{
					item2.PlaceBlock();
				}
				foreach (AutoPlatform item3 in base.things[typeof(AutoPlatform)])
				{
					item3.PlaceBlock();
					item3.UpdateNubbers();
				}
				foreach (BlockGroup item4 in base.things[typeof(BlockGroup)])
				{
					foreach (Block bl in item4.blocks)
					{
						if (bl is AutoBlock)
						{
							(bl as AutoBlock).PlaceBlock();
						}
					}
				}
			}
		}
		PauseLogic();
		if (_quit.value)
		{
			Level.current = _editor;
		}
		base.Update();
	}
}
