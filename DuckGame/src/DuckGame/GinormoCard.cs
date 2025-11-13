using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GinormoCard : Thing
{
	private float _slideWait;

	private Vec2 _start;

	private Vec2 _end;

	private List<SpriteMap> _sprites = new List<SpriteMap>();

	private Team _team;

	private BitmapFont _font;

	private BitmapFont _smallFont;

	private BoardMode _mode;

	private Sprite _trophy;

	private RenderTarget2D _faceTarget;

	private Sprite _targetSprite;

	private Sprite _gradient;

	private Sprite _edgeOverlay;

	private int index;

	private bool _smallMode;

	public GinormoCard(float slideWait, Vec2 start, Vec2 end, Team team, BoardMode mode, int idx, bool smallMode)
	{
		_smallMode = smallMode;
		base.layer = GinormoBoard.boardLayer;
		_start = start;
		_end = end;
		_slideWait = slideWait;
		position = _start;
		_team = team;
		base.depth = 0.98f;
		index = idx;
		_font = new BitmapFont("biosFont", 8);
		_smallFont = new BitmapFont("smallBiosFont", 7, 6);
		_mode = mode;
		if (_smallMode)
		{
			_faceTarget = new RenderTarget2D(104, 12);
		}
		else
		{
			_faceTarget = new RenderTarget2D(104, 24);
		}
		_targetSprite = new Sprite(_faceTarget);
		_gradient = new Sprite("rockThrow/headGradient2");
		if (_smallMode)
		{
			_edgeOverlay = new Sprite("rockThrow/edgeOverlayShort");
			_trophy = new Sprite("tinyTrophy");
		}
		else
		{
			_edgeOverlay = new Sprite("rockThrow/edgeOverlay");
			_trophy = new Sprite("littleTrophy");
		}
		_trophy.CenterOrigin();
	}

	public override void Update()
	{
		if (_slideWait < 0f)
		{
			position = Vec2.Lerp(position, _end, 0.15f);
		}
		_slideWait -= 0.4f;
		Graphics.SetRenderTarget(_faceTarget);
		Graphics.Clear(Color.Transparent);
		Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, Matrix.Identity);
		_gradient.depth = -0.6f;
		_gradient.alpha = 0.5f;
		if (_team.activeProfiles.Count == 1)
		{
			_gradient.color = _team.activeProfiles[0].persona.colorUsable;
		}
		else
		{
			switch (Teams.CurrentGameTeamIndex(_team))
			{
			case 0:
				_gradient.color = Color.Red;
				break;
			case 1:
				_gradient.color = Color.Blue;
				break;
			case 2:
				_gradient.color = Color.LimeGreen;
				break;
			}
		}
		if (_smallMode)
		{
			_gradient.yscale = 0.5f;
		}
		Graphics.Draw(_gradient, 0f, 0f);
		_edgeOverlay.depth = 0.9f;
		_edgeOverlay.alpha = 0.5f;
		Graphics.Draw(_edgeOverlay, 0f, 0f);
		int i = 0;
		foreach (Profile p in _team.activeProfiles)
		{
			float xpos = (i * 8 + 8) * 2;
			float ypos = 16f;
			p.persona.quackSprite.depth = 0.7f;
			p.persona.quackSprite.scale = new Vec2(2f, 2f);
			if (_smallMode)
			{
				Graphics.Draw(p.persona.quackSprite, 0, xpos - 8f, ypos - 8f);
			}
			else
			{
				Graphics.Draw(p.persona.quackSprite, 0, xpos, ypos, 2f, 2f);
			}
			p.persona.quackSprite.color = Color.White;
			p.persona.quackSprite.scale = new Vec2(1f, 1f);
			Vec2 offset = DuckRig.GetHatPoint(p.persona.sprite.imageIndex);
			SpriteMap hat = p.team.GetHat(p.persona);
			hat.depth = 0.8f;
			hat.center = new Vec2(16f, 16f) + p.team.hatOffset;
			hat.scale = new Vec2(2f, 2f);
			if ((float)hat.texture.width > 16f)
			{
				hat.frame = 1;
			}
			if (_smallMode)
			{
				Graphics.Draw(hat, hat.frame, xpos + offset.x - 8f, ypos + offset.y - 8f);
			}
			else
			{
				Graphics.Draw(hat, hat.frame, xpos + offset.x * 2f, ypos + offset.y * 2f, 2f, 2f);
			}
			hat.color = Color.White;
			hat.scale = new Vec2(1f, 1f);
			hat.frame = 0;
			i++;
		}
		Graphics.screen.End();
		Graphics.SetRenderTarget(null);
		base.Update();
	}

	public override void Draw()
	{
		_font.scale = new Vec2(1f, 1f);
		string name = _team.currentDisplayName;
		float hOffset = 0f;
		float vOffset = 0f;
		if (name.Length > 16)
		{
			name = name.Substring(0, 16);
		}
		string nameText = "@ICONGRADIENT@" + name;
		if (_team != null && _team.activeProfiles != null && _team.activeProfiles.Count > 0)
		{
			BitmapFont namefont = null;
			namefont = ((_team.activeProfiles.Count <= 1) ? _team.activeProfiles[0].font : Profiles.EnvironmentProfile.font);
			namefont.scale = new Vec2(1f, 1f);
			namefont.Draw(nameText, base.x + 182f + hOffset - namefont.GetWidth(nameText), base.y + 2f + vOffset, Color.White, base.depth);
		}
		_font.scale = new Vec2(1f, 1f);
		_targetSprite.scale = new Vec2(1f, 1f);
		Graphics.Draw(_targetSprite, base.x, base.y);
		int i = 0;
		if (_mode == BoardMode.Points)
		{
			string t = Change.ToString(_team.score);
			if (_smallMode)
			{
				_smallFont.scale = new Vec2(1f, 1f);
				_smallFont.Draw(t, base.x + 32f - _smallFont.GetWidth(t), base.y + 2f, Color.White, base.depth);
			}
			else
			{
				_smallFont.scale = new Vec2(2f, 2f);
				_smallFont.Draw(t, base.x + 183f - _smallFont.GetWidth(t), base.y + 10f, Color.White, base.depth);
			}
			return;
		}
		int wins = _team.wins;
		if (_team.activeProfiles.Count == 1)
		{
			wins = _team.activeProfiles[0].wins;
		}
		for (i = 0; i < wins; i++)
		{
			if (_smallMode)
			{
				_trophy.depth = 0.8f + (float)i * 0.01f;
				Graphics.Draw(_trophy, base.x + 24f + (float)(i * 6), base.y + 6f);
			}
			else
			{
				_trophy.depth = 0.8f - (float)i * 0.01f;
				Graphics.Draw(_trophy, base.x + 175f - (float)(i * 8), base.y + 18f);
			}
		}
	}
}
