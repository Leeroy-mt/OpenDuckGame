using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GinormoBoard : Thing
{
	private Sprite _board;

	private Sprite _boardTop;

	private Sprite _boardBottom;

	private GinormoScreen _screen;

	private SpriteMap _lighting;

	private bool _activated;

	private BoardMode _mode;

	private Vec2 _pos;

	private Layer boardLightingLayer;

	private Layer overlayLayer;

	private bool _smallMode;

	public bool activated => _activated;

	public static Layer boardLayer
	{
		get
		{
			return MonoMain.core.ginormoBoardLayer;
		}
		set
		{
			MonoMain.core.ginormoBoardLayer = value;
		}
	}

	public GinormoBoard(float xpos, float ypos, BoardMode mode, bool smallMode)
		: base(xpos, ypos)
	{
		_smallMode = smallMode;
		_board = new Sprite("rockThrow/boardMiddle");
		_board.center = new Vec2(_board.w / 2, _board.h / 2 - 30);
		_lighting = new SpriteMap("rockThrow/lighting", 191, 23);
		_lighting.frame = 1;
		boardLightingLayer = new Layer("LIGHTING", -85);
		BlendState blend = new BlendState
		{
			ColorSourceBlend = Blend.Zero,
			ColorDestinationBlend = Blend.SourceColor,
			ColorBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.SourceColor,
			AlphaBlendFunction = BlendFunction.Add
		};
		boardLightingLayer.blend = blend;
		Layer.Add(boardLightingLayer);
		Level.Add(new BoardLighting(base.x + 0.5f, base.y - 125f)
		{
			layer = boardLightingLayer
		});
		if (RockWeather.weather == Weather.Snowing)
		{
			_boardTop = new Sprite("rockThrow/boardTopSnow");
			_boardBottom = new Sprite("rockThrow/boardBottomSnow");
		}
		else
		{
			_boardTop = new Sprite("rockThrow/boardTop");
			_boardBottom = new Sprite("rockThrow/boardBottom");
		}
		_boardTop.center = new Vec2(_boardTop.w / 2, _boardTop.h / 2 - 30);
		_boardBottom.center = new Vec2(_boardBottom.w / 2, _boardBottom.h / 2 - 30);
		base.layer = Layer.Background;
		_pos = new Vec2(xpos, ypos);
		_mode = mode;
		boardLayer = new Layer("BOARD", -85, null, targetLayer: true, new Vec2(GinormoScreen.GetSize(_smallMode).x, GinormoScreen.GetSize(_smallMode).y));
		boardLayer.camera = new Camera(0f, 0f, GinormoScreen.GetSize(_smallMode).x, GinormoScreen.GetSize(_smallMode).y);
		boardLayer.targetOnly = true;
		boardLayer.targetClearColor = new Color(0.05f, 0.05f, 0.05f);
		Layer.Add(boardLayer);
		overlayLayer = new Layer("OVERLAY", 10);
		Layer.Add(overlayLayer);
		Level.Add(new GinormoOverlay(base.x - 182f, base.y - 65f, _smallMode)
		{
			z = -130f,
			position = position,
			layer = overlayLayer
		});
	}

	public void Activate()
	{
		if (!_activated)
		{
			_screen = new GinormoScreen(0f, 0f, _mode);
			Level.Add(_screen);
			_activated = true;
		}
	}

	public override void Draw()
	{
		boardLightingLayer.perspective = true;
		boardLightingLayer.projection = Layer.Background.projection;
		boardLightingLayer.view = Layer.Background.view;
		overlayLayer.perspective = true;
		overlayLayer.projection = Layer.Game.projection;
		overlayLayer.view = Layer.Game.view;
		overlayLayer.camera = Layer.Game.camera;
		float val = 1f - RockWeather.lightOpacity;
		boardLightingLayer.colorAdd = new Vec3(val);
		if (RockWeather.lightOpacity > 0.01f)
		{
			_lighting.frame = 1;
		}
		else
		{
			_lighting.frame = 0;
		}
		_board.depth = base.depth;
		Graphics.Draw(_board, base.x, base.y - 12f);
		Graphics.Draw(_boardBottom, base.x, base.y + 58f);
		Graphics.Draw(_boardTop, base.x, base.y - 68f);
		if (RockScoreboard._sunEnabled)
		{
			Graphics.Draw(_lighting, base.x - 95f, base.y - 67f);
		}
	}
}
