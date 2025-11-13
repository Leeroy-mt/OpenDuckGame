using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Graphics
{
	public static List<GraphicsResource> objectsToDispose = new List<GraphicsResource>();

	public static bool disposingObjects = false;

	private static List<Action>[] _renderTasks = new List<Action>[2]
	{
		new List<Action>(),
		new List<Action>()
	};

	private static int _targetFlip = 0;

	private static int _currentStateIndex = 0;

	private static Vec2 _currentDrawOffset = Vec2.Zero;

	private static int _currentDrawIndex = 0;

	public static uint currentDepthSpan;

	public static int effectsLevel = 2;

	private static RenderTarget2D _screenTarget;

	public static bool drawing = false;

	private static bool _recordOnly = false;

	private static GraphicsDevice _base;

	public static GraphicsDeviceManager _manager;

	private static SpriteMap _passwordFont;

	public static BitmapFont _biosFont;

	private static BitmapFont _biosFontCaseSensitive;

	private static FancyBitmapFont _fancyBiosFont;

	private static MTSpriteBatch _defaultBatch;

	private static MTSpriteBatch _currentBatch;

	private static Layer _currentLayer;

	private static int _width;

	private static int _height;

	public static Sprite tounge;

	private static bool _frameFlipFlop = false;

	private static RenderTarget2D _screenCapture;

	private static Tex2D _blank;

	private static Tex2D _blank2;

	private static float _depthBias = 0f;

	private static Matrix _projectionMatrix;

	public static float kSpanIncrement = 0.0001f;

	public static bool caseSensitiveStringDrawing = false;

	public static Vec2 topLeft;

	public static Vec2 bottomRight;

	public static bool didCalc = false;

	public static Material material;

	public static Effect tempEffect;

	public static float snap = 4f;

	public static bool skipReplayRender = false;

	public static bool recordMetadata = false;

	public static bool doSnap = true;

	public static long frame;

	private static Dictionary<Tex2D, Dictionary<Vec3, Tex2D>> _recolorMap = new Dictionary<Tex2D, Dictionary<Vec3, Tex2D>>();

	private static float _baseDeviceWidth = 0f;

	private static float _baseDeviceHeight = 0f;

	private static RenderTarget2D _currentRenderTarget;

	public static RenderTarget2D _screenBufferTarget;

	private static RenderTarget2D _defaultRenderTarget;

	private static bool _settingScreenTarget;

	private static Rectangle _currentTargetSize;

	private static Viewport _oldViewport;

	public static Viewport? _screenViewport;

	private static Viewport _lastViewport;

	private static bool _lastViewportSet = false;

	private static Stack<Rectangle> _scissorStack = new Stack<Rectangle>();

	public static int currentStateIndex
	{
		get
		{
			return _currentStateIndex;
		}
		set
		{
			_currentStateIndex = value;
		}
	}

	public static Vec2 currentDrawOffset
	{
		get
		{
			return _currentDrawOffset;
		}
		set
		{
			_currentDrawOffset = value;
		}
	}

	public static int currentDrawIndex
	{
		get
		{
			return _currentDrawIndex;
		}
		set
		{
			_currentDrawIndex = value;
		}
	}

	public static int fps => FPSCounter.GetFPS(0);

	public static RenderTarget2D screenTarget
	{
		get
		{
			return _screenTarget;
		}
		set
		{
			_screenTarget = value;
		}
	}

	public static bool inFocus => MonoMain.framesBackInFocus > 4;

	public static bool recordOnly
	{
		get
		{
			return _recordOnly;
		}
		set
		{
			_recordOnly = value;
		}
	}

	public static GraphicsDevice device
	{
		get
		{
			if (Thread.CurrentThread != MonoMain.mainThread && Thread.CurrentThread != MonoMain.initializeThread && Thread.CurrentThread != MonoMain.lazyLoadThread)
			{
				throw new Exception("accessing graphics device from thread other than main thread.");
			}
			return _base;
		}
		set
		{
			_base = value;
		}
	}

	public static MTSpriteBatch screen
	{
		get
		{
			return _currentBatch;
		}
		set
		{
			_currentBatch = value;
			if (_currentBatch == null)
			{
				_currentBatch = _defaultBatch;
			}
		}
	}

	public static Layer currentLayer
	{
		get
		{
			return _currentLayer;
		}
		set
		{
			_currentLayer = value;
		}
	}

	public static bool mouseVisible
	{
		get
		{
			return MonoMain.instance.IsMouseVisible;
		}
		set
		{
			MonoMain.instance.IsMouseVisible = value;
		}
	}

	public static int width
	{
		get
		{
			if (!_screenViewport.HasValue)
			{
				return device.Viewport.Width;
			}
			return _screenViewport.Value.Width;
		}
		set
		{
			_width = value;
		}
	}

	public static int height
	{
		get
		{
			if (!_screenViewport.HasValue)
			{
				return device.Viewport.Height;
			}
			return _screenViewport.Value.Height;
		}
		set
		{
			_height = value;
		}
	}

	public static bool frameFlipFlop
	{
		get
		{
			return _frameFlipFlop;
		}
		set
		{
			_frameFlipFlop = value;
		}
	}

	public static RenderTarget2D screenCapture
	{
		get
		{
			return _screenCapture;
		}
		set
		{
			_screenCapture = value;
		}
	}

	private static float _fade
	{
		get
		{
			return MonoMain.core._fade;
		}
		set
		{
			MonoMain.core._fade = value;
		}
	}

	public static float fade
	{
		get
		{
			return _fade;
		}
		set
		{
			_fade = value;
		}
	}

	private static float _fadeAdd
	{
		get
		{
			return MonoMain.core._fadeAdd;
		}
		set
		{
			MonoMain.core._fadeAdd = value;
		}
	}

	public static float fadeAdd
	{
		get
		{
			return _fadeAdd;
		}
		set
		{
			_fadeAdd = value;
		}
	}

	public static float fadeAddRenderValue
	{
		get
		{
			if (!Options.Data.flashing)
			{
				return 0f;
			}
			return _fadeAdd;
		}
	}

	private static float _flashAdd
	{
		get
		{
			return MonoMain.core._flashAdd;
		}
		set
		{
			MonoMain.core._flashAdd = value;
		}
	}

	public static float flashAdd
	{
		get
		{
			return _flashAdd;
		}
		set
		{
			_flashAdd = value;
		}
	}

	public static float flashAddRenderValue
	{
		get
		{
			if (!Options.Data.flashing)
			{
				return 0f;
			}
			return _flashAdd;
		}
	}

	public static Tex2D blankWhiteSquare => _blank;

	public static Matrix projectionMatrix => _projectionMatrix;

	public static Queue<List<DrawCall>> drawCalls
	{
		get
		{
			return Level.core.drawCalls;
		}
		set
		{
			Level.core.drawCalls = value;
		}
	}

	public static List<DrawCall> currentFrameCalls
	{
		get
		{
			return Level.core.currentFrameCalls;
		}
		set
		{
			Level.core.currentFrameCalls = value;
		}
	}

	public static bool skipFrameLog
	{
		get
		{
			return Level.core.skipFrameLog;
		}
		set
		{
			Level.core.skipFrameLog = value;
		}
	}

	public static float baseDeviceWidth => _baseDeviceWidth;

	public static float baseDeviceHeight => _baseDeviceHeight;

	public static bool fixedAspect
	{
		get
		{
			if (!(Resolution.current.aspect > 1.8f))
			{
				if (!(Level.current is XMLLevel))
				{
					return !(Level.current is Editor);
				}
				return false;
			}
			return true;
		}
	}

	public static float aspect => 0.5625f;

	public static bool sixteenTen => aspect > 0.57f;

	public static float barSize => ((float)width * aspect - (float)width * 0.5625f) / 2f;

	public static RenderTarget2D currentRenderTarget => _currentRenderTarget;

	public static RenderTarget2D defaultRenderTarget
	{
		get
		{
			if (_settingScreenTarget)
			{
				return null;
			}
			if (_defaultRenderTarget == null)
			{
				return _screenBufferTarget;
			}
			return _defaultRenderTarget;
		}
		set
		{
			_defaultRenderTarget = value;
		}
	}

	public static Viewport viewport
	{
		get
		{
			if (device == null || device.IsDisposed)
			{
				return _lastViewport;
			}
			return device.Viewport;
		}
		set
		{
			if (!_lastViewportSet)
			{
				_lastViewport = value;
				_lastViewportSet = true;
			}
			if (device.Viewport.Width == _lastViewport.Width && device.Viewport.Height == _lastViewport.Height)
			{
				Rectangle r = value.Bounds;
				if (_currentRenderTarget != null)
				{
					ClipRectangle(r, new Rectangle(0f, 0f, _currentRenderTarget.width, _currentRenderTarget.height));
				}
				else
				{
					ClipRectangle(r, device.PresentationParameters.Bounds);
				}
				value.X = (int)r.x;
				value.Y = (int)r.y;
				value.Width = (int)r.width;
				value.Height = (int)r.height;
				Internal_ViewportSet(value);
				_lastViewport = value;
			}
		}
	}

	public static void GarbageDisposal(bool pLevelTransition)
	{
		lock (objectsToDispose)
		{
			if (!pLevelTransition && objectsToDispose.Count <= 128)
			{
				return;
			}
			disposingObjects = true;
			foreach (GraphicsResource item in objectsToDispose)
			{
				item.Dispose();
			}
			objectsToDispose.Clear();
			disposingObjects = false;
		}
	}

	public static void GarbageDisposal()
	{
		GarbageDisposal(pLevelTransition: true);
	}

	public void Transition(TransitionDirection pDirection, Level pTarget)
	{
	}

	public static void AddRenderTask(Action a)
	{
		_renderTasks[_targetFlip % 2].Add(a);
	}

	public static void RunRenderTasks()
	{
		_targetFlip++;
		foreach (Action item in _renderTasks[(_targetFlip + 1) % 2])
		{
			item();
		}
		_renderTasks[(_targetFlip + 1) % 2].Clear();
	}

	public static void FlashScreen()
	{
		if (Options.Data.flashing)
		{
			flashAdd = 1.3f;
			Layer.Game.darken = 1.3f;
			Layer.Blocks.darken = 1.3f;
			Layer.Foreground.darken = 1.3f;
			Layer.Background.darken = -1.3f;
		}
	}

	public static void SetSize(int w, int h)
	{
		_width = w;
		_height = h;
	}

	public static bool IsBlankTexture(Tex2D tex)
	{
		if (tex != _blank)
		{
			return tex == _blank2;
		}
		return true;
	}

	public static void IncrementSpanAdjust()
	{
	}

	public static void ResetSpanAdjust()
	{
		Depth.ResetSpan();
	}

	public static float AdjustDepth(Depth depth)
	{
		float fDepth = (depth.value + 1f) / 2f * (1f - Depth.kDepthSpanMax) + depth.span;
		return 1f - fDepth;
	}

	public static void ResetDepthBias()
	{
	}

	public static void DrawString(string text, Vec2 position, Color color, Depth depth = default(Depth), InputProfile pro = null, float scale = 1f)
	{
		if (caseSensitiveStringDrawing)
		{
			_biosFontCaseSensitive.scale = new Vec2(scale);
			_biosFontCaseSensitive.Draw(text, position.x, position.y, color, depth, pro);
			_biosFontCaseSensitive.scale = new Vec2(1f);
		}
		else
		{
			_biosFont.scale = new Vec2(scale);
			_biosFont.Draw(text, position.x, position.y, color, depth, pro);
			_biosFont.scale = new Vec2(1f);
		}
	}

	public static void DrawPassword(string text, Vec2 position, Color color, Depth depth = default(Depth), float scale = 1f)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == 'L')
			{
				_passwordFont.frame = 0;
			}
			if (text[i] == 'R')
			{
				_passwordFont.frame = 1;
			}
			if (text[i] == 'U')
			{
				_passwordFont.frame = 2;
			}
			if (text[i] == 'D')
			{
				_passwordFont.frame = 3;
			}
			_passwordFont.scale = new Vec2(scale);
			_passwordFont.color = color;
			_passwordFont.depth = depth;
			Draw(_passwordFont, position.x, position.y);
			position.x += 8f * scale;
		}
	}

	public static void DrawStringColoredSymbols(string text, Vec2 position, Color color, Depth depth = default(Depth), InputProfile pro = null, float scale = 1f)
	{
		if (caseSensitiveStringDrawing)
		{
			_biosFontCaseSensitive.scale = new Vec2(scale);
			_biosFontCaseSensitive.Draw(text, position.x, position.y, color, depth, pro, colorSymbols: true);
			_biosFontCaseSensitive.scale = new Vec2(1f);
		}
		else
		{
			_biosFont.scale = new Vec2(scale);
			_biosFont.Draw(text, position.x, position.y, color, depth, pro, colorSymbols: true);
			_biosFont.scale = new Vec2(1f);
		}
	}

	public static void DrawStringOutline(string text, Vec2 position, Color color, Color outline, Depth depth = default(Depth), InputProfile pro = null, float scale = 1f)
	{
		_biosFont.scale = new Vec2(scale);
		_biosFont.DrawOutline(text, position, color, outline, depth);
		_biosFont.scale = new Vec2(1f);
	}

	public static float GetStringWidth(string text, bool thinButtons = false, float scale = 1f)
	{
		_biosFont.scale = new Vec2(scale);
		text = text.ToUpperInvariant();
		float result = _biosFont.GetWidth(text, thinButtons);
		_biosFont.scale = new Vec2(1f);
		return result;
	}

	public static float GetStringHeight(string text)
	{
		return (float)text.Split('\n').Count() * _biosFont.height;
	}

	public static void DrawFancyString(string text, Vec2 position, Color color, Depth depth = default(Depth), float scale = 1f)
	{
		_fancyBiosFont.scale = new Vec2(scale);
		_fancyBiosFont.Draw(text, position.x, position.y, color, depth);
		_fancyBiosFont.scale = new Vec2(1f);
	}

	public static float GetFancyStringWidth(string text, bool thinButtons = false, float scale = 1f)
	{
		_fancyBiosFont.scale = new Vec2(scale);
		text = text.ToUpperInvariant();
		float result = _fancyBiosFont.GetWidth(text, thinButtons);
		_fancyBiosFont.scale = new Vec2(1f);
		return result;
	}

	public static void DrawRecorderItem(ref RecorderFrameItem item)
	{
		_currentBatch.DrawRecorderItem(ref item);
	}

	public static void DrawRecorderItemLerped(ref RecorderFrameItem item, ref RecorderFrameItem lerpTo, float dist)
	{
		RecorderFrameItem lerped = item;
		lerped.topLeft = Vec2.Lerp(item.topLeft, lerpTo.topLeft, dist);
		lerped.bottomRight = Vec2.Lerp(item.bottomRight, lerpTo.bottomRight, dist);
		float rot1 = item.rotation % 360f;
		float rot2 = lerpTo.rotation % 360f;
		if (rot1 > 180f)
		{
			rot1 -= 360f;
		}
		else if (rot1 < -180f)
		{
			rot1 += 360f;
		}
		if (rot2 > 180f)
		{
			rot2 -= 360f;
		}
		else if (rot2 < -180f)
		{
			rot2 += 360f;
		}
		lerped.rotation = MathHelper.Lerp(rot1, rot2, dist);
		lerped.color = Color.Lerp(item.color, lerpTo.color, dist);
		_currentBatch.DrawRecorderItem(ref lerped);
	}

	public static void Calc()
	{
		if (!didCalc)
		{
			didCalc = true;
			Viewport vp = new Viewport(0, 0, 32, 32);
			Matrix.CreateOrthographicOffCenter(0f, vp.Width, vp.Height, 0f, 0f, -1f, out var projection);
			projection.M41 += -0.5f * projection.M11;
			projection.M42 += -0.5f * projection.M22;
			bottomRight = new Vec2(32f, 32f);
			bottomRight = Vec2.Transform(bottomRight, projection);
			topLeft = new Vec2(0f, 0f);
			topLeft = Vec2.Transform(topLeft, projection);
		}
	}

	public static void Draw(MTSpriteBatchItem item)
	{
		_currentBatch.DrawExistingBatchItem(item);
	}

	public static void Draw(Tex2D texture, Vec2 position, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, Vec2 scale, SpriteEffects effects, Depth depth = default(Depth))
	{
		if (texture.nativeObject is Microsoft.Xna.Framework.Graphics.RenderTarget2D)
		{
			if ((texture.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D).IsDisposed)
			{
				return;
			}
			if (texture.textureIndex == 0)
			{
				Content.AssignTextureIndex(texture);
			}
		}
		if (doSnap)
		{
			position.x = (float)Math.Round(position.x * snap) / snap;
			position.y = (float)Math.Round(position.y * snap) / snap;
		}
		if (effects == SpriteEffects.FlipHorizontally)
		{
			origin.x = (sourceRectangle.HasValue ? sourceRectangle.Value.width : ((float)texture.w)) - origin.x;
		}
		float deep = AdjustDepth(depth);
		if (material != null)
		{
			_currentBatch.DrawWithMaterial(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep, material);
		}
		else
		{
			_currentBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep);
		}
	}

	//public static SpriteBatch Batch; //fna batch test

    //public static void DrawLIne(Vec2 p1, Vec2 p2, Color color, float thickness, Depth depth) //fna batch test
    //{
    //	float angle = (float)Math.Atan2(p2.y - p1.y, p2.x - p1.x);
    //	float length = (p1 - p2).length;
    //	Batch.Draw(
    //	 _blank,
    //	 p1,
    //	 null,
    //	 color,
    //	 angle,
    //	 new Vector2(0f, 0.5f),
    //	 new Vector2(length, thickness),
    //	 SpriteEffects.None,
    //	 AdjustDepth(depth));
    //}

    //public static void DrawRectangleFilled(Rectangle rectangle, Color color, Depth depth) //fna batch test
    //{
    //	Batch.Draw(
    //	 _blank2,
    //	 rectangle.tl,
    //	 null,
    //	 color,
    //	 0f,
    //	 Vec2.Zero,
    //	 new Vector2(-(rectangle.x - rectangle.br.x), -(rectangle.y - rectangle.br.y)),
    //	 SpriteEffects.None,
    //	 AdjustDepth(depth));
    //}

    //public static void Draw(Sprite sprite, Rectangle rect) //fna batch test
    //{
    //	Batch.Draw(
    //		sprite.texture,
    //		sprite.position,
    //		rect,
    //		sprite.color,
    //		sprite.angle,
    //		sprite.center,
    //		sprite.scale,
    //		sprite.flipH
    //		? SpriteEffects.FlipHorizontally
    //		:
    //		(sprite.flipV
    //		? SpriteEffects.FlipVertically
    //		: SpriteEffects.None),
    //		AdjustDepth(sprite.depth));
    //}

    //public static void Draw(Sprite sprite) //fna batch test
    //{
    //	Batch.Draw(
    //		sprite.texture,
    //		sprite.position,
    //		null,
    //		sprite.color,
    //		sprite.angle,
    //		sprite.center,
    //		sprite.scale,
    //		sprite.flipH
    //		? SpriteEffects.FlipHorizontally
    //		:
    //		(sprite.flipV
    //		? SpriteEffects.FlipVertically
    //		: SpriteEffects.None),
    //		AdjustDepth(sprite.depth));
    //}

    //public static void Draw(SpriteMap sprite) //fna batch test
    //{
    //	sprite.UpdateSpriteBox();
    //	Batch.Draw(
    //		sprite.texture,
    //		sprite.position,
    //		sprite.GetSpriteBox(),
    //		sprite.color,
    //		sprite.angle,
    //		sprite.center,
    //		sprite.scale,
    //		sprite.flipH
    //		? SpriteEffects.FlipHorizontally
    //		: 
    //		(sprite.flipV
    //		? SpriteEffects.FlipVertically 
    //		: SpriteEffects.None),
    //		AdjustDepth(sprite.depth));
    //}

    public static void Draw(Sprite g, float x, float y)
	{
		g.x = x;
		g.y = y;
		g.Draw();
	}

	public static void Draw(Sprite g, float x, float y, Rectangle sourceRectangle)
	{
		g.x = x;
		g.y = y;
		g.Draw(sourceRectangle);
	}

	public static void Draw(Sprite g, float x, float y, Rectangle sourceRectangle, Vec2 scale)
	{
		g.x = x;
		g.y = y;
		g.scale = scale;
		g.Draw(sourceRectangle);
	}

	public static void Draw(Sprite g, float x, float y, Rectangle sourceRectangle, Depth depth)
	{
		g.x = x;
		g.y = y;
		g.depth = depth;
		g.Draw(sourceRectangle);
	}

	public static void Draw(Sprite g, float x, float y, Depth depth = default(Depth))
	{
		g.x = x;
		g.y = y;
		g.depth = depth;
		g.Draw();
	}

	public static void Draw(Sprite g, float x, float y, float scaleX, float scaleY)
	{
		g.x = x;
		g.y = y;
		g.xscale = scaleX;
		g.yscale = scaleY;
		g.Draw();
	}

	public static void Draw(Tex2D target, float x, float y, float xscale = 1f, float yscale = 1f, Depth depth = default(Depth))
	{
		Draw(target, new Vec2(x, y), null, Color.White, 0f, Vec2.Zero, new Vec2(xscale, yscale), SpriteEffects.None, depth);
	}

	public static void Draw(SpriteMap g, int frame, float x, float y, float scaleX = 1f, float scaleY = 1f, bool maintainFrame = false)
	{
		g.x = x;
		g.y = y;
		g.xscale = scaleX;
		g.yscale = scaleY;
		int fr = g.frame;
		g.SetFrameWithoutReset(frame);
		g.Draw();
		if (maintainFrame)
		{
			g.SetFrameWithoutReset(fr);
		}
	}

	public static void DrawWithoutUpdate(SpriteMap g, float x, float y, float scaleX = 1f, float scaleY = 1f, bool maintainFrame = false)
	{
		g.x = x;
		g.y = y;
		g.xscale = scaleX;
		g.yscale = scaleY;
		g.DrawWithoutUpdate();
	}

	public static void DrawLine(Vec2 p1, Vec2 p2, Color col, float width = 1f, Depth depth = default(Depth))
	{
		currentDrawIndex++;
		p1 = new Vec2(p1.x, p1.y);
		p2 = new Vec2(p2.x, p2.y);
		float angle = (float)Math.Atan2(p2.y - p1.y, p2.x - p1.x);
		float length = (p1 - p2).length;
		Draw(_blank, p1, null, col, angle, new Vec2(0f, 0.5f), new Vec2(length, width), SpriteEffects.None, depth);
	}

	public static void DrawDottedLine(Vec2 p1, Vec2 p2, Color col, float width = 1f, float dotLength = 8f, Depth depth = default(Depth))
	{
		currentDrawIndex++;
		Vec2 start = p1;
		Vec2 travel = p2 - p1;
		float length = travel.length;
		int num = (int)(length / dotLength);
		travel.Normalize();
		bool off = false;
		for (int i = 0; i < num; i++)
		{
			Vec2 end = start;
			end += travel * dotLength;
			if ((end - p1).length > length)
			{
				end = p2;
			}
			if (!off)
			{
				DrawLine(new Vec2(start.x, start.y), new Vec2(end.x, end.y), col, width, depth);
			}
			off = !off;
			start = end;
		}
	}

	public static void DrawCircle(Vec2 pos, float radius, Color col, float width = 1f, Depth depth = default(Depth), int iterations = 32)
	{
		Vec2 prev = Vec2.Zero;
		for (int i = 0; i < iterations; i++)
		{
			float val = Maths.DegToRad(360f / (float)(iterations - 1) * (float)i);
			Vec2 cur = new Vec2((float)Math.Cos(val) * radius, (0f - (float)Math.Sin(val)) * radius);
			if (i > 0)
			{
				DrawLine(pos + cur, pos + prev, col, width, depth);
			}
			prev = cur;
		}
	}

	public static void DrawTexturedLine(Tex2D texture, Vec2 p1, Vec2 p2, Color col, float width = 1f, Depth depth = default(Depth))
	{
		currentDrawIndex++;
		if (texture.width > 1)
		{
			p1 = new Vec2(p1.x, p1.y);
			p2 = new Vec2(p2.x, p2.y);
			float angle = (float)Math.Atan2(p2.y - p1.y, p2.x - p1.x);
			float length = (p1 - p2).length / (float)texture.width;
			Draw(texture, p1, null, col, angle, new Vec2(0f, texture.height / 2), new Vec2(length, width), SpriteEffects.None, depth);
		}
		else
		{
			p1 = new Vec2(p1.x, p1.y);
			p2 = new Vec2(p2.x, p2.y);
			float angle2 = (float)Math.Atan2(p2.y - p1.y, p2.x - p1.x);
			float length2 = (p1 - p2).length;
			Draw(texture, p1, null, col, angle2, new Vec2(0f, texture.height / 2), new Vec2(length2, width), SpriteEffects.None, depth);
		}
	}

	public static void DrawRect(Vec2 p1, Vec2 p2, Color col, Depth depth = default(Depth), bool filled = true, float borderWidth = 1f)
	{
		currentDrawIndex++;
		if (filled)
		{
			Draw(_blank2, p1, null, col, 0f, Vec2.Zero, new Vec2(0f - (p1.x - p2.x), 0f - (p1.y - p2.y)), SpriteEffects.None, depth);
			return;
		}
		float wideDiv = borderWidth / 2f;
		DrawLine(new Vec2(p1.x, p1.y + wideDiv), new Vec2(p2.x, p1.y + wideDiv), col, borderWidth, depth);
		DrawLine(new Vec2(p1.x + wideDiv, p1.y + borderWidth), new Vec2(p1.x + wideDiv, p2.y - borderWidth), col, borderWidth, depth);
		DrawLine(new Vec2(p2.x, p2.y - wideDiv), new Vec2(p1.x, p2.y - wideDiv), col, borderWidth, depth);
		DrawLine(new Vec2(p2.x - wideDiv, p2.y - borderWidth), new Vec2(p2.x - wideDiv, p1.y + borderWidth), col, borderWidth, depth);
	}

	public static void DrawRect(Rectangle r, Color col, Depth depth = default(Depth), bool filled = true, float borderWidth = 1f)
	{
		currentDrawIndex++;
		Vec2 p1 = new Vec2(r.Left, r.Top);
		Vec2 p2 = new Vec2(r.Right, r.Bottom);
		if (filled)
		{
			Draw(_blank2, p1, null, col, 0f, Vec2.Zero, new Vec2(0f - (p1.x - p2.x), 0f - (p1.y - p2.y)), SpriteEffects.None, depth);
			return;
		}
		float wideDiv = borderWidth / 2f;
		DrawLine(new Vec2(p1.x, p1.y + wideDiv), new Vec2(p2.x, p1.y + wideDiv), col, borderWidth, depth);
		DrawLine(new Vec2(p1.x + wideDiv, p1.y + borderWidth), new Vec2(p1.x + wideDiv, p2.y - borderWidth), col, borderWidth, depth);
		DrawLine(new Vec2(p2.x, p2.y - wideDiv), new Vec2(p1.x, p2.y - wideDiv), col, borderWidth, depth);
		DrawLine(new Vec2(p2.x - wideDiv, p2.y - borderWidth), new Vec2(p2.x - wideDiv, p1.y + borderWidth), col, borderWidth, depth);
	}

	public static void DrawDottedRect(Vec2 p1, Vec2 p2, Color col, Depth depth = default(Depth), float borderWidth = 1f, float dotLength = 8f)
	{
		currentDrawIndex++;
		float wideDiv = borderWidth / 2f;
		DrawDottedLine(new Vec2(p1.x, p1.y + wideDiv), new Vec2(p2.x, p1.y + wideDiv), col, borderWidth, dotLength, depth);
		DrawDottedLine(new Vec2(p1.x + wideDiv, p1.y + borderWidth), new Vec2(p1.x + wideDiv, p2.y - borderWidth), col, borderWidth, dotLength, depth);
		DrawDottedLine(new Vec2(p2.x, p2.y - wideDiv), new Vec2(p1.x, p2.y - wideDiv), col, borderWidth, dotLength, depth);
		DrawDottedLine(new Vec2(p2.x - wideDiv, p2.y - borderWidth), new Vec2(p2.x - wideDiv, p1.y + borderWidth), col, borderWidth, dotLength, depth);
	}

	public static Tex2D Recolor(string sprite, Vec3 color)
	{
		return RecolorOld(Content.Load<Tex2D>(sprite), color);
	}

	public static Tex2D Recolor(Tex2D sprite, Vec3 color)
	{
		Dictionary<Vec3, Tex2D> innerMap = null;
		if (_recolorMap.TryGetValue(sprite, out innerMap))
		{
			Tex2D ret = null;
			if (innerMap.TryGetValue(color, out ret))
			{
				return ret;
			}
		}
		else
		{
			_recolorMap[sprite] = new Dictionary<Vec3, Tex2D>();
		}
		Material mat = new MaterialRecolor(new Vec3(color.x / 255f, color.y / 255f, color.z / 255f));
		RenderTarget2D target = new RenderTarget2D(sprite.w, sprite.h);
		SetRenderTarget(target);
		Clear(new Color(0, 0, 0, 0));
		mat.Apply();
		screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, mat.effect, Matrix.Identity);
		Draw(sprite, default(Vec2), null, Color.White, 0f, default(Vec2), new Vec2(1f, 1f), SpriteEffects.None, 0.5f);
		screen.End();
		device.SetRenderTarget(null);
		Tex2D tex = new Tex2D(sprite.w, sprite.h);
		tex.SetData(target.GetData());
		tex.AssignTextureName("RESKIN");
		target.Dispose();
		_recolorMap[sprite][color] = tex;
		return tex;
	}

	public static Tex2D RecolorOld(Tex2D sprite, Vec3 color)
	{
		Material mat = new MaterialRecolor(new Vec3(color.x / 255f, color.y / 255f, color.z / 255f));
		RenderTarget2D target = new RenderTarget2D(sprite.w, sprite.h);
		SetRenderTarget(target);
		Clear(new Color(0, 0, 0, 0));
		mat.Apply();
		screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, mat.effect, Matrix.Identity);
		Draw(sprite, default(Vec2), null, Color.White, 0f, default(Vec2), new Vec2(1f, 1f), SpriteEffects.None, 0.5f);
		screen.End();
		device.SetRenderTarget(null);
		Tex2D tex2D = new Tex2D(sprite.w, sprite.h);
		tex2D.SetData(target.GetData());
		target.Dispose();
		return tex2D;
	}

	public static Tex2D RecolorNew(Tex2D sprite, Color color1, Color color2)
	{
		Color replace1 = new Color(255, 255, 255);
		Color replace2 = new Color(157, 157, 157);
		Color[] colors = sprite.GetData();
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i] == replace1)
			{
				colors[i] = color1;
			}
			else if (colors[i] == replace2)
			{
				colors[i] = color2;
			}
		}
		Tex2D tex2D = new Tex2D(sprite.w, sprite.h);
		tex2D.SetData(colors);
		return tex2D;
	}

	public static Tex2D RecolorM(Tex2D sprite, Color color1, Color color2)
	{
		Color replace1 = new Color(195, 184, 172);
		Color replace2 = new Color(163, 147, 128);
		Color[] colors = sprite.GetData();
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i] == replace1)
			{
				colors[i] = color1;
			}
			else if (colors[i] == replace2)
			{
				colors[i] = color2;
			}
		}
		Tex2D tex2D = new Tex2D(sprite.w, sprite.h);
		tex2D.SetData(colors);
		return tex2D;
	}

	public static Tex2D RecolorM(Tex2D sprite, Color color1, Color color2, Color color3)
	{
		Color replace1 = new Color(161, 146, 130);
		Color replace2 = new Color(128, 113, 96);
		Color replace3 = new Color(191, 181, 171);
		Color foot1 = new Color(236, 89, 60);
		Color foot2 = new Color(248, 131, 99);
		Color footr1 = new Color(235, 137, 49);
		Color footr2 = new Color(247, 224, 90);
		Color beak1 = new Color(219, 88, 31);
		Color beak2 = new Color(236, 116, 60);
		Color beakr1 = new Color(164, 100, 34);
		Color beakr2 = new Color(235, 137, 49);
		Color[] colors = sprite.GetData();
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i] == replace1)
			{
				colors[i] = color1;
			}
			else if (colors[i] == replace2)
			{
				colors[i] = color2;
			}
			else if (colors[i] == replace3)
			{
				colors[i] = color3;
			}
			else if (colors[i] == foot1)
			{
				colors[i] = footr1;
			}
			else if (colors[i] == foot2)
			{
				colors[i] = footr2;
			}
			else if (colors[i] == beak1)
			{
				colors[i] = beakr1;
			}
			else if (colors[i] == beak2)
			{
				colors[i] = beakr2;
			}
		}
		Tex2D tex2D = new Tex2D(sprite.w, sprite.h);
		tex2D.SetData(colors);
		return tex2D;
	}

	public static void InitializeBase(GraphicsDeviceManager m, int widthVal, int heightVal)
	{
		_manager = m;
		_width = widthVal;
		_baseDeviceWidth = _width;
		_height = heightVal;
		_baseDeviceHeight = _height;
	}

	public static void Initialize(GraphicsDevice d)
	{
		_base = d;
		_defaultBatch = new MTSpriteBatch(_base);
		screen = _defaultBatch;
		_blank = new Tex2D(1, 1);
		_blank.SetData(new Color[1] { Color.White });
		_blank2 = new Tex2D(1, 1);
		_blank2.SetData(new Color[1] { Color.White });
		_biosFont = new BitmapFont("biosFont", 8);
		_biosFontCaseSensitive = new BitmapFont("biosFontCaseSensitive", 8);
		_fancyBiosFont = new FancyBitmapFont("smallFont");
		_passwordFont = new SpriteMap("passwordFont", 8, 8);
		Matrix.CreateOrthographicOffCenter(0f, d.Viewport.Width, d.Viewport.Height, 0f, 0f, 1f, out _projectionMatrix);
		_projectionMatrix.M41 += -0.5f * _projectionMatrix.M11;
		_projectionMatrix.M42 += -0.5f * _projectionMatrix.M22;
		tounge = new Sprite("tounge");


        //Batch = new(d); //fna batch test
    }

    public static void SetRenderTargetToScreen()
	{
		_settingScreenTarget = true;
		SetRenderTarget(null);
		_settingScreenTarget = false;
	}

	public static void SetRenderTarget(RenderTarget2D t)
	{
		if (t != null && t.IsDisposed)
		{
			return;
		}
		if (t == null)
		{
			Microsoft.Xna.Framework.Graphics.RenderTarget2D screenTarget = ((defaultRenderTarget != null) ? (defaultRenderTarget.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D) : null);
			if (screenTarget == null)
			{
				_currentTargetSize.width = Resolution.current.x;
				_currentTargetSize.height = Resolution.current.y;
			}
			else
			{
				_currentTargetSize.width = screenTarget.Width;
				_currentTargetSize.height = screenTarget.Height;
			}
			device.SetRenderTarget(screenTarget);
			if (!_settingScreenTarget && _defaultRenderTarget == null)
			{
				UpdateScreenViewport();
			}
		}
		else
		{
			device.SetRenderTarget(t.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D);
			_currentTargetSize.width = t.width;
			_currentTargetSize.height = t.height;
		}
		_lastViewport = device.Viewport;
		_currentRenderTarget = t;
	}

	public static RenderTarget2D GetRenderTarget()
	{
		return _currentRenderTarget;
	}

	public static void SetFullViewport()
	{
		_oldViewport = device.Viewport;
		Internal_ViewportSet(new Viewport
		{
			X = 0,
			Y = 0,
			Width = (int)_currentTargetSize.width,
			Height = (int)_currentTargetSize.height
		});
	}

	public static void RestoreOldViewport()
	{
		Internal_ViewportSet(_oldViewport);
	}

	private static void Internal_ViewportSet(Viewport pViewport)
	{
		try
		{
			device.Viewport = pViewport;
		}
		catch (Exception)
		{
			DevConsole.Log("Error: Invalid Viewport (x = " + pViewport.X + ", y = " + pViewport.Y + ", w = " + pViewport.Width + ", h = " + pViewport.Height + ", minDepth = " + pViewport.MinDepth + ", maxDepth = " + pViewport.MaxDepth + ")");
		}
	}

	public static void UpdateScreenViewport(bool pForceReset = false)
	{
		try
		{
			if (pForceReset || !_screenViewport.HasValue)
			{
				Viewport v = default(Viewport);
				if (_currentTargetSize.aspect < 1.77f)
				{
					v.Width = (int)_currentTargetSize.width;
					v.Height = Math.Min((int)Math.Round(_currentTargetSize.width / 1.77777f), (int)_currentTargetSize.height);
				}
				else
				{
					v.Height = (int)_currentTargetSize.height;
					v.Width = Math.Min((int)Math.Round(_currentTargetSize.height * 1.77777f), (int)_currentTargetSize.width);
				}
				v.X = Math.Max((int)((_currentTargetSize.width - (float)v.Width) / 2f), 0);
				v.Y = Math.Max((int)((_currentTargetSize.height - (float)v.Height) / 2f), 0);
				v.MinDepth = 0f;
				v.MaxDepth = 1f;
				_screenViewport = v;
			}
			Internal_ViewportSet(_screenViewport.Value);
		}
		catch (Exception)
		{
		}
		_lastViewport = device.Viewport;
	}

	public static void SetScreenTargetViewport()
	{
		Viewport v = default(Viewport);
		if (Resolution.adapterResolution.aspect < Resolution.current.aspect)
		{
			v.Width = Resolution.adapterResolution.x;
			v.Height = Math.Min((int)Math.Round((float)Resolution.adapterResolution.x / Resolution.current.aspect), Resolution.adapterResolution.y);
		}
		else
		{
			v.Height = Resolution.adapterResolution.y;
			v.Width = Math.Min((int)Math.Round((float)Resolution.adapterResolution.y * Resolution.current.aspect), Resolution.adapterResolution.x);
		}
		v.X = Math.Max((Resolution.adapterResolution.x - v.Width) / 2, 0);
		v.Y = Math.Max((Resolution.adapterResolution.y - v.Height) / 2, 0);
		v.MinDepth = 0f;
		v.MaxDepth = 1f;
		Internal_ViewportSet(v);
	}

	public static Rectangle GetScissorRectangle()
	{
		return new Rectangle(device.ScissorRectangle.X, device.ScissorRectangle.Y, device.ScissorRectangle.Width, device.ScissorRectangle.Height);
	}

	public static void SetScissorRectangle(Rectangle r)
	{
		float mul = (float)device.Viewport.Bounds.Width / (float)width;
		if (r.width >= 0f && r.height >= 0f)
		{
			r.width *= mul;
			r.height *= mul;
			r.x *= mul;
			r.y *= mul;
			r.x += viewport.X;
			r.y += viewport.Y;
			device.ScissorRectangle = ClipRectangle(r, device.Viewport.Bounds);
		}
	}

	public static void PushLayerScissor(Rectangle pRect)
	{
		if (screen != null)
		{
			screen.FlushSettingScissor();
		}
		_scissorStack.Push(pRect);
		float widthDif = (float)width / currentLayer.width;
		float heightDif = (float)height / currentLayer.height;
		pRect.x *= widthDif;
		pRect.y *= heightDif;
		pRect.width *= widthDif;
		pRect.height *= heightDif;
		SetScissorRectangle(pRect);
	}

	public static void PopLayerScissor()
	{
		if (screen != null)
		{
			screen.FlushAndClearScissor();
		}
		_scissorStack.Pop();
		if (_scissorStack.Count == 0)
		{
			SetScissorRectangle(new Rectangle(0f, 0f, width, height));
		}
		else
		{
			SetScissorRectangle(_scissorStack.Peek());
		}
	}

	public static Rectangle ClipRectangle(Rectangle r, Rectangle clipTo)
	{
		if (r.x > clipTo.Right)
		{
			r.x = clipTo.Right - r.width;
		}
		if (r.y > clipTo.Bottom)
		{
			r.y = clipTo.Bottom - r.height;
		}
		if (r.x < clipTo.Left)
		{
			r.x = clipTo.Left;
		}
		if (r.y < clipTo.Top)
		{
			r.y = clipTo.Top;
		}
		if (r.x < 0f)
		{
			r.x = 0f;
		}
		if (r.y < 0f)
		{
			r.y = 0f;
		}
		if (r.x + r.width > clipTo.x + clipTo.width)
		{
			r.width = clipTo.Right - r.x;
		}
		if (r.y + r.height > clipTo.y + clipTo.height)
		{
			r.height = clipTo.Bottom - r.y;
		}
		if (r.width < 0f)
		{
			r.width = 0f;
		}
		if (r.height < 0f)
		{
			r.height = 0f;
		}
		return r;
	}

	public static void Clear(Color c)
	{
		device.Clear(c);
	}

	public static void PushMarker(string s)
	{
	}

	public static void PopMarker()
	{
	}
}
