using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class UIFriendInfo : UIMenuItem
{
    #region Private Fields

    UIMenu _rootMenu;

    Sprite _avatar;

    #endregion

    #region Public Constructors

#if FACEPUNCH
    public UIFriendInfo(Friend friend, UIMenu rootMenu)
#else
    public UIFriendInfo(User friend, UIMenu rootMenu)
#endif
        : base(" " + friend.Name)
    {
#if FACEPUNCH
        var avatarImage = friend.GetSmallAvatarAsync()
            .GetAwaiter()
            .GetResult();

        if (avatarImage != null)
        {
            Texture2D tex = new(Graphics.device, 32, 32);
            tex.SetData(avatarImage.Value.Data);
            _avatar = new Sprite(tex);
            _avatar.CenterOrigin();
        }
#else
        byte[] data = friend.AvatarSmall;
        if (data != null)
        {
            Texture2D tex = new(Graphics.device, 32, 32);
            tex.SetData(data);
            _avatar = new Sprite(tex);
            _avatar.CenterOrigin();
        }
#endif
        _rootMenu = rootMenu;
        _collisionSize.Y = 14;
        BitmapFont littleFont = new("smallBiosFont", 7, 6);
        _textElement.SetFont(littleFont);
        _textElement.text = "  " + friend.Name + "\n  |LIME|WANTS TO PLAY";
    }

#endregion

    #region Public Methods

    public override void Activate(string trigger)
    {
    }

    public override void Draw()
    {
        Graphics.DrawRect(leftSection.topLeft, rightSection.bottomRight, Colors.BlueGray, Depth - 1);
        if (_avatar != null)
        {
            _avatar.Depth = Depth + 2;
            _avatar.Scale = new Vector2(0.25f);
            Graphics.Draw(_avatar, leftSection.left + _avatar.width * _avatar.Scale.X / 2f + 6f, Y + 3f);
        }
        base.Draw();
    }

    #endregion
}
