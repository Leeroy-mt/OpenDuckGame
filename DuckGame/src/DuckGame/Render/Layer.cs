using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class Layer : DrawList
{
    #region Public Fields

    public bool enableCulling;
    public bool aspectReliesOnGameLayer;

    public float flashAddInfluence;
    public float flashAddClearInfluence;
    public float currentSpanOffset;

    public Camera _tallCamera;
    public RenderTarget2D _target;

    public static bool lightingTwoPointOh;
    public static bool blurry;
    public static bool ignoreTransparent;
    public static bool skipDrawing;

    public static Vector3 kGameLayerFade;
    public static Vector3 kGameLayerAdd;

    #endregion

    #region Protected Fields

    protected float _fade = 1;
    protected float _fadeAdd;
    protected float _darken;

    protected Vector3 _colorAdd;
    protected RectangleF _scissor;

#if !MODERN_BATCH
    protected MTSpriteBatch _batch;
#else
    protected TriangleBatch _batch;
#endif
    protected Camera _camera;
    protected RasterizerState _state;

    #endregion

    #region Private Fields

    bool _visible = true;
    bool _blurEffect;
    bool _perspective;
    bool _allowTallAspect;

    int _depth;

    string _name;

    Color _targetClearColor;
    Viewport _oldViewport;

    Effect _effect;
    BlendState _blend = BlendState.AlphaBlend;
    BlendState _targetBlend = BlendState.AlphaBlend;
    DepthStencilState _targetDepthStencil = DepthStencilState.Default;
    Sprite _dropShadow = new("dropShadow");
    RenderTarget2D _oldRenderTarget;
    Camera _targetCamera = new();

    static bool _lighting;

    static LayerCore _core = new();
    static Layer _preDrawLayer = new("PREDRAW");

    #endregion

    #region Public Properties

    public bool targetOnly { get; set; }
    public bool visible
    {
        get => _visible;
        set => _visible = value;
    }
    public bool perspective
    {
        get => _perspective;
        set => _perspective = value;
    }
    public bool allowTallAspect
    {
        get => _allowTallAspect && !camera.sixteenNine;
        set => _allowTallAspect = value;
    }

    public int depth
    {
        get => _depth;
        set => _depth = value;
    }

    public float barSize => (camera.width * Graphics.aspect - camera.width * 0.5625f) / 2f;
    public float width => camera.width;
    public float height => camera.height;
    public float fade
    {
        get => _fade;
        set => _fade = value;
    }
    public float fadeAdd
    {
        get => _fadeAdd;
        set => _fadeAdd = value;
    }
    public float darken
    {
        get => _darken;
        set => _darken = value;
    }

    public string name => _name;

#if !MODERN_BATCH
    public Matrix fullMatrix => _batch.fullMatrix;
#else
    public Matrix fullMatrix => _batch.FullMatrix;
#endif
    public Matrix projection { get; set; }
    public Matrix view { get; set; }
    public Vector3 colorMul { get; set; } = Vector3.One;
    public Color targetClearColor
    {
        get => _targetClearColor;
        set => _targetClearColor = value;
    }
    public RectangleF scissor
    {
        get => _scissor;
        set
        {
            if (_scissor.Width == 0 && value.Width != 0)
                _state = new()
                {
                    CullMode = CullMode.None,
                    ScissorTestEnable = true
                };
            _scissor = value;
        }
    }
    public Vector3 colorAdd
    {
        get => _colorAdd;
        set => _colorAdd = value;
    }

    public RenderTarget2D target => _target;
    public Effect effect
    {
        get => _effect;
        set => _effect = value;
    }
    public BlendState blend
    {
        get => _blend;
        set => _blend = value;
    }
    public BlendState targetBlend
    {
        get => _targetBlend;
        set => _targetBlend = value;
    }
    public DepthStencilState targetDepthStencil
    {
        get => _targetDepthStencil;
        set => _targetDepthStencil = value;
    }
    public Camera camera
    {
        get => _camera == null && Level.activeLevel != null ? Level.activeLevel.camera : _camera;
        set => _camera = value;
    }

    public static bool allVisible
    {
        set => _core.allVisible = value;
    }
    public static bool lighting
    {
        get => Options.Data.lighting && _lighting && Level.current is not Editor;
        set => _lighting = value;
    }
    public static bool doVirtualEffect
    {
        get => _core.doVirtualEffect;
        set => _core.doVirtualEffect = value;
    }
    public static bool basicWireframeTex
    {
        get => _core.basicWireframeTex;
        set => _core.basicWireframeTex = value;
    }

    public static Layer PreDrawLayer => _preDrawLayer;
    public static Layer Parallax => _core._parallax;
    public static Layer Virtual => _core._virtual;
    public static Layer Background => _core._background;
    public static Layer Game => _core._game;
    public static Layer Blocks => _core._blocks;
    public static Layer Glow => _core._glow;
    public static Layer Lighting => _core._lighting;
    public static Layer Foreground => _core._foreground;
    public static Layer HUD => _core._hud;
    public static Layer Console => _core._console;
    public static MTEffect basicWireframeEffect => _core.basicWireframeEffect;
    public static MTEffect basicLayerEffect => _core._basicEffectFadeAdd;
    public static LayerCore core
    {
        get => _core;
        set => _core = value;
    }

    #endregion

    public Layer(string nameval, int depthval = 0, Camera cam = null, bool targetLayer = false, Vector2 targetSize = default)
    {
        _name = nameval;
        _depth = depthval;
        _batch = new(Graphics.device);
        _state = new()
        {
            CullMode = CullMode.None
        };
        _camera = cam;
        _dropShadow.CenterOrigin();
        _dropShadow.Alpha = 0.5f;
        if (targetLayer)
        {
            if (targetSize == default)
                _target = new(Graphics.width, Graphics.height);
            else
                _target = new((int)targetSize.X, (int)targetSize.Y);
        }
    }

    #region Public Methods

    public void ClearScissor()
    {
        if (_scissor.Width != 0)
        {
            _scissor = default;
            _state = new()
            {
                CullMode = CullMode.None
            };
        }
    }

    public virtual void Update()
    {
        foreach (Thing t in _transparentRemove)
            _transparent.Remove(t);

        foreach (Thing t2 in _opaqueRemove)
            _opaque.Remove(t2);

        _transparentRemove.Clear();
        _opaqueRemove.Clear();
    }

    public virtual void Begin(bool transparent, bool isTargetDraw = false)
    {
        if (aspectReliesOnGameLayer && camera != Game.camera)
        {
            camera.width = 320f;
            camera.height = 320f / Game.camera.aspect;
        }

        if (allowTallAspect)
            Graphics.SetFullViewport();

        try
        {
            if (isTargetDraw && transparent && _target != null)
            {
                _oldRenderTarget = Graphics.GetRenderTarget();
                _oldViewport = Graphics.viewport;
                Graphics.SetRenderTarget(_target);
                if (flashAddClearInfluence > 0)
                    Graphics.Clear(new(
                        (byte)float.Min(_targetClearColor.R + flashAddClearInfluence * Graphics.flashAddRenderValue * 255, 255),
                        (byte)float.Min(_targetClearColor.G + flashAddClearInfluence * Graphics.flashAddRenderValue * 255, 255),
                        (byte)float.Min(_targetClearColor.B + flashAddClearInfluence * Graphics.flashAddRenderValue * 255, 255),
                        _targetClearColor.A
                        ));
                else
                    Graphics.Clear(_targetClearColor);
            }

            if (!isTargetDraw && (Graphics.currentRenderTarget == null || Graphics.currentRenderTarget.depth))
                Graphics.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0);
        }
        catch (Exception ex)
        {
            DevConsole.Log($"|DGRED|Layer.Begin exception: {ex.Message}");
        }

        Graphics.ResetSpanAdjust();
        Effect effect = _core._basicEffect;
        var fade = new Vector3(Graphics.fade * _fade * (1 - _darken)) * colorMul;
        var fadeAdd = _colorAdd + new Vector3(_fadeAdd) + new Vector3(Graphics.flashAddRenderValue) * flashAddInfluence + new Vector3(Graphics.fadeAddRenderValue) - new Vector3(darken);
        fadeAdd = new Vector3(Maths.Clamp(fadeAdd.X, -1, 1), Maths.Clamp(fadeAdd.Y, -1, 1), Maths.Clamp(fadeAdd.Z, -1, 1));
        fadeAdd *= fade;
        if (this == Game)
        {
            kGameLayerFade = fade;
            kGameLayerAdd = fadeAdd;
        }

        if (_darken > 0)
            _darken -= 0.15f;
        else if (_darken < 0)
            _darken += 0.15f;

        if (Math.Abs(_darken) < 0.16f)
            _darken = 0;

        if (_effect != null)
        {
            effect = _effect;
            effect.Parameters["fade"]?.SetValue(fade);
            effect.Parameters["add"]?.SetValue(fadeAdd);
        }
        else
        {
            float fadeLen = fadeAdd.LengthSquared();
            if (fade != Vector3.One && fadeLen > 0.001f)
            {
                effect = _core._basicEffectFadeAdd;
                effect.Parameters["fade"].SetValue(fade);
                effect.Parameters["add"].SetValue(fadeAdd);
            }
            else if (fade != Vector3.One)
            {
                effect = _core._basicEffectFade;
                effect.Parameters["fade"].SetValue(fade);
            }
            else if (fadeLen > 0.001f)
            {
                effect = _core._basicEffectAdd;
                effect.Parameters["add"].SetValue(fadeAdd);
            }

            if (doVirtualEffect && (Game == this || Foreground == this || Blocks == this || Background == this))
                effect = !basicWireframeTex ? (Effect)_core._basicWireframeEffect : (Effect)_core._basicWireframeEffectTex;
        }

        if (_state.ScissorTestEnable)
            Graphics.SetScissorRectangle(_scissor);

        Graphics.screen = _batch;
        Camera c = camera;

        if (target != null && isTargetDraw && !targetOnly)
        {
            _targetCamera.x = float.Round(camera.x - 1);
            _targetCamera.y = float.Round(camera.y - 1);
            _targetCamera.width = Math.Max(camera.width, Graphics.width);
            _targetCamera.height = Math.Max(camera.height, Graphics.height);
            c = _targetCamera;
        }

        BlendState blendState = _blend;

        if (isTargetDraw)
            blendState = _targetBlend;

        if (target != null && isTargetDraw)
        {
            Vector2 pos = c.position;
            pos.X = float.Floor(pos.X);
            pos.Y = float.Floor(pos.Y);
            Vector2 size = c.size;
            size.X = float.Floor(size.X);
            size.Y = float.Floor(size.Y);
            Vector2 realPos = c.position;
            Vector2 realSize = c.size;
            _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.PointClamp, _targetDepthStencil, _state, effect, c.getMatrix());
            c.position = realPos;
            c.size = realSize;
        }
        else if (blurry || _blurEffect)
        {
            if (!transparent)
                _batch.Begin(SpriteSortMode.FrontToBack, blendState, SamplerState.LinearClamp, DepthStencilState.Default, _state, effect, c.getMatrix());
            else
                _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.LinearClamp, DepthStencilState.DepthRead, _state, effect, c.getMatrix());
        }
        else if (!transparent)
            _batch.Begin(SpriteSortMode.FrontToBack, blendState, SamplerState.PointClamp, DepthStencilState.Default, _state, effect, c.getMatrix());
        else
            _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.PointClamp, DepthStencilState.DepthRead, _state, effect, c.getMatrix());
    }

    public void End(bool transparent, bool isTargetDraw = false)
    {
        _batch.End();

        Graphics.screen = null;
        Graphics.currentLayer = null;

        if (isTargetDraw && transparent && _target != null)
        {
            Graphics.SetRenderTarget(_oldRenderTarget);
            Graphics.viewport = _oldViewport;
        }

        if (allowTallAspect)
            Graphics.RestoreOldViewport();
    }

    public virtual void Draw(bool transparent, bool isTargetDraw = false)
    {
        if (currentSpanOffset > 10000)
            currentSpanOffset = 0;

        if ((!transparent && ignoreTransparent) || (target != null && !isTargetDraw && targetOnly))
            return;

        if (Network.isActive && this == Game)
            Graphics.currentFrameCalls = [];

        Level.activeLevel.InitializeDraw(this);
        Graphics.currentLayer = this;

        Begin(transparent, isTargetDraw);

        if (target != null && !isTargetDraw)
        {
            Vector2 pos = Level.activeLevel.camera.position - Vector2.One;
            pos.X = float.Round(pos.X);
            pos.Y = float.Round(pos.Y);
            Color c = new(1f, 1f, 1f, 1f);
            Vector2 sizo = new(Math.Max(camera.width, Graphics.width), Math.Max(camera.height, Graphics.height));
            Graphics.skipReplayRender = true;
            Graphics.Draw(target, pos, null, c, 0, Vector2.Zero, new Vector2(sizo.X / target.width, sizo.Y / target.height), SpriteEffects.None, 1);

            if (name == "LIGHTING")
            {
                if (VirtualTransition.core._scanStage == 1)
                    targetClearColor = Lerp.ColorSmooth(new Color(120, 120, 120, 255), Color.White, VirtualTransition.core._stick);
                else if (VirtualTransition.core._scanStage == 3)
                    targetClearColor = new Color(120, 120, 120, 255);
            }

            Graphics.skipReplayRender = false;
        }
        else
        {
            if (transparent)
                Level.activeLevel.PreDrawLayer(this);

            HashSet<Thing> drawListTransparent = _transparent;
            HashSet<Thing> drawListOpaque = _opaque;

            if (!skipDrawing)
            {
                if (transparent)
                {
                    if (Network.isActive)
                    {
                        foreach (Thing drawable in drawListTransparent)
                        {
                            if (!drawable.visible || (drawable.ghostObject != null && !drawable.ghostObject.IsInitialized()))
                                continue;

                            if (_perspective)
                            {
                                Vector2 pos2 = drawable.Position;
                                Vector3 newPos = new(pos2.X, drawable.Z, drawable.bottom);
                                Viewport v = new(0, 0, 320, 180);
                                newPos = v.Project(newPos, projection, view, Matrix.Identity);
                                drawable.Position = new Vector2(newPos.X, newPos.Y - drawable.CenterY);
                                drawable.DoDraw();
                                Graphics.material = null;
                                drawable.Position = pos2;
                                if (drawable is PhysicsObject)
                                {
                                    float dist = Maths.NormalizeSection(0 - drawable.Y, 8, 64);
                                    _dropShadow.Alpha = 0.5f - 0.5f * dist;
                                    _dropShadow.Scale = new Vector2(1 - dist, 1 - dist);
                                    _dropShadow.Depth = drawable.Depth - 10;
                                    newPos = new Vector3(pos2.X, drawable.Z, 0);
                                    newPos = v.Project(newPos, projection, view, Matrix.Identity);
                                    Graphics.Draw(_dropShadow, newPos.X - 1, newPos.Y - 1);
                                }
                            }
                            else
                                drawable.DoDraw();

                            Graphics.material = null;
                        }
                    }
                    else if (this == Lighting)
                    {
                        foreach (Thing drawable2 in drawListTransparent)
                        {
                            if (drawable2.visible)
                            {
                                drawable2.DoDraw();
                                Graphics.material = null;
                            }
                        }
                    }
                    else
                    {
                        foreach (Thing drawable3 in drawListTransparent)
                        {
                            if (!drawable3.visible)
                                continue;

                            if (_perspective)
                            {
                                Vector2 pos3 = drawable3.Position;
                                Vector3 newPos2 = new(pos3.X, drawable3.Z, drawable3.bottom);
                                Viewport v2 = new(0, 0, 320, 180);
                                newPos2 = v2.Project(newPos2, projection, view, Matrix.Identity);
                                drawable3.Position = new Vector2(newPos2.X, newPos2.Y - drawable3.CenterY);
                                drawable3.DoDraw();
                                Graphics.material = null;
                                drawable3.Position = pos3;
                                if (drawable3 is PhysicsObject)
                                {
                                    float dist2 = Maths.NormalizeSection(0 - drawable3.Y, 8, 64);
                                    _dropShadow.Alpha = 0.5f - 0.5f * dist2;
                                    _dropShadow.Scale = new Vector2(1 - dist2, 1 - dist2);
                                    _dropShadow.Depth = drawable3.Depth - 10;
                                    newPos2 = new Vector3(pos3.X, drawable3.Z, 0);
                                    newPos2 = v2.Project(newPos2, projection, view, Matrix.Identity);
                                    Graphics.Draw(_dropShadow, newPos2.X - 1, newPos2.Y - 1);
                                }
                            }
                            else
                                drawable3.DoDraw();

                            Graphics.material = null;
                        }

                        if (DevConsole.showCollision)
                            foreach (Thing drawable4 in drawListTransparent)
                                if (drawable4.visible)
                                    drawable4.DrawCollision();
                    }

                    if (ignoreTransparent)
                    {
                        foreach (Thing drawable5 in drawListOpaque)
                        {
                            if (drawable5.visible)
                                drawable5.DoDraw();
                            Graphics.material = null;
                        }
                        StaticRenderer.RenderLayer(this);
                    }
                }
                else
                {
                    foreach (Thing drawable6 in drawListOpaque)
                        if (drawable6.visible)
                            drawable6.DoDraw();
                    StaticRenderer.RenderLayer(this);
                }
            }

            if (transparent)
                Level.activeLevel.PostDrawLayer(this);
        }

        if (Network.isActive && Network.inputDelayFrames > 0 && this == Game)
        {
            Graphics.drawCalls.Enqueue(Graphics.currentFrameCalls);
            if (Graphics.drawCalls.Count > 0)
            {
                List<DrawCall> list = Graphics.drawCalls.Peek();
                if (Graphics.drawCalls.Count > Network.inputDelayFrames)
                    Graphics.drawCalls.Dequeue();

                foreach (DrawCall c2 in list)
                {
#if !MODERN_BATCH
                    if (c2.material != null)
                        Graphics.screen.DrawWithMaterial(c2.texture, c2.position, c2.sourceRect, c2.color, c2.rotation, c2.origin, c2.scale, c2.effects, c2.depth, c2.material);
                    else
                        Graphics.screen.Draw(c2.texture, c2.position, c2.sourceRect, c2.color, c2.rotation, c2.origin, c2.scale, c2.effects, c2.depth);
#else
                    Graphics.screen.DrawTexture(c2.texture, c2.position, c2.sourceRect, c2.color, c2.rotation, c2.origin, c2.scale, c2.effects, c2.depth, c2.material);
#endif
                }
            }
        }
        End(transparent, isTargetDraw);
    }

    public static void InitializeLayers()
    {
        _core.InitializeLayers();
    }

    public static void ClearLayers()
    {
        _core.ClearLayers();
    }

    public static void DrawLayers()
    {
        _core.DrawLayers();
    }

    public static void DrawTargetLayers()
    {
        _core.DrawTargetLayers();
    }

    public static void UpdateLayers()
    {
        _core.UpdateLayers();
    }

    public static void ResetLayers()
    {
        _core.ResetLayers();
    }

    public static void Add(Layer l)
    {
        _core.Add(l);
    }

    public static void Remove(Layer l)
    {
        _core.Remove(l);
    }

    public static bool IsBasicLayerEffect(MTEffect e)
    {
        return _core.IsBasicLayerEffect(e);
    }

    public static bool Contains(Layer l)
    {
        return _core.Contains(l);
    }

    #endregion
}