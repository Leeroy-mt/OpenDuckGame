using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class VirtualTransitionCore
{
    #region Public Fields

    public bool _fullyVirtual = true;
    public bool _fullyNonVirtual;
    public bool _virtualMode;
    public bool _visible = true;

    public int _scanStage = -1;

    public float _stick;

    public Level _transitionLevel;

    #endregion

    #region Private Fields

    bool _done = true;
    bool _incStage;
    bool _decStage;

    Color _backgroundColor;
    Rectangle _scissor;
    Color _curBackgroundColor;
    Vector2 _position;

    Sprite _scanner;
    BackgroundUpdater _realBackground;
    ParallaxBackground _parallax;

    #endregion

    protected float _lastCameraX;

    #region Public Properties

    public bool doingVirtualTransition => _virtualMode && !_done;

    public bool active => !_done;

    #endregion

    #region Public Methods

    public void Initialize()
    {
        _fullyVirtual = false;
        _fullyNonVirtual = false;
        _virtualMode = false;
        _parallax = new ParallaxBackground("background/virtual", 0, 0, 3);
        float speed = .4f;
        float rearDist = .8f;
        _parallax.AddZone(0, rearDist, speed);
        _parallax.AddZone(1, rearDist, speed);
        _parallax.AddZone(2, rearDist, speed);
        _parallax.AddZone(3, rearDist, speed);
        float closeDist = .6f;
        float div = (rearDist - closeDist) / 4;
        float speedo = .6f;
        _parallax.AddZone(4, rearDist - div, speedo, moving: true);
        _parallax.AddZone(5, rearDist - div * 2, -speedo, moving: true);
        _parallax.AddZone(6, rearDist - div * 3, speedo, moving: true);
        _parallax.AddZone(7, closeDist, speed);
        _parallax.AddZone(8, closeDist, speed);
        _parallax.AddZone(19, closeDist, speed);
        _parallax.AddZone(20, closeDist, speed);
        _parallax.AddZone(21, rearDist - div * 3, -speedo, moving: true);
        _parallax.AddZone(22, rearDist - div * 2, speedo, moving: true);
        _parallax.AddZone(23, rearDist - div, -speedo, moving: true);
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
        _parallax.Y = 0;
        _scanner = new Sprite("background/scanbeam");
        _backgroundColor = Color.Black;
        _parallax.layer = Layer.Virtual;
    }

    public void SetVisible(bool vis)
    {
        _parallax.scissor = _scissor;
        _parallax.visible = vis;
        if (_scissor.width != 0)
            _parallax.layer.scissor = _scissor;
    }

    public void GoVirtual()
    {
        if (_virtualMode)
            return;
        _scanStage = 2;
        _stick = 1;
        _virtualMode = true;
        _done = false;
        if (_realBackground == null)
        {
            using IEnumerator<Thing> enumerator = Level.activeLevel.things[typeof(BackgroundUpdater)].GetEnumerator();
            if (enumerator.MoveNext())
                _curBackgroundColor = (_realBackground = (BackgroundUpdater)enumerator.Current).backgroundColor;
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
            _stick = 0;
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
                _realBackground.scissor = new Rectangle(0, 0, Resolution.current.x, Resolution.current.y);
                _realBackground = null;
            }
            return;
        }
        if (Level.current._waitingOnTransition)
            _realBackground = null;
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
            Layer.Glow.fade = Lerp.FloatSmooth(Layer.Glow.fade, 0, _stick);
        }
        if (_scanStage == 0 && !_virtualMode && _realBackground != null)
        {
            Level.activeLevel.backgroundColor = Lerp.ColorSmoothNoAlpha(_backgroundColor, _realBackground.backgroundColor, _stick);
            Layer.Glow.fade = Lerp.FloatSmooth(Layer.Glow.fade, 1, _stick);
        }
        if (_scanStage == -1)
            Level.activeLevel.backgroundColor = Lerp.ColorSmoothNoAlpha(_backgroundColor, Color.Black, .1f);
        if (_scanStage < 2)
            backStick = 0;
        Rectangle sc = new((int)((1 - backStick) * Resolution.current.x), 0, Resolution.current.x - (int)((1 - backStick) * Resolution.current.x), Resolution.current.y);
        if (_realBackground != null)
        {
            if (sc.width == 0)
                _realBackground.SetVisible(vis: false);
            else
            {
                _realBackground.scissor = sc;
                _realBackground.SetVisible(vis: true);
            }
        }
        Rectangle sc2 = new(0, 0, Resolution.current.x - sc.width, Resolution.current.y);
        if (sc2.width == 0)
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
        float lerpSpeed = .04f;
        float outLerpSpeed = .06f;
        if (Level.activeLevel != null)
        {
            lerpSpeed *= Level.activeLevel.transitionSpeedMultiplier;
            outLerpSpeed *= Level.activeLevel.transitionSpeedMultiplier;
        }
        if (!_virtualMode)
        {
            if (_scanStage == 0)
            {
                _stick = Lerp.Float(_stick, 1, lerpSpeed);
                if (_stick > .99f)
                {
                    _stick = 1;
                    _incStage = true;
                }
            }
            else if (_scanStage == 1)
            {
                _stick = Lerp.Float(_stick, 0, lerpSpeed);
                if (_stick < .01f)
                {
                    _stick = 0;
                    _incStage = true;
                }
            }
            else if (_scanStage == 2)
            {
                Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(0);
                if (Layer.basicWireframeTex)
                    Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(0);
                _stick = Lerp.Float(_stick, 1, lerpSpeed);
                if (_stick > .99f)
                {
                    _stick = 1;
                    _incStage = true;
                    _done = true;
                }
            }
        }
        else if (_scanStage == 2)
        {
            Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(0);
            if (Layer.basicWireframeTex)
                Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(0);
            _stick = Lerp.Float(_stick, 0, outLerpSpeed);
            if (_stick < .01f)
            {
                _stick = 0;
                _decStage = true;
            }
        }
        else if (_scanStage == 1)
        {
            _stick = Lerp.Float(_stick, 1, outLerpSpeed);
            if (_stick > .99f)
            {
                _stick = 1;
                _decStage = true;
            }
        }
        else if (_scanStage == 0)
        {
            _stick = Lerp.Float(_stick, 0, outLerpSpeed);
            if (_stick < .01f)
            {
                _stick = 0;
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
            Layer.basicWireframeTex = _scanStage == 1;
            Layer.basicWireframeEffect.effect.Parameters["screenCross"].SetValue(_stick);
            if (Layer.basicWireframeTex)
                Layer.basicWireframeEffect.effect.Parameters["scanMul"].SetValue(1);
        }
        _fullyVirtual = false;
        _fullyNonVirtual = false;
        if (_scanStage == 3)
            _fullyNonVirtual = true;
        else if (_scanStage == -1)
            _fullyVirtual = true;
        _lastCameraX = Level.activeLevel.camera.centerX;
        if (_scissor.width != 0)
            _parallax.scissor = _scissor;
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
            _position = _parallax.Position;
            float scannerX = _stick * 300;
            float scannerFrontX = 360 - _stick * 400;
            Vector2 scannerPos = new(_position.X + scannerX, _position.Y + 72);
            Graphics.Draw(_scanner, scannerPos.X, scannerPos.Y);
            float scanMiddle = Math.Abs(_stick - .5f);
            float a = .5f - scanMiddle;
            Graphics.DrawLine(scannerPos + new Vector2(18, 20), new Vector2(scannerFrontX, scannerPos.Y - 100 + scanMiddle * 250), Color.Red * a, 2, .9f);
            Graphics.DrawLine(scannerPos + new Vector2(18, 34), new Vector2(scannerFrontX, scannerPos.Y - 10 + 80 * scanMiddle), Color.Red * a, 2, .9f);
            Vector2 scannerPosBottom = scannerPos + new Vector2(0, _scanner.height);
            Graphics.DrawLine(scannerPosBottom + new Vector2(18, -20), new Vector2(scannerFrontX, scannerPosBottom.Y + 100 - scanMiddle * 250), Color.Red * a, 2, .9f);
            Graphics.DrawLine(scannerPosBottom + new Vector2(18, -34), new Vector2(scannerFrontX, scannerPosBottom.Y + 10 - 80 * scanMiddle), Color.Red * a, 2, .9f);
            _parallax.Update();
            _parallax.Draw();
            Graphics.PopMarker();
        }
    }

    #endregion
}
