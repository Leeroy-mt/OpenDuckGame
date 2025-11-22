using System;

namespace DuckGame;

[EditorGroup("Background|Parallax")]
public class VirtualBackground : BackgroundUpdater
{
	private Sprite _scanner;

	private BackgroundUpdater _realBackground;

	private int scanStage = -1;

	private float stick;

	public bool needsWireframe;

	public bool fullyVirtual = true;

	public bool fullyNonVirtual;

	public bool virtualMode = true;

	public new bool visible = true;

	private bool _foreground;

	private static ParallaxBackground _para;

	private bool done;

	private bool incStage;

	private bool decStage;

	public void SetLayer(Layer l)
	{
		base.layer = l;
		_parallax.layer = l;
	}

	public VirtualBackground(float xpos, float ypos, BackgroundUpdater realBackground, bool fore = false)
		: base(xpos, ypos)
	{
		graphic = new SpriteMap("backgroundIcons", 16, 16)
		{
			frame = 2
		};
		center = new Vec2(8f, 8f);
		_collisionSize = new Vec2(16f, 16f);
		_collisionOffset = new Vec2(-8f, -8f);
		base.depth = 0.9f;
		base.layer = Layer.Foreground;
		_visibleInGame = false;
		_editorName = "Virtual";
		_realBackground = realBackground;
		_foreground = fore;
	}

	public static void InitializeBack()
	{
		_para = new ParallaxBackground("background/virtual", 0f, 0f, 3);
		float speed = 0.4f;
		float rearDist = 0.8f;
		_para.AddZone(0, rearDist, speed);
		_para.AddZone(1, rearDist, speed);
		_para.AddZone(2, rearDist, speed);
		_para.AddZone(3, rearDist, speed);
		float closeDist = 0.6f;
		float div = (rearDist - closeDist) / 4f;
		float speedo = 1f;
		_para.AddZone(4, rearDist - div * 1f, speedo, moving: true);
		_para.AddZone(5, rearDist - div * 2f, 0f - speedo, moving: true);
		_para.AddZone(6, rearDist - div * 3f, speedo, moving: true);
		_para.AddZone(7, closeDist, speed);
		_para.AddZone(8, closeDist, speed);
		_para.AddZone(19, closeDist, speed);
		_para.AddZone(20, closeDist, speed);
		_para.AddZone(21, rearDist - div * 3f, 0f - speedo, moving: true);
		_para.AddZone(22, rearDist - div * 2f, speedo, moving: true);
		_para.AddZone(23, rearDist - div * 1f, 0f - speedo, moving: true);
		_para.AddZone(24, rearDist, speed);
		_para.AddZone(25, rearDist, speed);
		_para.AddZone(26, rearDist, speed);
		_para.AddZone(27, rearDist, speed);
		_para.AddZone(28, rearDist, speed);
		_para.AddZone(29, rearDist, speed);
		_para.AddZone(30, rearDist, speed);
		_para.AddZone(31, rearDist, speed);
		_para.AddZone(32, rearDist, speed);
		_para.AddZone(33, rearDist, speed);
		_para.AddZone(34, rearDist, speed);
		_para.restrictBottom = false;
		_para.layer = new Layer("VIRTUALPARALLAX", 95, new Camera(0f, 0f, 320f, 320f * Graphics.aspect));
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			fullyVirtual = true;
			fullyNonVirtual = false;
			virtualMode = true;
			needsWireframe = true;
			backgroundColor = new Color(0, 0, 0);
			Level.current.backgroundColor = backgroundColor;
			_parallax = _para;
			Layer.Add(_parallax.layer);
			_parallax.layer.Clear();
			Level.Add(_parallax);
			visible = true;
			base.parallax.y = 0f;
			base.layer = _parallax.layer;
			base.layer.fade = 1f;
			_scanner = new Sprite("background/scanbeam");
			_skipMovement = true;
		}
	}

	public override void Terminate()
	{
		fullyVirtual = false;
		fullyNonVirtual = true;
		virtualMode = false;
		needsWireframe = false;
	}

	public override void Update()
	{
		float backStick = stick;
		if (scanStage < 2)
		{
			Level.current.backgroundColor = Lerp.Color(Level.current.backgroundColor, backgroundColor, 0.04f);
			backStick = 0f;
		}
		else if (_realBackground != null)
		{
			Level.current.backgroundColor = Lerp.Color(Level.current.backgroundColor, _realBackground.backgroundColor, 0.04f);
		}
		Rectangle sc = new Rectangle((int)((1f - backStick) * (float)Resolution.current.x), 0f, Resolution.current.x - (int)((1f - backStick) * (float)Resolution.current.x), Resolution.current.y);
		if (_realBackground != null)
		{
			if (sc.width == 0f)
			{
				_realBackground.SetVisible(vis: false);
			}
			else
			{
				_realBackground.scissor = sc;
				_realBackground.SetVisible(vis: true);
			}
		}
		Rectangle sc2 = new Rectangle(0f, 0f, (float)Resolution.current.x - sc.width, Resolution.current.y);
		if (sc2.width == 0f)
		{
			SetVisible(vis: false);
			visible = false;
		}
		else
		{
			scissor = sc2;
			SetVisible(vis: true);
			visible = true;
		}
		if (virtualMode && done && scanStage == 3)
		{
			scanStage--;
		}
		else if (!virtualMode && !done && scanStage == -1)
		{
			scanStage++;
		}
		float lerpSpeed = 0.04f;
		float outLerpSpeed = 0.06f;
		if (Level.current != null)
		{
			lerpSpeed *= Level.current.transitionSpeedMultiplier;
			outLerpSpeed *= Level.current.transitionSpeedMultiplier;
		}
		if (!done)
		{
			if (scanStage == 0)
			{
				stick = Lerp.Float(stick, 1f, lerpSpeed);
				if (stick > 0.95f)
				{
					stick = 1f;
					incStage = true;
				}
			}
			else if (scanStage == 1)
			{
				stick = Lerp.Float(stick, 0f, lerpSpeed);
				if (stick < 0.05f)
				{
					stick = 0f;
					incStage = true;
				}
			}
			else if (scanStage == 2)
			{
				stick = Lerp.Float(stick, 1f, lerpSpeed);
				if (stick > 0.95f)
				{
					stick = 1f;
					incStage = true;
					done = true;
				}
			}
		}
		else if (scanStage == 2)
		{
			stick = Lerp.Float(stick, 0f, outLerpSpeed);
			if (stick < 0.05f)
			{
				stick = 0f;
				decStage = true;
			}
		}
		else if (scanStage == 1)
		{
			stick = Lerp.Float(stick, 1f, outLerpSpeed);
			if (stick > 0.95f)
			{
				stick = 1f;
				decStage = true;
			}
		}
		else if (scanStage == 0)
		{
			stick = Lerp.Float(stick, 0f, outLerpSpeed);
			if (stick < 0.05f)
			{
				stick = 0f;
				decStage = true;
				done = false;
			}
		}
		if (scanStage < 2)
		{
			Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(stick);
			if (scanStage == 1)
			{
				Layer.basicWireframeTex = true;
			}
			else
			{
				Layer.basicWireframeTex = false;
			}
		}
		if (incStage)
		{
			incStage = false;
			scanStage++;
		}
		if (decStage)
		{
			decStage = false;
			scanStage--;
		}
		fullyVirtual = false;
		fullyNonVirtual = false;
		if (scanStage == 3)
		{
			needsWireframe = false;
			fullyNonVirtual = true;
		}
		else
		{
			needsWireframe = true;
			if (scanStage == -1)
			{
				fullyVirtual = true;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (_parallax != null)
		{
			if (!visible)
			{
				_parallax.visible = false;
				return;
			}
			position = _parallax.position;
			float scannerX = stick * 300f;
			float scannerFrontX = 360f - stick * 400f;
			Vec2 scannerPos = new Vec2(base.x + scannerX, base.y + 72f);
			Graphics.Draw(_scanner, scannerPos.x, scannerPos.y);
			float scanMiddle = Math.Abs(stick - 0.5f);
			float a = 0.5f - scanMiddle;
			Graphics.DrawLine(scannerPos + new Vec2(18f, 20f), new Vec2(scannerFrontX, scannerPos.y - 100f + scanMiddle * 250f), Color.Red * a, 2f, 0.9f);
			Graphics.DrawLine(scannerPos + new Vec2(18f, 34f), new Vec2(scannerFrontX, scannerPos.y - 10f + 80f * scanMiddle), Color.Red * a, 2f, 0.9f);
			Vec2 scannerPosBottom = scannerPos + new Vec2(0f, _scanner.height);
			Graphics.DrawLine(scannerPosBottom + new Vec2(18f, -20f), new Vec2(scannerFrontX, scannerPosBottom.y + 100f - scanMiddle * 250f), Color.Red * a, 2f, 0.9f);
			Graphics.DrawLine(scannerPosBottom + new Vec2(18f, -34f), new Vec2(scannerFrontX, scannerPosBottom.y + 10f - 80f * scanMiddle), Color.Red * a, 2f, 0.9f);
		}
	}
}
