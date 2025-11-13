using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIUnlockBox : UIMenu
{
	private Sprite _frame;

	private Sprite _wrappedFrame;

	private BitmapFont _font;

	private FancyBitmapFont _fancyFont;

	private bool _wrapped = true;

	private bool _flash;

	private Unlockable _unlock;

	private List<Unlockable> _unlocks;

	private float yOffset = 150f;

	public bool down = true;

	private float _downWait = 1f;

	private string _oldSong;

	private float _openWait = 1f;

	public bool finished;

	public UIUnlockBox(List<Unlockable> unlocks, float xpos, float ypos, float wide = -1f, float high = -1f)
		: base("", xpos, ypos, wide, high)
	{
		Graphics.fade = 1f;
		_frame = new Sprite("unlockFrame");
		_frame.CenterOrigin();
		_wrappedFrame = new Sprite("unlockFrameWrapped");
		_wrappedFrame.CenterOrigin();
		_font = new BitmapFont("biosFontUI", 8, 7);
		_fancyFont = new FancyBitmapFont("smallFont");
		_unlocks = unlocks;
		_unlock = _unlocks.First();
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Update()
	{
		yOffset = Lerp.FloatSmooth(yOffset, down ? 150f : 0f, 0.3f, 1.1f);
		if (down)
		{
			if (_unlocks.Count == 0)
			{
				if (!finished)
				{
					finished = true;
					Close();
				}
			}
			else
			{
				_downWait -= 0.06f;
				if (_downWait <= 0f)
				{
					_openWait = 1f;
					_wrapped = true;
					_downWait = 1f;
					_unlock = _unlocks.First();
					_unlocks.RemoveAt(0);
					down = false;
					SFX.Play("pause", 0.6f);
				}
			}
		}
		else
		{
			_openWait -= 0.06f;
			if (_openWait <= 0f && _wrapped && !_flash)
			{
				_flash = true;
			}
			if (_flash)
			{
				Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 1f, 0.2f);
				if (Graphics.flashAdd > 0.99f)
				{
					_wrapped = !_wrapped;
					if (!_wrapped)
					{
						if (_unlock != null && _unlock.name == "UR THE BEST")
						{
							_oldSong = Music.currentSong;
							Music.Play("jollyjingle");
						}
						SFX.Play("harp");
						HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
						_unlock.DoUnlock();
					}
					_flash = false;
				}
			}
			else
			{
				Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 0f, 0.2f);
			}
			if (!_wrapped && Input.Pressed("SELECT"))
			{
				HUD.CloseAllCorners();
				SFX.Play("resume", 0.6f);
				if (_oldSong != null && _unlock != null && _unlock.name == "UR THE BEST")
				{
					Music.Play(_oldSong);
				}
				down = true;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		base.y += yOffset;
		if (_wrapped)
		{
			_wrappedFrame.depth = base.depth;
			Graphics.Draw(_wrappedFrame, base.x, base.y);
		}
		else
		{
			_frame.depth = -0.9f;
			Graphics.Draw(_frame, base.x, base.y);
			string text = "@LWING@UNLOCK@RWING@";
			if (_unlock.name == "UR THE BEST")
			{
				text = "@LWING@WOAH!@RWING@";
			}
			Vec2 fontPos = new Vec2(0f - _font.GetWidth(text) / 2f, -42f);
			_font.DrawOutline(text, position + fontPos, Color.White, Color.Black, base.depth + 2);
			string unlockText = "} " + _unlock.name + " }";
			_fancyFont.scale = new Vec2(1f, 1f);
			Vec2 unlockFontPos = new Vec2(0f - _fancyFont.GetWidth(unlockText) / 2f, -25f);
			_fancyFont.DrawOutline(unlockText, position + unlockFontPos, Colors.DGYellow, Color.Black, base.depth + 2);
			_fancyFont.scale = new Vec2(0.5f, 0.5f);
			string descriptionText = _unlock.description;
			Vec2 descFontPos = new Vec2(0f - _fancyFont.GetWidth(descriptionText) / 2f, 38f);
			_fancyFont.DrawOutline(descriptionText, position + descFontPos, Colors.DGGreen, Color.Black, base.depth + 2, 0.5f);
			_unlock.Draw(base.x, base.y + 10f, base.depth + 4);
		}
		base.y -= yOffset;
	}
}
