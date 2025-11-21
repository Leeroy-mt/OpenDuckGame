using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DuckGame;

public class UIModManagement : UIMenu
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;

		[MarshalAs(UnmanagedType.U4)]
		public int wFunc;

		public string pFrom;

		public string pTo;

		public short fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public IntPtr hNameMappings;

		public string lpszProgressTitle;
	}

	private sealed class UI_ModSettings : Mod
	{
	}

	private const int FO_DELETE = 3;

	private const int FOF_ALLOWUNDO = 64;

	private const int FOF_NOCONFIRMATION = 16;

	private UIMenu _openOnClose;

	private Sprite _moreArrow;

	private Sprite _noImage;

	private Sprite _steamIcon;

	private SpriteMap _cursor;

	private SpriteMap _localIcon;

	private SpriteMap _newIcon;

	private SpriteMap _settingsIcon;

	private IList<Mod> _mods;

	private int _hoverIndex;

	private UIBox _box;

	private FancyBitmapFont _fancyFont;

	private int _maxModsToShow;

	private UIMenuItem _uploadItem;

	private UIMenuItem _disableOrEnableItem;

	private UIMenuItem _deleteOrUnsubItem;

	private UIMenuItem _visitItem;

	public UIMenu _editModMenu;

	public UIMenu _yesNoMenu;

	private UIMenuItem _yesNoYes;

	private UIMenuItem _yesNoNo;

	private SteamUploadDialog _uploadDialog;

	private WorkshopItem _transferItem;

	private bool _transferring;

	private bool _awaitingChanges;

	private Textbox _updateTextBox;

	private Rectangle _updateButton;

	private string _updateButtonText = "UPDATE MOD!";

	private int _pressWait;

	private Sprite _modErrorIcon;

	public UIMenu _modSettingsMenu;

	private bool _showingMenu;

	private bool _draggingScrollbar;

	private Vec2 _oldPos;

	private Mod _selectedMod;

	private bool modsChanged;

	public string showingError;

	private bool fixView = true;

	private const int boxHeight = 36;

	private const int scrollWidth = 12;

	private const int boxSideMargin = 14;

	private const int scrollBarHeight = 32;

	private int scrollBarTop;

	private int scrollBarBottom;

	private int scrollBarScrollableHeight;

	private int scrollBarOffset;

	private int _scrollItemOffset;

	private bool _gamepadMode = true;

	private bool _needsUpdateNotes;

	public override void Close()
	{
		if (!fixView)
		{
			_showingMenu = false;
			_editModMenu.Close();
			Layer.HUD.camera.width /= 2f;
			Layer.HUD.camera.height /= 2f;
			fixView = true;
			DevConsole.RestoreDevConsole();
		}
		base.Close();
	}

	private void EnableDisableMod()
	{
		_awaitingChanges = true;
		if (_selectedMod.configuration.disabled)
		{
			_selectedMod.configuration.Enable();
		}
		else
		{
			_selectedMod.configuration.Disable();
		}
		modsChanged = true;
		_editModMenu.Close();
		Open();
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	private static void DeleteFileOrFolder(string path)
	{
		SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
		{
			wFunc = 3,
			pFrom = path + "\0\0",
			fFlags = 80
		};
		SHFileOperation(ref fileop);
	}

	private void DeleteMod()
	{
		ShowYesNo(_editModMenu, delegate
		{
			_awaitingChanges = true;
			if (_selectedMod.configuration.workshopID == 0L)
			{
				DeleteFileOrFolder(_selectedMod.configuration.directory);
			}
			else
			{
				Steam.WorkshopUnsubscribe(_selectedMod.configuration.workshopID);
			}
			_mods.Remove(_selectedMod);
			_hoverIndex = -1;
			_yesNoMenu.Close();
			_editModMenu.Close();
			Open();
		});
	}

	private void ShowYesNo(UIMenu goBackTo, UIMenuActionCallFunction.Function onYes)
	{
		_yesNoNo.menuAction = new UIMenuActionCallFunction(delegate
		{
			_yesNoMenu.Close();
			goBackTo.Open();
		});
		_yesNoYes.menuAction = new UIMenuActionCallFunction(onYes);
		new UIMenuActionOpenMenu(_editModMenu, _yesNoMenu).Activate();
	}

	private void UploadMod()
	{
		_editModMenu.Close();
		Open();
		if (_selectedMod.configuration.workshopID == 0L)
		{
			_transferItem = Steam.CreateItem();
		}
		else
		{
			_transferItem = new WorkshopItem(_selectedMod.configuration.workshopID);
			_needsUpdateNotes = true;
			_updateTextBox.GainFocus();
			_gamepadMode = false;
		}
		_transferring = false;
	}

	private void VisitModPage()
	{
		_editModMenu.Close();
		Open();
		Steam.OverlayOpenURL("http://steamcommunity.com/sharedfiles/filedetails/?id=" + _selectedMod.configuration.workshopID);
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = directoryInfo.GetDirectories();
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, overwrite: false);
			File.SetAttributes(temppath, FileAttributes.Normal);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array = dirs;
			foreach (DirectoryInfo subdir in array)
			{
				string temppath2 = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath2, copySubDirs);
			}
		}
	}

	public UIModManagement(UIMenu openOnClose, string title, float xpos, float ypos, float wide = -1f, float high = -1f, string conString = "", InputProfile conProfile = null)
		: base(title, xpos, ypos, wide, high, conString, conProfile)
	{
		_splitter.topSection.components[0].align = UIAlign.Left;
		_openOnClose = openOnClose;
		_moreArrow = new Sprite("moreArrow");
		_moreArrow.CenterOrigin();
		_steamIcon = new Sprite("steamIconSmall");
		_steamIcon.scale = new Vec2(1f) / 2f;
		_localIcon = new SpriteMap("iconSheet", 16, 16);
		_localIcon.scale = new Vec2(1f) / 2f;
		_localIcon.SetFrameWithoutReset(1);
		_modErrorIcon = new Sprite("modloadError");
		_newIcon = new SpriteMap("presents", 16, 16);
		_newIcon.scale = new Vec2(2f);
		_newIcon.SetFrameWithoutReset(0);
		_settingsIcon = new SpriteMap("settingsWrench", 16, 16);
		_settingsIcon.scale = new Vec2(2f);
		_noImage = new Sprite("notexture");
		_noImage.scale = new Vec2(2f);
		_cursor = new SpriteMap("cursors", 16, 16);
		_mods = ModLoader.allMods.Where((Mod a) => !(a is CoreMod)).ToList();
		_mods.Insert(0, new UI_ModSettings());
		_mods.Add(null);
		_maxModsToShow = 8;
		_box = new UIBox(0f, 0f, -1f, _maxModsToShow * 36, vert: true, isVisible: false);
		Add(_box);
		_fancyFont = new FancyBitmapFont("smallFont");
		_fancyFont.maxWidth = (int)base.width - 60;
		_fancyFont.maxRows = 2;
		scrollBarOffset = 0;
		_editModMenu = new UIMenu("<mod name>", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@SELECT@SELECT");
		_editModMenu.Add(_disableOrEnableItem = new UIMenuItem("DISABLE", new UIMenuActionCallFunction(EnableDisableMod)));
		_deleteOrUnsubItem = new UIMenuItem("DELETE", new UIMenuActionCallFunction(DeleteMod));
		_uploadItem = new UIMenuItem("UPLOAD", new UIMenuActionCallFunction(UploadMod));
		_visitItem = new UIMenuItem("VISIT PAGE", new UIMenuActionCallFunction(VisitModPage));
		_editModMenu.Add(new UIText(" ", Color.White));
		_editModMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_editModMenu, this)));
		_editModMenu.Close();
		_yesNoMenu = new UIMenu("ARE YOU SURE?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@SELECT@SELECT");
		_yesNoMenu.Add(_yesNoYes = new UIMenuItem("YES"));
		_yesNoMenu.Add(_yesNoNo = new UIMenuItem("NO"));
		_yesNoMenu.Close();
		_updateTextBox = new Textbox(0f, 0f, 0f, 0f);
		_updateTextBox.depth = 0.9f;
		_updateTextBox.maxLength = 5000;
		_modSettingsMenu = new UIMenu("@WRENCH@MOD SETTINGS@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@WASD@ADJUST @CANCEL@EXIT");
		_modSettingsMenu.Add(new UIText("If CRASH DISABLE is ON,", Colors.DGBlue));
		_modSettingsMenu.Add(new UIText("a mod will automatically be", Colors.DGBlue));
		_modSettingsMenu.Add(new UIText(" disabled if it causes", Colors.DGBlue));
		_modSettingsMenu.Add(new UIText("the game to crash.", Colors.DGBlue));
		_modSettingsMenu.Add(new UIText(" ", Colors.DGBlue));
		_modSettingsMenu.Add(new UIMenuItemToggle("CRASH DISABLE", null, new FieldBinding(Options.Data, "disableModOnCrash")));
		_modSettingsMenu.Add(new UIMenuItemToggle("LOAD FAILURE DISABLE", null, new FieldBinding(Options.Data, "disableModOnLoadFailure")));
		_modSettingsMenu.Add(new UIMenuItemToggle("SHOW NETWORK WARNING", null, new FieldBinding(Options.Data, "showNetworkModWarning")));
		_modSettingsMenu.Add(new UIText(" ", Colors.DGBlue));
		_modSettingsMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_modSettingsMenu, this), UIAlign.Center, default(Color), backButton: true));
		_modSettingsMenu.Close();
	}

	public override void Open()
	{
		if (_uploadDialog == null)
		{
			_uploadDialog = new SteamUploadDialog();
			Level.Add(_uploadDialog);
			Level.current.things.RefreshState();
		}
		_pressWait = 30;
		base.Open();
		DevConsole.SuppressDevConsole();
		_oldPos = Mouse.positionScreen;
	}

	public override void Update()
	{
		if (_uploadDialog != null && _uploadDialog.opened)
		{
			Editor.clickedMenu = false;
			Editor.inputMode = EditorInput.Mouse;
			Level.current.things.RefreshState();
			{
				foreach (ContextMenu item in Level.current.things[typeof(ContextMenu)])
				{
					item.Update();
				}
				return;
			}
		}
		if (_pressWait > 0)
		{
			_pressWait--;
		}
		if (showingError != null)
		{
			_controlString = "@CANCEL@BACK";
			if (Input.Pressed("QUACK"))
			{
				showingError = null;
			}
			base.Update();
			return;
		}
		if (_editModMenu.open)
		{
			if (!UIMenu.globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.Escape)))
			{
				_editModMenu.Close();
				Open();
				return;
			}
		}
		else if (_modSettingsMenu.open)
		{
			if (!UIMenu.globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.Escape)))
			{
				_modSettingsMenu.Close();
				Open();
				return;
			}
		}
		else if (base.open)
		{
			if (_transferItem != null && !_needsUpdateNotes)
			{
				if (!_transferring)
				{
					if (_transferItem.result == SteamResult.OK)
					{
						WorkshopItemData data = new WorkshopItemData();
						if (_selectedMod.configuration.workshopID == 0L)
						{
							_selectedMod.configuration.SetWorkshopID(_transferItem.id);
							data.name = _selectedMod.configuration.displayName;
							data.description = _selectedMod.configuration.description;
							data.visibility = RemoteStoragePublishedFileVisibility.Private;
							data.tags = new List<string>();
							data.tags.Add("Mod");
							if (_selectedMod.configuration.modType == ModConfiguration.Type.MapPack)
							{
								data.tags.Add("Map Pack");
							}
							else if (_selectedMod.configuration.modType == ModConfiguration.Type.HatPack)
							{
								data.tags.Add("Hat Pack");
							}
							else if (_selectedMod.configuration.modType == ModConfiguration.Type.Reskin)
							{
								data.tags.Add("Texture Pack");
							}
						}
						else
						{
							data.changeNotes = _updateTextBox.text;
						}
						string screenshotPath = _selectedMod.generateAndGetPathToScreenshot;
						data.previewPath = screenshotPath;
						string folderPath = DuckFile.workshopDirectory + _transferItem.id + "/content";
						if (Directory.Exists(folderPath))
						{
							Directory.Delete(folderPath, recursive: true);
						}
						DuckFile.CreatePath(folderPath);
						DirectoryCopy(_selectedMod.configuration.directory, folderPath + "/" + _selectedMod.configuration.name, copySubDirs: true);
						if (Directory.Exists(folderPath + _selectedMod.configuration.name + "/build"))
						{
							Directory.Delete(folderPath + _selectedMod.configuration.name + "/build", recursive: true);
						}
						if (Directory.Exists(folderPath + _selectedMod.configuration.name + "/.vs"))
						{
							Directory.Delete(folderPath + _selectedMod.configuration.name + "/.vs", recursive: true);
						}
						if (File.Exists(folderPath + _selectedMod.configuration.name + "/" + _selectedMod.configuration.name + "_compiled.dll"))
						{
							string path = folderPath + _selectedMod.configuration.name + "/" + _selectedMod.configuration.name + "_compiled.dll";
							File.SetAttributes(path, FileAttributes.Normal);
							File.Delete(path);
						}
						if (File.Exists(folderPath + _selectedMod.configuration.name + "/" + _selectedMod.configuration.name + "_compiled.hash"))
						{
							string path2 = folderPath + _selectedMod.configuration.name + "/" + _selectedMod.configuration.name + "_compiled.hash";
							File.SetAttributes(path2, FileAttributes.Normal);
							File.Delete(path2);
						}
						data.contentFolder = folderPath;
						_transferItem.ApplyWorkshopData(data);
						if (_transferItem.needsLegal)
						{
							Steam.ShowWorkshopLegalAgreement(_transferItem.id.ToString());
						}
						_transferring = true;
						_transferItem.ResetProcessing();
					}
				}
				else if (_transferItem.finishedProcessing)
				{
					Steam.OverlayOpenURL("http://steamcommunity.com/sharedfiles/filedetails/?id=" + _transferItem.id);
					Directory.Delete(DuckFile.workshopDirectory + _transferItem.id + "/", recursive: true);
					_transferItem.ResetProcessing();
					_transferItem = null;
					_transferring = false;
				}
				base.Update();
				return;
			}
			if (_gamepadMode)
			{
				if (_hoverIndex < 0)
				{
					_hoverIndex = 0;
				}
			}
			else
			{
				_hoverIndex = -1;
				for (int i = 0; i < _maxModsToShow && _scrollItemOffset + i < _mods.Count; i++)
				{
					float boxLeft = _box.x - _box.halfWidth;
					float boxTop = _box.y - _box.halfHeight + (float)(36 * i);
					if (new Rectangle((int)boxLeft, (int)boxTop, (int)_box.width - 14, 36f).Contains(Mouse.position))
					{
						_hoverIndex = _scrollItemOffset + i;
						break;
					}
				}
			}
			if (_transferItem != null)
			{
				if (_updateTextBox != null)
				{
					Editor.hoverTextBox = false;
					_updateTextBox.position = new Vec2(_box.x - _box.halfWidth + 16f, _box.y - _box.halfHeight + 48f);
					_updateTextBox.size = new Vec2(_box.width - 32f, _box.height - 80f);
					_updateTextBox._maxLines = (int)(_updateTextBox.size.y / (float)_fancyFont.characterHeight);
					_updateTextBox.Update();
					float sw = Graphics.GetStringWidth(_updateButtonText, thinButtons: false, 2f);
					float sh = Graphics.GetStringHeight(_updateButtonText) * 2f;
					_updateButton = new Rectangle(_box.x - sw / 2f, _box.y + _box.halfHeight - 24f, sw, sh);
					if (_updateButton.Contains(Mouse.position) && Mouse.left == InputState.Pressed)
					{
						_needsUpdateNotes = false;
						_updateTextBox.LoseFocus();
					}
					else if (Keyboard.Pressed(Keys.Escape))
					{
						_needsUpdateNotes = false;
						_transferItem = null;
						_updateTextBox.LoseFocus();
						new UIMenuActionOpenMenu(this, _editModMenu).Activate();
						return;
					}
				}
			}
			else if (_hoverIndex != -1)
			{
				_selectedMod = _mods[_hoverIndex];
				if (_selectedMod is UI_ModSettings)
				{
					_controlString = "@WASD@@SELECT@SETTINGS @CANCEL@BACK";
				}
				else if (_selectedMod != null && _selectedMod.configuration.error != null)
				{
					if (_selectedMod.configuration.forceHarmonyLegacyLoad)
					{
						_controlString = "@WASD@@SELECT@ADJUST @MENU1@TOGGLE @MENU2@DISABLE FORCED LOAD @START@SHOW ERROR";
					}
					else
					{
						_controlString = "@WASD@@SELECT@ADJUST @MENU1@TOGGLE @MENU2@FORCE LEGACY LOAD @START@SHOW ERROR";
					}
				}
				else
				{
					_controlString = "@WASD@@SELECT@ADJUST @MENU1@TOGGLE @CANCEL@BACK";
				}
				if (Input.Pressed("MENU1"))
				{
					if (_selectedMod != null && _selectedMod.configuration != null)
					{
						if (_selectedMod.configuration.disabled)
						{
							_selectedMod.configuration.Enable();
						}
						else
						{
							_selectedMod.configuration.Disable();
						}
						_selectedMod.configuration.error = null;
						modsChanged = true;
						SFX.Play("rockHitGround", 0.8f);
					}
				}
				else if (_selectedMod != null && _selectedMod.configuration != null && _selectedMod.configuration.error != null && Input.Pressed("MENU2"))
				{
					if (_selectedMod.configuration != null)
					{
						_selectedMod.configuration.forceHarmonyLegacyLoad = !_selectedMod.configuration.forceHarmonyLegacyLoad;
						ModLoader.DisabledModsChanged();
						modsChanged = true;
						SFX.Play("rockHitGround", 0.8f);
					}
				}
				else
				{
					if (Input.Pressed("START") && _selectedMod != null && _selectedMod.configuration != null && _selectedMod.configuration.error != null)
					{
						string text = DuckFile.saveDirectory + "error_info.txt";
						File.WriteAllText(text, _selectedMod.configuration.error);
						Process.Start(text);
						SFX.Play("rockHitGround", 0.8f);
						return;
					}
					if ((Input.Pressed("SELECT") && _pressWait == 0 && _gamepadMode) || (!_gamepadMode && Mouse.left == InputState.Pressed))
					{
						if (_selectedMod != null)
						{
							if (_selectedMod is UI_ModSettings)
							{
								SFX.Play("rockHitGround", 0.8f);
								_modSettingsMenu.dirty = true;
								new UIMenuActionOpenMenu(this, _modSettingsMenu).Activate();
								return;
							}
							if (!_selectedMod.configuration.loaded)
							{
								_editModMenu.title = "|YELLOW|" + _selectedMod.configuration.name;
							}
							else
							{
								_editModMenu.title = "|YELLOW|" + _selectedMod.configuration.displayName;
							}
							_editModMenu.Remove(_deleteOrUnsubItem);
							_editModMenu.Remove(_uploadItem);
							_editModMenu.Remove(_visitItem);
							if (!_selectedMod.configuration.isWorkshop && _selectedMod.configuration.loaded)
							{
								if (_selectedMod.configuration.workshopID != 0L)
								{
									_uploadItem.text = "UPDATE";
								}
								else
								{
									_uploadItem.text = "UPLOAD";
								}
								_editModMenu.Insert(_uploadItem, 1);
							}
							if (!_selectedMod.configuration.isWorkshop && !_selectedMod.configuration.loaded)
							{
								_deleteOrUnsubItem.text = "DELETE";
								_editModMenu.Insert(_deleteOrUnsubItem, 1);
							}
							else if (_selectedMod.configuration.isWorkshop)
							{
								_deleteOrUnsubItem.text = "UNSUBSCRIBE";
								_editModMenu.Insert(_deleteOrUnsubItem, 1);
							}
							if (_selectedMod.configuration.isWorkshop)
							{
								_editModMenu.Insert(_visitItem, 1);
							}
							_disableOrEnableItem.text = (_selectedMod.configuration.disabled ? "ENABLE" : "DISABLE");
							_editModMenu.dirty = true;
							SFX.Play("rockHitGround", 0.8f);
							new UIMenuActionOpenMenu(this, _editModMenu).Activate();
							return;
						}
						Steam.OverlayOpenURL("http://steamcommunity.com/workshop/browse/?appid=312530&searchtext=&childpublishedfileid=0&browsesort=trend&section=readytouseitems&requiredtags%5B%5D=Mod");
					}
				}
			}
			else
			{
				_selectedMod = null;
			}
			if (_gamepadMode)
			{
				_draggingScrollbar = false;
				if (Input.Pressed("MENUDOWN"))
				{
					_hoverIndex++;
				}
				else if (Input.Pressed("MENUUP"))
				{
					_hoverIndex--;
				}
				if (Input.Pressed("STRAFE"))
				{
					_hoverIndex -= 10;
				}
				else if (Input.Pressed("RAGDOLL"))
				{
					_hoverIndex += 10;
				}
				if (_hoverIndex < 0)
				{
					_hoverIndex = 0;
				}
				if ((_oldPos - Mouse.positionScreen).lengthSq > 200f)
				{
					_gamepadMode = false;
				}
			}
			else
			{
				if (!_draggingScrollbar)
				{
					if (Mouse.left == InputState.Pressed && ScrollBarBox().Contains(Mouse.position))
					{
						_draggingScrollbar = true;
						_oldPos = Mouse.position;
					}
					if (Mouse.scroll > 0f)
					{
						_scrollItemOffset += 5;
						_hoverIndex += 5;
					}
					else if (Mouse.scroll < 0f)
					{
						_scrollItemOffset -= 5;
						_hoverIndex -= 5;
						if (_hoverIndex < 0)
						{
							_hoverIndex = 0;
						}
					}
				}
				else if (Mouse.left != InputState.Down)
				{
					_draggingScrollbar = false;
				}
				else
				{
					Vec2 delta = Mouse.position - _oldPos;
					_oldPos = Mouse.position;
					scrollBarOffset += (int)delta.y;
					if (scrollBarOffset > scrollBarScrollableHeight)
					{
						scrollBarOffset = scrollBarScrollableHeight;
					}
					else if (scrollBarOffset < 0)
					{
						scrollBarOffset = 0;
					}
					float heightScrolled = (float)scrollBarOffset / (float)scrollBarScrollableHeight;
					_scrollItemOffset = (int)((float)(_mods.Count - _maxModsToShow) * heightScrolled);
				}
				if (Input.Pressed("ANY"))
				{
					_gamepadMode = true;
					_oldPos = Mouse.positionScreen;
				}
			}
			if (_scrollItemOffset < 0)
			{
				_scrollItemOffset = 0;
			}
			else if (_scrollItemOffset > Math.Max(0, _mods.Count - _maxModsToShow))
			{
				_scrollItemOffset = Math.Max(0, _mods.Count - _maxModsToShow);
			}
			if (_hoverIndex >= _mods.Count)
			{
				_hoverIndex = _mods.Count - 1;
			}
			else if (_hoverIndex >= _scrollItemOffset + _maxModsToShow)
			{
				_scrollItemOffset += _hoverIndex - (_scrollItemOffset + _maxModsToShow) + 1;
			}
			else if (_hoverIndex >= 0 && _hoverIndex < _scrollItemOffset)
			{
				_scrollItemOffset -= _scrollItemOffset - _hoverIndex;
			}
			if (_scrollItemOffset != 0)
			{
				scrollBarOffset = (int)Lerp.FloatSmooth(0f, scrollBarScrollableHeight, (float)_scrollItemOffset / (float)(_mods.Count - _maxModsToShow));
			}
			else
			{
				scrollBarOffset = 0;
			}
			if (!Editor.hoverTextBox && !UIMenu.globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.Escape)))
			{
				if (modsChanged)
				{
					Close();
					MonoMain.pauseMenu = DuckNetwork.OpenModsRestartWindow(_openOnClose);
				}
				else
				{
					new UIMenuActionOpenMenu(this, _openOnClose).Activate();
				}
				modsChanged = false;
				return;
			}
		}
		if (_showingMenu)
		{
			HUD.CloseAllCorners();
			_showingMenu = false;
		}
		base.Update();
	}

	private Rectangle ScrollBarBox()
	{
		return new Rectangle(_box.x + _box.halfWidth - 12f + 1f, _box.y - _box.halfHeight + 1f + (float)scrollBarOffset, 10f, 32f);
	}

	public override void Draw()
	{
		if (base.open)
		{
			if (Mouse.available && !_gamepadMode)
			{
				_cursor.depth = 1f;
				_cursor.scale = new Vec2(1f, 1f);
				_cursor.position = Mouse.position;
				_cursor.frame = 0;
				if (Editor.hoverTextBox)
				{
					_cursor.frame = 7;
					_cursor.position.y -= 4f;
					_cursor.scale = new Vec2(0.5f, 1f);
				}
				_cursor.Draw();
			}
			if (_uploadDialog != null && _uploadDialog.opened)
			{
				Editor.hoverTextBox = false;
				_gamepadMode = false;
				{
					foreach (ContextMenu item in Level.current.things[typeof(ContextMenu)])
					{
						item.Draw();
					}
					return;
				}
			}
			if (showingError != null)
			{
				float boxLeft = _box.x - _box.halfWidth;
				float boxTop = _box.y - _box.halfHeight;
				_fancyFont.scale = new Vec2(1f);
				int wide = _fancyFont.maxWidth;
				_fancyFont.maxRows = 40;
				_fancyFont.maxWidth = (int)base.width - 10;
				_fancyFont.Draw(showingError, new Vec2(boxLeft + 4f, boxTop + 4f), Color.White, 0.5f);
				_fancyFont.maxRows = 2;
				_fancyFont.maxWidth = wide;
				base.Draw();
				return;
			}
			scrollBarTop = (int)(_box.y - _box.halfHeight + 1f + 16f);
			scrollBarBottom = (int)(_box.y + _box.halfHeight - 1f - 16f);
			scrollBarScrollableHeight = scrollBarBottom - scrollBarTop;
			if (fixView)
			{
				Layer.HUD.camera.width *= 2f;
				Layer.HUD.camera.height *= 2f;
				fixView = false;
			}
			Graphics.DrawRect(new Vec2(_box.x - _box.halfWidth, _box.y - _box.halfHeight), new Vec2(_box.x + _box.halfWidth - 12f - 2f, _box.y + _box.halfHeight), Color.Black, 0.4f);
			Graphics.DrawRect(new Vec2(_box.x + _box.halfWidth - 12f, _box.y - _box.halfHeight), new Vec2(_box.x + _box.halfWidth, _box.y + _box.halfHeight), Color.Black, 0.4f);
			Rectangle sb = ScrollBarBox();
			Graphics.DrawRect(sb, (_draggingScrollbar || sb.Contains(Mouse.position)) ? Color.LightGray : Color.Gray, 0.5f);
			for (int i = 0; i < _maxModsToShow; i++)
			{
				int modIndex = _scrollItemOffset + i;
				if (modIndex >= _mods.Count)
				{
					break;
				}
				float boxLeft2 = _box.x - _box.halfWidth;
				float boxTop2 = _box.y - _box.halfHeight + (float)(36 * i);
				if (_transferItem == null && _hoverIndex == modIndex)
				{
					Graphics.DrawRect(new Vec2(boxLeft2, boxTop2), new Vec2(boxLeft2 + _box.width - 14f, boxTop2 + 36f), Color.White * 0.6f, 0.45f);
				}
				else if ((modIndex & 1) != 0)
				{
					Graphics.DrawRect(new Vec2(boxLeft2, boxTop2), new Vec2(boxLeft2 + _box.width - 14f, boxTop2 + 36f), Color.White * 0.1f, 0.45f);
				}
				Mod mod = _mods[modIndex];
				if (mod != null)
				{
					if (mod is UI_ModSettings)
					{
						Graphics.Draw(_settingsIcon, boxLeft2 + 2f, boxTop2 + 1f, 0.5f);
						_fancyFont.scale = new Vec2(1.5f);
						_fancyFont.Draw("Mod Settings", new Vec2(boxLeft2 + 36f, boxTop2 + 11f), Color.White, 0.5f);
						_fancyFont.scale = new Vec2(1f);
						continue;
					}
					Tex2D preview = mod.previewTexture;
					if (preview != null && _noImage.texture != preview)
					{
						_noImage.texture = preview;
						_noImage.scale = new Vec2(32f / (float)preview.width);
					}
					Graphics.DrawRect(new Vec2(boxLeft2 + 2f, boxTop2 + 2f), new Vec2(boxLeft2 + 36f - 2f, boxTop2 + 36f - 2f), Color.Gray, 0.44f, filled: false, 2f);
					Graphics.Draw(_noImage, boxLeft2 + 2f, boxTop2 + 2f, 0.5f);
					string titleString = "#" + modIndex + ": ";
					if (mod.configuration.error != null)
					{
						_modErrorIcon.scale = new Vec2(2f);
						Graphics.Draw(_modErrorIcon, boxLeft2 + 2f, boxTop2 + 2f, 0.55f);
						titleString += "|DGRED|";
					}
					if (mod.configuration.error != null || mod.configuration.disabled)
					{
						Graphics.DrawRect(new Vec2(boxLeft2, boxTop2), new Vec2(boxLeft2 + _box.width - 14f, boxTop2 + 36f), Color.Black * 0.4f, 0.85f);
					}
					bool reskin = mod.configuration.modType == ModConfiguration.Type.Reskin || mod.configuration.isExistingReskinMod;
					titleString = ((!mod.configuration.loaded) ? (titleString + mod.configuration.name) : ((!reskin) ? (titleString + mod.configuration.displayName + "|WHITE| v" + mod.configuration.version.ToString() + " by |PURPLE|" + mod.configuration.author) : (titleString + mod.configuration.displayName + "|WHITE| by |PURPLE|" + mod.configuration.author)));
					if (reskin)
					{
						titleString += "|DGPURPLE| (Reskin Pack)";
					}
					else if (mod.configuration.modType == ModConfiguration.Type.MapPack)
					{
						titleString += "|DGPURPLE| (Map Pack)";
					}
					else if (mod.configuration.modType == ModConfiguration.Type.HatPack)
					{
						titleString += "|DGPURPLE| (Hat Pack)";
					}
					titleString += (mod.configuration.disabled ? "|DGRED| (Disabled)" : "|DGGREEN| (Enabled)");
					_fancyFont.Draw(titleString, new Vec2(boxLeft2 + 36f + 10f, boxTop2 + 2f), Color.Yellow, 0.5f);
					Graphics.Draw((!mod.configuration.isWorkshop) ? _localIcon : _steamIcon, boxLeft2 + 36f, boxTop2 + 2.5f, 0.5f);
					if (mod.configuration.error != null && (mod.configuration.disabled || mod is ErrorMod))
					{
						string er = mod.configuration.error;
						if (er.Length > 150)
						{
							er = er.Substring(0, 150);
						}
						_fancyFont.Draw(mod.configuration.error.StartsWith("!") ? ("|DGYELLOW|" + er.Substring(1, er.Length - 1)) : ("|DGRED|Failed with error: " + er), new Vec2(boxLeft2 + 36f, boxTop2 + 6f + (float)_fancyFont.characterHeight), Color.White, 0.5f);
					}
					else if (!mod.configuration.loaded)
					{
						if (mod.configuration.disabled)
						{
							_fancyFont.Draw("Mod is disabled.", new Vec2(boxLeft2 + 36f, boxTop2 + 6f + (float)_fancyFont.characterHeight), Color.LightGray, 0.5f);
						}
						else
						{
							_fancyFont.Draw("|DGGREEN|Mod will be enabled on next restart.", new Vec2(boxLeft2 + 36f, boxTop2 + 6f + (float)_fancyFont.characterHeight), Color.Orange, 0.5f);
						}
					}
					else if (mod.configuration.disabled)
					{
						_fancyFont.Draw("|DGRED|Mod will be disabled on next restart.", new Vec2(boxLeft2 + 36f, boxTop2 + 6f + (float)_fancyFont.characterHeight), Color.Orange, 0.5f);
					}
					else
					{
						_fancyFont.Draw(mod.configuration.description, new Vec2(boxLeft2 + 36f, boxTop2 + 6f + (float)_fancyFont.characterHeight), Color.White, 0.5f);
					}
				}
				else
				{
					Graphics.Draw(_newIcon, boxLeft2 + 2f, boxTop2 + 1f, 0.5f);
					_fancyFont.scale = new Vec2(1.5f);
					_fancyFont.Draw("Get " + ((_mods.Count == 1) ? "some" : "more") + " mods!", new Vec2(boxLeft2 + 36f, boxTop2 + 11f), Color.White, 0.5f);
					_fancyFont.scale = new Vec2(1f);
				}
			}
			if (_awaitingChanges)
			{
				Graphics.DrawString("Restart required for some changes to take effect!", new Vec2(base.x - base.halfWidth + 128f, base.y - base.halfHeight + 8f), Color.Red, 0.6f);
			}
			if (_transferItem != null)
			{
				Graphics.DrawRect(new Rectangle(_box.x - _box.halfWidth, _box.y - _box.halfHeight, _box.width, _box.height), Color.Black * 0.9f, 0.7f);
				string centerTopText = "Creating item...";
				if (_transferring)
				{
					TransferProgress pct = _transferItem.GetUploadProgress();
					centerTopText = pct.status switch
					{
						ItemUpdateStatus.CommittingChanges => "Committing changes", 
						ItemUpdateStatus.PreparingConfig => "Preparing config", 
						ItemUpdateStatus.PreparingContent => "Preparing content", 
						ItemUpdateStatus.UploadingContent => "Uploading content", 
						ItemUpdateStatus.UploadingPreviewFile => "Uploading preview", 
						_ => "Waiting", 
					};
					if (pct.bytesTotal != 0L)
					{
						float percent = (float)pct.bytesDownloaded / (float)pct.bytesTotal;
						centerTopText = centerTopText + " (" + (int)(percent * 100f) + "%)";
						Graphics.DrawRect(new Rectangle(_box.x - _box.halfWidth + 8f, _box.y - 8f, _box.width - 16f, 16f), Color.LightGray, 0.8f);
						Graphics.DrawRect(new Rectangle(_box.x - _box.halfWidth + 8f, _box.y - 8f, Lerp.FloatSmooth(0f, _box.width - 16f, percent), 16f), Color.Green, 0.8f);
					}
					centerTopText += "...";
				}
				else if (_needsUpdateNotes)
				{
					Graphics.DrawRect(new Rectangle(_updateTextBox.position.x - 1f, _updateTextBox.position.y - 1f, _updateTextBox.size.x + 2f, _updateTextBox.size.y + 2f), Color.Gray, 0.85f, filled: false);
					Graphics.DrawRect(new Rectangle(_updateTextBox.position.x, _updateTextBox.position.y, _updateTextBox.size.x, _updateTextBox.size.y), Color.Black, 0.85f);
					_updateTextBox.Draw();
					centerTopText = "Enter change notes:";
					Graphics.DrawString(_updateButtonText, new Vec2(_updateButton.x, _updateButton.y), _updateButton.Contains(Mouse.position) ? Color.Yellow : Color.White, 0.9f, null, 2f);
				}
				float width = Graphics.GetStringWidth(centerTopText, thinButtons: false, 2f);
				Graphics.DrawString(centerTopText, new Vec2(_box.x - width / 2f, _box.y - _box.halfHeight + 24f), Color.White, 0.8f, null, 2f);
			}
		}
		base.Draw();
	}
}
