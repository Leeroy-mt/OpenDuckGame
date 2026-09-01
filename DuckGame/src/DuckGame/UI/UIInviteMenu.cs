using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;


#if FACEPUNCH
using Steamworks;
using Steamworks.Data;
#else
using Steam;
#endif

namespace DuckGame;

public class UIInviteMenu : UIMenu
{
    private static Dictionary<int, int> _sortDictionary = new()
    {
        { 0, 6 },
        { 1, 1 },
        { 2, 3 },
        { 3, 4 },
        { 4, 5 },
        { 5, 2 },
        { 6, 0 }
    };

    public static Dictionary<ulong, Sprite> avatars = [];

    private List<UIInviteUser> _users = [];

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
#if FACEPUNCH
        if (!SteamClient.IsValid)
            return;

        foreach (var u in SteamFriends.GetFriends())
            avatars[u.Id] = PrepareSprite(u);
#else
        if (!DGSteam.IsInitialized())
            return;

        foreach (User u in DGSteam.Friends)
            avatars[u.Id] = PrepareSprite(u);
#endif
    }

#if FACEPUNCH
    public static Sprite PrepareSprite(Friend friend)
    {
        var avatarImage = friend.GetMediumAvatarAsync()
            .GetAwaiter()
            .GetResult();

        Sprite av = null;
        if (avatarImage?.Data is byte[] imageData && imageData.Length == 16384)
        {
            Texture2D texture2D = new(Graphics.device, 64, 64);
            texture2D.SetData(imageData);
            av = new(texture2D);
            av.CenterOrigin();
        }
        return av;
    }

    public static Sprite GetAvatar(Friend u)
    {
        Sprite spr = null;
        if (!avatars.TryGetValue(u.Id, out spr))
            spr = PrepareSprite(u);
        return spr;
    }
#else
    public static Sprite PrepareSprite(User u)
    {
        byte[] data = u.AvatarMedium;
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
        if (!avatars.TryGetValue(u.Id, out spr))
            spr = PrepareSprite(u);
        return spr;
    }
#endif

    public void SetAction(UIMenuAction a)
    {
        _menuAction = a;
    }

    public UIInviteMenu(string title, UIMenuAction act, float xpos, float ypos, float wide = -1f, float high = -1f, string conString = "", InputProfile conProfile = null, bool tiny = false)
        : base(title, xpos, ypos, wide, high)
    {
#if FACEPUNCH
        if (SteamClient.IsValid)
        {
            int numEl = SteamFriends.GetFriends()
                .OrderBy(u => _sortDictionary[(int)u.State])
                .Count();
#else
        if (DGSteam.IsInitialized())
        {
            int numEl = DGSteam.Friends.OrderBy(u => _sortDictionary[(int)u.State]).Count();
#endif
            if (numEl > _maxShow)
                numEl = _maxShow;
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
#if FACEPUNCH
        if (SteamClient.IsValid)
        {
            var fends = SteamFriends.GetFriends().OrderBy(user => _sortDictionary[(int)user.State]);
            int numEl = fends.Count();
            for (int i = 0; i < numEl; i++)
            {
                var u = fends.ElementAt(i);
                string nam = u.Name;
                if (nam.Length > 17)
                    nam = nam.Substring(0, 16) + ".";

                if (u.Relationship == Relationship.Friend)
                {
                    _users.Add(new UIInviteUser
                    {
                        user = u,
                        sprite = GetAvatar(u),
                        state = u.State,
                        name = nam,
                        inGame = u.GameInfo?.GameID > 0,
                        inDuckGame = u.IsPlayingThisGame,
                        inMyLobby = u.GameInfo?.Lobby?.Id == FacepunchSteam.Lobby.Id
                    });
                }
            }
            _users = _users.OrderBy((UIInviteUser h) => h, new CompareUsers()).ToList();
        }
#else
        if (DGSteam.IsInitialized())
        {
            IOrderedEnumerable<User> fends = DGSteam.Friends.OrderBy((User user) => _sortDictionary[(int)user.State]);
            int numEl = fends.Count();
            for (int i = 0; i < numEl; i++)
            {
                User u = fends.ElementAt(i);
                string nam = u.Name;
                if (nam.Count() > 17)
                {
                    nam = nam.Substring(0, 16) + ".";
                }
                UserInfo info = u.Info;
                if (info.Relationship == FriendRelationship.Friend)
                {
                    _users.Add(new UIInviteUser
                    {
                        user = u,
                        sprite = GetAvatar(u),
                        state = info.State,
                        name = nam,
                        inGame = info.InGame,
                        inDuckGame = info.InCurrentGame,
                        inMyLobby = info.InMyLobby // was info.inLobby. mistake???
                    });
                }
            }
            _users = _users.OrderBy((UIInviteUser h) => h, new CompareUsers()).ToList();
        }
#endif
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
            float yPos = base.Y - yZone / 2f + yOff;
            float xPos = base.X - 68f;
            Sprite spr = user.sprite;
            if (spr == null)
            {
                spr = _noAvatar;
            }
            spr.Depth = base.Depth + 4;
            spr.Scale = new Vector2(0.25f);
            spr.Alpha = ((_selection == i) ? 1f : 0.3f);
            Graphics.Draw(spr, xPos + 8f, yPos + 8f, new RectangleF(6f, 6f, 52f, 52f));
            _littleFont.Draw(user.name, new Vector2(xPos + 15f, yPos), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            if (user.triedInvite)
            {
                _littleFont.Draw("|LIME|@CHECK@INVITED", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            }
            else if (user.inGame)
            {
                if (user.inDuckGame)
                {
                    _littleFont.Draw("@ITEMBOX@|DGBLUE|IN DUCK GAME!", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
                }
                else
                {
                    _littleFont.Draw("@USERONLINE@|YELLOW|IN SOME GAME", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
                }
            }
#if FACEPUNCH
            else if (user.state == FriendState.Online)
                _littleFont.Draw("@USERONLINE@|DGGREEN|ONLINE", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.Away)
                _littleFont.Draw("@USERAWAY@|YELLOW|AWAY", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.Busy)
                _littleFont.Draw("@USERBUSY@|YELLOW|BUSY", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.Snooze)
                _littleFont.Draw("@USERBUSY@|YELLOW|SNOOZE", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.Offline)
                _littleFont.Draw("@USEROFFLINE@|LIGHTGRAY|OFFLINE", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.LookingToPlay)
                _littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO PLAY", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
            else if (user.state == FriendState.LookingToTrade)
                _littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO TRADE", new(xPos + 15, yPos + 6), Color.White * ((_selection == i) ? 1 : .3f), Depth + 4);
#else
            else if (user.state == SteamUserState.Online)
                _littleFont.Draw("@USERONLINE@|DGGREEN|ONLINE", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.Away)
                _littleFont.Draw("@USERAWAY@|YELLOW|AWAY", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.Busy)
                _littleFont.Draw("@USERBUSY@|YELLOW|BUSY", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.Snooze)
                _littleFont.Draw("@USERBUSY@|YELLOW|SNOOZE", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.Offline)
                _littleFont.Draw("@USEROFFLINE@|LIGHTGRAY|OFFLINE", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.LookingToPlay)
                _littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO PLAY", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
            else if (user.state == SteamUserState.LookingToTrade)
                _littleFont.Draw("@USERONLINE@|DGGREEN|WANTS TO TRADE", new Vector2(xPos + 15f, yPos + 6f), Color.White * ((_selection == i) ? 1f : 0.3f), base.Depth + 4);
#endif
            Graphics.DrawRect(new Vector2(xPos, yPos), new Vector2(xPos + 135f, yPos + 13f), (second ? Colors.BlueGray : (Colors.BlueGray * 0.6f)) * ((_selection == i) ? 1f : 0.3f), base.Depth + 2);
            yOff += 14f;
            second = !second;
        }
        if (_viewTop < _users.Count - _maxShow)
        {
            _moreArrow.Depth = base.Depth + 2;
            _moreArrow.flipV = false;
            Graphics.Draw(_moreArrow, base.X, base.Y + yZone / 2f + 13f);
        }
        if (_viewTop > 0)
        {
            _moreArrow.Depth = base.Depth + 2;
            _moreArrow.flipV = true;
            Graphics.Draw(_moreArrow, base.X, base.Y - yZone / 2f - 2f);
        }
        base.Draw();
    }
}
