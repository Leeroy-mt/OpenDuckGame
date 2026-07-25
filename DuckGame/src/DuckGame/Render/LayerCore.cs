using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class LayerCore
{
    struct MapEntry
    {
        public int index;

        public int order;
    }

    #region Public Fields

    public bool doVirtualEffect;
    public bool basicWireframeTex;

    public Layer _parallax;
    public Layer _virtual;
    public Layer _background;
    public Layer _game;
    public Layer _blocks;
    public Layer _glow;
    public Layer _lighting;
    public Layer _foreground;
    public Layer _hud;
    public Layer _console;

    public List<Layer> _layers = [];
    public List<Layer> _extraLayers = [];
    public List<Layer> _hybridList = [];

    public MTEffect _basicEffectFadeAdd;
    public MTEffect _basicEffectAdd;
    public MTEffect _basicEffectFade;
    public MTEffect _basicEffect;
    public MTEffect _basicWireframeEffect;
    public MTEffect _basicWireframeEffectTex;

    #endregion

    #region Private Fields

    int _lastDrawIndexCount;

    MapEntry[] _layerMap;

    #endregion

    #region Public Properties

    public bool allVisible
    {
        set
        {
            foreach (var layer in _layers)
                layer.visible = value;
            foreach (var extraLayer in _extraLayers)
                extraLayer.visible = value;
        }
    }

    public MTEffect basicWireframeEffect =>
        !basicWireframeTex ? _basicWireframeEffect : _basicWireframeEffectTex;

    #endregion

    #region Public Methods

    public void InitializeLayers()
    {
        Layer.lightingTwoPointOh = false;

        _parallax = new("PARALLAX", 100)
        {
            allowTallAspect = true,
            aspectReliesOnGameLayer = true
        };
        _layers.Add(_parallax);

        _virtual = new("VIRTUAL", 95)
        {
            allowTallAspect = true,
            aspectReliesOnGameLayer = true
        };
        _layers.Add(_virtual);

        _background = new("BACKGROUND", 90)
        {
            enableCulling = true,
            allowTallAspect = true
        };
        _layers.Add(_background);

        _game = new("GAME")
        {
            enableCulling = false,
            allowTallAspect = true
        };
        _layers.Add(_game);

        _blocks = new("BLOCKS", -18)
        {
            enableCulling = true,
            allowTallAspect = true
        };
        _layers.Add(_blocks);

        _foreground = new("FOREGROUND", -19)
        {
            allowTallAspect = true
        };
        _layers.Add(_foreground);

        _layers.Add(_hud = new("HUD", -90));

        _console = new("CONSOLE", -100, new(Resolution.current.x / 2, Resolution.current.y / 2))
        {
            allowTallAspect = true
        };
        _layers.Add(_console);

        _glow = new("GLOW", -21)
        {
            allowTallAspect = true,
            blend = BlendState.Additive
        };
        _layers.Add(_glow);

        _lighting = new("LIGHTING", Layer.lightingTwoPointOh ? -20 : -10, null, true, new(Graphics.width, Graphics.height))
        {
            allowTallAspect = true,
            targetBlend = new()
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Add
            },
            blend = new()
            {
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.SourceColor,
                ColorBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.SourceAlpha,
                AlphaBlendFunction = BlendFunction.Add
            },
            targetClearColor = new(120, 120, 120, 255),
            targetDepthStencil = DepthStencilState.None,
            flashAddClearInfluence = 1
        };
        _layers.Add(_lighting);

        _layers = [.. _layers.OrderBy(l => -l.depth)];

        Layer.Parallax.flashAddInfluence = 1;
        Layer.HUD.flashAddInfluence = 1;

        if (_basicEffect == null)
        {
            _basicWireframeEffect = Content.Load<MTEffect>("Shaders/wireframe");
            _basicWireframeEffectTex = Content.Load<MTEffect>("Shaders/wireframeTex");
            _basicEffect = Content.Load<MTEffect>("Shaders/basic");
            _basicEffect.effect.Name = "Shaders/basic";
            _basicEffectFade = Content.Load<MTEffect>("Shaders/basicFade");
            _basicEffectFade.effect.Name = "Shaders/basicFade";
            _basicEffectAdd = Content.Load<MTEffect>("Shaders/basicAdd");
            _basicEffectAdd.effect.Name = "Shaders/basicAdd";
            _basicEffectFadeAdd = Content.Load<MTEffect>("Shaders/basicFadeAdd");
            _basicEffectFadeAdd.effect.Name = "Shaders/basicFadeAdd";
        }

        ReinitializeLightingTargets();
        ResetLayers();
    }

    public void ClearLayers()
    {
        foreach (Layer hybrid in _hybridList)
            hybrid.Clear();
    }

    public void DrawTargetLayers()
    {
        SortLayers();

        uint drawIndex = 0u;
        for (int i = 0; i < _hybridList.Count; i++)
        {
            Layer layer = _hybridList[_layerMap[i].index];
            if (layer.visible && layer.target != null && ((Layer.lighting && !NetworkDebugger.enabled) || layer != _lighting))
            {
                layer.Draw(transparent: true, isTargetDraw: true);
                drawIndex++;
            }
        }
    }

    public void DrawLayers()
    {
        SortLayers();

        if (_lastDrawIndexCount == 0)
            _lastDrawIndexCount = _hybridList.Count;

        int drawIndex = 0;
        for (int i = 0; i < _hybridList.Count; i++)
        {
            Layer layer = _hybridList[_layerMap[i].index];
            if (layer.visible && (Layer.lighting || layer != _lighting))
            {
                int spanNum = 1;
                if (layer == Layer.Game)
                    spanNum = 3;

                layer.Draw(transparent: true);
                drawIndex += spanNum;
            }
        }
        _lastDrawIndexCount = drawIndex;
    }

    public void UpdateLayers()
    {
        foreach (Layer hybrid in _hybridList)
            hybrid.Update();
    }

    public void ResetLayers()
    {
        Layer.lightingTwoPointOh = false;
        foreach (Layer l in _layers)
        {
            l.fade = 1f;
            l.effect = null;
            l.camera = null;
            l.perspective = false;
            l.fadeAdd = 0f;
            l.colorAdd = Vector3.Zero;
            l.colorMul = Vector3.One;
            if (l != _glow && l != _lighting)
            {
                l.blend = BlendState.AlphaBlend;
                l.targetBlend = BlendState.AlphaBlend;
            }
            l.ClearScissor();
            l.Clear();
        }
        _extraLayers.Clear();
        _parallax.camera = new Camera(0f, 0f, 320f, 320f * Resolution.current.aspect);
        _virtual.camera = new Camera(0f, 0f, 320f, 320f * Resolution.current.aspect);
        _hud.camera = new Camera();
        _hud.allowTallAspect = false;
        _console.camera = new Camera(0f, 0f, Resolution.current.x / 2, Resolution.current.y / 2);
        _hybridList.Clear();
        _hybridList.AddRange(_layers);
    }

    public void Add(Layer l)
    {
        if (!_extraLayers.Contains(l))
        {
            _extraLayers.Add(l);
            _hybridList.Add(l);
        }
    }

    public void Remove(Layer l)
    {
        _extraLayers.Remove(l);
        _hybridList.Remove(l);
    }

    public bool IsBasicLayerEffect(MTEffect e)
    {
        if (e == null)
            return false;

        if (e.EffectIndex != _basicEffect.EffectIndex && e.EffectIndex != _basicEffectAdd.EffectIndex && e.EffectIndex != _basicEffectFade.EffectIndex)
            return e.EffectIndex == _basicEffectFadeAdd.EffectIndex;

        return true;
    }

    public bool Contains(Layer l)
    {
        return _hybridList.Contains(l);
    }

    public static void ReinitializeLightingTargets()
    {
        if (Layer.core._lighting != null)
        {
#if NO_TEX2D
            Layer.core._lighting._target = XnaRenderTarget2D.CreateSetUpTarget(Resolution.current.x, Resolution.current.y);
#else
            Layer.core._lighting._target = new RenderTarget2D(Resolution.current.x, Resolution.current.y);
#endif
            Layer.core._console.camera = new Camera(0f, 0f, DevConsole.size.X, DevConsole.size.Y);
        }
    }

#endregion

    #region Private Methods

    void SortLayers()
    {
        if (_layerMap == null || _layerMap.Length != _hybridList.Count)
            _layerMap = new MapEntry[_hybridList.Count];

        bool sorted = true;
        int idx = 0;
        int maxDepth = int.MinValue;

        foreach (Layer hybrid in _hybridList)
        {
            int depth = -hybrid.depth;

            _layerMap[idx].index = idx;
            _layerMap[idx].order = depth;

            if (depth < maxDepth)
                sorted = false;
            else
                maxDepth = depth;

            idx++;
        }

        if (!sorted)
            Array.Sort(_layerMap, (x, y) => x.order.CompareTo(y.order));
    }

    #endregion
}