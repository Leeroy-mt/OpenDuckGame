using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ProfileSelector : Thing
{
	private float _fade;

	private bool _open;

	private bool _closing;

	private int _controlPage;

	private ControlSetting _selectedSetting;

	private List<List<ControlSetting>> _controlSettingPages = new List<List<ControlSetting>>
	{
		new List<ControlSetting>
		{
			new ControlSetting
			{
				name = "{",
				trigger = "LEFT",
				position = new Vec2(0f, 0f),
				column = 0,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "/",
				trigger = "RIGHT",
				position = new Vec2(35f, 0f),
				column = 0,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "}",
				trigger = "UP",
				position = new Vec2(70f, 0f),
				column = 1,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "~",
				trigger = "DOWN",
				position = new Vec2(105f, 0f),
				column = 1,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "JUMP  ",
				trigger = "JUMP",
				position = new Vec2(0f, 12f),
				column = 0
			},
			new ControlSetting
			{
				name = "GRAB  ",
				trigger = "GRAB",
				position = new Vec2(0f, 24f),
				column = 0
			},
			new ControlSetting
			{
				name = "STRAFE",
				trigger = "STRAFE",
				position = new Vec2(0f, 36f),
				column = 0
			},
			new ControlSetting
			{
				name = "USE   ",
				trigger = "SHOOT",
				position = new Vec2(70f, 12f),
				column = 1
			},
			new ControlSetting
			{
				name = "QUACK ",
				trigger = "QUACK",
				position = new Vec2(70f, 24f),
				column = 1
			},
			new ControlSetting
			{
				name = "FALL  ",
				trigger = "RAGDOLL",
				position = new Vec2(70f, 36f),
				column = 1
			},
			new ControlSetting
			{
				name = "START ",
				trigger = "START",
				position = new Vec2(0f, 48f),
				column = 0,
				condition = (InputDevice x) => x.allowStartRemap
			},
			new ControlSetting
			{
				name = "PAGE 2>",
				trigger = "ANY",
				position = new Vec2(70f, 48f),
				column = 1,
				condition = (InputDevice x) => !(x is Keyboard),
				action = delegate(ProfileSelector x)
				{
					x._controlPage++;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			},
			new ControlSetting
			{
				name = "PAGE 2>",
				trigger = "ANY",
				position = new Vec2(70f, 48f),
				column = 1,
				condition = (InputDevice x) => x is Keyboard,
				action = delegate(ProfileSelector x)
				{
					x._controlPage += 2;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			}
		},
		new List<ControlSetting>
		{
			new ControlSetting
			{
				name = "MOVE  ",
				trigger = "LSTICK",
				position = new Vec2(0f, 12f),
				column = 0,
				condition = (InputDevice x) => x.numSticks > 1
			},
			new ControlSetting
			{
				name = "PITCH ",
				trigger = "LTRIGGER",
				position = new Vec2(0f, 24f),
				column = 0,
				condition = (InputDevice x) => x.numTriggers > 1
			},
			new ControlSetting
			{
				name = "LICK  ",
				trigger = "RSTICK",
				position = new Vec2(70f, 12f),
				column = 1,
				condition = (InputDevice x) => x.numSticks > 1
			},
			new ControlSetting
			{
				name = "ZOOM  ",
				trigger = "RTRIGGER",
				position = new Vec2(70f, 24f),
				column = 1,
				condition = (InputDevice x) => x.numTriggers > 1
			},
			new ControlSetting
			{
				name = "<PAGE 1",
				trigger = "ANY",
				position = new Vec2(0f, 60f),
				column = 0,
				action = delegate(ProfileSelector x)
				{
					x._controlPage--;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			},
			new ControlSetting
			{
				name = "PAGE 3>",
				trigger = "ANY",
				position = new Vec2(70f, 60f),
				column = 1,
				action = delegate(ProfileSelector x)
				{
					x._controlPage++;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			}
		},
		new List<ControlSetting>
		{
			new ControlSetting
			{
				name = "UI CONTROLS...",
				trigger = "ANY",
				position = new Vec2(0f, 0f),
				column = 0,
				caption = true
			},
			new ControlSetting
			{
				name = "{",
				trigger = "MENULEFT",
				position = new Vec2(0f, 12f),
				column = 0,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "/",
				trigger = "MENURIGHT",
				position = new Vec2(35f, 12f),
				column = 0,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "}",
				trigger = "MENUUP",
				position = new Vec2(70f, 12f),
				column = 1,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "~",
				trigger = "MENUDOWN",
				position = new Vec2(105f, 12f),
				column = 1,
				condition = (InputDevice x) => x.allowDirectionalMapping
			},
			new ControlSetting
			{
				name = "SELECT",
				trigger = "SELECT",
				position = new Vec2(0f, 24f),
				column = 0
			},
			new ControlSetting
			{
				name = "MENU 1",
				trigger = "MENU1",
				position = new Vec2(0f, 36f),
				column = 0
			},
			new ControlSetting
			{
				name = "CANCEL",
				trigger = "CANCEL",
				position = new Vec2(70f, 24f),
				column = 1
			},
			new ControlSetting
			{
				name = "MENU 2",
				trigger = "MENU2",
				position = new Vec2(70f, 36f),
				column = 1
			},
			new ControlSetting
			{
				name = "<PAGE 2",
				trigger = "ANY",
				position = new Vec2(0f, 48f),
				column = 0,
				condition = (InputDevice x) => !(x is Keyboard),
				action = delegate(ProfileSelector x)
				{
					x._controlPage--;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			},
			new ControlSetting
			{
				name = "<PAGE 1",
				trigger = "ANY",
				position = new Vec2(0f, 48f),
				column = 0,
				condition = (InputDevice x) => x is Keyboard,
				action = delegate(ProfileSelector x)
				{
					x._controlPage -= 2;
					x._selectedSetting = null;
					SFX.Play("page");
				}
			},
			new ControlSetting
			{
				name = "RESET ",
				trigger = "ANY",
				position = new Vec2(70f, 48f),
				column = 1,
				action = delegate(ProfileSelector x)
				{
					_madeControlChanges = true;
					x._configInputMapping = Input.GetDefaultMapping(x._inputProfile.lastActiveDevice.productName, x._inputProfile.lastActiveDevice.productGUID).Clone();
					SFX.Play("consoleSelect");
				}
			}
		}
	};

	private float _takenFlash;

	private string _name = "";

	private string _maskName = "aaaaaaaaa";

	private List<char> _characters = new List<char>
	{
		'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
		'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
		'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3',
		'4', '5', '6', '7', '8', '9', '{', '}', ' ', '-',
		'!'
	};

	private float _slide;

	private float _slideTo;

	private int _editControlSelection;

	private bool _changeName;

	private int _currentLetter;

	private int _controlPosition = 1;

	private bool _editControl;

	private PSMode _mode;

	private PSMode _desiredMode;

	private PSCreateSelection _createSelection;

	private InputProfile _inputProfile;

	private int _selectorPosition = -1;

	private int _desiredSelectorPosition = -1;

	private Profile _profile;

	private BitmapFont _font;

	private BitmapFont _smallFont;

	private BitmapFont _controlsFont;

	private List<Profile> _profiles;

	private ProfileBox2 _box;

	private SpriteMap _happyIcons;

	private SpriteMap _angryIcons;

	private SpriteMap _spinnerArrows;

	private Profile _starterProfile;

	private HatSelector _selector;

	public static bool _madeControlChanges;

	private DeviceInputMapping _configInputMapping;

	private UIMenu _confirmMenu;

	private MenuBoolean _deleteProfile = new MenuBoolean();

	private Profile _deleteContext;

	private bool _wasDown;

	private bool _autoSelect;

	private List<DeviceInputMapping> _pendingMaps = new List<DeviceInputMapping>();

	private bool isEditing;

	private float _moodVal = 0.5f;

	private int _preferredColor;

	public float fade => _fade;

	public bool open => _open;

	private Profile hoveredProfile
	{
		get
		{
			if (_selectorPosition >= 0 && _selectorPosition < _profiles.Count)
			{
				return _profiles[_selectorPosition];
			}
			return Profiles.DefaultPlayer1;
		}
	}

	public ProfileSelector(float xpos, float ypos, ProfileBox2 box, HatSelector sel)
		: base(xpos, ypos)
	{
		_font = new BitmapFont("biosFontUI", 8, 7);
		_font.scale = new Vec2(0.5f, 0.5f);
		_collisionSize = new Vec2(141f, 89f);
		_spinnerArrows = new SpriteMap("spinnerArrows", 8, 4);
		_box = box;
		_selector = sel;
		_happyIcons = new SpriteMap("happyFace", 16, 16);
		_happyIcons.CenterOrigin();
		_angryIcons = new SpriteMap("angryFace", 16, 16);
		_angryIcons.CenterOrigin();
		_smallFont = new BitmapFont("smallBiosFont", 7, 6);
		_smallFont.scale = new Vec2(0.5f, 0.5f);
		_controlsFont = new BitmapFont("biosFontUIArrows", 8, 7);
		_controlsFont.scale = new Vec2(1f);
	}

	public override void Initialize()
	{
		float wide = 320f;
		float high = 180f;
		_confirmMenu = new UIMenu("DELETE PROFILE!?", wide / 2f, high / 2f, 160f, -1f, "@SELECT@SELECT @CANCEL@OH NO!");
		_confirmMenu.Add(new UIMenuItem("WHAT? NO!", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, default(Color), backButton: true));
		_confirmMenu.Add(new UIMenuItem("YEAH!", new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _deleteProfile)));
		_confirmMenu.Close();
		Level.Add(_confirmMenu);
		base.Initialize();
	}

	public void Reset()
	{
		_open = false;
		_selector.fade = 1f;
		_fade = 0f;
		_desiredMode = PSMode.SelectProfile;
		_controlPage = 0;
		_selectedSetting = null;
		_configInputMapping = null;
	}

	public string GetMaskName(int length)
	{
		string n = "";
		for (int i = 0; i < 9; i++)
		{
			n = ((i >= length) ? (n + " ") : (n + _maskName[i]));
		}
		return n;
	}

	public void SelectDown()
	{
		if (_desiredSelectorPosition >= _profiles.Count - 1)
		{
			_desiredSelectorPosition = -1;
		}
		else
		{
			_desiredSelectorPosition++;
		}
		_slideTo = 1f;
	}

	public void SelectUp()
	{
		if (_desiredSelectorPosition <= -1)
		{
			_desiredSelectorPosition = _profiles.Count - 1;
		}
		else
		{
			_desiredSelectorPosition--;
		}
		_slideTo = -1f;
	}

	public int GetCharIndex(char c)
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i] == c)
			{
				return i;
			}
		}
		return -1;
	}

	private void ApplyInputSettings(Profile p)
	{
		if (p == null)
		{
			return;
		}
		if (_pendingMaps.Count > 0)
		{
			p.inputMappingOverrides.Clear();
			foreach (DeviceInputMapping pendingMap in _pendingMaps)
			{
				Input.SetDefaultMapping(pendingMap, p);
			}
		}
		p.inputProfile = _inputProfile;
		_pendingMaps.Clear();
		Input.ApplyDefaultMapping(_inputProfile, _profile);
	}

	private void RebuildProfileList()
	{
		_profiles = Profiles.allCustomProfiles;
		if (_box.controllerIndex != 0 || !Options.Data.defaultAccountMerged)
		{
			_profiles.Add(Profiles.universalProfileList.ElementAt(_box.controllerIndex));
		}
		if (Network.isActive)
		{
			_profiles.Remove(Profiles.experienceProfile);
		}
	}

	private void SaveSettings(bool pIsEditing, bool pAccepted)
	{
		if (!pIsEditing && !pAccepted)
		{
			return;
		}
		string realName = _name.Replace(" ", "");
		Profile p = _profile;
		if (!pIsEditing)
		{
			p = new Profile(realName);
		}
		p.funslider = _moodVal;
		p.preferredColor = _preferredColor;
		p.UpdatePersona();
		ApplyInputSettings(p);
		if (!pIsEditing)
		{
			Profiles.Add(p);
		}
		if (!pIsEditing)
		{
			RebuildProfileList();
			for (int i = 0; i < _profiles.Count; i++)
			{
				if (_profiles[i].name == realName)
				{
					_selectorPosition = i;
					_desiredSelectorPosition = _selectorPosition;
					break;
				}
			}
			_desiredMode = PSMode.SelectProfile;
			_autoSelect = true;
		}
		else
		{
			Profiles.Save(_profile);
			_desiredMode = PSMode.SelectProfile;
			_mode = PSMode.SelectProfile;
			_open = false;
			_selector.fade = 1f;
			_fade = 0f;
			_selector.screen.DoFlashTransition();
		}
		SFX.Play("consoleSelect", 0.4f);
	}

	private bool HoveredProfileIsCustom()
	{
		if (_selectorPosition != -1 && hoveredProfile.steamID == 0L && Profiles.experienceProfile != hoveredProfile)
		{
			return !Profiles.IsDefault(hoveredProfile);
		}
		return false;
	}

	public override void Update()
	{
		if (_selector.screen.transitioning)
		{
			return;
		}
		_takenFlash = Lerp.Float(_takenFlash, 0f, 0.02f);
		if (!_open)
		{
			if (_fade < 0.01f && _closing)
			{
				_closing = false;
			}
			return;
		}
		if (_configInputMapping != null && _inputProfile != null)
		{
			string text = _configInputMapping.device.productName + _configInputMapping.device.productGUID;
			string yours = _inputProfile.lastActiveDevice.productName + _inputProfile.lastActiveDevice.productGUID;
			if (text != yours)
			{
				_open = false;
				_selector.fade = 1f;
				_fade = 0f;
				_selector.screen.DoFlashTransition();
				_desiredMode = PSMode.SelectProfile;
				SFX.Play("consoleCancel", 0.4f);
				return;
			}
		}
		if (_mode != _desiredMode)
		{
			_selector.screen.DoFlashTransition();
			_mode = _desiredMode;
		}
		if (_fade > 0.9f && _mode != PSMode.CreateProfile && _mode != PSMode.EditProfile && _mode != PSMode.EditControls && _mode != PSMode.EditControlsConfirm && _desiredSelectorPosition == _selectorPosition)
		{
			if (_inputProfile.Down("MENUUP"))
			{
				SelectUp();
				_wasDown = false;
				if (_profiles.Count > 0)
				{
					SFX.Play("consoleTick");
				}
			}
			if (_inputProfile.Down("MENUDOWN"))
			{
				SelectDown();
				_wasDown = true;
				if (_profiles.Count > 0)
				{
					SFX.Play("consoleTick");
				}
			}
			if (HoveredProfileIsCustom() && MonoMain.pauseMenu == null && _inputProfile.Pressed("MENU2"))
			{
				_deleteContext = _profiles[_selectorPosition];
				MonoMain.pauseMenu = _confirmMenu;
				_confirmMenu.Open();
				SFX.Play("pause", 0.6f);
			}
			if (_deleteProfile.value)
			{
				_deleteProfile.value = false;
				if (_deleteContext != null)
				{
					Profiles.Delete(_deleteContext);
					SelectUp();
					RebuildProfileList();
					_slide = _slideTo;
					_deleteContext = null;
				}
			}
			if (_inputProfile.Pressed("CANCEL"))
			{
				if (Profiles.IsDefault(_starterProfile) || !(Level.current is TeamSelect2))
				{
					_box.ChangeProfile(_starterProfile);
				}
				_open = false;
				_selector.fade = 1f;
				_fade = 0f;
				_selector.screen.DoFlashTransition();
				SFX.Play("consoleCancel", 0.4f);
				return;
			}
			if (_inputProfile.Pressed("SELECT") || _autoSelect)
			{
				_autoSelect = false;
				if (_selectorPosition == -1)
				{
					_desiredMode = PSMode.CreateProfile;
					_changeName = true;
					_currentLetter = 0;
					_createSelection = PSCreateSelection.ChangeName;
					_maskName = "aaaaaaaaa";
					_name = GetMaskName(1);
					SFX.Play("consoleSelect", 0.4f);
				}
				else if (ProfileAlreadySelected(_profiles[_selectorPosition]))
				{
					SFX.Play("consoleError");
				}
				else
				{
					if (_profiles[_selectorPosition].linkedProfile == null)
					{
						if (Network.isActive)
						{
							_profile.linkedProfile = _profiles[_selectorPosition];
							Input.ApplyDefaultMapping(_inputProfile, _profile);
							_profile.UpdatePersona();
						}
						else if (_selectorPosition != -1)
						{
							_box.ChangeProfile(_profiles[_selectorPosition]);
							_profile = _profiles[_selectorPosition];
							_profile.inputProfile = null;
							_profile.inputProfile = _inputProfile;
							Input.ApplyDefaultMapping(_inputProfile, _profile);
						}
					}
					_selector.ConfirmProfile();
					_open = false;
					_selector.fade = 1f;
					_fade = 0f;
					_selector.screen.DoFlashTransition();
					SFX.Play("consoleSelect", 0.4f);
				}
			}
		}
		else if (_mode == PSMode.EditControlsConfirm)
		{
			if (_inputProfile.Pressed("MENUUP"))
			{
				SFX.Play("consoleTick");
				_editControlSelection--;
			}
			else if (_inputProfile.Pressed("MENUDOWN"))
			{
				SFX.Play("consoleTick");
				_editControlSelection++;
			}
			else
			{
				if (_inputProfile.Pressed("CANCEL"))
				{
					_desiredMode = PSMode.EditControls;
					SFX.Play("consoleError");
					return;
				}
				if (_inputProfile.Pressed("SELECT"))
				{
					SFX.Play("consoleSelect");
					if (_editControlSelection == 0)
					{
						_pendingMaps.Add(_configInputMapping);
						ApplyInputSettings(_profile);
					}
					else
					{
						_configInputMapping = Input.GetDefaultMapping(_inputProfile.lastActiveDevice.productName, _inputProfile.lastActiveDevice.productGUID, presets: false, makeClone: true, isEditing ? _profile : null).Clone();
					}
					_desiredMode = PSMode.CreateProfile;
				}
			}
			if (_editControlSelection > 1)
			{
				_editControlSelection = 1;
			}
			if (_editControlSelection < 0)
			{
				_editControlSelection = 0;
			}
		}
		else if (_mode == PSMode.EditControls)
		{
			if (!_editControl)
			{
				InputDevice d = _inputProfile.lastActiveDevice;
				if (d is GenericController)
				{
					d = (d as GenericController).device;
				}
				if (_selectedSetting == null)
				{
					_selectedSetting = _controlSettingPages[_controlPage].Find((ControlSetting x) => (x.condition == null || x.condition(d)) && !x.caption);
				}
				Vec2 move = Vec2.Zero;
				if (_inputProfile.Pressed("MENUUP"))
				{
					move += new Vec2(0f, -8f);
				}
				else if (_inputProfile.Pressed("MENUDOWN"))
				{
					move += new Vec2(0f, 8f);
				}
				else if (_inputProfile.Pressed("MENULEFT"))
				{
					move += new Vec2(-30f, 0f);
				}
				else if (_inputProfile.Pressed("MENURIGHT"))
				{
					move += new Vec2(30f, 0f);
				}
				if (move != Vec2.Zero)
				{
					ControlSetting nearest = null;
					foreach (ControlSetting s in _controlSettingPages[_controlPage])
					{
						if ((s.condition != null && !s.condition(d)) || s.caption)
						{
							continue;
						}
						if (move.x != 0f)
						{
							if (s.position.y != _selectedSetting.position.y)
							{
								continue;
							}
							if (move.x > 0f)
							{
								if (s.position.x > _selectedSetting.position.x && (nearest == null || s.position.x < nearest.position.x))
								{
									nearest = s;
								}
							}
							else if (s.position.x < _selectedSetting.position.x && (nearest == null || s.position.x > nearest.position.x))
							{
								nearest = s;
							}
						}
						else
						{
							if (s.position.x != _selectedSetting.position.x && s.column != _selectedSetting.column)
							{
								continue;
							}
							if (move.y > 0f)
							{
								if (s.position.y > _selectedSetting.position.y && (nearest == null || s.position.y < nearest.position.y))
								{
									nearest = s;
								}
							}
							else if (s.position.y < _selectedSetting.position.y && (nearest == null || s.position.y > nearest.position.y))
							{
								nearest = s;
							}
						}
					}
					if (nearest != null)
					{
						_selectedSetting = nearest;
					}
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("SELECT"))
				{
					if (_selectedSetting.action != null)
					{
						_selectedSetting.action(this);
					}
					else
					{
						_editControl = true;
						SFX.Play("consoleTick");
					}
				}
				else
				{
					if (_inputProfile.Pressed("CANCEL"))
					{
						if (_madeControlChanges)
						{
							_editControlSelection = 0;
							_desiredMode = PSMode.EditControlsConfirm;
							SFX.Play("consoleError");
						}
						else
						{
							_desiredMode = PSMode.CreateProfile;
							SFX.Play("consoleError");
						}
						return;
					}
					if (_inputProfile.Pressed("START"))
					{
						_pendingMaps.Add(_configInputMapping);
						ApplyInputSettings(_profile);
						_desiredMode = PSMode.CreateProfile;
						SFX.Play("consoleSelect");
						return;
					}
				}
			}
			else if (_inputProfile.Pressed("START"))
			{
				_editControl = false;
				SFX.Play("consoleError");
			}
			else
			{
				_configInputMapping.deviceOverride = _inputProfile.lastActiveDevice;
				if (_configInputMapping.deviceOverride is GenericController)
				{
					_configInputMapping.deviceOverride = (_configInputMapping.deviceOverride as GenericController).device;
				}
				if (_selectedSetting.trigger != "ANY" && _configInputMapping.RunMappingUpdate(_selectedSetting.trigger, allowDupes: false))
				{
					_editControl = false;
					SFX.Play("consoleSelect");
					_madeControlChanges = true;
					_configInputMapping.deviceOverride = null;
					return;
				}
				_configInputMapping.deviceOverride = null;
			}
		}
		else if (_mode == PSMode.CreateProfile)
		{
			if (!_changeName)
			{
				if (_createSelection == PSCreateSelection.Controls && _inputProfile.Pressed("SELECT"))
				{
					_desiredMode = PSMode.EditControls;
					_selectedSetting = null;
					_controlPage = 0;
					_madeControlChanges = false;
					if (_configInputMapping == null)
					{
						_configInputMapping = Input.GetDefaultMapping(_inputProfile.lastActiveDevice.productName, _inputProfile.lastActiveDevice.productGUID, presets: false, makeClone: true, isEditing ? _profile : null).Clone();
					}
					SFX.Play("consoleTick");
				}
				if (_createSelection == PSCreateSelection.Mood)
				{
					if (_inputProfile.Pressed("MENULEFT"))
					{
						_moodVal = Maths.Clamp(_moodVal - 0.25f, 0f, 1f);
						SFX.Play("consoleTick");
					}
					if (_inputProfile.Pressed("MENURIGHT"))
					{
						_moodVal = Maths.Clamp(_moodVal + 0.25f, 0f, 1f);
						SFX.Play("consoleTick");
					}
				}
				if (_createSelection == PSCreateSelection.Color)
				{
					if (_inputProfile.Pressed("MENULEFT"))
					{
						_preferredColor = Maths.Clamp(_preferredColor - 1, -1, DG.MaxPlayers - 1);
						SFX.Play("consoleTick");
					}
					if (_inputProfile.Pressed("MENURIGHT"))
					{
						_preferredColor = Maths.Clamp(_preferredColor + 1, -1, DG.MaxPlayers - 1);
						SFX.Play("consoleTick");
					}
				}
				if (_inputProfile.Pressed("MENUDOWN") && _name != "" && _createSelection < PSCreateSelection.Accept)
				{
					_createSelection++;
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("MENUUP") && _name != "" && (int)_createSelection > (isEditing ? 1 : 0))
				{
					_createSelection--;
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("SELECT"))
				{
					if (_createSelection == PSCreateSelection.ChangeName)
					{
						if (!isEditing)
						{
							_changeName = true;
							if (_name == "")
							{
								_name = GetMaskName(1);
							}
							SFX.Play("consoleSelect", 0.4f);
						}
						else
						{
							SFX.Play("consoleError", 0.8f);
						}
					}
					else if (_createSelection == PSCreateSelection.Accept)
					{
						SaveSettings(isEditing, pAccepted: true);
						SFX.Play("consoleSelect", 0.4f);
					}
				}
				if (_inputProfile.Pressed("CANCEL"))
				{
					SaveSettings(isEditing, pAccepted: false);
					if (!isEditing)
					{
						_desiredMode = PSMode.SelectProfile;
					}
					else
					{
						_desiredMode = PSMode.SelectProfile;
						_mode = PSMode.SelectProfile;
						_open = false;
						_selector.fade = 1f;
						_fade = 0f;
						_selector.screen.DoFlashTransition();
					}
					SFX.Play("consoleCancel", 0.4f);
				}
			}
			else
			{
				InputProfile.repeat = true;
				Keyboard.repeat = true;
				if (_inputProfile.Pressed("SELECT"))
				{
					string realName = _name.Replace(" ", "");
					if (realName == "")
					{
						realName = "duckis91";
						_name = realName + " ";
						_currentLetter = 7;
					}
					List<Profile> profiles = Profiles.allCustomProfiles;
					bool taken = false;
					if (_selector == null || !_selector.isArcadeHatSelector)
					{
						foreach (Profile item in profiles)
						{
							if (item.name == realName)
							{
								taken = true;
								break;
							}
						}
					}
					if (taken)
					{
						_takenFlash = 1f;
					}
					else
					{
						_changeName = false;
					}
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("MENULEFT"))
				{
					_currentLetter--;
					if (_currentLetter < 0)
					{
						_currentLetter = 0;
					}
					else
					{
						_name = _name.Remove(_currentLetter + 1, 1);
						_name = _name.Insert(_currentLetter + 1, " ");
					}
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("MENURIGHT"))
				{
					_currentLetter++;
					if (_currentLetter > 8)
					{
						_currentLetter = 8;
					}
					else
					{
						_name = _name.Remove(_currentLetter, 1);
						if (_currentLetter > 0)
						{
							_name = _name.Insert(_currentLetter, _name[_currentLetter - 1].ToString() ?? "");
						}
					}
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("MENUUP"))
				{
					char c = _name[_currentLetter];
					int idx = GetCharIndex(c);
					idx++;
					if (idx >= _characters.Count)
					{
						idx = 0;
					}
					c = _characters[idx];
					_name = _name.Remove(_currentLetter, 1);
					_name = _name.Insert(_currentLetter, c.ToString() ?? "");
					_maskName = _maskName.Remove(_currentLetter, 1);
					_maskName = _maskName.Insert(_currentLetter, c.ToString() ?? "");
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("MENUDOWN"))
				{
					char c2 = _name[_currentLetter];
					int idx2 = GetCharIndex(c2);
					idx2--;
					if (idx2 < 0)
					{
						idx2 = _characters.Count - 1;
					}
					c2 = _characters[idx2];
					_name = _name.Remove(_currentLetter, 1);
					_name = _name.Insert(_currentLetter, c2.ToString() ?? "");
					_maskName = _maskName.Remove(_currentLetter, 1);
					_maskName = _maskName.Insert(_currentLetter, c2.ToString() ?? "");
					SFX.Play("consoleTick");
				}
				if (_inputProfile.Pressed("CANCEL"))
				{
					_desiredMode = PSMode.SelectProfile;
					SFX.Play("consoleCancel", 0.4f);
				}
			}
		}
		if (_slideTo != 0f && _slide != _slideTo)
		{
			_slide = Lerp.Float(_slide, _slideTo, 0.1f);
		}
		else if (_slideTo != 0f && _slide == _slideTo)
		{
			_slide = 0f;
			_slideTo = 0f;
			if (_desiredSelectorPosition != -1 && ProfileAlreadySelected(_profiles[_desiredSelectorPosition]))
			{
				_selectorPosition = _desiredSelectorPosition;
				if (_wasDown)
				{
					SelectDown();
				}
				else
				{
					SelectUp();
				}
			}
			else
			{
				_selectorPosition = _desiredSelectorPosition;
				if (!(Level.current is TeamSelect2))
				{
					if (_selectorPosition != -1)
					{
						_box.ChangeProfile(_profiles[_selectorPosition]);
						_profile = _profiles[_selectorPosition];
					}
					else
					{
						_box.ChangeProfile(null);
						_profile = _box.profile;
					}
				}
			}
		}
		_font.alpha = _fade;
		_font.depth = 0.96f;
		_font.scale = new Vec2(1f, 1f);
		if (_mode == PSMode.EditControlsConfirm)
		{
			Vec2 realPos = position;
			position = Vec2.Zero;
			_selector.screen.BeginDraw();
			string caption = "SAVE CHANGES?";
			_smallFont.Draw(caption, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _smallFont.GetWidth(caption) / 2f, base.y + 22f)), Colors.MenuOption * ((_controlPosition == 0) ? 1f : 0.6f), 0.95f);
			_ = new Vec2(base.x + base.width / 2f - 66f, base.y + 18f) + new Vec2(0.5f, 0f);
			_smallFont.Draw("YES", Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _smallFont.GetWidth("YES") / 2f, base.y + 34f)), Colors.MenuOption * ((_editControlSelection == 0) ? 1f : 0.6f), 0.95f);
			_smallFont.Draw("NO", Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _smallFont.GetWidth("NO") / 2f, base.y + 34f + 8f)), Colors.MenuOption * ((_editControlSelection == 1) ? 1f : 0.6f), 0.95f);
			string buttons = "@SELECT@";
			_font.Draw(buttons, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			buttons = "@CANCEL@";
			_font.Draw(buttons, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			position = realPos;
			_selector.screen.EndDraw();
		}
		else if (_mode == PSMode.EditControls)
		{
			Vec2 realPos2 = position;
			position = Vec2.Zero;
			_selector.screen.BeginDraw();
			InputProfile pro = _inputProfile;
			_smallFont.scale = new Vec2(1f, 1f);
			float yOff = 6f;
			string currentDevice = pro.lastActiveDevice.productName;
			if (currentDevice == null)
			{
				_desiredMode = PSMode.CreateProfile;
				SFX.Play("consoleError");
				return;
			}
			if (currentDevice == "Joy-Con (L)" || currentDevice == "Joy-Con (R)")
			{
				currentDevice = "Joy-Con (L)/(R)";
			}
			if (currentDevice.Length > 15)
			{
				currentDevice = currentDevice.Substring(0, 15) + "...";
			}
			if (_controlPosition == 0)
			{
				currentDevice = "< " + currentDevice + " >";
			}
			_smallFont.Draw(currentDevice, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _smallFont.GetWidth(currentDevice) / 2f, base.y + yOff)), Colors.MenuOption * ((_controlPosition == 0) ? 1f : 0.6f), 0.95f);
			Vec2 controlsDrawOffset = new Vec2(base.x + base.width / 2f - 66f, base.y + yOff + 9f) + new Vec2(0.5f, 0f);
			bool drewFirstLine = false;
			foreach (ControlSetting s2 in _controlSettingPages[_controlPage])
			{
				InputDevice d2 = _inputProfile.lastActiveDevice;
				if (d2 is GenericController)
				{
					d2 = (d2 as GenericController).device;
				}
				if (s2.condition != null && !s2.condition(d2))
				{
					continue;
				}
				string controlLine = s2.name;
				Vec2 pos = s2.position;
				if (pos.y == 0f)
				{
					drewFirstLine = true;
				}
				else if (!drewFirstLine && (_controlPage != 0 || s2 != _controlSettingPages[_controlPage][_controlSettingPages[_controlPage].Count - 1]))
				{
					pos.y -= 12f;
				}
				if (s2.trigger != "ANY")
				{
					controlLine += ":|DGBLUE|";
					if (!_editControl || _selectedSetting != s2)
					{
						Graphics.Draw(pro.lastActiveDevice.GetMapImage(_configInputMapping.map[s2.trigger]), controlsDrawOffset.x + pos.x + _smallFont.GetWidth(controlLine) - 2f, controlsDrawOffset.y + pos.y - 3f);
					}
					else
					{
						controlLine += "_";
					}
				}
				_smallFont.Draw(controlLine, Maths.RoundToPixel(new Vec2(pos.x, pos.y) + controlsDrawOffset), Colors.MenuOption * ((s2 == _selectedSetting) ? 1f : 0.6f), 0.95f);
			}
			if (!_editControl)
			{
				string buttons2 = "@SELECT@";
				_font.Draw(buttons2, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons2 = "@START@";
				_font.Draw(buttons2, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			else
			{
				string buttons3 = "@START@";
				_font.Draw(buttons3, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			position = realPos2;
			_selector.screen.EndDraw();
		}
		else if (_mode == PSMode.SelectProfile)
		{
			_pendingMaps.Clear();
			Vec2 realPos3 = position;
			position = Vec2.Zero;
			_selector.screen.BeginDraw();
			string pickTeam = "@LWING@PICK PROFILE@RWING@";
			_font.Draw(pickTeam, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(pickTeam) / 2f, base.y + 8f)), Color.White, 0.95f);
			float yOff2 = 8f;
			for (int i = 0; i < 7; i++)
			{
				int add = -3 + i;
				int index = ProfileIndexAdd(_selectorPosition, add);
				string name = "NEW PROFILE";
				bool n = true;
				bool def = false;
				if (index != -1)
				{
					if (Profiles.IsDefault(_profiles[index]))
					{
						name = "DEFAULT";
						def = true;
					}
					else
					{
						name = _profiles[index].name;
					}
					n = false;
					if (_profiles[index] == Profiles.experienceProfile)
					{
						name = "@RAINBOWICON@|DGBLUE|" + name + "|WHITE|";
					}
					else if (_profiles[index].steamID != 0L)
					{
						name = "@STEAMICON@|DGBLUE|" + name + "|WHITE|";
					}
				}
				string selectorString = null;
				if (_desiredSelectorPosition == index && (i == 3 || (_slideTo > 0f && i == 4) || (_slideTo < 0f && i == 2)))
				{
					selectorString = "> " + name + " <";
				}
				float middle = base.y + yOff2 + 33f;
				float ypos = base.y + yOff2 + (float)(i * 11) + (0f - _slide) * 11f;
				float distMult = Maths.Clamp((33f - Math.Abs(ypos - middle)) / 33f, 0f, 1f);
				float colorMult = distMult * Maths.NormalizeSection(distMult, 0f, 0.9f);
				float m = 0.2f;
				colorMult = ((distMult < 0.3f) ? (Maths.NormalizeSection(distMult, 0f, 0.3f) * m) : ((!(distMult < 0.8f)) ? (Maths.NormalizeSection(distMult, 0.8f, 1f) + m) : m));
				colorMult = Maths.Clamp(colorMult, 0f, 1f);
				bool taken2 = false;
				if ((_selector == null || !_selector.isArcadeHatSelector) && index != -1 && (Profiles.active.Contains(_profiles[index]) || Profiles.active.FirstOrDefault((Profile x) => x.linkedProfile == _profiles[index]) != null))
				{
					taken2 = true;
				}
				if (taken2)
				{
					name = name.Replace("|DGBLUE|", "");
				}
				_font.Draw(name, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(name) / 2f, ypos)), (taken2 ? Color.Red : (n ? Color.Lime : (def ? Colors.DGYellow : Colors.MenuOption))) * colorMult, 0.95f);
				if (selectorString != null)
				{
					_font.Draw(selectorString, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(selectorString) / 2f, ypos)), Color.White, 0.92f);
				}
			}
			float barY = yOff2 + 32f;
			Graphics.DrawRect(position + new Vec2(2f, barY), position + new Vec2(138f, barY + 9f), new Color(30, 30, 30) * _fade, 0.8f);
			string buttons4 = "@SELECT@";
			_font.Draw(buttons4, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			buttons4 = (HoveredProfileIsCustom() ? "@MENU2@" : "@CANCEL@");
			_font.Draw(buttons4, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			position = realPos3;
			_selector.screen.EndDraw();
		}
		else
		{
			if (_mode != PSMode.CreateProfile)
			{
				return;
			}
			Vec2 realPos4 = position;
			position = Vec2.Zero;
			_selector.screen.BeginDraw();
			string profileName = "NONAME";
			if (_name != "")
			{
				if (ParentalControls.RunProfanityCheck(ref _name) > 0)
				{
					profileName = "NONAME";
				}
				profileName = _name;
			}
			Vec2 namePos = new Vec2(base.x + 36f, base.y + 8f);
			if (!isEditing)
			{
				if (_changeName)
				{
					namePos.x -= 2f;
					for (int i2 = 0; i2 < 9; i2++)
					{
						Graphics.DrawRect(namePos + new Vec2(i2 * 8, 0f), namePos + new Vec2(i2 * 8 + 7, 7f), new Color(60, 60, 60), 0.8f);
						if (i2 == _currentLetter)
						{
							_spinnerArrows.frame = 0;
							Vec2 tlArrow = namePos + new Vec2(i2 * 8, -6f);
							Graphics.Draw(_spinnerArrows, tlArrow.x, tlArrow.y, 0.95f);
							_spinnerArrows.frame = 1;
							Vec2 brArrow = namePos + new Vec2(i2 * 8, 9f);
							Graphics.Draw(_spinnerArrows, brArrow.x, brArrow.y, 0.95f);
							Graphics.DrawRect(namePos + new Vec2(i2 * 8 - 2, -2f), namePos + new Vec2(i2 * 8 + 9, 9f), Color.White * 0.8f, 0.97f, filled: false);
						}
					}
					_font.Draw(profileName, Maths.RoundToPixel(namePos), Color.Lime * ((_createSelection == PSCreateSelection.ChangeName) ? 1f : 0.6f), 0.95f);
					namePos.x += 2f;
					string arrows = ">              <";
					_font.Draw(arrows, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(arrows) / 2f, namePos.y)), Color.White * ((_createSelection == PSCreateSelection.ChangeName) ? 1f : 0.6f), 0.95f);
					if (_takenFlash > 0.05f)
					{
						string nameTaken = "Name Taken";
						_font.Draw(nameTaken, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(nameTaken) / 2f, namePos.y)), Color.Red * _takenFlash, 0.97f);
						Graphics.DrawRect(new Vec2(base.x + 20f, namePos.y), new Vec2(base.x + base.width - 20f, namePos.y + 8f), Color.Black, 0.96f);
					}
				}
				else
				{
					profileName = profileName.Replace(" ", "");
					profileName = ((_createSelection != PSCreateSelection.ChangeName) ? ("@LWING@" + profileName.Reduced(12) + "@RWING@") : ("> " + profileName.Reduced(12) + " <"));
					_font.Draw(profileName, Maths.RoundToPixel(new Vec2(base.x + 2f + base.width / 2f - _font.GetWidth(profileName) / 2f, namePos.y)), Color.White * ((_createSelection == PSCreateSelection.ChangeName) ? 1f : 0.6f), 0.95f);
				}
			}
			else
			{
				string createProfile = "@LWING@" + profileName.Reduced(12) + "@RWING@";
				_font.Draw(createProfile, Maths.RoundToPixel(new Vec2(base.x + base.width / 2f - _font.GetWidth(createProfile) / 2f, base.y + 8f)), Color.White * (1f - Math.Min(1f, _takenFlash * 2f)), 0.95f);
			}
			namePos.y += 14f;
			string setMood = "            ";
			if (_createSelection == PSCreateSelection.Mood)
			{
				setMood = "< " + setMood + " >";
			}
			_font.Draw(setMood, base.x + base.width / 2f - _font.GetWidth(setMood) / 2f, namePos.y, Color.White * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f), 0.95f);
			Graphics.DrawLine(new Vec2(base.x + base.width / 4f + 4f, namePos.y + 5f), new Vec2(base.x + base.width / 4f * 3f, namePos.y + 5f), Colors.MenuOption * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f), 2f, 0.95f);
			float barW = 60f;
			Graphics.DrawLine(new Vec2(base.x + base.width / 2f - barW / 2f + barW * _moodVal + 2f, namePos.y + 1f), new Vec2(base.x + base.width / 2f - barW / 2f + barW * _moodVal + 2f, namePos.y + 4f), Colors.MenuOption * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f), 3f, 0.95f);
			Graphics.DrawLine(new Vec2(base.x + base.width / 2f - barW / 2f + barW * _moodVal + 2f, namePos.y + 6f), new Vec2(base.x + base.width / 2f - barW / 2f + barW * _moodVal + 2f, namePos.y + 9f), Colors.MenuOption * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f), 3f, 0.95f);
			_happyIcons.color = Color.White * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f);
			_happyIcons.alpha = _fade;
			_happyIcons.frame = (int)Math.Round(_moodVal * 4f);
			_happyIcons.depth = 0.95f;
			Graphics.Draw(_happyIcons, base.x + base.width / 6f + 2f, namePos.y + 4f);
			_angryIcons.color = Color.White * ((_createSelection == PSCreateSelection.Mood) ? 1f : 0.6f);
			_angryIcons.alpha = _fade;
			_angryIcons.frame = (int)Math.Round((1f - _moodVal) * 4f);
			_angryIcons.depth = 0.95f;
			Graphics.Draw(_angryIcons, base.x + base.width / 6f * 5f, namePos.y + 4f);
			namePos.y += 16f;
			string col = ((_preferredColor >= 0) ? "COLOR" : "NO COLOR");
			if (_createSelection == PSCreateSelection.Color)
			{
				col = "< " + col + " >";
			}
			if (_preferredColor >= 0)
			{
				Graphics.DrawRect(new Vec2(base.x + 20f, namePos.y - 2f), new Vec2(base.x + (base.width - 20f), namePos.y + 9f), Persona.all.ElementAt(_preferredColor).colorDark.ToColor() * ((_createSelection == PSCreateSelection.Color) ? 1f : 0.6f), 0.93f, filled: false);
				_font.Draw(col, Maths.RoundToPixel(new Vec2(base.x + 2f + base.width / 2f - _font.GetWidth(col) / 2f, namePos.y)), Persona.all.ElementAt(_preferredColor).color.ToColor() * ((_createSelection == PSCreateSelection.Color) ? 1f : 0.6f), 0.95f);
			}
			else
			{
				Graphics.DrawRect(new Vec2(base.x + 20f, namePos.y - 2f), new Vec2(base.x + (base.width - 20f), namePos.y + 9f), Colors.BlueGray * ((_createSelection == PSCreateSelection.Color) ? 1f : 0.6f), 0.93f, filled: false);
				_font.Draw(col, Maths.RoundToPixel(new Vec2(base.x + 2f + base.width / 2f - _font.GetWidth(col) / 2f, namePos.y)), Colors.BlueGray * ((_createSelection == PSCreateSelection.Color) ? 1f : 0.6f), 0.95f);
			}
			namePos.y += 12f;
			string controls = "CONTROLS";
			if (_createSelection == PSCreateSelection.Controls)
			{
				controls = "> " + controls + " <";
			}
			_font.Draw(controls, Maths.RoundToPixel(new Vec2(base.x + 2f + base.width / 2f - _font.GetWidth(controls) / 2f, namePos.y)), Colors.MenuOption * ((_createSelection == PSCreateSelection.Controls) ? 1f : 0.6f), 0.95f);
			string ok = "OK";
			if (_createSelection == PSCreateSelection.Accept)
			{
				ok = "> " + ok + " <";
			}
			namePos.y += 12f;
			_font.Draw(ok, Maths.RoundToPixel(new Vec2(base.x + 2f + base.width / 2f - _font.GetWidth(ok) / 2f, namePos.y)), Colors.MenuOption * ((_createSelection == PSCreateSelection.Accept) ? 1f : 0.6f), 0.95f);
			if (_changeName)
			{
				string buttons5 = "@DPAD@";
				if (_selector != null && (_selector.profileBoxNumber == 0 || _selector.profileBoxNumber == 2))
				{
					buttons5 = "@WASD@";
				}
				_font.Draw(buttons5, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons5 = "@SELECT@";
				_font.Draw(buttons5, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			else if (_createSelection == PSCreateSelection.ChangeName)
			{
				string buttons6 = "@SELECT@";
				_font.Draw(buttons6, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons6 = "@CANCEL@";
				_font.Draw(buttons6, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			else if (_createSelection == PSCreateSelection.Mood)
			{
				string buttons7 = "@DPAD@";
				if (_selector != null && (_selector.profileBoxNumber == 0 || _selector.profileBoxNumber == 2))
				{
					buttons7 = "@WASD@";
				}
				_font.Draw(buttons7, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons7 = "@CANCEL@";
				_font.Draw(buttons7, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			else if (_createSelection == PSCreateSelection.Color)
			{
				string buttons8 = "@DPAD@";
				if (_selector != null && (_selector.profileBoxNumber == 0 || _selector.profileBoxNumber == 2))
				{
					buttons8 = "@WASD@";
				}
				_font.Draw(buttons8, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons8 = "@CANCEL@";
				_font.Draw(buttons8, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			else
			{
				string buttons9 = "@SELECT@";
				_font.Draw(buttons9, 4f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
				buttons9 = "@CANCEL@";
				_font.Draw(buttons9, 122f, 79f, new Color(180, 180, 180), 0.95f, _inputProfile);
			}
			position = realPos4;
			_selector.screen.EndDraw();
		}
	}

	public void Open(Profile p)
	{
		_desiredSelectorPosition = (_selectorPosition = 0);
		if (_box == null && Level.current is TeamSelect2)
		{
			_box = (Level.current as TeamSelect2).GetBox(p.networkIndex);
		}
		if (_box == null)
		{
			return;
		}
		isEditing = false;
		_inputProfile = p.inputProfile;
		_profile = (_starterProfile = p);
		RebuildProfileList();
		for (int i = 0; i < _profiles.Count; i++)
		{
			if (_profiles[i] == _profile)
			{
				_selectorPosition = i;
				break;
			}
		}
		_desiredSelectorPosition = _selectorPosition;
		_open = true;
		_fade = 1f;
	}

	private bool ProfileAlreadySelected(Profile p)
	{
		if (_profile.linkedProfile != null)
		{
			if (p != null && Profiles.active.FirstOrDefault((Profile x) => x.linkedProfile == p) != null)
			{
				return p != _profile.linkedProfile;
			}
			return false;
		}
		if (p != null && Profiles.active.Contains(p))
		{
			return p != _profile;
		}
		return false;
	}

	public void EditProfile(Profile p)
	{
		Open(p);
		isEditing = true;
		_mode = PSMode.EditProfile;
		_desiredMode = PSMode.EditProfile;
		_name = p.name;
		_desiredMode = PSMode.CreateProfile;
		_changeName = false;
		_currentLetter = 0;
		_moodVal = p.funslider;
		_preferredColor = p.preferredColor;
		_createSelection = PSCreateSelection.Accept;
		_configInputMapping = Input.GetDefaultMapping(_inputProfile.lastActiveDevice.productName, _inputProfile.lastActiveDevice.productGUID, presets: false, makeClone: true, isEditing ? _profile : null).Clone();
	}

	private int ProfileIndexAdd(int index, int plus)
	{
		if (_profiles.Count == 0)
		{
			return -1;
		}
		int val;
		for (val = index + plus; val >= _profiles.Count; val -= _profiles.Count + 1)
		{
		}
		for (; val < -1; val += _profiles.Count + 1)
		{
		}
		return val;
	}

	public override void Draw()
	{
		if (_fade < 0.01f)
		{
			return;
		}
		if (_mode == PSMode.EditControlsConfirm)
		{
			_selector.firstWord = "OK";
			_selector.secondWord = "BACK";
		}
		else if (_mode == PSMode.CreateProfile)
		{
			if (_changeName)
			{
				_selector.firstWord = "MOVE";
				_selector.secondWord = "OK";
			}
			else if (_createSelection == PSCreateSelection.ChangeName)
			{
				_selector.firstWord = "ALTER";
				_selector.secondWord = "BACK";
			}
			else if (_createSelection == PSCreateSelection.Mood)
			{
				_selector.firstWord = "MOVE";
				_selector.secondWord = "BACK";
			}
			else
			{
				_selector.firstWord = "OK";
				_selector.secondWord = "BACK";
			}
		}
		else if (_mode == PSMode.SelectProfile)
		{
			if (!HoveredProfileIsCustom())
			{
				_selector.firstWord = "PICK";
				_selector.secondWord = "BACK";
			}
			else
			{
				_selector.firstWord = "PICK";
				_selector.secondWord = "KILL";
			}
		}
		else if (!_editControl)
		{
			_selector.firstWord = "EDIT";
			_selector.secondWord = "SAVE";
		}
		else
		{
			_selector.firstWord = "BACK";
			_selector.secondWord = "";
		}
	}
}
