using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DuckGame;

public class UICloudManagement : UIMenu
{
	[DebuggerDisplay("{name}")]
	public class File
	{
		public string name;

		public string fullPath;

		public List<File> files;

		public File parent;
	}

	private List<File> _flagged = new List<File>();

	private Sprite _downArrow;

	private BitmapFont _littleFont;

	public File root = new File
	{
		files = new List<File>()
	};

	public File profileRoot;

	public File currentFolder;

	public UIMenu _deleteMenu;

	private UIMenu _openOnClose;

	private bool _opening;

	private int _topOffset;

	private readonly int kMaxInView = 16;

	public UICloudManagement(UIMenu openOnClose)
		: base("MANAGE CLOUD", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 260f, 180f)
	{
		UIBox box = new UIBox(0f, 0f, 100f, 150f, vert: true, isVisible: false);
		Add(box);
		_littleFont = new BitmapFont("smallBiosFont", 7, 6);
		_downArrow = new Sprite("cloudDown");
		_downArrow.CenterOrigin();
		_deleteMenu = new UIMenu("ARE YOU SURE?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@WASD@ADJUST @CANCEL@EXIT");
		_deleteMenu.Add(new UIText("The selected files will be", Colors.DGBlue));
		_deleteMenu.Add(new UIText("|DGRED|permenantly deleted|DGBLUE| from", Colors.DGBlue));
		_deleteMenu.Add(new UIText("everywhere, |DGRED|forever!", Colors.DGBlue));
		_deleteMenu.Add(new UIText(" ", Colors.DGBlue));
		_deleteMenu.Add(new UIMenuItem("|DGRED|DELETE", new UIMenuActionCallFunctionOpenMenu(_deleteMenu, this, DeleteFiles), UIAlign.Center, default(Color), backButton: true));
		_deleteMenu.Add(new UIMenuItem("|DGGREEN|CANCEL", new UIMenuActionOpenMenu(_deleteMenu, this), UIAlign.Center, default(Color), backButton: true));
		_deleteMenu._defaultSelection = 1;
		_deleteMenu.SetBackFunction(new UIMenuActionOpenMenu(_deleteMenu, this));
		_deleteMenu.Close();
		_openOnClose = openOnClose;
	}

	private void DeleteFiles()
	{
		foreach (File f in _flagged)
		{
			if (f.files != null)
			{
				DeleteFolder(f);
			}
			else
			{
				DuckFile.Delete(f.fullPath);
			}
		}
	}

	private void DeleteFolder(File pFolder)
	{
		foreach (File f in pFolder.files)
		{
			if (f.files != null)
			{
				DeleteFolder(f);
			}
			else
			{
				DuckFile.Delete(f.fullPath);
			}
		}
	}

	private File GetFolder(string pPath, string pCloudPath)
	{
		File ret = root;
		if (pCloudPath.StartsWith("nq500000_"))
		{
			if (profileRoot == null)
			{
				profileRoot = new File
				{
					parent = root,
					name = Steam.user.id.ToString(),
					files = new List<File>()
				};
				root.files.Add(profileRoot);
			}
			ret = profileRoot;
		}
		string[] array = pPath.Split('/');
		foreach (string s in array)
		{
			if (s == "")
			{
				break;
			}
			bool found = false;
			foreach (File f in ret.files)
			{
				if (f.name == s)
				{
					ret = f;
					found = true;
					break;
				}
			}
			if (!found)
			{
				File f2 = new File
				{
					name = s,
					files = new List<File>
					{
						new File
						{
							name = "..",
							files = new List<File>()
						}
					},
					parent = ret
				};
				ret.files.Add(f2);
				ret = f2;
			}
		}
		return ret;
	}

	private void SortFiles(File pRoot)
	{
		if (pRoot.files == null)
		{
			return;
		}
		pRoot.files = (from x in pRoot.files
			orderby x.name != "..", x.files == null, x.name
			select x).ToList();
		foreach (File f in pRoot.files)
		{
			SortFiles(f);
		}
	}

	public override void Open()
	{
		HUD.CloseAllCorners();
		_opening = true;
		_flagged.Clear();
		root = new File
		{
			files = new List<File>()
		};
		profileRoot = null;
		int num = Steam.FileGetCount();
		for (int i = 0; i < num; i++)
		{
			CloudFile c = CloudFile.Get(Steam.FileGetName(i), pDelete: true);
			if (c != null)
			{
				string nonCloudFilename = c.cloudPath.Substring(9, c.cloudPath.Length - 9);
				string dir = Path.GetDirectoryName(nonCloudFilename).Replace('\\', '/');
				GetFolder(dir, c.cloudPath).files.Add(new File
				{
					name = Path.GetFileName(nonCloudFilename),
					fullPath = c.localPath
				});
			}
		}
		SortFiles(root);
		currentFolder = root;
		base.Open();
	}

	public override void Close()
	{
		HUD.CloseAllCorners();
		base.Close();
	}

	public override void Update()
	{
		if (base.open && !_opening)
		{
			if (Input.Pressed("MENUUP") && _selection > 0)
			{
				_selection--;
				if (_selection < _topOffset)
				{
					_topOffset--;
				}
				SFX.Play("textLetter", 0.7f);
			}
			if (Input.Pressed("MENUDOWN") && _selection < currentFolder.files.Count - 1)
			{
				_selection++;
				if (_selection > _topOffset + kMaxInView)
				{
					_topOffset++;
				}
				SFX.Play("textLetter", 0.7f);
			}
			if (Input.Pressed("SELECT") && currentFolder.files.Count > 0)
			{
				if (currentFolder.files[_selection].name == "..")
				{
					SelectFolder(currentFolder.parent);
				}
				else if (currentFolder.files[_selection].files != null)
				{
					SelectFolder(currentFolder.files[_selection]);
				}
			}
			if (Input.Pressed("MENU1") && currentFolder.files.Count > 0)
			{
				if (_flagged.Contains(currentFolder.files[_selection]))
				{
					_flagged.Remove(currentFolder.files[_selection]);
				}
				else
				{
					_flagged.Add(currentFolder.files[_selection]);
				}
				SFX.Play("textLetter", 0.7f);
			}
			if (Input.Pressed("MENU2") && _flagged.Count > 0)
			{
				_deleteMenu.dirty = true;
				new UIMenuActionOpenMenu(this, _deleteMenu).Activate();
			}
			if (Input.Pressed("CANCEL"))
			{
				if (currentFolder.parent != null)
				{
					SelectFolder(currentFolder.parent);
				}
				else if (_openOnClose != null)
				{
					new UIMenuActionOpenMenu(this, _openOnClose).Activate();
				}
				else
				{
					new UIMenuActionCloseMenu(this).Activate();
				}
			}
		}
		_opening = false;
		base.Update();
	}

	private void SelectFolder(File pFolder)
	{
		currentFolder = pFolder;
		_selection = 0;
		_topOffset = 0;
		SFX.Play("textLetter", 0.7f);
	}

	public override void Draw()
	{
		if (base.open && currentFolder != null)
		{
			Vec2 pos = new Vec2(base.x - 124f, base.y - 66f);
			float yOffset = 0f;
			int idx = 0;
			int drawIndex = 0;
			foreach (File f in currentFolder.files)
			{
				if (idx < _topOffset)
				{
					idx++;
					continue;
				}
				if (_topOffset > 0)
				{
					_downArrow.flipV = true;
					Graphics.Draw(_downArrow, base.x, pos.y - 2f, 0.5f);
				}
				if (drawIndex > kMaxInView)
				{
					_downArrow.flipV = false;
					Graphics.Draw(_downArrow, base.x, pos.y + yOffset, 0.5f);
					break;
				}
				string drawName = f.name;
				if (drawName.Length > 31)
				{
					drawName = drawName.Substring(0, 30) + "..";
				}
				drawName = ((f.files == null) ? ((_flagged.Contains(f) ? "@DELETEFLAG_ON@" : "@DELETEFLAG_OFF@") + drawName) : ((_flagged.Contains(f) ? "@FOLDERDELETEICON@" : "@FOLDERICON@") + drawName));
				drawName = ((idx != _selection) ? (" " + drawName) : ("@SELECTICON@" + drawName));
				_littleFont.Draw(drawName, pos + new Vec2(0f, yOffset), Color.White, 0.5f);
				yOffset += 8f;
				idx++;
				drawIndex++;
			}
			string controlsText = "@CANCEL@BACK";
			if (currentFolder.files.Count > 0)
			{
				if (currentFolder.files[_selection].files != null)
				{
					controlsText += " @SELECT@OPEN";
				}
				controlsText = ((!_flagged.Contains(currentFolder.files[_selection])) ? (controlsText + " @MENU1@FLAG") : (controlsText + " @MENU1@UNFLAG"));
			}
			if (_flagged.Count > 0)
			{
				controlsText += " @MENU2@DELETE";
			}
			_littleFont.Draw(controlsText, new Vec2(base.x - _littleFont.GetWidth(controlsText) / 2f, base.y + 74f), Color.White, 0.5f);
		}
		base.Draw();
	}
}
