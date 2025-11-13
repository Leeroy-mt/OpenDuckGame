using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class UIInviteMenu : UIMenu
{
	private static Dictionary<int, int> _sortDictionary = new Dictionary<int, int>
	{
		{ 0, 6 },
		{ 1, 1 },
		{ 2, 3 },
		{ 3, 4 },
		{ 4, 5 },
		{ 5, 2 },
		{ 6, 0 }
	};

	public static Dictionary<ulong, Sprite> avatars = new Dictionary<ulong, Sprite>();

	private List<UIInviteUser> _users = new List<UIInviteUser>();

	private BitmapFont _littleFont;

	private UIBox _box;

	private new int _selection;

	private int _viewTop;

	private Sprite _moreArrow;

	private Sprite _noAvatar;

	private UIMenuAction _menuAction;

	private int _maxShow = 9;

	public new static void Initialize()
	{
		if (!Steam.IsInitialized())
		{
			return;
		}
		foreach (User u in Steam.friends)
		{
			avatars[u.id] = PrepareSprite(u);
		}
	}

	public static Sprite PrepareSprite(User u)
	{
		byte[] data = u.avatarMedium;
		Sprite av = null;
		if (data != null && data.Length == 16384)
		{
			Texture2D texture2D = new Texture2D(Graphics.device, 64, 64);
			texture2D.SetData(data);
			av = new Sprite(texture2D);
			av.CenterOrigin();
		}
		return av;
	}

	public static Sprite GetAvatar(User u)
	{
		Sprite spr = null;
		if (!avatars.TryGetValue(u.id, out spr))
		{
			spr = PrepareSprite(u);
		}
		return spr;
	}

	public void SetAction(UIMenuAction a)
	{
		_menuAction = a;
	}

	public UIInviteMenu(string title, UIMenuAction act, float xpos, float ypos, float wide = -1f, float high = -1f, string conString = "", InputProfile conProfile = null, bool tiny = false)
		: base(title, xpos, ypos, wide, high)
	{
		if (Steam.IsInitialized())
		{
			int numEl = Steam.friends.OrderBy((User u) => _sortDictionary[(int)u.state]).Count();
			if (numEl > _maxShow)
			{
				numEl = _maxShow;
			}
			_littleFont = new BitmapFont("smallBiosFont", 7, 6);
			_moreArrow = new Sprite("moreArrow");
			_moreArrow.CenterOrigin();
			_box = new UIBox(0f, 0f, 100f, 14 * numEl + 8, vert: true, isVisible: false);
			_noAvatar = new Sprite("noAvatar");
			_noAvatar.CenterOrigin();
			Add(_box);
		}
		_menuAction = act;
	}

	public override void Open()
	{
		HUD.CloseAllCorners();
		HUD.AddCornerControl(HUDCorner.BottomRight, "@MENU1@INVITE");
		HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@EXIT");
		_users.Clear();
		if (Steam.IsInitialized())
		{
			IOrderedEnumerable<User> fends = Steam.friends.OrderBy((User user) => _sortDictionary[(int)user.state]);
			int numEl = fends.Count();
			for (int i = 0; i < numEl; i++)
			{
				User u = fends.ElementAt(i);
				string nam = u.name;
				if (nam.Count() > 17)
				{
					nam = nam.Substring(0, 16) + ".";
				}
				UserInfo info = u.info;
				if (info.relationship == FriendRelationship.Friend)
				{
					_users.Add(new UIInviteUser
					{
						user = u,
						sprite = GetAvatar(u),
						state = info.state,
						name = nam,
						inGame = info.inGame,
						inDuckGame = info.inCurrentGame,
						inMyLobby = info.inLobby
					});
				}
			}
			_users = _users.OrderBy((UIInviteUser h) => h, new CompareUsers()).ToList();
		}
		base.Open();
	}

	public override void Close()
	{
		HUD.CloseAllCorners();
		base.Close();
	}

	public override void Update()
	{
		if (base.open)
		{
			if (Input.Pressed("MENUUP") && _selection > 0)
			{
				_selection--;
				SFX.Play("textLetter", 0.7f);
			}
			if (Input.Pressed("MENUDOWN") && _selection < _users.Count - 1)
			{
				_selection++;
				SFX.Play("textLetter", 0.7f);
			}
			if (_selection >= _viewTop + _maxShow)
			{
				_viewTop = _selection - (_maxShow - 1);
			}
			if (_selection < _viewTop)
			{
				_viewTop = _selection;
			}
			if (Input.Pressed("CANCEL"))
			{
				_menuAction.Activate();
				SFX.Play("resume", 0.6f);
			}
			if (_users.Count > 0 && Input.Pressed("MENU1") && !_users[_selection].triedInvite)
			{
				SFX.Play("rockHitGround", 0.8f);
				_users[_selection].triedInvite = true;
				TeamSelect2.InvitedFriend(_users[_selection].user);
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		int numEl = _users.Count;
		if (numEl > _maxShow)
		{
			numEl = _maxShow;
		}
		float yZone = 14 * numEl - 12;
		float yOff = 0f;
		bool second = false;
		for (int i = _viewTop; i < _viewTop + _maxShow && i < _users.Count; i++)
		{
			UIInviteUser user = _users[i];
			float yPos = base.y - yZone / 2f + yOff;
			float xPos = base.x - 68f;
			Sprite spr = user.sprite;
			if (spr == null)
			{
				spr = _noAvatar;
			}
			spr.depth = base.depth + 4;
			spr.scale = new Vec2(0.25f);
			spr.alpha = ((_selection == i) ? 1f : 0.3f);
			Graphics.Draw(spr, xPos + 8f, yPos + 8f, new Rectangle(6f, 6f, 52f, 52f));
			_littleFont.Draw(user.name, new Vec2(xPos + 15f, yPos), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			if (user.triedInvite)
			{
				_littleFont.Draw("|LIME|@CHECK@INVITED", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.inGame)
			{
				if (user.inDuckGame)
				{
					_littleFont.Draw("@ITEMBOX@|DGBLUE|IN DUCK GAME!", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
				}
				else
				{
					_littleFont.Draw("@USERONLINE@|YELLOW|IN SOME GAME", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
				}
			}
			else if (user.state == SteamUserState.Online)
			{
				_littleFont.Draw("@USERONLINE@|DGGREEN|ONLINE", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.Away)
			{
				_littleFont.Draw("@USERAWAY@|YELLOW|AWAY", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.Busy)
			{
				_littleFont.Draw("@USERBUSY@|YELLOW|BUSY", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.Snooze)
			{
				_littleFont.Draw("@USERBUSY@|YELLOW|SNOOZE", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.Offline)
			{
				_littleFont.Draw("@USEROFFLINE@|LIGHTGRAY|OFFLINE", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.LookingToPlay)
			{
				_littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO PLAY", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			else if (user.state == SteamUserState.LookingToTrade)
			{
				_littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO TRADE", new Vec2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.depth + 4);
			}
			Graphics.DrawRect(new Vec2(xPos, yPos), new Vec2(xPos + 135f, yPos + 13f), (second ? Colors.BlueGray : (Colors.BlueGray * 0.6f)) * ((_selection == i) ? 1f : 0.3f), base.depth + 2);
			yOff += 14f;
			second = !second;
		}
		if (_viewTop < _users.Count - _maxShow)
		{
			_moreArrow.depth = base.depth + 2;
			_moreArrow.flipV = false;
			Graphics.Draw(_moreArrow, base.x, base.y + yZone / 2f + 13f);
		}
		if (_viewTop > 0)
		{
			_moreArrow.depth = base.depth + 2;
			_moreArrow.flipV = true;
			Graphics.Draw(_moreArrow, base.x, base.y - yZone / 2f - 2f);
		}
		base.Draw();
	}
}
