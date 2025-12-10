using System;
using System.Collections.Generic;

namespace DuckGame;

public class VirtualTransitionCore
{
    private Sprite _scanner;

    private BitmapFont _smallBios;

    private BackgroundUpdater _realBackground;

    public int _scanStage = -1;

    public float _stick;

    public bool _fullyVirtual = true;

    public bool _fullyNonVirtual;

    public bool _virtualMode;

    public bool _visible = true;

    private ParallaxBackground _parallax;

    private Color _backgroundColor;

    protected float _lastCameraX;

    private Rectangle _scissor = new Rectangle(0f, 0f, 0f, 0f);

    private bool _done = true;

    private bool _incStage;

    private bool _decStage;

    private Color _curBackgroundColor;

    public Level _transitionLevel;

    private Vec2 _position;

    public bool doingVirtualTransition
    {
        get
        {
            if (_virtualMode)
            {
                return !_done;
            }
            return false;
        }
    }

    public bool active => !_done;

    public void Initialize()
    {
        _fullyVirtual = false;
        _fullyNonVirtual = false;
        _virtualMode = false;
        _smallBios = new BitmapFont("smallBiosFont", 7, 6);
        _parallax = new ParallaxBackground("background/virtual", 0f, 0f, 3);
        float speed = 0.4f;
        float rearDist = 0.8f;
        _parallax.AddZone(0, rearDist, speed);
        _parallax.AddZone(1, rearDist, speed);
        _parallax.AddZone(2, rearDist, speed);
        _parallax.AddZone(3, rearDist, speed);
        float closeDist = 0.6f;
        float div = (rearDist - closeDist) / 4f;
        float speedo = 0.6f;
        _parallax.AddZone(4, rearDist - div * 1f, speedo, moving: true);
        _parallax.AddZone(5, rearDist - div * 2f, 0f - speedo, moving: true);
        _parallax.AddZone(6, rearDist - div * 3f, speedo, moving: true);
        _parallax.AddZone(7, closeDist, speed);
        _parallax.AddZone(8, closeDist, speed);
        _parallax.AddZone(19, closeDist, speed);
        _parallax.AddZone(20, closeDist, speed);
        _parallax.AddZone(21, rearDist - div * 3f, 0f - speedo, moving: true);
        _parallax.AddZone(22, rearDist - div * 2f, speedo, moving: true);
        _parallax.AddZone(23, rearDist - div * 1f, 0f - speedo, moving: true);
        _parallax.AddZone(24, rearDist, speed);
        _parallax.AddZone(25, rearDist, speed);
        _parallax.AddZone(26, rearDist, speed);
        _parallax.AddZone(27, rearDist, speed);
        _parallax.AddZone(28, rearDist, speed);
        _parallax.AddZone(29, rearDist, speed);
        _parallax.AddZone(30, rearDist, speed);
        _parallax.AddZone(31, rearDist, speed);
        _parallax.AddZone(32, rearDist, speed);
        _parallax.AddZone(33, rearDist, speed);
        _parallax.AddZone(34, rearDist, speed);
        _parallax.restrictBottom = false;
        _visible = true;
        _parallax.y = 0f;
        _scanner = new Sprite("background/scanbeam");
        _backgroundColor = Color.Black;
        _parallax.layer = Layer.Virtual;
    }

    public void SetVisible(bool vis)
    {
        _parallax.scissor = _scissor;
        _parallax.visible = vis;
        if (_scissor.width != 0f)
        {
            _parallax.layer.scissor = _scissor;
        }
    }

    public void GoVirtual()
    {
        if (_virtualMode)
        {
            return;
        }
        _scanStage = 2;
        _stick = 1f;
        _virtualMode = true;
        _done = false;
        if (_realBackground == null)
        {
            using IEnumerator<Thing> enumerator = Level.activeLevel.things[typeof(BackgroundUpdater)].GetEnumerator();
            if (enumerator.MoveNext())
            {
                _curBackgroundColor = (_realBackground = (BackgroundUpdater)enumerator.Current).backgroundColor;
            }
        }
        _transitionLevel = Level.activeLevel;
    }

    public void GoUnVirtual()
    {
        if (_virtualMode)
        {
            _realBackground = null;
            _virtualMode = false;
            _scanStage = 0;
            _stick = 0f;
            _done = false;
        }
    }

    public void Update()
    {
        if (_done && !Level.current._waitingOnTransition)
        {
            Layer.doVirtualEffect = false;
            if (_realBackground != null)
            {
                Level.activeLevel.backgroundColor = _realBackground.backgroundColor;
                _realBackground.scissor = new Rectangle(0f, 0f, Resolution.current.x, Resolution.current.y);
                _realBackground = null;
            }
            return;
        }
        if (Level.current._waitingOnTransition)
        {
            _realBackground = null;
        }
        if (_realBackground == null)
        {
            using IEnumerator<Thing> enumerator = Level.activeLevel.things[typeof(BackgroundUpdater)].GetEnumerator();
            if (enumerator.MoveNext())
            {
                BackgroundUpdater bg = (BackgroundUpdater)enumerator.Current;
                _realBackground = bg;
            }
        }
        float backStick = _stick;
        if (_scanStage == 2 && _virtualMode)
        {
            _backgroundColor = _curBackgroundColor;
            Level.activeLevel.backgroundColor = Lerp.ColorSmoothNoAlpha(_backgroundColor, _curBackgroundColor, _stick);
            Layer.Glow.fade = Lerp.FloatSmooth(Layer.Glow.fade, 0f, _stick);
        }
        if (_scanStage == 0 && !_virtualMode && _realBackground != null)
        {
            Level.activeLevel.backgroundColor = Lerp.ColorSmoothNoAlpha(_backgroundColor, _realBackground.backgroundColor, _stick);
            Layer.Glow.fade = Lerp.FloatSmooth(Layer.Glow.fade, 1f, _stick);
        }
        if (_scanStage == -1)
        {
            Level.activeLevel.backgroundColor = Lerp.ColorSmoothNoAlpha(_backgroundColor, Color.Black, 0.1f);
        }
        if (_scanStage < 2)
        {
            backStick = 0f;
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
            _visible = false;
        }
        else
        {
            _scissor = sc2;
            SetVisible(vis: true);
            _visible = true;
        }
        float lerpSpeed = 0.04f;
        float outLerpSpeed = 0.06f;
        if (Level.activeLevel != null)
        {
            lerpSpeed *= Level.activeLevel.transitionSpeedMultiplier;
            outLerpSpeed *= Level.activeLevel.transitionSpeedMultiplier;
        }
        if (!_virtualMode)
        {
            if (_scanStage == 0)
            {
                _stick = Lerp.Float(_stick, 1f, lerpSpeed);
                if (_stick > 0.99f)
                {
                    _stick = 1f;
                    _incStage = true;
                }
            }
            else if (_scanStage == 1)
            {
                _stick = Lerp.Float(_stick, 0f, lerpSpeed);
                if (_stick < 0.01f)
                {
                    _stick = 0f;
                    _incStage = true;
                }
            }
            else if (_scanStage == 2)
            {
                Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(0f);
                if (Layer.basicWireframeTex)
                {
                    Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(0f);
                }
                _stick = Lerp.Float(_stick, 1f, lerpSpeed);
                if (_stick > 0.99f)
                {
                    _stick = 1f;
                    _incStage = true;
                    _done = true;
                }
            }
        }
        else if (_scanStage == 2)
        {
            Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(0f);
            if (Layer.basicWireframeTex)
            {
                Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(0f);
            }
            _stick = Lerp.Float(_stick, 0f, outLerpSpeed);
            if (_stick < 0.01f)
            {
                _stick = 0f;
                _decStage = true;
            }
        }
        else if (_scanStage == 1)
        {
            _stick = Lerp.Float(_stick, 1f, outLerpSpeed);
            if (_stick > 0.99f)
            {
                _stick = 1f;
                _decStage = true;
            }
        }
        else if (_scanStage == 0)
        {
            _stick = Lerp.Float(_stick, 0f, outLerpSpeed);
            if (_stick < 0.01f)
            {
                _stick = 0f;
                _decStage = true;
                _done = true;
            }
        }
        if (_incStage)
        {
            _incStage = false;
            _scanStage++;
        }
        if (_decStage)
        {
            _decStage = false;
            _scanStage--;
        }
        if (_scanStage < 2)
        {
            Layer.doVirtualEffect = true;
            if (_scanStage == 1)
            {
                Layer.basicWireframeTex = true;
            }
            else
            {
                Layer.basicWireframeTex = false;
            }
            Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(_stick);
            if (Layer.basicWireframeTex)
            {
                Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(1f);
            }
        }
        _fullyVirtual = false;
        _fullyNonVirtual = false;
        if (_scanStage == 3)
        {
            _fullyNonVirtual = true;
        }
        else if (_scanStage == -1)
        {
            _fullyVirtual = true;
        }
        _lastCameraX = Level.activeLevel.camera.centerX;
        if (_scissor.width != 0f)
        {
            _parallax.scissor = _scissor;
        }
    }

    public void Draw()
    {
        if ((!_done || (_virtualMode && _transitionLevel == Level.activeLevel) || Level.current._waitingOnTransition) && _parallax != null)
        {
            if (!_visible)
            {
                _parallax.visible = false;
                return;
            }
            Graphics.PushMarker("TransitionDraw");
            _position = _parallax.position;
            float scannerX = _stick * 300f;
            float scannerFrontX = 360f - _stick * 400f;
            Vec2 scannerPos = new Vec2(_position.x + scannerX, _position.y + 72f);
            Graphics.Draw(_scanner, scannerPos.x, scannerPos.y);
            float scanMiddle = Math.Abs(_stick - 0.5f);
            float a = 0.5f - scanMiddle;
            Graphics.DrawLine(scannerPos + new Vec2(18f, 20f), new Vec2(scannerFrontX, scannerPos.y - 100f + scanMiddle * 250f), Color.Red * a, 2f, 0.9f);
            Graphics.DrawLine(scannerPos + new Vec2(18f, 34f), new Vec2(scannerFrontX, scannerPos.y - 10f + 80f * scanMiddle), Color.Red * a, 2f, 0.9f);
            Vec2 scannerPosBottom = scannerPos + new Vec2(0f, _scanner.height);
            Graphics.DrawLine(scannerPosBottom + new Vec2(18f, -20f), new Vec2(scannerFrontX, scannerPosBottom.y + 100f - scanMiddle * 250f), Color.Red * a, 2f, 0.9f);
            Graphics.DrawLine(scannerPosBottom + new Vec2(18f, -34f), new Vec2(scannerFrontX, scannerPosBottom.y + 10f - 80f * scanMiddle), Color.Red * a, 2f, 0.9f);
            _parallax.Update();
            _parallax.Draw();
            Graphics.PopMarker();
        }
    }
}
