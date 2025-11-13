using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class LayerCore
{
	private struct MapEntry
	{
		public int index;

		public int order;
	}

	private const string NameParallax = "PARALLAX";

	private const string NameVirtual = "VIRTUAL";

	private const string NameBackground = "BACKGROUND";

	private const string NameGame = "GAME";

	private const string NameBlocks = "BLOCKS";

	private const string NameGlow = "GLOW";

	private const string NameLighting = "LIGHTING";

	private const string NameForeground = "FOREGROUND";

	private const string NameHUD = "HUD";

	private const string NameConsole = "CONSOLE";

	public bool doVirtualEffect;

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

	public List<Layer> _layers = new List<Layer>();

	public List<Layer> _extraLayers = new List<Layer>();

	public List<Layer> _hybridList = new List<Layer>();

	public MTEffect _basicEffectFadeAdd;

	public MTEffect _basicEffectAdd;

	public MTEffect _basicEffectFade;

	public MTEffect _basicEffect;

	public MTEffect _basicWireframeEffect;

	public MTEffect _basicWireframeEffectTex;

	public MTEffect _itemSpawnEffect;

	public bool basicWireframeTex;

	private MapEntry[] _layerMap;

	private int _lastDrawIndexCount;

	public bool allVisible
	{
		set
		{
			foreach (Layer layer in _layers)
			{
				layer.visible = value;
			}
			foreach (Layer extraLayer in _extraLayers)
			{
				extraLayer.visible = value;
			}
		}
	}

	public MTEffect basicWireframeEffect
	{
		get
		{
			if (!basicWireframeTex)
			{
				return _basicWireframeEffect;
			}
			return _basicWireframeEffectTex;
		}
	}

	public MTEffect basicLayerEffect => _basicEffectFadeAdd;

	public bool IsBasicLayerEffect(MTEffect e)
	{
		if (e == null)
		{
			return false;
		}
		if (e.effectIndex != _basicEffect.effectIndex && e.effectIndex != _basicEffectAdd.effectIndex && e.effectIndex != _basicEffectFade.effectIndex)
		{
			return e.effectIndex == _basicEffectFadeAdd.effectIndex;
		}
		return true;
	}

	public void InitializeLayers()
	{
		Layer.lightingTwoPointOh = false;
		_layers.Add(new Layer("PARALLAX", 100));
		_parallax = _layers[_layers.Count - 1];
		_parallax.allowTallAspect = true;
		_parallax.aspectReliesOnGameLayer = true;
		_layers.Add(new Layer("VIRTUAL", 95));
		_virtual = _layers[_layers.Count - 1];
		_virtual.allowTallAspect = true;
		_virtual.aspectReliesOnGameLayer = true;
		_layers.Add(new Layer("BACKGROUND", 90));
		_background = _layers[_layers.Count - 1];
		_background.enableCulling = true;
		_background.allowTallAspect = true;
		_layers.Add(new Layer("GAME"));
		_game = _layers[_layers.Count - 1];
		_game.enableCulling = false;
		_game.allowTallAspect = true;
		_layers.Add(new Layer("BLOCKS", -18));
		_blocks = _layers[_layers.Count - 1];
		_blocks.enableCulling = true;
		_blocks.allowTallAspect = true;
		_layers.Add(new Layer("FOREGROUND", -19));
		_foreground = _layers[_layers.Count - 1];
		_foreground.allowTallAspect = true;
		_layers.Add(new Layer("HUD", -90));
		_hud = _layers[_layers.Count - 1];
		_layers.Add(new Layer("CONSOLE", -100, new Camera(Resolution.current.x / 2, Resolution.current.y / 2)));
		_console = _layers[_layers.Count - 1];
		_console.allowTallAspect = true;
		_layers.Add(new Layer("GLOW", -21));
		_glow = _layers[_layers.Count - 1];
		_glow.allowTallAspect = true;
		_layers.Add(new Layer("LIGHTING", Layer.lightingTwoPointOh ? (-20) : (-10), null, targetLayer: true, new Vec2(Graphics.width, Graphics.height)));
		_lighting = _layers[_layers.Count - 1];
		_lighting.allowTallAspect = true;
		BlendState blend = new BlendState();
		blend.ColorSourceBlend = Blend.Zero;
		blend.ColorDestinationBlend = Blend.SourceColor;
		blend.ColorBlendFunction = BlendFunction.Add;
		blend.AlphaSourceBlend = Blend.Zero;
		blend.AlphaDestinationBlend = Blend.SourceAlpha;
		blend.AlphaBlendFunction = BlendFunction.Add;
		_glow.blend = BlendState.Additive;
		_lighting.targetBlend = BlendState.Additive;
		BlendState addblend = new BlendState();
		addblend.ColorSourceBlend = Blend.One;
		addblend.ColorDestinationBlend = Blend.One;
		addblend.ColorBlendFunction = BlendFunction.Add;
		addblend.AlphaSourceBlend = Blend.One;
		addblend.AlphaDestinationBlend = Blend.One;
		addblend.AlphaBlendFunction = BlendFunction.Add;
		_lighting.targetBlend = addblend;
		_lighting.blend = blend;
		_lighting.targetClearColor = new Color(120, 120, 120, 255);
		_lighting.targetDepthStencil = DepthStencilState.None;
		_lighting.flashAddClearInfluence = 1f;
		new BlendState
		{
			ColorBlendFunction = BlendFunction.Add,
			ColorSourceBlend = Blend.DestinationColor,
			ColorDestinationBlend = Blend.Zero,
			AlphaBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.DestinationColor,
			AlphaDestinationBlend = Blend.Zero
		};
		_layers = _layers.OrderBy((Layer l) => -l.depth).ToList();
		Layer.Parallax.flashAddInfluence = 1f;
		Layer.HUD.flashAddInfluence = 1f;
		if (_basicEffect == null)
		{
			_itemSpawnEffect = Content.Load<MTEffect>("Shaders/wireframeTex");
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

	public static void ReinitializeLightingTargets()
	{
		if (Layer.core._lighting != null)
		{
			Layer.core._lighting._target = new RenderTarget2D(Resolution.current.x, Resolution.current.y);
			Layer.core._console.camera = new Camera(0f, 0f, DevConsole.size.x, DevConsole.size.y);
		}
	}

	public void ClearLayers()
	{
		foreach (Layer hybrid in _hybridList)
		{
			hybrid.Clear();
		}
	}

	private void SortLayers()
	{
		if (_layerMap == null || _layerMap.Length != _hybridList.Count)
		{
			_layerMap = new MapEntry[_hybridList.Count];
		}
		bool sorted = true;
		int idx = 0;
		int maxDepth = int.MinValue;
		foreach (Layer hybrid in _hybridList)
		{
			int depth = -hybrid.depth;
			_layerMap[idx].index = idx;
			_layerMap[idx].order = depth;
			if (depth < maxDepth)
			{
				sorted = false;
			}
			else
			{
				maxDepth = depth;
			}
			idx++;
		}
		if (!sorted)
		{
			Array.Sort(_layerMap, (MapEntry x, MapEntry y) => x.order.CompareTo(y.order));
		}
	}

	public void DrawTargetLayers()
	{
		SortLayers();
		uint drawIndex = 0u;
		for (int i = 0; i < _hybridList.Count; i++)
		{
			Layer layer = _hybridList[_layerMap[i].index];
			if (layer.visible && layer.isTargetLayer && ((Layer.lighting && !NetworkDebugger.enabled) || layer != _lighting))
			{
				_ = 2f / (float)_hybridList.Count * (float)i / 2f;
				_ = 2f / (float)_hybridList.Count * (float)(i + 1) / 2f;
				layer.Draw(transparent: true, isTargetDraw: true);
				drawIndex++;
			}
		}
	}

	public void DrawLayers()
	{
		SortLayers();
		if (_lastDrawIndexCount == 0)
		{
			_lastDrawIndexCount = _hybridList.Count;
		}
		int drawIndex = 0;
		for (int i = 0; i < _hybridList.Count; i++)
		{
			Layer layer = _hybridList[_layerMap[i].index];
			if (layer.visible && (Layer.lighting || layer != _lighting))
			{
				int spanNum = 1;
				if (layer == Layer.Game)
				{
					spanNum = 3;
				}
				_ = 2f / (float)_lastDrawIndexCount * (float)drawIndex / 2f;
				_ = 2f / (float)_lastDrawIndexCount * (float)(drawIndex + spanNum) / 2f;
				layer.Draw(transparent: true);
				drawIndex += spanNum;
			}
		}
		_lastDrawIndexCount = drawIndex;
	}

	public void UpdateLayers()
	{
		foreach (Layer hybrid in _hybridList)
		{
			hybrid.Update();
		}
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
			l.colorAdd = Vec3.Zero;
			l.colorMul = Vec3.One;
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

	public Layer Get(string layer)
	{
		return _layers.FirstOrDefault((Layer x) => x.name == layer);
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

	public bool Contains(Layer l)
	{
		return _hybridList.Contains(l);
	}
}
