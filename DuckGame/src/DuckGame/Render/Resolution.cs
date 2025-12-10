using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Resolution
{
    public bool recommended;

    private static Resolution _lastApplied;

    private static IntPtr _window;

    private static float _screenDPI;

    private static int _takeFocus;

    private static GraphicsDeviceManager _device;

    private static Resolution _pendingResolution;

    private static Matrix _matrix;

    public bool isDefault;

    public ScreenMode mode;

    public Vec2 dimensions;

    public static Dictionary<ScreenMode, List<Resolution>> supportedDisplaySizes;

    public static Resolution adapterResolution => GetDefault(ScreenMode.Fullscreen);

    public static Resolution current => Options.LocalData.currentResolution;

    public static float fontSizeMultiplier => (float)current.x / 1280f;

    public static Resolution lastApplied => _lastApplied;

    public static Vec2 size
    {
        get
        {
            if (Graphics._screenViewport.HasValue)
            {
                return new Vec2(Graphics._screenViewport.Value.Width, Graphics._screenViewport.Value.Height);
            }
            return current.dimensions;
        }
    }

    public float aspect => dimensions.x / dimensions.y;

    public int x
    {
        get
        {
            return (int)dimensions.x;
        }
        set
        {
            dimensions.x = value;
        }
    }

    public int y
    {
        get
        {
            return (int)dimensions.y;
        }
        set
        {
            dimensions.y = value;
        }
    }

    private static float GetScreenDPI()
    {
        System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
        float dpiX = graphics.DpiX;
        graphics.Dispose();
        return dpiX;
    }

    public static void Set(Resolution pResolution)
    {
        _pendingResolution = pResolution;
    }

    public static void Apply()
    {
        Graphics.snap = 4f;
        if (_pendingResolution == null || (Program.isLinux && !Keyboard.NothingPressed()))
        {
            return;
        }
        _lastApplied = _pendingResolution;
        Options.LocalData.currentResolution = _pendingResolution;
        DevConsole.Log(DCSection.General, "Applying resolution (" + _pendingResolution.ToString() + ")");
        bool valid = false;
        foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
        {
            if (_pendingResolution.x <= dm.Width && _pendingResolution.y <= dm.Height)
            {
                valid = true;
                if (_pendingResolution.mode == ScreenMode.Borderless)
                {
                    _device.PreferredBackBufferWidth = adapterResolution.x;
                    _device.PreferredBackBufferHeight = adapterResolution.y;
                }
                else
                {
                    _device.PreferredBackBufferWidth = _pendingResolution.x;
                    _device.PreferredBackBufferHeight = _pendingResolution.y;
                }
                _device.IsFullScreen = _pendingResolution.mode == ScreenMode.Fullscreen;
                _device.ApplyChanges();
                break;
            }
        }
        if (!valid)
        {
            RestoreDefaults();
            Apply();
            return;
        }
        switch (Options.LocalData.currentResolution.mode)
        {
            case ScreenMode.Windowed:
                Graphics.mouseVisible = false;
                Graphics._screenBufferTarget = null;
                SDL.SDL_SetWindowBordered(_window, true);
                //_window.Location = new System.Drawing.Point(adapterResolution.x / 2 - Options.LocalData.currentResolution.x / 2, adapterResolution.y / 2 - Options.LocalData.currentResolution.y / 2 - 16);
                break;
            case ScreenMode.Borderless:
                Graphics.mouseVisible = false;
                Graphics._screenBufferTarget = new RenderTarget2D(Options.LocalData.currentResolution.x, Options.LocalData.currentResolution.y, pdepth: true, RenderTargetUsage.PreserveContents);
                SDL.SDL_SetWindowBordered(_window, false);
                //_window.Location = new System.Drawing.Point(0, 0);
                if (Graphics._screenBufferTarget.width < 400)
                {
                    Graphics.snap = 1f;
                }
                else if (Graphics._screenBufferTarget.width < 800)
                {
                    Graphics.snap = 2f;
                }
                break;
            case ScreenMode.Fullscreen:
                Graphics.mouseVisible = false;
                Graphics._screenBufferTarget = null;
                break;
        }
        MonoMain._screenCapture = new RenderTarget2D(current.x, current.y, pdepth: true);
        MonoMain.RetakePauseCapture();
        LayerCore.ReinitializeLightingTargets();
        Options.ResolutionChanged();
        if (NetworkDebugger.enabled)
        {
            NetworkDebugger.instance.RefreshRectSizes();
        }
        if (Layer.Game != null && Layer.Game.camera != null)
        {
            Layer.Game.camera.DoUpdate();
        }
        if (Program.isLinux)
        {
            _takeFocus = 10;
        }
        _pendingResolution = null;
        Graphics._screenViewport = null;
    }

    public static bool Update()
    {
        if (_takeFocus > 0)
        {
            _takeFocus--;
            if (_takeFocus == 0)
            {
                SDL.SDL_RaiseWindow(MonoMain.instance.Window.Handle);
                //_window.Focus();
            }
        }
        if (_pendingResolution != null)
        {
            Apply();
            return true;
        }
        return false;
    }

    public static Matrix getTransformationMatrix()
    {
        return Matrix.CreateScale((float)Graphics.viewport.Width / (float)MonoMain.screenWidth, (float)Graphics.viewport.Height / (float)MonoMain.screenHeight, 1f);
    }

    public static void Initialize(object pWindow, GraphicsDeviceManager pDeviceManager)
    {
        _window = (IntPtr)pWindow;
        _device = pDeviceManager;
        supportedDisplaySizes = new Dictionary<ScreenMode, List<Resolution>>();
        DevConsole.Log(DCSection.General, "Enumerating display modes (" + GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Count() + " found...)");
        if (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode == null)
        {
            throw new Exception("No graphics display modes found, your graphics card may not be supported!");
        }
        DevConsole.Log(DCSection.General, "Default adapter size is (" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + "x" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height + ")");
        DevConsole.Log(DCSection.General, "Registered adapter size is (" + MonoMain.instance._adapterW + "x" + MonoMain.instance._adapterH + ")");
        RegisterDisplaySize(ScreenMode.Fullscreen, new Resolution
        {
            dimensions = new Vec2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
        }, pSort: false);
        RegisterDisplaySize(ScreenMode.Borderless, new Resolution
        {
            dimensions = new Vec2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
        }, pSort: false);
        RegisterDisplaySize(ScreenMode.Windowed, new Resolution
        {
            dimensions = new Vec2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
        }, pSort: false);
        foreach (DisplayMode m in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
        {
            RegisterDisplaySize(ScreenMode.Fullscreen, new Resolution
            {
                dimensions = new Vec2(m.Width, m.Height)
            }, pSort: false);
            if (m.Width <= MonoMain.instance._adapterW && m.Height <= MonoMain.instance._adapterH)
            {
                RegisterDisplaySize(ScreenMode.Windowed, new Resolution
                {
                    dimensions = new Vec2(m.Width, m.Height)
                }, pSort: false);
                RegisterDisplaySize(ScreenMode.Borderless, new Resolution
                {
                    dimensions = new Vec2(m.Width, m.Height)
                }, pSort: false);
            }
        }
        RegisterDisplaySize(ScreenMode.Borderless, new Resolution
        {
            dimensions = new Vec2(640f, 360f)
        }, pSort: false);
        RegisterDisplaySize(ScreenMode.Borderless, new Resolution
        {
            dimensions = new Vec2(320f, 180f)
        }, pSort: false);
        RegisterDisplaySize(ScreenMode.Windowed, new Resolution
        {
            dimensions = new Vec2(1280f, 720f)
        }, pSort: false, pRecommended: true);
        RegisterDisplaySize(ScreenMode.Windowed, new Resolution
        {
            dimensions = new Vec2(1920f, 1080f)
        }, pSort: false, pRecommended: true);
        RegisterDisplaySize(ScreenMode.Windowed, new Resolution
        {
            dimensions = new Vec2(2560f, 1440f)
        }, pSort: false, pRecommended: true);
        RegisterDisplaySize(ScreenMode.Windowed, new Resolution
        {
            dimensions = new Vec2(2880f, 1620f)
        }, pSort: false, pRecommended: true);
        DevConsole.Log(DCSection.General, "Finished enumerating display modes (F(" + supportedDisplaySizes[ScreenMode.Fullscreen].Count + ") W(" + supportedDisplaySizes[ScreenMode.Windowed].Count + ") B(" + supportedDisplaySizes[ScreenMode.Borderless].Count + "))");
        SortDisplaySizes();
        DevConsole.Log(DCSection.General, "Finished sorting display modes (F(" + supportedDisplaySizes[ScreenMode.Fullscreen].Count + ") W(" + supportedDisplaySizes[ScreenMode.Windowed].Count + ") B(" + supportedDisplaySizes[ScreenMode.Borderless].Count + "))");
        if (supportedDisplaySizes[ScreenMode.Windowed].Count == 0 && supportedDisplaySizes[ScreenMode.Fullscreen].Count > 0)
        {
            Resolution res = supportedDisplaySizes[ScreenMode.Fullscreen].LastOrDefault();
            RegisterDisplaySize(ScreenMode.Windowed, new Resolution
            {
                dimensions = res.dimensions
            }, pSort: false, pRecommended: false, pForce: true);
            RegisterDisplaySize(ScreenMode.Borderless, new Resolution
            {
                dimensions = res.dimensions
            }, pSort: false, pRecommended: false, pForce: true);
        }
        FindNearest(ScreenMode.Fullscreen, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height).isDefault = true;
        FindNearest(ScreenMode.Windowed, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 1.7777f, pRecommended: true).isDefault = true;
        FindNearest(ScreenMode.Borderless, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height).isDefault = true;
        RestoreDefaults();
        try
        {
            _screenDPI = GetScreenDPI();
        }
        catch (Exception)
        {
            _screenDPI = 120f;
        }
    }

    public static void RestoreDefaults()
    {
        Options.LocalData.fullscreenResolution = GetDefault(ScreenMode.Fullscreen);
        Options.LocalData.windowedResolution = GetDefault(ScreenMode.Windowed);
        Options.LocalData.windowedFullscreenResolution = GetDefault(ScreenMode.Borderless);
    }

    public override string ToString()
    {
        string[] obj = new string[5]
        {
            x.ToString(),
            "x",
            y.ToString(),
            "x",
            null
        };
        int num = (int)mode;
        obj[4] = num.ToString();
        return string.Concat(obj);
    }

    public string ToShortString()
    {
        return x + "x" + y;
    }

    public static Resolution Load(string pSize, string pMemberName = null)
    {
        if (supportedDisplaySizes == null)
        {
            return null;
        }
        try
        {
            string[] parts = pSize.ToLower().Trim().Split('x');
            if (parts.Length >= 2)
            {
                int x = Convert.ToInt32(parts[0]);
                int y = Convert.ToInt32(parts[1]);
                ScreenMode mode = ScreenMode.Windowed;
                if (parts.Length == 3)
                {
                    mode = (ScreenMode)Convert.ToInt32(parts[2]);
                }
                else
                {
                    switch (pMemberName)
                    {
                        case "windowedResolution":
                            mode = ScreenMode.Windowed;
                            break;
                        case "windowedFullscreenResolution":
                            mode = ScreenMode.Borderless;
                            break;
                        case "fullscreenResolution":
                            mode = ScreenMode.Fullscreen;
                            break;
                    }
                }
                Resolution r = FindNearest(mode, x, y);
                if (r != null)
                {
                    return r;
                }
            }
        }
        catch (Exception)
        {
            DevConsole.Log(DCSection.General, "Failed to load resolution (" + pSize + ")");
        }
        return null;
    }

    public static Resolution GetDefault(ScreenMode pMode)
    {
        Resolution r = supportedDisplaySizes[pMode].FirstOrDefault((Resolution x) => x.isDefault);
        if (r == null)
        {
            r = supportedDisplaySizes[pMode].Last();
        }
        return r;
    }

    public static Resolution FindNearest(ScreenMode pMode, int pX, int pY, float pAspect = -1f, bool pRecommended = false)
    {
        Resolution best = FindNearest_Internal(pMode, pX, pY, pAspect, pRecommended);
        if (best == null)
        {
            ScreenMode alternateMode = (ScreenMode)((int)(pMode + 1) % 3);
            while (alternateMode != pMode && best == null)
            {
                best = FindNearest_Internal(alternateMode, pX, pY, pAspect, pRecommended);
            }
        }
        if (best == null)
        {
            DevConsole.Log(DCSection.General, "Failed to find display mode (" + pMode.ToString() + ", " + pX + "x" + pY + ")");
        }
        return best;
    }

    private static Resolution FindNearest_Internal(ScreenMode pMode, int pX, int pY, float pAspect = -1f, bool pRecommended = false)
    {
        Resolution best = supportedDisplaySizes[pMode].LastOrDefault();
        foreach (Resolution r in supportedDisplaySizes[pMode])
        {
            float aspectDifference = Math.Abs(r.aspect - pAspect);
            float bestAspectDifference = Math.Abs(best.aspect - pAspect);
            bool betterAspect = aspectDifference < bestAspectDifference && Math.Abs(aspectDifference - bestAspectDifference) > 0.05f;
            bool sameAspect = Math.Abs(aspectDifference - bestAspectDifference) < 0.05f;
            if ((best == null || Math.Abs(r.x - pX) + Math.Abs(r.y - pY) < Math.Abs(best.x - pX) + Math.Abs(best.y - pY) || (betterAspect && pAspect > 0f) || (pRecommended && r.recommended && !best.recommended)) && (best == null || pAspect < 0f || ((betterAspect || sameAspect) && (r.recommended || !pRecommended))))
            {
                best = r;
            }
        }
        return best;
    }

    internal static Resolution RegisterDisplaySize(ScreenMode pMode, Resolution pResolution, bool pSort = true, bool pRecommended = false, bool pForce = false)
    {
        List<Resolution> elements = null;
        if (!supportedDisplaySizes.TryGetValue(pMode, out elements))
        {
            elements = (supportedDisplaySizes[pMode] = new List<Resolution>());
        }
        if (pResolution.x > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width || pResolution.y > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
        {
            DevConsole.Log("Invalid resolution (" + pResolution.ToString() + "): Larger than adapter (" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + "x" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height + ")");
            return null;
        }
        if (pMode == ScreenMode.Windowed && pResolution.x == MonoMain.instance._adapterW && pResolution.y == MonoMain.instance._adapterH && !pForce)
        {
            DevConsole.Log("Invalid resolution (" + pResolution.ToString() + "): Windowed resolution must not equal adapter size (" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + "x" + GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height + ")");
            return null;
        }
        Resolution existing = elements.FirstOrDefault((Resolution x) => x.x == pResolution.x && x.y == pResolution.y);
        if (existing == null)
        {
            elements.Add(pResolution);
        }
        else
        {
            pResolution = existing;
        }
        if (pSort)
        {
            supportedDisplaySizes[pMode] = SortDisplaySizes(elements);
        }
        pResolution.mode = pMode;
        pResolution.recommended = pRecommended;
        return pResolution;
    }

    internal static void SortDisplaySizes()
    {
        supportedDisplaySizes[ScreenMode.Windowed] = SortDisplaySizes(supportedDisplaySizes[ScreenMode.Windowed]);
        supportedDisplaySizes[ScreenMode.Borderless] = SortDisplaySizes(supportedDisplaySizes[ScreenMode.Borderless]);
        supportedDisplaySizes[ScreenMode.Fullscreen] = SortDisplaySizes(supportedDisplaySizes[ScreenMode.Fullscreen]);
    }

    internal static List<Resolution> SortDisplaySizes(List<Resolution> pList)
    {
        if (pList == null)
        {
            return new List<Resolution>();
        }
        return pList.OrderBy((Resolution x) => x.x * 100 + x.y).ToList();
    }
}
