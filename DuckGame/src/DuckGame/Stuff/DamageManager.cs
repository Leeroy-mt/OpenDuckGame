using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DuckGame;

public static class DamageManager
{
    private const int kNumTargets = 256;

    private static List<RenderTarget2D> _targets = new List<RenderTarget2D>();

    private static List<DamageMap> _damageMaps = new List<DamageMap>();

    private static int _nextTarget = 0;

    private static int _nextDamageMap = 0;

    private static int _targetsPerFrame = 1;

    private static List<DamageHit> _hits = new List<DamageHit>();

    private static BlendState _blendState;

    private static BlendState _subtractiveBlend;

    private static SpriteMap _burns;

    private static SpriteMap _bulletHoles;

    public static void Initialize()
    {
        for (int i = 0; i < 256; i++)
        {
            _targets.Add(new RenderTarget2D(16, 16, pdepth: true));
            _damageMaps.Add(new DamageMap());
        }
        _blendState = new BlendState();
        _blendState.ColorSourceBlend = Blend.Zero;
        _blendState.ColorDestinationBlend = Blend.SourceColor;
        _blendState.ColorBlendFunction = BlendFunction.Add;
        _blendState.AlphaSourceBlend = Blend.Zero;
        _blendState.AlphaDestinationBlend = Blend.SourceColor;
        _blendState.AlphaBlendFunction = BlendFunction.Add;
        _subtractiveBlend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract
        };
        _burns = new SpriteMap("scratches", 16, 16);
        _burns.CenterOrigin();
        _bulletHoles = new SpriteMap("bulletHoles", 8, 8);
        _bulletHoles.CenterOrigin();
    }

    public static RenderTarget2D Get16x16Target()
    {
        _nextTarget = (_nextTarget + 1) % 256;
        return _targets[_nextTarget];
    }

    public static DamageMap GetDamageMap()
    {
        _nextDamageMap = (_nextDamageMap + 1) % 256;
        _damageMaps[_nextDamageMap].Clear();
        return _damageMaps[_nextDamageMap];
    }

    public static void RegisterHit(Vector2 pt, Thing t, DamageType tp)
    {
        bool found = false;
        foreach (DamageHit hit in _hits)
        {
            if (hit.thing == t)
            {
                hit.points.Add(pt);
                hit.types.Add(tp);
                found = true;
                break;
            }
        }
        if (!found)
        {
            DamageHit d = new DamageHit();
            d.thing = t;
            d.points.Add(pt);
            d.types.Add(tp);
            _hits.Add(d);
        }
    }

    public static void ClearHits()
    {
        _hits.Clear();
    }

    public static void Update()
    {
        int i = _targetsPerFrame;
        int index = 0;
        while (i > 0 && _hits.Count > 0 && index < _hits.Count)
        {
            DamageHit hit = _hits[index];
            if (hit.thing.graphic.renderTexture == null)
            {
                hit.thing.graphic = hit.thing.GetEditorImage(0, 0, transparentBack: true, null, Get16x16Target());
                index++;
                i--;
                continue;
            }
            _hits.RemoveAt(index);
            float s = (float)hit.thing.graphic.width / (float)hit.thing.graphic.width;
            Camera cam = new Camera(0f, 0f, hit.thing.graphic.width, hit.thing.graphic.height);
            cam.position = new Vector2(hit.thing.X - hit.thing.CenterX * s, hit.thing.Y - hit.thing.CenterY * s);
            Graphics.SetRenderTarget(hit.thing.graphic.renderTexture);
            DepthStencilState state = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            Graphics.screen.Begin(SpriteSortMode.BackToFront, _blendState, SamplerState.PointClamp, state, RasterizerState.CullNone, null, cam.getMatrix());
            foreach (Vector2 p in hit.points)
            {
                _bulletHoles.Depth = 1f;
                _bulletHoles.X = p.X + Rando.Float(-1f, 1f);
                _bulletHoles.Y = p.Y + Rando.Float(-1f, 1f);
                _bulletHoles.imageIndex = Rando.Int(4);
                _bulletHoles.Draw();
            }
            Graphics.screen.End();
            Graphics.device.SetRenderTarget(null);
            i--;
        }
    }
}
