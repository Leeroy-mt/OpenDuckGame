using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class ArcadeHatConsole : Thing
{
	private ProfileBox2 _profileBox;

	private Sprite _consoleHighlight;

	private Sprite _consoleFlash;

	private SpriteMap _selectConsole;

	private Sprite _base;

	private PointLight _light;

	private float _consoleFade;

	private Profile _profile;

	private Duck _duck;

	public bool hover;

	public ArcadeHatConsole(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_consoleHighlight = new Sprite("consoleHighlight");
		_base = new Sprite("hatConsoleBase");
		_consoleFlash = new Sprite("consoleFlash");
		_consoleFlash.CenterOrigin();
		_selectConsole = new SpriteMap("selectConsole", 20, 19);
		_selectConsole.AddAnimation("idle", 1f, true, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		_selectConsole.SetAnimation("idle");
		_collisionSize = new Vec2(16f, 16f);
		_collisionOffset = new Vec2(0f, 0f);
		base.depth = -0.5f;
		graphic = _selectConsole;
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			_light = new PointLight(base.x + 9f, base.y + 7f, new Color(160, 255, 160), 70f, new List<LightOccluder>());
			Level.Add(_light);
		}
	}

	public void MakeHatSelector(Duck d)
	{
		if (_profileBox == null && d != null)
		{
			_profileBox = new ProfileBox2(9999f, 9999f, d.inputProfile, d.profile, null, 0);
			_profileBox.duck = d;
			_profileBox._hatSelector.layer = Layer.HUD;
			_profileBox._hatSelector.isArcadeHatSelector = true;
			_profile = d.profile;
			_duck = d;
			Level.Add(_profileBox);
		}
	}

	public bool IsOpen()
	{
		return _profileBox._hatSelector.open;
	}

	public void Open()
	{
		if (_duck != null)
		{
			_profileBox._hatSelector.position = new Vec2(85f, 45f);
			_profileBox._hatSelector.Open(_duck.profile);
			_profileBox.OpenCorners();
			SFX.Play("consoleOpen", 0.5f);
		}
	}

	public override void Update()
	{
		bool lastHover = hover;
		Duck d = Level.Nearest<Duck>(base.x, base.y);
		if (d != null && (d.position - (position + new Vec2(8f, 0f))).length < 16f)
		{
			hover = true;
		}
		else
		{
			hover = false;
		}
		if (!lastHover && hover)
		{
			HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@PROFILE");
		}
		else if (lastHover && !hover)
		{
			HUD.CloseAllCorners();
		}
		_consoleFade = Lerp.Float(_consoleFade, hover ? 1f : 0f, 0.1f);
		base.Update();
	}

	public override void Draw()
	{
		if (_light != null)
		{
			_consoleFlash.scale = new Vec2(0.75f, 0.75f);
			if (_selectConsole.imageIndex == 0)
			{
				_light.visible = true;
				_consoleFlash.alpha = 0.3f;
			}
			else if (_selectConsole.imageIndex == 1)
			{
				_light.visible = true;
				_consoleFlash.alpha = 0.1f;
			}
			else if (_selectConsole.imageIndex == 2)
			{
				_light.visible = false;
				_consoleFlash.alpha = 0f;
			}
		}
		_consoleFlash.depth = base.depth + 10;
		Graphics.Draw(_consoleFlash, base.x + 9f, base.y + 7f);
		_base.depth = base.depth - 10;
		Graphics.Draw(_base, base.x + 3f, base.y + 13f);
		if (_consoleFade > 0.01f)
		{
			_consoleHighlight.depth = base.depth + 5;
			_consoleHighlight.alpha = _consoleFade;
			Graphics.Draw(_consoleHighlight, base.x, base.y);
		}
		base.Draw();
	}
}
