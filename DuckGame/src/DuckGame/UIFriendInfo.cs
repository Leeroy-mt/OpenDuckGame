using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class UIFriendInfo : UIMenuItem
{
	private UIMenu _rootMenu;

	private Sprite _avatar;

	public UIFriendInfo(User friend, UIMenu rootMenu)
		: base(" " + friend.name)
	{
		byte[] data = friend.avatarSmall;
		if (data != null)
		{
			Texture2D tex = new Texture2D(Graphics.device, 32, 32);
			tex.SetData(data);
			_avatar = new Sprite(tex);
			_avatar.CenterOrigin();
		}
		_rootMenu = rootMenu;
		_collisionSize.y = 14f;
		BitmapFont littleFont = new BitmapFont("smallBiosFont", 7, 6);
		_textElement.SetFont(littleFont);
		_textElement.text = "  " + friend.name + "\n  |LIME|WANTS TO PLAY";
	}

	public override void Activate(string trigger)
	{
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Draw()
	{
		Graphics.DrawRect(base.leftSection.topLeft, base.rightSection.bottomRight, Colors.BlueGray, base.depth - 1);
		if (_avatar != null)
		{
			_avatar.depth = base.depth + 2;
			_avatar.scale = new Vec2(0.25f);
			Graphics.Draw(_avatar, base.leftSection.left + (float)_avatar.width * _avatar.scale.x / 2f + 6f, base.y + 3f);
		}
		base.Draw();
	}
}
