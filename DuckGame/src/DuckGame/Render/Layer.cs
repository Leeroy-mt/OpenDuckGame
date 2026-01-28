using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class Layer : DrawList
{
    public bool enableCulling;

    public static bool lightingTwoPointOh = false;

    private static bool _lighting = false;

    private static LayerCore _core = new LayerCore();

    private static Layer _preDrawLayer = new Layer("PREDRAW");

    protected MTSpriteBatch _batch;

    private string _name;

    private int _depth;

    private Vec2 _depthSpan;

    private Effect _effect;

    private bool _visible = true;

    private bool _blurEffect;

    private bool _perspective;

    private BlendState _blend = BlendState.AlphaBlend;

    private BlendState _targetBlend = BlendState.AlphaBlend;

    private Color _targetClearColor = new Color(0, 0, 0, 0);

    private DepthStencilState _targetDepthStencil = DepthStencilState.Default;

    private RenderTarget2D _slaveTarget;

    private float _targetFade = 1f;

    protected Rectangle _scissor;

    protected float _fade = 1f;

    protected float _fadeAdd;

    protected Vec3 _colorAdd = Vec3.Zero;

    protected Vec3 _colorMul = Vec3.One;

    protected float _darken;

    public Camera _tallCamera;

    protected Camera _camera;

    protected RasterizerState _state;

    private Sprite _dropShadow = new Sprite("dropShadow");

    public RenderTarget2D _target;

    private Layer _shareDrawObjects;

    private bool _targetOnly;

    public bool aspectReliesOnGameLayer;

    private bool _allowTallAspect;

    public float flashAddInfluence;

    public float flashAddClearInfluence;

    private Viewport _oldViewport;

    private RenderTarget2D _oldRenderTarget;

    private Camera _targetCamera = new Camera();

    public static Vec3 kGameLayerFade;

    public static Vec3 kGameLayerAdd;

    public static bool blurry = false;

    public static bool ignoreTransparent = false;

    public static bool skipDrawing = false;

    public float currentSpanOffset;

    public static bool lighting
    {
        get
        {
            if (Options.Data.lighting && _lighting)
            {
                return !(Level.current is Editor);
            }
            return false;
        }
        set
        {
            _lighting = value;
        }
    }

    public static LayerCore core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
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

    public static Layer HUD
    {
        get
        {
            return _core._hud;
        }
        set
        {
            _core._hud = value;
        }
    }

    public static Layer Console => _core._console;

    public static bool doVirtualEffect
    {
        get
        {
            return _core.doVirtualEffect;
        }
        set
        {
            _core.doVirtualEffect = value;
        }
    }

    public static MTEffect basicWireframeEffect => _core.basicWireframeEffect;

    public static bool basicWireframeTex
    {
        get
        {
            return _core.basicWireframeTex;
        }
        set
        {
            _core.basicWireframeTex = value;
        }
    }

    public static MTEffect itemSpawnEffect => _core._itemSpawnEffect;

    public static bool allVisible
    {
        set
        {
            _core.allVisible = value;
        }
    }

    public static MTEffect basicLayerEffect => _core._basicEffectFadeAdd;

    public Matrix fullMatrix => _batch.fullMatrix;

    public string name => _name;

    public int depth
    {
        get
        {
            return _depth;
        }
        set
        {
            _depth = value;
        }
    }

    public Vec2 depthSpan
    {
        get
        {
            return _depthSpan;
        }
        set
        {
            _depthSpan = value;
        }
    }

    public Effect effect
    {
        get
        {
            return _effect;
        }
        set
        {
            _effect = value;
        }
    }

    public bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
        }
    }

    public bool blurEffect
    {
        get
        {
            return _blurEffect;
        }
        set
        {
            _blurEffect = value;
        }
    }

    public float barSize => (camera.width * Graphics.aspect - camera.width * 0.5625f) / 2f;

    public Matrix projection { get; set; }

    public Matrix view { get; set; }

    public bool perspective
    {
        get
        {
            return _perspective;
        }
        set
        {
            _perspective = value;
        }
    }

    public BlendState blend
    {
        get
        {
            return _blend;
        }
        set
        {
            _blend = value;
        }
    }

    public BlendState targetBlend
    {
        get
        {
            return _targetBlend;
        }
        set
        {
            _targetBlend = value;
        }
    }

    public Color targetClearColor
    {
        get
        {
            return _targetClearColor;
        }
        set
        {
            _targetClearColor = value;
        }
    }

    public DepthStencilState targetDepthStencil
    {
        get
        {
            return _targetDepthStencil;
        }
        set
        {
            _targetDepthStencil = value;
        }
    }

    public RenderTarget2D slaveTarget
    {
        get
        {
            return _slaveTarget;
        }
        set
        {
            _slaveTarget = value;
        }
    }

    public float targetFade
    {
        get
        {
            return _targetFade;
        }
        set
        {
            _targetFade = value;
        }
    }

    public Rectangle scissor
    {
        get
        {
            return _scissor;
        }
        set
        {
            if (_scissor.width == 0f && value.width != 0f)
            {
                _state = new RasterizerState();
                _state.CullMode = CullMode.None;
                _state.ScissorTestEnable = true;
            }
            _scissor = value;
        }
    }

    public float fade
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

    public float fadeAdd
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

    public Vec3 colorAdd
    {
        get
        {
            return _colorAdd;
        }
        set
        {
            _colorAdd = value;
        }
    }

    public Vec3 colorMul
    {
        get
        {
            return _colorMul;
        }
        set
        {
            _colorMul = value;
        }
    }

    public float darken
    {
        get
        {
            return _darken;
        }
        set
        {
            _darken = value;
        }
    }

    public Camera camera
    {
        get
        {
            if (_camera == null && Level.activeLevel != null)
            {
                return Level.activeLevel.camera;
            }
            return _camera;
        }
        set
        {
            _camera = value;
        }
    }

    public float width => camera.width;

    public float height => camera.height;

    public RenderTarget2D target
    {
        get
        {
            if (_slaveTarget != null)
            {
                return _slaveTarget;
            }
            return _target;
        }
    }

    public Layer shareDrawObjects
    {
        get
        {
            return _shareDrawObjects;
        }
        set
        {
            _shareDrawObjects = value;
        }
    }

    public bool targetOnly
    {
        get
        {
            return _targetOnly;
        }
        set
        {
            _targetOnly = value;
        }
    }

    public bool allowTallAspect
    {
        get
        {
            if (_allowTallAspect)
            {
                return !camera.sixteenNine;
            }
            return false;
        }
        set
        {
            _allowTallAspect = value;
        }
    }

    public bool isTargetLayer => target != null;

    public static bool IsBasicLayerEffect(MTEffect e)
    {
        return _core.IsBasicLayerEffect(e);
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

    public static Layer Get(string layer)
    {
        return _core.Get(layer);
    }

    public static void Add(Layer l)
    {
        _core.Add(l);
    }

    public static void Remove(Layer l)
    {
        _core.Remove(l);
    }

    public static bool Contains(Layer l)
    {
        return _core.Contains(l);
    }

    public void ClearScissor()
    {
        if (_scissor.width != 0f)
        {
            _scissor = new Rectangle(0f, 0f, 0f, 0f);
            _state = new RasterizerState();
            _state.CullMode = CullMode.None;
        }
    }

    public Layer(string nameval, int depthval = 0, Camera cam = null, bool targetLayer = false, Vec2 targetSize = default(Vec2))
    {
        _name = nameval;
        _depth = depthval;
        _batch = new MTSpriteBatch(Graphics.device);
        _state = new RasterizerState();
        _state.CullMode = CullMode.None;
        _camera = cam;
        _dropShadow.CenterOrigin();
        _dropShadow.Alpha = 0.5f;
        if (targetLayer)
        {
            if (targetSize == default(Vec2))
            {
                _target = new RenderTarget2D(Graphics.width, Graphics.height);
            }
            else
            {
                _target = new RenderTarget2D((int)targetSize.X, (int)targetSize.Y);
            }
        }
    }

    public virtual void Update()
    {
        foreach (Thing t in _transparentRemove)
        {
            _transparent.Remove(t);
        }
        foreach (Thing t2 in _opaqueRemove)
        {
            _opaque.Remove(t2);
        }
        _transparentRemove.Clear();
        _opaqueRemove.Clear();
    }

    public virtual void Begin(bool transparent, bool isTargetDraw = false)
    {
        _ = name == "LIGHTING";
        if (aspectReliesOnGameLayer && camera != Game.camera)
        {
            camera.width = 320f;
            camera.height = 320f / Game.camera.aspect;
        }
        if (allowTallAspect)
        {
            Graphics.SetFullViewport();
        }
        try
        {
            if (isTargetDraw && transparent && _target != null)
            {
                _oldRenderTarget = Graphics.GetRenderTarget();
                _oldViewport = Graphics.viewport;
                Graphics.SetRenderTarget(_target);
                if (flashAddClearInfluence > 0f)
                {
                    Graphics.Clear(new Color((byte)Math.Min((float)(int)_targetClearColor.r + flashAddClearInfluence * Graphics.flashAddRenderValue * 255f, 255f), (byte)Math.Min((float)(int)_targetClearColor.g + flashAddClearInfluence * Graphics.flashAddRenderValue * 255f, 255f), (byte)Math.Min((float)(int)_targetClearColor.b + flashAddClearInfluence * Graphics.flashAddRenderValue * 255f, 255f), _targetClearColor.a));
                }
                else
                {
                    Graphics.Clear(_targetClearColor);
                }
            }
            if (!isTargetDraw && (Graphics.currentRenderTarget == null || Graphics.currentRenderTarget.depth))
            {
                Graphics.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log("|DGRED|Layer.Begin exception: " + ex.Message);
        }
        Graphics.ResetSpanAdjust();
        Effect effect = _core._basicEffect;
        Vec3 fade = new Vec3(Graphics.fade * _fade * (1f - _darken)) * colorMul;
        Vec3 fadeAdd = _colorAdd + new Vec3(_fadeAdd) + new Vec3(Graphics.flashAddRenderValue) * flashAddInfluence + new Vec3(Graphics.fadeAddRenderValue) - new Vec3(darken);
        fadeAdd = new Vec3(Maths.Clamp(fadeAdd.x, -1f, 1f), Maths.Clamp(fadeAdd.y, -1f, 1f), Maths.Clamp(fadeAdd.z, -1f, 1f));
        fadeAdd *= fade;
        if (this == Game)
        {
            kGameLayerFade = fade;
            kGameLayerAdd = fadeAdd;
        }
        if (_darken > 0f)
        {
            _darken -= 0.15f;
        }
        else if (_darken < 0f)
        {
            _darken += 0.15f;
        }
        if (Math.Abs(_darken) < 0.16f)
        {
            _darken = 0f;
        }
        if (_effect != null)
        {
            effect = _effect;
            effect.Parameters["fade"]?.SetValue(fade);
            effect.Parameters["add"]?.SetValue(fadeAdd);
        }
        else
        {
            float fadeLen = fadeAdd.LengthSquared();
            if (fade != Vec3.One && fadeLen > 0.001f)
            {
                effect = _core._basicEffectFadeAdd;
                effect.Parameters["fade"].SetValue(fade);
                effect.Parameters["add"].SetValue(fadeAdd);
            }
            else if (fade != Vec3.One)
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
            {
                effect = ((!basicWireframeTex) ? ((Effect)_core._basicWireframeEffect) : ((Effect)_core._basicWireframeEffectTex));
            }
        }
        if (_state.ScissorTestEnable)
        {
            Graphics.SetScissorRectangle(_scissor);
        }
        Graphics.screen = _batch;
        Camera c = camera;
        if (target != null && isTargetDraw && !targetOnly)
        {
            _targetCamera.x = (float)Math.Round(camera.x - 1f);
            _targetCamera.y = (float)Math.Round(camera.y - 1f);
            _targetCamera.width = Math.Max(camera.width, Graphics.width);
            _targetCamera.height = Math.Max(camera.height, Graphics.height);
            c = _targetCamera;
        }
        BlendState blendState = _blend;
        if (isTargetDraw)
        {
            blendState = _targetBlend;
        }
        if (target != null && isTargetDraw)
        {
            Vec2 pos = c.position;
            pos.X = (float)Math.Floor(pos.X);
            pos.Y = (float)Math.Floor(pos.Y);
            Vec2 size = c.size;
            size.X = (float)Math.Floor(size.X);
            size.Y = (float)Math.Floor(size.Y);
            Vec2 realPos = c.position;
            Vec2 realSize = c.size;
            _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.PointClamp, _targetDepthStencil, _state, effect, c.getMatrix());
            c.position = realPos;
            c.size = realSize;
        }
        else if (blurry || _blurEffect)
        {
            if (!transparent)
            {
                _batch.Begin(SpriteSortMode.FrontToBack, blendState, SamplerState.LinearClamp, DepthStencilState.Default, _state, effect, c.getMatrix());
            }
            else
            {
                _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.LinearClamp, DepthStencilState.DepthRead, _state, effect, c.getMatrix());
            }
        }
        else if (!transparent)
        {
            _batch.Begin(SpriteSortMode.FrontToBack, blendState, SamplerState.PointClamp, DepthStencilState.Default, _state, effect, c.getMatrix());
        }
        else
        {
            _batch.Begin(SpriteSortMode.BackToFront, blendState, SamplerState.PointClamp, DepthStencilState.DepthRead, _state, effect, c.getMatrix());
        }
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
        {
            Graphics.RestoreOldViewport();
        }
    }

    public virtual void Draw(bool transparent, bool isTargetDraw = false)
    {
        if (currentSpanOffset > 10000f)
        {
            currentSpanOffset = 0f;
        }
        if ((!transparent && ignoreTransparent) || (isTargetDraw && slaveTarget != null) || (target != null && !isTargetDraw && targetOnly))
        {
            return;
        }
        if (Network.isActive && this == Game)
        {
            Graphics.currentFrameCalls = new List<DrawCall>();
        }
        Level.activeLevel.InitializeDraw(this);
        Graphics.currentLayer = this;
        Begin(transparent, isTargetDraw);
        if (target != null && !isTargetDraw)
        {
            Vec2 pos = Level.activeLevel.camera.position - new Vec2(1f, 1f);
            pos.X = (float)Math.Round(pos.X);
            pos.Y = (float)Math.Round(pos.Y);
            Color c = new Color(1f * _targetFade, 1f * _targetFade, 1f * _targetFade, 1f);
            Vec2 sizo = new Vec2(Math.Max(camera.width, Graphics.width), Math.Max(camera.height, Graphics.height));
            Graphics.skipReplayRender = true;
            Graphics.Draw(target, pos, null, c, 0f, Vec2.Zero, new Vec2(sizo.X / (float)target.width, sizo.Y / (float)target.height), SpriteEffects.None, 1f);
            if (name == "LIGHTING")
            {
                if (VirtualTransition.core._scanStage == 1)
                {
                    targetClearColor = Lerp.ColorSmooth(new Color(120, 120, 120, 255), Color.White, VirtualTransition.core._stick);
                }
                else if (VirtualTransition.core._scanStage == 3)
                {
                    targetClearColor = new Color(120, 120, 120, 255);
                }
            }
            Graphics.skipReplayRender = false;
        }
        else
        {
            if (transparent)
            {
                Level.activeLevel.PreDrawLayer(this);
            }
            HashSet<Thing> drawListTransparent = _transparent;
            HashSet<Thing> drawListOpaque = _opaque;
            if (_shareDrawObjects != null)
            {
                drawListTransparent = _shareDrawObjects._transparent;
                drawListOpaque = _shareDrawObjects._opaque;
            }
            if (!skipDrawing)
            {
                if (transparent)
                {
                    if (Network.isActive)
                    {
                        foreach (Thing drawable in drawListTransparent)
                        {
                            if (!drawable.visible || (drawable.ghostObject != null && !drawable.ghostObject.IsInitialized()))
                            {
                                continue;
                            }
                            if (_perspective)
                            {
                                Vec2 pos2 = drawable.Position;
                                Vec3 newPos = new Vec3(pos2.X, drawable.Z, drawable.bottom);
                                Viewport v = new Viewport(0, 0, 320, 180);
                                newPos = v.Project(newPos, projection, view, Matrix.Identity);
                                drawable.Position = new Vec2(newPos.x, newPos.y - drawable.CenterY);
                                drawable.DoDraw();
                                Graphics.material = null;
                                drawable.Position = pos2;
                                if (drawable is PhysicsObject)
                                {
                                    float dist = Maths.NormalizeSection(0f - drawable.Y, 8f, 64f);
                                    _dropShadow.Alpha = 0.5f - 0.5f * dist;
                                    _dropShadow.Scale = new Vec2(1f - dist, 1f - dist);
                                    _dropShadow.Depth = drawable.Depth - 10;
                                    newPos = new Vec3(pos2.X, drawable.Z, 0f);
                                    newPos = v.Project(newPos, projection, view, Matrix.Identity);
                                    Graphics.Draw(_dropShadow, newPos.x - 1f, newPos.y - 1f);
                                }
                            }
                            else
                            {
                                drawable.DoDraw();
                            }
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
                            {
                                continue;
                            }
                            if (_perspective)
                            {
                                Vec2 pos3 = drawable3.Position;
                                Vec3 newPos2 = new Vec3(pos3.X, drawable3.Z, drawable3.bottom);
                                Viewport v2 = new Viewport(0, 0, 320, 180);
                                newPos2 = v2.Project(newPos2, projection, view, Matrix.Identity);
                                drawable3.Position = new Vec2(newPos2.x, newPos2.y - drawable3.CenterY);
                                drawable3.DoDraw();
                                Graphics.material = null;
                                drawable3.Position = pos3;
                                if (drawable3 is PhysicsObject)
                                {
                                    float dist2 = Maths.NormalizeSection(0f - drawable3.Y, 8f, 64f);
                                    _dropShadow.Alpha = 0.5f - 0.5f * dist2;
                                    _dropShadow.Scale = new Vec2(1f - dist2, 1f - dist2);
                                    _dropShadow.Depth = drawable3.Depth - 10;
                                    newPos2 = new Vec3(pos3.X, drawable3.Z, 0f);
                                    newPos2 = v2.Project(newPos2, projection, view, Matrix.Identity);
                                    Graphics.Draw(_dropShadow, newPos2.x - 1f, newPos2.y - 1f);
                                }
                            }
                            else
                            {
                                drawable3.DoDraw();
                            }
                            Graphics.material = null;
                        }
                        if (DevConsole.showCollision)
                        {
                            foreach (Thing drawable4 in drawListTransparent)
                            {
                                if (drawable4.visible)
                                {
                                    drawable4.DrawCollision();
                                }
                            }
                        }
                    }
                    if (ignoreTransparent)
                    {
                        foreach (Thing drawable5 in drawListOpaque)
                        {
                            if (drawable5.visible)
                            {
                                drawable5.DoDraw();
                            }
                            Graphics.material = null;
                        }
                        StaticRenderer.RenderLayer(this);
                    }
                }
                else
                {
                    foreach (Thing drawable6 in drawListOpaque)
                    {
                        if (drawable6.visible)
                        {
                            drawable6.DoDraw();
                        }
                    }
                    StaticRenderer.RenderLayer(this);
                }
            }
            if (transparent)
            {
                Level.activeLevel.PostDrawLayer(this);
            }
        }
        if (Network.isActive && Network.inputDelayFrames > 0 && this == Game)
        {
            Graphics.drawCalls.Enqueue(Graphics.currentFrameCalls);
            if (Graphics.drawCalls.Count > 0)
            {
                List<DrawCall> list = Graphics.drawCalls.Peek();
                if (Graphics.drawCalls.Count > Network.inputDelayFrames)
                {
                    Graphics.drawCalls.Dequeue();
                }
                foreach (DrawCall c2 in list)
                {
                    if (c2.material != null)
                    {
                        Graphics.screen.DrawWithMaterial(c2.texture, c2.position, c2.sourceRect, c2.color, c2.rotation, c2.origin, c2.scale, c2.effects, c2.depth, c2.material);
                    }
                    else
                    {
                        Graphics.screen.Draw(c2.texture, c2.position, c2.sourceRect, c2.color, c2.rotation, c2.origin, c2.scale, c2.effects, c2.depth);
                    }
                }
            }
        }
        End(transparent, isTargetDraw);
    }
}
