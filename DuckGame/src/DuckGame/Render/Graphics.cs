using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public static class Graphics
{
    #region Public Fields

    public static bool disposingObjects;
    public static bool caseSensitiveStringDrawing;
    public static bool skipReplayRender;
    public static bool recordMetadata;
    public static bool doSnap = true;

    public static int effectsLevel = 2;

    public static float snap = 4;

    public static long frame;

    public static Viewport? _screenViewport;

    public static GraphicsDeviceManager _manager;
    public static Sprite tounge;
    public static BitmapFont _biosFont;
    public static AutoEffect material;
#if NO_TEX2D
    public static XnaRenderTarget2D _screenBufferTarget;
#else
    public static RenderTarget2D _screenBufferTarget;
#endif

    public static List<GraphicsResource> objectsToDispose = [];

    #endregion

    #region Private Fields

    static bool _frameFlipFlop;
    static bool _settingScreenTarget;
    static bool _lastViewportSet;

    static int _targetFlip;
    static int _currentStateIndex;

    static Matrix _projectionMatrix;
    static RectangleF _currentTargetSize;
    static Viewport _oldViewport;
    static Viewport _lastViewport;

    static GraphicsDevice _base;
    static SpriteMap _passwordFont;
    static BitmapFont _biosFontCaseSensitive;
    static FancyBitmapFont _fancyBiosFont;
#if !MODERN_BATCH
    static MTSpriteBatch _defaultBatch;
    static MTSpriteBatch _currentBatch;
#else
    static TriangleBatch _defaultBatch;
    static TriangleBatch _currentBatch;
#endif
    static Layer _currentLayer;
#if NO_TEX2D
    static XnaRenderTarget2D _screenCapture;
    static XnaRenderTarget2D _currentRenderTarget;
    static XnaRenderTarget2D _defaultRenderTarget;
#else
    static RenderTarget2D _screenCapture;
    static RenderTarget2D _currentRenderTarget;
    static RenderTarget2D _defaultRenderTarget;
#endif
#if !MODERN_BATCH
    static Tex2D blank;
#endif

    static readonly List<Action>[] _renderTasks = [[], []];

    #endregion

    #region Public Properties

    public static bool inFocus => MonoMain.framesBackInFocus > 4;
    public static bool mouseVisible
    {
        get => MonoMain.instance.IsMouseVisible;
        set => MonoMain.instance.IsMouseVisible = value;
    }
    public static bool frameFlipFlop
    {
        get => _frameFlipFlop;
        set => _frameFlipFlop = value;
    }

    public static int currentStateIndex
    {
        get => _currentStateIndex;
        set => _currentStateIndex = value;
    }
    public static int fps => FPSCounter.GetFPS(0);
    public static int width
    {
        get
        {
            if (!_screenViewport.HasValue)
                return device.Viewport.Width;
            return _screenViewport.Value.Width;
        }
    }
    public static int height
    {
        get
        {
            if (!_screenViewport.HasValue)
                return device.Viewport.Height;
            return _screenViewport.Value.Height;
        }
    }

    public static float flashAdd
    {
        get => _flashAdd;
        set => _flashAdd = value;
    }
    public static float flashAddRenderValue
    {
        get
        {
            if (!Options.Data.flashing)
                return 0;
            return _flashAdd;
        }
    }
    public static float fade
    {
        get => _fade;
        set => _fade = value;
    }
    public static float fadeAdd
    {
        get => _fadeAdd;
        set => _fadeAdd = value;
    }
    public static float fadeAddRenderValue
    {
        get
        {
            if (!Options.Data.flashing)
                return 0;
            return _fadeAdd;
        }
    }
    public static float aspect => .5625f;

    public static Viewport viewport
    {
        get
        {
            if (device == null || device.IsDisposed)
                return _lastViewport;
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
                RectangleF r = value.Bounds;
                if (_currentRenderTarget != null)
#if NO_TEX2D
                    ClipRectangle(r, new RectangleF(0, 0, _currentRenderTarget.Width, _currentRenderTarget.Height));
#else
                    ClipRectangle(r, new RectangleF(0, 0, _currentRenderTarget.width, _currentRenderTarget.height));
#endif
                else
                    ClipRectangle(r, device.PresentationParameters.Bounds);
                value.X = (int)r.X;
                value.Y = (int)r.Y;
                value.Width = (int)r.Width;
                value.Height = (int)r.Height;
                Internal_ViewportSet(value);
                _lastViewport = value;
            }
        }
    }

    public static GraphicsDevice device
    {
        get
        {
            if (Thread.CurrentThread != MonoMain.mainThread)
                throw new("accessing graphics device from thread other than main thread.");
            return _base;
        }
        set => _base = value;
    }
#if !MODERN_BATCH
    public static MTSpriteBatch screen
#else
    public static TriangleBatch screen
#endif
    {
        get => _currentBatch;
        set
        {
            _currentBatch = value;
            _currentBatch ??= _defaultBatch;
        }
    }
    public static Layer currentLayer
    {
        get => _currentLayer;
        set => _currentLayer = value;
    }
#if NO_TEX2D
    public static XnaRenderTarget2D screenCapture
    {
        get => _screenCapture;
        set => _screenCapture = value;
    }
    public static XnaRenderTarget2D currentRenderTarget => _currentRenderTarget;
    public static XnaRenderTarget2D defaultRenderTarget
#else
    public static RenderTarget2D screenCapture
    {
        get => _screenCapture;
        set => _screenCapture = value;
    }
    public static RenderTarget2D currentRenderTarget => _currentRenderTarget;
    public static RenderTarget2D defaultRenderTarget
#endif
    {
        get
        {
            if (_settingScreenTarget)
                return null;
            if (_defaultRenderTarget == null)
                return _screenBufferTarget;
            return _defaultRenderTarget;
        }
        set => _defaultRenderTarget = value;
    }

    public static Queue<List<DrawCall>> drawCalls
    {
        get => Level.core.drawCalls;
        set => Level.core.drawCalls = value;
    }
    public static List<DrawCall> currentFrameCalls
    {
        get => Level.core.currentFrameCalls;
        set => Level.core.currentFrameCalls = value;
    }

#endregion

    #region Private Properties

    static float _fade
    {
        get => MonoMain.core._fade;
        set => MonoMain.core._fade = value;
    }

    static float _fadeAdd
    {
        get => MonoMain.core._fadeAdd;
        set => MonoMain.core._fadeAdd = value;
    }

    static float _flashAdd
    {
        get => MonoMain.core._flashAdd;
        set => MonoMain.core._flashAdd = value;
    }

    #endregion

    #region Public Methods

    public static void GarbageDisposal(bool pLevelTransition)
    {
        lock (objectsToDispose)
        {
            if (!pLevelTransition && objectsToDispose.Count <= 128)
                return;

            disposingObjects = true;
            foreach (GraphicsResource item in objectsToDispose)
                item.Dispose();

            objectsToDispose.Clear();
            disposingObjects = false;
        }
    }

    public static void AddRenderTask(Action a)
    {
        _renderTasks[_targetFlip % 2].Add(a);
    }

    public static void RunRenderTasks()
    {
        _targetFlip++;
        foreach (Action item in _renderTasks[(_targetFlip + 1) % 2])
            item();
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

    public static void ResetSpanAdjust()
    {
        Depth.ResetSpan();
    }

    public static float AdjustDepth(Depth depth)
    {
        float fDepth = (depth.value + 1) / 2 * (1 - Depth.kDepthSpanMax) + depth.span;
        return 1 - fDepth;
    }

    public static void DrawString(string text, Vector2 position, Color color, Depth depth = default, InputProfile pro = null, float scale = 1)
    {
        if (caseSensitiveStringDrawing)
        {
            _biosFontCaseSensitive.Scale = new Vector2(scale);
            _biosFontCaseSensitive.Draw(text, position.X, position.Y, color, depth, pro);
            _biosFontCaseSensitive.Scale = Vector2.One;
        }
        else
        {
            _biosFont.Scale = new Vector2(scale);
            _biosFont.Draw(text, position.X, position.Y, color, depth, pro);
            _biosFont.Scale = Vector2.One;
        }
    }

    public static void DrawPassword(string text, Vector2 position, Color color, Depth depth = default, float scale = 1)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == 'L')
                _passwordFont.frame = 0;
            if (text[i] == 'R')
                _passwordFont.frame = 1;
            if (text[i] == 'U')
                _passwordFont.frame = 2;
            if (text[i] == 'D')
                _passwordFont.frame = 3;
            _passwordFont.Scale = new Vector2(scale);
            _passwordFont.color = color;
            _passwordFont.Depth = depth;
            Draw(_passwordFont, position.X, position.Y);
            position.X += 8 * scale;
        }
    }

    public static void DrawStringColoredSymbols(string text, Vector2 position, Color color, Depth depth = default, InputProfile pro = null, float scale = 1)
    {
        if (caseSensitiveStringDrawing)
        {
            _biosFontCaseSensitive.Scale = new Vector2(scale);
            _biosFontCaseSensitive.Draw(text, position.X, position.Y, color, depth, pro, colorSymbols: true);
            _biosFontCaseSensitive.Scale = Vector2.One;
        }
        else
        {
            _biosFont.Scale = new Vector2(scale);
            _biosFont.Draw(text, position.X, position.Y, color, depth, pro, colorSymbols: true);
            _biosFont.Scale = Vector2.One;
        }
    }

    public static void DrawStringOutline(string text, Vector2 position, Color color, Color outline, Depth depth = default, float scale = 1)
    {
        _biosFont.Scale = new Vector2(scale);
        _biosFont.DrawOutline(text, position, color, outline, depth);
        _biosFont.Scale = Vector2.One;
    }

    public static float GetStringWidth(string text, bool thinButtons = false, float scale = 1)
    {
        var previousScale = _biosFont.Scale;
        _biosFont.Scale = new(scale);
        text = text.ToUpperInvariant();
        float result = _biosFont.GetWidth(text, thinButtons);
        _biosFont.Scale = previousScale;
        return result;
    }

    public static float GetStringHeight(string text, float scale = 1)
    {
        return text.Split('\n').Length * _biosFont.height * scale;
    }

    public static void DrawFancyString(string text, Vector2 position, Color color, Depth depth = default, float scale = 1)
    {
        _fancyBiosFont.Scale = new Vector2(scale);
        _fancyBiosFont.Draw(text, position.X, position.Y, color, depth);
        _fancyBiosFont.Scale = Vector2.One;
    }

    public static float GetFancyStringWidth(string text, bool thinButtons = false, float scale = 1)
    {
        _fancyBiosFont.Scale = new Vector2(scale);
        text = text.ToUpperInvariant();
        float result = _fancyBiosFont.GetWidth(text, thinButtons);
        _fancyBiosFont.Scale = Vector2.One;
        return result;
    }

    public static void DrawRecorderItem(ref RecorderFrameItem item)
    {
        _currentBatch.DrawRecorderItem(ref item);
    }

#if !MODERN_BATCH
    public static void DrawRecorderItemLerped(ref RecorderFrameItem item, ref RecorderFrameItem lerpTo, float dist)
    {
        RecorderFrameItem lerped = item;
        lerped.topLeft = Vector2.Lerp(item.topLeft, lerpTo.topLeft, dist);
        lerped.bottomRight = Vector2.Lerp(item.bottomRight, lerpTo.bottomRight, dist);
        float rot1 = item.rotation % 360;
        float rot2 = lerpTo.rotation % 360;
        if (rot1 > 180)
            rot1 -= 360;
        else if (rot1 < -180)
            rot1 += 360;
        if (rot2 > 180)
            rot2 -= 360;
        else if (rot2 < -180)
            rot2 += 360;
        lerped.rotation = MathHelper.Lerp(rot1, rot2, dist);
        lerped.color = Color.Lerp(item.color, lerpTo.color, dist);
        _currentBatch.DrawRecorderItem(ref lerped);
    }
#endif

#if !MODERN_BATCH
    public static void Draw(MTSpriteBatchItem item)
    {
        _currentBatch.DrawExistingBatchItem(item);
    }
#else
    public static void Draw(TriangleBatch.TriangleInfo triangle)
    {
        _currentBatch.AppendTriangle(triangle);
    }
#endif
#if NO_TEX2D
    public static void Draw(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Depth depth = default)
    {
        if (texture is Microsoft.Xna.Framework.Graphics.RenderTarget2D)
        {
            if ((texture as Microsoft.Xna.Framework.Graphics.RenderTarget2D).IsDisposed)
                return;
            if (texture.GetTextureIndex() == 0)
                Content.AssignTextureIndex(texture);
        }
        if (doSnap)
        {
            position.X = float.Round(position.X * snap) / snap;
            position.Y = float.Round(position.Y * snap) / snap;
        }
        if (effects == SpriteEffects.FlipHorizontally)
            origin.X = (sourceRectangle.HasValue ? sourceRectangle.Value.Width : texture.Width) - origin.X;
        float deep = AdjustDepth(depth);
#if !MODERN_BATCH
        if (material != null)
            _currentBatch.DrawWithMaterial(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep, material);
        else
            _currentBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep);
#else
        _currentBatch.DrawTexture(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep, material);
        //_currentBatch.DrawTexture(texture, new(position, position + new Vector2(sourceRectangle?.width ?? texture.w, sourceRectangle?.height ?? texture.h) * scale), sourceRectangle, color, rotation, origin * scale, effects, deep, material?.effect);
#endif
    }
#else
    public static void Draw(Tex2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Depth depth = default)
    {
        if (texture.nativeObject is Microsoft.Xna.Framework.Graphics.RenderTarget2D)
        {
            if ((texture.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D).IsDisposed)
                return;
            if (texture.textureIndex == 0)
                Content.AssignTextureIndex(texture);
        }
        if (doSnap)
        {
            position.X = float.Round(position.X * snap) / snap;
            position.Y = float.Round(position.Y * snap) / snap;
        }
        if (effects == SpriteEffects.FlipHorizontally)
            origin.X = (sourceRectangle.HasValue ? sourceRectangle.Value.Width : texture.width) - origin.X;
        float deep = AdjustDepth(depth);
#if !MODERN_BATCH
        if (material != null)
            _currentBatch.DrawWithMaterial(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep, material);
        else
            _currentBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep);
#else
        _currentBatch.DrawTexture(texture, position, sourceRectangle, color, rotation, origin, scale, effects, deep, material);
        //_currentBatch.DrawTexture(texture, new(position, position + new Vector2(sourceRectangle?.width ?? texture.w, sourceRectangle?.height ?? texture.h) * scale), sourceRectangle, color, rotation, origin * scale, effects, deep, material?.effect);
#endif
    }
#endif

    public static void Draw(Sprite g, float x, float y)
    {
        g.X = x;
        g.Y = y;
        g.Draw();
    }

    public static void Draw(Sprite g, float x, float y, RectangleF sourceRectangle)
    {
        g.X = x;
        g.Y = y;
        g.Draw(sourceRectangle);
    }

    public static void Draw(Sprite g, float x, float y, RectangleF sourceRectangle, Vector2 scale)
    {
        g.X = x;
        g.Y = y;
        g.Scale = scale;
        g.Draw(sourceRectangle);
    }

    public static void Draw(Sprite g, float x, float y, RectangleF sourceRectangle, Depth depth)
    {
        g.X = x;
        g.Y = y;
        g.Depth = depth;
        g.Draw(sourceRectangle);
    }

    public static void Draw(Sprite g, float x, float y, Depth depth = default)
    {
        g.X = x;
        g.Y = y;
        g.Depth = depth;
        g.Draw();
    }

    public static void Draw(Sprite g, float x, float y, float scaleX, float scaleY)
    {
        g.X = x;
        g.Y = y;
        g.ScaleX = scaleX;
        g.ScaleY = scaleY;
        g.Draw();
    }

#if NO_TEX2D
    public static void Draw(Texture2D target, float x, float y, float xscale = 1, float yscale = 1, Depth depth = default)
#else
    public static void Draw(Tex2D target, float x, float y, float xscale = 1, float yscale = 1, Depth depth = default)
#endif
    {
        Draw(target, new Vector2(x, y), null, Color.White, 0, Vector2.Zero, new Vector2(xscale, yscale), SpriteEffects.None, depth);
    }

    public static void Draw(SpriteMap g, int frame, float x, float y, float scaleX = 1, float scaleY = 1, bool maintainFrame = false)
    {
        g.X = x;
        g.Y = y;
        g.ScaleX = scaleX;
        g.ScaleY = scaleY;
        int fr = g.frame;
        g.SetFrameWithoutReset(frame);
        g.Draw();
        if (maintainFrame)
            g.SetFrameWithoutReset(fr);
    }

    public static void DrawWithoutUpdate(SpriteMap g, float x, float y, float scaleX = 1, float scaleY = 1)
    {
        g.X = x;
        g.Y = y;
        g.ScaleX = scaleX;
        g.ScaleY = scaleY;
        g.DrawWithoutUpdate();
    }

    public static void DrawLine(Vector2 p1, Vector2 p2, Color col, float width = 1, Depth depth = default)
    {
#if !MODERN_BATCH
        float angle = float.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        float length = (p1 - p2).Length();
        Draw(blank, p1, null, col, angle, new Vector2(0, .5f), new Vector2(length, width), SpriteEffects.None, depth);
#else
        var n = Vector2.Normalize(new(p1.Y - p2.Y, p2.X - p1.X)) * width / 2;
        _currentBatch.DrawQuad(p1 + n, p1 - n, p2 + n, p2 - n, default, default, default, default, AdjustDepth(depth), null, col);
#endif
    }

    public static void DrawDottedLine(Vector2 p1, Vector2 p2, Color col, float width = 1, float dotLength = 8, Depth depth = default)
    {
        Vector2 start = p1;
        Vector2 travel = p2 - p1;
        float length = travel.Length();
        int num = (int)(length / dotLength);
        travel.Normalize();
        bool off = false;
        for (int i = 0; i < num; i++)
        {
            Vector2 end = start;
            end += travel * dotLength;
            if ((end - p1).Length() > length)
                end = p2;
            if (!off)
                DrawLine(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y), col, width, depth);
            off = !off;
            start = end;
        }
    }

    public static void DrawRing(Vector2 pos, float radius, Color col, float width = 1, Depth depth = default, int iterations = 32)
    {
#if !MODERN_BATCH
        Vector2 prev = Vector2.Zero;
        for (int i = 0; i < iterations; i++)
        {
            float val = Maths.DegToRad(360f / (iterations - 1) * i);
            Vector2 cur = new(float.Cos(val) * radius, -float.Sin(val) * radius);
            if (i > 0)
                DrawLine(pos + cur, pos + prev, col, width, depth);
            prev = cur;
        }
#else
        var segment0 = new Vector2(1, 0);
        for (int i = 1; i < iterations + 1; i++)
        {
            var theta = (float)i / iterations * float.Tau;
            var segment1 = new Vector2(float.Cos(theta), float.Sin(theta));
            _currentBatch.DrawQuad(
                pos + segment0 * (radius + width / 2),
                pos + segment1 * (radius + width / 2),
                pos + segment0 * (radius - width / 2),
                pos + segment1 * (radius - width / 2),
                default,
                default,
                default,
                default,
                AdjustDepth(depth),
                null,
                col
                );

            segment0 = segment1;
        }
#endif
    }

#if MODERN_BATCH
    public static void DrawCircle(Vector2 pos, float radius, Color col, Depth depth = default, int iterations = 32)
    {
        var segment0 = new Vector2(radius, 0);
        for (int i = 1; i < iterations + 1; i++)
        {
            var theta = (float)i / iterations * float.Tau;
            var segment1 = new Vector2(float.Cos(theta), float.Sin(theta)) * radius;
            _currentBatch.DrawTriangle(
                pos,
                pos + segment0,
                pos + segment1,
                default,
                default,
                default,
                AdjustDepth(depth),
                null,
                col
                );

            segment0 = segment1;
        }
    }
#endif

#if NO_TEX2D
    public static void DrawTexturedLine(Texture2D texture, Vector2 p1, Vector2 p2, Color col, float width = 1, Depth depth = default)
    {
        if (texture.Width > 1)
        {
            p1 = new Vector2(p1.X, p1.Y);
            p2 = new Vector2(p2.X, p2.Y);
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = (p1 - p2).Length() / texture.Width;
            Draw(texture, p1, null, col, angle, new Vector2(0f, texture.Height / 2), new Vector2(length, width), SpriteEffects.None, depth);
        }
        else
        {
            p1 = new Vector2(p1.X, p1.Y);
            p2 = new Vector2(p2.X, p2.Y);
            float angle2 = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length2 = (p1 - p2).Length();
            Draw(texture, p1, null, col, angle2, new Vector2(0, texture.Height / 2), new Vector2(length2, width), SpriteEffects.None, depth);
        }
    }
#else
    public static void DrawTexturedLine(Tex2D texture, Vector2 p1, Vector2 p2, Color col, float width = 1, Depth depth = default)
    {
        if (texture.width > 1)
        {
            p1 = new Vector2(p1.X, p1.Y);
            p2 = new Vector2(p2.X, p2.Y);
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = (p1 - p2).Length() / texture.width;
            Draw(texture, p1, null, col, angle, new Vector2(0f, texture.height / 2), new Vector2(length, width), SpriteEffects.None, depth);
        }
        else
        {
            p1 = new Vector2(p1.X, p1.Y);
            p2 = new Vector2(p2.X, p2.Y);
            float angle2 = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length2 = (p1 - p2).Length();
            Draw(texture, p1, null, col, angle2, new Vector2(0, texture.height / 2), new Vector2(length2, width), SpriteEffects.None, depth);
        }
    }
#endif

#if !MODERN_BATCH
    public static void DrawRect(Vector2 p1, Vector2 p2, Color col, Depth depth = default, bool filled = true, float borderWidth = 1)
    {
        if (filled)
        {
            Draw(blank, p1, null, col, 0, Vector2.Zero, new Vector2(-(p1.X - p2.X), -(p1.Y - p2.Y)), SpriteEffects.None, depth);
            return;
        }
        float wideDiv = borderWidth / 2;
        DrawLine(new Vector2(p1.X, p1.Y + wideDiv), new Vector2(p2.X, p1.Y + wideDiv), col, borderWidth, depth);
        DrawLine(new Vector2(p1.X + wideDiv, p1.Y + borderWidth), new Vector2(p1.X + wideDiv, p2.Y - borderWidth), col, borderWidth, depth);
        DrawLine(new Vector2(p2.X, p2.Y - wideDiv), new Vector2(p1.X, p2.Y - wideDiv), col, borderWidth, depth);
        DrawLine(new Vector2(p2.X - wideDiv, p2.Y - borderWidth), new Vector2(p2.X - wideDiv, p1.Y + borderWidth), col, borderWidth, depth);
    }
#else
    public static void DrawRect(Vector2 p1, Vector2 p2, Color col, Depth depth = default, bool filled = true, float borderWidth = 1)
    {
        if (filled)
        {
            _currentBatch.DrawQuad(
                p1,
                new(p2.X, p1.Y),
                new(p1.X, p2.Y),
                p2,
                AdjustDepth(depth),
                col
                );
            return;
        }

        Vector2 borderVec = new(borderWidth);
        Vector2 innerLT = p1 + borderVec,
                innerRB = p2 - borderVec;
        _currentBatch.DrawQuad(
            p1,
            new(p2.X, p1.Y),
            innerLT,
            new(innerRB.X, innerLT.Y),
            AdjustDepth(depth),
            col
            );
        _currentBatch.DrawQuad(
            p1,
            innerLT,
            new(p1.X, p2.Y),
            new(innerLT.X, innerRB.Y),
            AdjustDepth(depth),
            col
            );
        _currentBatch.DrawQuad(
            new(innerLT.X, innerRB.Y),
            innerRB,
            new(p1.X, p2.Y),
            p2,
            AdjustDepth(depth),
            col
            );
        _currentBatch.DrawQuad(
            new(innerRB.X, innerLT.Y),
            new(p2.X, p1.Y),
            innerRB,
            p2,
            AdjustDepth(depth),
            col
            );
    }
#endif

    public static void DrawRect(RectangleF r, Color col, Depth depth = default, bool filled = true, float borderWidth = 1)
    {
        DrawRect(r.LeftTop, r.RightBottom, col, depth, filled, borderWidth);
    }

    public static void DrawDottedRect(Vector2 p1, Vector2 p2, Color col, Depth depth = default, float borderWidth = 1, float dotLength = 8)
    {
        float wideDiv = borderWidth / 2;
        DrawDottedLine(new Vector2(p1.X, p1.Y + wideDiv), new Vector2(p2.X, p1.Y + wideDiv), col, borderWidth, dotLength, depth);
        DrawDottedLine(new Vector2(p1.X + wideDiv, p1.Y + borderWidth), new Vector2(p1.X + wideDiv, p2.Y - borderWidth), col, borderWidth, dotLength, depth);
        DrawDottedLine(new Vector2(p2.X, p2.Y - wideDiv), new Vector2(p1.X, p2.Y - wideDiv), col, borderWidth, dotLength, depth);
        DrawDottedLine(new Vector2(p2.X - wideDiv, p2.Y - borderWidth), new Vector2(p2.X - wideDiv, p1.Y + borderWidth), col, borderWidth, dotLength, depth);
    }

#if NO_TEX2D
    public static Texture2D RecolorOld(Texture2D sprite, Vector3 color)
    {
        MaterialRecolor mat = new(new Vector3(color.X / 255, color.Y / 255, color.Z / 255));
        var target = XnaRenderTarget2D.CreateSetUpTarget(sprite.Width, sprite.Height);
        SetRenderTarget(target);
        Clear(new Color(0, 0, 0, 0));
        mat.Apply();
        screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, mat, Matrix.Identity);
        Draw(sprite, default, null, Color.White, 0, default, Vector2.One, SpriteEffects.None, .5f);
        screen.End();
        device.SetRenderTarget(null);
        Texture2D tex2D = new(device, sprite.Width, sprite.Height);
        var data = new Color[sprite.Width * sprite.Height];
        target.GetData(data);
        tex2D.SetData(data);
        target.Dispose();
        return tex2D;
    }
#else
    public static Tex2D RecolorOld(Tex2D sprite, Vector3 color)
    {
        MaterialRecolor mat = new(new Vector3(color.X / 255, color.Y / 255, color.Z / 255));
        RenderTarget2D target = new(sprite.width, sprite.height);
        SetRenderTarget(target);
        Clear(new Color(0, 0, 0, 0));
        mat.Apply();
        screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, mat, Matrix.Identity);
        Draw(sprite, default, null, Color.White, 0, default, Vector2.One, SpriteEffects.None, .5f);
        screen.End();
        device.SetRenderTarget(null);
        Tex2D tex2D = new(sprite.width, sprite.height);
        tex2D.SetData(target.GetData());
        target.Dispose();
        return tex2D;
    }
#endif

#if NO_TEX2D
    public static Texture2D RecolorNew(Texture2D sprite, Color color1, Color color2)
    {
        Color replace1 = new(255, 255, 255);
        Color replace2 = new(157, 157, 157);
        var colors = new Color[sprite.Width * sprite.Height];
        sprite.GetData(colors);
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == replace1)
                colors[i] = color1;
            else if (colors[i] == replace2)
                colors[i] = color2;
        }
        Texture2D tex2D = new(device, sprite.Width, sprite.Height);
        tex2D.SetData(colors);
        return tex2D;
    }
#else
    public static Tex2D RecolorNew(Tex2D sprite, Color color1, Color color2)
    {
        Color replace1 = new(255, 255, 255);
        Color replace2 = new(157, 157, 157);
        Color[] colors = sprite.GetData();
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == replace1)
                colors[i] = color1;
            else if (colors[i] == replace2)
                colors[i] = color2;
        }
        Tex2D tex2D = new(sprite.width, sprite.height);
        tex2D.SetData(colors);
        return tex2D;
    }
#endif

#if NO_TEX2D
    public static Texture2D RecolorM(Texture2D sprite, Color color1, Color color2, Color color3)
    {
        Color replace1 = new(161, 146, 130);
        Color replace2 = new(128, 113, 96);
        Color replace3 = new(191, 181, 171);
        Color foot1 = new(236, 89, 60);
        Color foot2 = new(248, 131, 99);
        Color footr1 = new(235, 137, 49);
        Color footr2 = new(247, 224, 90);
        Color beak1 = new(219, 88, 31);
        Color beak2 = new(236, 116, 60);
        Color beakr1 = new(164, 100, 34);
        Color beakr2 = new(235, 137, 49);
        var colors = new Color[sprite.Width * sprite.Height];
        sprite.GetData(colors);
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == replace1)
                colors[i] = color1;
            else if (colors[i] == replace2)
                colors[i] = color2;
            else if (colors[i] == replace3)
                colors[i] = color3;
            else if (colors[i] == foot1)
                colors[i] = footr1;
            else if (colors[i] == foot2)
                colors[i] = footr2;
            else if (colors[i] == beak1)
                colors[i] = beakr1;
            else if (colors[i] == beak2)
                colors[i] = beakr2;
        }
        Texture2D tex2D = new(device, sprite.Width, sprite.Height);
        tex2D.SetData(colors);
        return tex2D;
    }
#else
    public static Tex2D RecolorM(Tex2D sprite, Color color1, Color color2, Color color3)
    {
        Color replace1 = new(161, 146, 130);
        Color replace2 = new(128, 113, 96);
        Color replace3 = new(191, 181, 171);
        Color foot1 = new(236, 89, 60);
        Color foot2 = new(248, 131, 99);
        Color footr1 = new(235, 137, 49);
        Color footr2 = new(247, 224, 90);
        Color beak1 = new(219, 88, 31);
        Color beak2 = new(236, 116, 60);
        Color beakr1 = new(164, 100, 34);
        Color beakr2 = new(235, 137, 49);
        Color[] colors = sprite.GetData();
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == replace1)
                colors[i] = color1;
            else if (colors[i] == replace2)
                colors[i] = color2;
            else if (colors[i] == replace3)
                colors[i] = color3;
            else if (colors[i] == foot1)
                colors[i] = footr1;
            else if (colors[i] == foot2)
                colors[i] = footr2;
            else if (colors[i] == beak1)
                colors[i] = beakr1;
            else if (colors[i] == beak2)
                colors[i] = beakr2;
        }
        Tex2D tex2D = new(sprite.width, sprite.height);
        tex2D.SetData(colors);
        return tex2D;
    }
#endif

    public static void InitializeBase(GraphicsDeviceManager m)
    {
        _manager = m;
    }

    public static void Initialize(GraphicsDevice d)
    {
        _base = d;
        _defaultBatch = new(_base);
        screen = _defaultBatch;
#if !MODERN_BATCH
        blank = new Tex2D(1, 1);
        blank.SetData([Color.White]);
#endif
        _biosFont = new BitmapFont("biosFont", 8);
        _biosFontCaseSensitive = new BitmapFont("biosFontCaseSensitive", 8);
        _fancyBiosFont = new FancyBitmapFont("smallFont");
        _passwordFont = new SpriteMap("passwordFont", 8, 8);
        Matrix.CreateOrthographicOffCenter(0, d.Viewport.Width, d.Viewport.Height, 0, 0, 1, out _projectionMatrix);
        _projectionMatrix.M41 += -.5f * _projectionMatrix.M11;
        _projectionMatrix.M42 += -.5f * _projectionMatrix.M22;
        tounge = new Sprite("tounge");
    }

    public static void SetRenderTargetToScreen()
    {
        _settingScreenTarget = true;
        SetRenderTarget(null);
        _settingScreenTarget = false;
    }

#if NO_TEX2D
    public static void SetRenderTarget(XnaRenderTarget2D t)
    {
        if (t != null && t.IsDisposed)
            return;

        if (t == null)
        {
            XnaRenderTarget2D screenTarget = defaultRenderTarget;
            if (screenTarget == null)
            {
                _currentTargetSize.Width = Resolution.current.x;
                _currentTargetSize.Height = Resolution.current.y;
            }
            else
            {
                _currentTargetSize.Width = screenTarget.Width;
                _currentTargetSize.Height = screenTarget.Height;
            }
            device.SetRenderTarget(screenTarget);
            if (!_settingScreenTarget && _defaultRenderTarget == null)
                UpdateScreenViewport();
        }
        else
        {
            device.SetRenderTarget(t);
            _currentTargetSize.Width = t.Width;
            _currentTargetSize.Height = t.Height;
        }
        _lastViewport = device.Viewport;
        _currentRenderTarget = t;
    }
#else
    public static void SetRenderTarget(RenderTarget2D t)
    {
        if (t != null && t.IsDisposed)
            return;

        if (t == null)
        {
            Microsoft.Xna.Framework.Graphics.RenderTarget2D screenTarget = ((defaultRenderTarget != null) ? (defaultRenderTarget.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D) : null);
            if (screenTarget == null)
            {
                _currentTargetSize.Width = Resolution.current.x;
                _currentTargetSize.Height = Resolution.current.y;
            }
            else
            {
                _currentTargetSize.Width = screenTarget.Width;
                _currentTargetSize.Height = screenTarget.Height;
            }
            device.SetRenderTarget(screenTarget);
            if (!_settingScreenTarget && _defaultRenderTarget == null)
                UpdateScreenViewport();
        }
        else
        {
            device.SetRenderTarget(t.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D);
            _currentTargetSize.Width = t.width;
            _currentTargetSize.Height = t.height;
        }
        _lastViewport = device.Viewport;
        _currentRenderTarget = t;
    }
#endif

#if NO_TEX2D
    public static XnaRenderTarget2D GetRenderTarget()
#else
    public static RenderTarget2D GetRenderTarget()
#endif
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
            Width = (int)_currentTargetSize.Width,
            Height = (int)_currentTargetSize.Height
        });
    }

    public static void RestoreOldViewport()
    {
        Internal_ViewportSet(_oldViewport);
    }

    public static void UpdateScreenViewport(bool pForceReset = false)
    {
        try
        {
            if (pForceReset || !_screenViewport.HasValue)
            {
                Viewport v = default;
                if (_currentTargetSize.Aspect < 1.77f)
                {
                    v.Width = (int)_currentTargetSize.Width;
                    v.Height = Math.Min((int)Math.Round(_currentTargetSize.Width / 1.77777f), (int)_currentTargetSize.Height);
                }
                else
                {
                    v.Height = (int)_currentTargetSize.Height;
                    v.Width = Math.Min((int)Math.Round(_currentTargetSize.Height * 1.77777f), (int)_currentTargetSize.Width);
                }
                v.X = Math.Max((int)((_currentTargetSize.Width - v.Width) / 2), 0);
                v.Y = Math.Max((int)((_currentTargetSize.Height - v.Height) / 2), 0);
                v.MinDepth = 0;
                v.MaxDepth = 1;
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
        Viewport v = default;
        if (Resolution.adapterResolution.aspect < Resolution.current.aspect)
        {
            v.Width = Resolution.adapterResolution.x;
            v.Height = Math.Min((int)Math.Round(Resolution.adapterResolution.x / Resolution.current.aspect), Resolution.adapterResolution.y);
        }
        else
        {
            v.Height = Resolution.adapterResolution.y;
            v.Width = Math.Min((int)Math.Round(Resolution.adapterResolution.y * Resolution.current.aspect), Resolution.adapterResolution.x);
        }
        v.X = Math.Max((Resolution.adapterResolution.x - v.Width) / 2, 0);
        v.Y = Math.Max((Resolution.adapterResolution.y - v.Height) / 2, 0);
        v.MinDepth = 0;
        v.MaxDepth = 1;
        Internal_ViewportSet(v);
    }

    public static void SetScissorRectangle(RectangleF r)
    {
        float mul = (float)device.Viewport.Bounds.Width / width;
        if (r.Width >= 0 && r.Height >= 0)
        {
            r.Width *= mul;
            r.Height *= mul;
            r.X *= mul;
            r.Y *= mul;
            r.X += viewport.X;
            r.Y += viewport.Y;
            device.ScissorRectangle = ClipRectangle(r, device.Viewport.Bounds);
        }
    }

    public static RectangleF ClipRectangle(RectangleF r, RectangleF clipTo)
    {
        if (r.X > clipTo.Right)
            r.X = clipTo.Right - r.Width;
        if (r.Y > clipTo.Bottom)
            r.Y = clipTo.Bottom - r.Height;
        if (r.X < clipTo.Left)
            r.X = clipTo.Left;
        if (r.Y < clipTo.Top)
            r.Y = clipTo.Top;
        if (r.X < 0)
            r.X = 0;
        if (r.Y < 0)
            r.Y = 0;
        if (r.X + r.Width > clipTo.X + clipTo.Width)
            r.Width = clipTo.Right - r.X;
        if (r.Y + r.Height > clipTo.Y + clipTo.Height)
            r.Height = clipTo.Bottom - r.Y;
        if (r.Width < 0)
            r.Width = 0;
        if (r.Height < 0)
            r.Height = 0;
        return r;
    }

    public static void Clear(Color c)
    {
        device.Clear(c);
    }

#endregion

    static void Internal_ViewportSet(Viewport pViewport)
    {
        try
        {
            device.Viewport = pViewport;
        }
        catch (Exception)
        {
            DevConsole.Log($"Error: Invalid Viewport (x = {pViewport.X}, y = {pViewport.Y}, w = {pViewport.Width}, h = {pViewport.Height}, minDepth = {pViewport.MinDepth}, maxDepth = {pViewport.MaxDepth})");
        }
    }
}
