using System;
using System.Collections.Generic;

namespace DuckGame;

public class UnlockScreen : Thing
{
	private Sprite _tail;

	public bool quitOut;

	private UnlockTree _tree;

	private PlasmaLayer _plasma;

	private Layer _treeLayer;

	public static bool open;

	private UIComponent _pauseGroup;

	private UIMenu _confirmMenu;

	private MenuBoolean _confirm = new MenuBoolean();

	private UnlockData _tryBuy;

	public override bool visible
	{
		get
		{
			if (!(base.alpha < 0.01f))
			{
				return base.visible;
			}
			return false;
		}
		set
		{
			base.visible = value;
		}
	}

	public UnlockScreen()
	{
		_tail = new Sprite("arcade/bubbleTail");
		base.layer = Layer.HUD;
	}

	public override void Initialize()
	{
		_plasma = new PlasmaLayer("PLASMA", -85);
		Layer.Add(_plasma);
		Camera treeCam = new Camera();
		_treeLayer = new Layer("TREE", -95, treeCam);
		Layer.Add(_treeLayer);
		_tree = new UnlockTree(this, _treeLayer);
		Level.Add(_tree);
	}

	public void OpenBuyConfirmation(UnlockData unlock)
	{
		if (_pauseGroup != null)
		{
			Level.Remove(_pauseGroup);
			_pauseGroup = null;
		}
		_confirm.value = false;
		_pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
		_confirmMenu = new UIMenu("UNLOCK FEATURE", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@CANCEL  @SELECT@BUY");
		_confirmMenu.Add(new UIText(unlock.GetNameForDisplay(), Color.Green));
		_confirmMenu.Add(new UIText(" ", Color.White));
		float width = 190f;
		string text = unlock.longDescription;
		string curText = "";
		string nextWord = "";
		while (true)
		{
			if (text.Length > 0 && text[0] != ' ')
			{
				nextWord += text[0];
			}
			else
			{
				if ((float)((curText.Length + nextWord.Length) * 8) > width)
				{
					_confirmMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
					curText = "";
				}
				if (curText.Length > 0)
				{
					curText += " ";
				}
				curText += nextWord;
				nextWord = "";
			}
			if (text.Length == 0)
			{
				break;
			}
			text = text.Remove(0, 1);
		}
		if (nextWord.Length > 0)
		{
			if (curText.Length > 0)
			{
				curText += " ";
			}
			curText += nextWord;
		}
		if (curText.Length > 0)
		{
			_confirmMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
			curText = "";
		}
		_confirmMenu.Add(new UIText(" ", Color.White));
		_confirmMenu.Add(new UIMenuItem("CANCEL", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Center, Colors.MenuOption, backButton: true));
		_confirmMenu.Add(new UIMenuItem("BUY UNLOCK |WHITE|(|LIME|" + unlock.cost + "|WHITE| TICKETS)", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _confirm)));
		_confirmMenu.Close();
		_pauseGroup.Add(_confirmMenu, doAnchor: false);
		_pauseGroup.Close();
		Level.Add(_pauseGroup);
		for (int i = 0; i < 10; i++)
		{
			_pauseGroup.Update();
			_confirmMenu.Update();
		}
		_pauseGroup.Open();
		_confirmMenu.Open();
		MonoMain.pauseMenu = _pauseGroup;
		SFX.Play("pause", 0.6f);
		_tryBuy = unlock;
	}

	public void ChangeSpeech()
	{
		Chancy.Clear();
		string unlockType = "";
		if (_tree.selected.type == UnlockType.Hat)
		{
			unlockType = "HAT";
		}
		else if (_tree.selected.type == UnlockType.Level)
		{
			unlockType = "LEVEL";
		}
		else if (_tree.selected.type == UnlockType.Modifier)
		{
			unlockType = "GAMEPLAY MODIFIER";
		}
		else if (_tree.selected.type == UnlockType.Weapon)
		{
			unlockType = "WEAPON";
		}
		else if (_tree.selected.type == UnlockType.Special)
		{
			unlockType = "SPECIAL";
		}
		string line = _tree.selected.description + "^|ORANGE|" + unlockType + "|WHITE| - ";
		if (_tree.selected.ProfileUnlocked(Profiles.active[0]))
		{
			line += "|GREEN|UNLOCKED";
		}
		else if (_tree.selected.parent != null)
		{
			List<UnlockData> treeLayer = Unlocks.GetTreeLayer(_tree.selected.parent.layer);
			bool lockd = false;
			foreach (UnlockData parent in treeLayer)
			{
				if (parent.children.Contains(_tree.selected) && !parent.ProfileUnlocked(Profiles.active[0]))
				{
					line += "|RED|LOCKED";
					lockd = true;
					List<string> obj = new List<string> { "Wonder what this one is?", "I think you're gonna like this one.", "This one is just perfect for you.", "Yeah this ones out of stock." };
					line = obj[Rando.Int(obj.Count - 1)];
					break;
				}
			}
			if (!lockd)
			{
				line = line + "|YELLOW|COSTS @TICKET@ " + Convert.ToString(_tree.selected.cost);
			}
		}
		else
		{
			line = line + "|YELLOW|COSTS @TICKET@ " + Convert.ToString(_tree.selected.cost);
		}
		Chancy.Add(line);
	}

	public void SelectionChanged()
	{
		if (_tree.selected.AllParentsUnlocked(Profiles.active[0]))
		{
			if (_tree.selected.ProfileUnlocked(Profiles.active[0]))
			{
				HUD.AddCornerMessage(HUDCorner.BottomRight, "|LIME|UNLOCKED");
			}
			else if (_tree.selected.cost <= Profiles.active[0].ticketCount)
			{
				HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@|LIME|BUY");
			}
			else
			{
				HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@|RED|BUY");
			}
		}
		else
		{
			HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@|RED|LOCKED");
		}
	}

	public void MakeActive()
	{
		HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
		HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
		SelectionChanged();
	}

	public override void Update()
	{
		float mul = (float)Graphics.width / (_treeLayer.camera.width * 2f);
		float mul2 = (float)Graphics.height / (_treeLayer.camera.height * 2f);
		_treeLayer.scissor = new Rectangle(50f * mul, 44f * mul, (float)Graphics.width - 180f * mul, 214f * mul2);
		if (_confirmMenu != null && !_confirmMenu.open && _tryBuy != null)
		{
			if (_confirm.value)
			{
				SFX.Play("ching");
				Profiles.active[0].ticketCount -= _tryBuy.cost;
				if (!Profiles.active[0].unlocks.Contains(_tryBuy.id))
				{
					Profiles.active[0].unlocks.Add(_tryBuy.id);
				}
				Profiles.Save(Profiles.active[0]);
				SelectionChanged();
			}
			else
			{
				SFX.Play("resume");
			}
			_tryBuy = null;
		}
		if (_confirmMenu != null && !_confirmMenu.open && _pauseGroup != null)
		{
			Level.Remove(_pauseGroup);
			_pauseGroup = null;
			_confirmMenu = null;
		}
		if (!Layer.Contains(_plasma))
		{
			Layer.Add(_plasma);
		}
		if (!Layer.Contains(_treeLayer))
		{
			Layer.Add(_treeLayer);
		}
		_plasma.alpha = base.alpha;
		_tree.alpha = base.alpha;
		if (base.alpha > 0.9f)
		{
			open = true;
			if (Input.Pressed("CANCEL"))
			{
				SFX.Play("menu_back");
				quitOut = true;
			}
		}
		else
		{
			open = false;
		}
	}

	public override void Draw()
	{
		if (!(base.alpha < 0.01f))
		{
			Graphics.DrawRect(new Vec2(26f, 22f), new Vec2(Layer.HUD.width - 105f, 140f), new Color(20, 20, 20) * base.alpha * 0.7f, -0.9f);
			Vec2 namePos = new Vec2(20f, 8f);
			Vec2 nameSize = new Vec2(226f, 11f);
			Graphics.DrawRect(namePos, namePos + nameSize, Color.Black);
			bool unlo = _tree.selected.ProfileUnlocked(Profiles.active[0]);
			bool canSee = true;
			if (!_tree.selected.AllParentsUnlocked(Profiles.active[0]))
			{
				canSee = false;
			}
			string name = _tree.selected.GetNameForDisplay();
			if (!canSee)
			{
				name = "???";
			}
			Graphics.DrawString(name, namePos + new Vec2((nameSize.x - 27f) / 2f - Graphics.GetStringWidth(name) / 2f, 2f), (unlo ? new Color(163, 206, 39) : Color.Red) * base.alpha, 0.5f);
			_tail.depth = 0.5f;
			_tail.alpha = base.alpha;
			_tail.flipH = false;
			_tail.flipV = false;
			Graphics.Draw(_tail, 222f, 18f);
			Chancy.alpha = base.alpha;
			Chancy.Draw();
		}
	}
}
