using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class UIFriendInfo : UIMenuItem
{
    #region Private Fields

    UIMenu _rootMenu;

    Sprite _avatar;

    #endregion

    #region Public Constructors

    public UIFriendInfo(User friend, UIMenu rootMenu)
        : base(" " + friend.name)
    {
        byte[] data = friend.avatarSmall;
        if (data != null)
        {
            Texture2D tex = new(Graphics.device, 32, 32);
            tex.SetData(data);
            _avatar = new Sprite(tex);
            _avatar.CenterOrigin();
        }
        _rootMenu = rootMenu;
        _collisionSize.Y = 14;
        BitmapFont littleFont = new("smallBiosFont", 7, 6);
        _textElement.SetFont(littleFont);
        _textElement.text = "  " + friend.name + "\n  |LIME|WANTS TO PLAY";
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
