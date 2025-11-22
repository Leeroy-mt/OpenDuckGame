namespace DuckGame;

public class BackgroundUpdater : Thing
{
	protected ParallaxBackground _parallax;

	protected float _lastCameraX;

	protected bool _update = true;

	protected bool _yParallax = true;

	protected float _yOffset;

	public Rectangle scissor = new Rectangle(0f, 0f, 0f, 0f);

	public bool overrideBaseScissorCall;

	public float _extraYOffset;

	public Color backgroundColor;

	protected bool _skipMovement;

	public ParallaxBackground parallax => _parallax;

	public bool update
	{
		get
		{
			return _update;
		}
		set
		{
			_update = value;
		}
	}

	public void SetVisible(bool vis)
	{
		_parallax.scissor = scissor;
		_parallax.visible = vis;
		if (scissor.width != 0f)
		{
			_parallax.layer.scissor = scissor;
		}
	}

	public BackgroundUpdater(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_isStatic = true;
		_opaque = true;
		editorTooltip = "Adds a parallaxing background visual to the level (limit 1 per level)";
	}

	public static Vec2 GetWallScissor()
	{
		Matrix m = Level.current.camera.getMatrix();
		int xScissor = 0;
		int xRight = 0;
		float xMul = (float)Graphics.width / Resolution.size.x;
		foreach (RockWall wall in Level.current.things[typeof(RockWall)])
		{
			if (xRight == 0)
			{
				xRight = (int)Resolution.size.x;
			}
			Vec2 wallPos = Vec2.Transform(wall.position, m) * xMul;
			if (!wall.flipHorizontal && wallPos.x > (float)xScissor)
			{
				xScissor = (int)wallPos.x;
			}
			else if (wall.flipHorizontal && wallPos.x < (float)xRight)
			{
				xRight = (int)wallPos.x;
			}
		}
		if (xRight != 0)
		{
			xRight -= xScissor;
		}
		if (xRight == 0)
		{
			xRight = (int)Resolution.size.x;
		}
		return new Vec2(xScissor, xRight);
	}

	public override void Update()
	{
		if (!overrideBaseScissorCall)
		{
			Vec2 wallScissor = GetWallScissor();
			if (wallScissor != Vec2.Zero)
			{
				scissor = new Rectangle((int)wallScissor.x, 0f, (int)wallScissor.y, Resolution.current.y);
			}
		}
		if (!_update)
		{
			return;
		}
		if (!_skipMovement)
		{
			float sfactor = Level.current.camera.width * 4f / (float)Graphics.width;
			if (_yParallax)
			{
				_parallax.y = 0f - Level.current.camera.centerY / 12f - 5f + _yOffset;
			}
			else
			{
				Layer.Parallax.camera = Level.current.camera;
				_parallax.y = -108f + _extraYOffset;
			}
			float dif = _lastCameraX - Level.current.camera.centerX;
			_parallax.xmove = dif / sfactor;
		}
		_lastCameraX = Level.current.camera.centerX;
		if (scissor.width != 0f)
		{
			_parallax.scissor = scissor;
		}
		base.Update();
	}

	public override ContextMenu GetContextMenu()
	{
		return null;
	}
}
