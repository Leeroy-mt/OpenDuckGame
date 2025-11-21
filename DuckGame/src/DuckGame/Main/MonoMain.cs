using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL3;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuckGame;

public class MonoMain : Game
{
	public class WebCharData
	{
		public Tex2D image;

		public string name;

		public string quote;
	}

	public class MoonInfo
	{
		public double age;

		public double phase;
	}

	private static MonoMainCore _core = new MonoMainCore();

	public static TransitionDirection transitionDirection = TransitionDirection.None;

	public static Level transitionLevel = null;

	public static float transitionWait;

	public static RenderTarget2D _screenCapture;

	private static bool _didPauseCapture = false;

	private static bool _started = false;

	public static MonoMain instance;

	private GraphicsDeviceManager graphics;

	private static int _screenWidth = 1280;

	private static int _screenHeight = 720;

	public static bool _fullScreen = false;

    public static volatile List<string> LoadMessages = [];

	static float Progress;

    private SpriteMap _duckRun;

	private SpriteMap _duckArm;

	public static Thing thing;

	public static Thread mainThread;

	private bool _canStartLoading;

	public int _adapterW;

	public int _adapterH;

	private static List<Func<string>> _extraExceptionDetails = new List<Func<string>>
	{
		() => "Date: " + DateTime.UtcNow.ToString(DateTimeFormatInfo.InvariantInfo),
		() => "Version: " + DG.version,
		() => "Platform: " + DG.platform + " (Steam Build " + Program.steamBuildID + ")(" + (SFX.NoSoundcard ? "NO SFX" : "SFX") + ")",
		() => GetOnlineString(),
		() => "Mods: " + ModLoader.modHash,
		() => "Time Played: " + TimeString(DateTime.Now - startTime) + " (" + Graphics.frame + ")",
		() => "Special Code: " + Main.SpecialCode + " " + Main.SpecialCode2,
		() => "Resolution: (A)" + Resolution.adapterResolution.x + "x" + Resolution.adapterResolution.y + " (G)" + Resolution.current.x + "x" + Resolution.current.y + (Options.Data.fullscreen ? (" (Fullscreen(" + (Options.Data.windowedFullscreen ? "W" : "H") + "))") : " (Windowed)") + "(RF " + framesSinceFocusChange + ")",
		() => "Level: " + GetLevelString(),
		() => "Command Line: " + Program.commandLine
	};

	private static string kCleanupString = "C:\\gamedev\\duckgame_try2\\duckgame\\DuckGame\\src\\";

	public static int timeInMatches;

	public static int timeInArcade;

	public static int timeInEditor;

	public static DateTime startTime;

	private RenderTarget2D saveShot;

	public static bool hidef = false;

	public static string[] startupAssemblies;

	public static bool notOnlineError = false;

	public static int cultureCode;

	public static bool fourK;

	public static string infiniteLoopDetails;

	public static bool hadInfiniteLoop;

	private static Stopwatch _loopTimer = new Stopwatch();

	public bool lastCanSyncFramerateVal;

	public bool lastWindowedFullscreenSetting;

	public static bool infiniteLoopDebug = false;

	public static bool atPostCloudLogic = false;

	private IGraphicsDeviceService graphicsService;

	public static bool closingGame = false;

	private Thread _infiniteLoopDetector;

	private bool _killedEverything;

	public static Queue<LoadingAction> currentActionQueue;

	private static Queue<LoadingAction> _thingsToLoad = new Queue<LoadingAction>();

	public static bool cancelLazyLoad = false;

	private static Thread _lazyLoadThread;

	public static Queue<Action> lazyLoadActions = new Queue<Action>();

	public static string lobbyPassword = "";

	public static int forceFullscreenMode = 0;

	public static bool moddingEnabled = true;

	public static bool nomodsMode = false;

	public static bool enableThreadedLoading = true;

	public static bool defaultControls = false;

	public static bool oldDefaultControls = false;

	public static bool noFullscreen = false;

	public static bool lostsave = false;

	/// <summary>
	/// deprecated- this variable does nothing.
	/// </summary>
	public static bool cloudNoSave = false;

	/// <summary>
	/// deprecated- this variable does nothing.
	/// </summary>
	public static bool cloudNoLoad = false;

	/// <summary>
	/// deprecated- this variable does nothing.
	/// </summary>
	public static bool disableCloud = false;

	public static bool disableGraphics = true;

	public static bool noConnectionTimeout = false;

	public static bool logFileOperations = false;

	public static bool logLevelOperations = false;

	public static bool recoversave = false;

	public static bool noHidef;

	public static bool oldAngles;

	public static bool alternateFullscreen = false;

	/// <summary>
	/// deprecated
	/// </summary>
	public static bool alternateAudioMode = false;

	/// <summary>
	/// deprecated
	/// </summary>
	public static bool directAudio = false;

	public static bool networkDebugger = false;

	public static bool disableSteam = false;

	public static bool noIntro = false;

	public static bool startInEditor = false;

	public static bool preloadModContent = true;

	public static bool breakSteam = false;

	public static bool modDebugging = false;

	public static bool launchedFromSteam = false;

	public static bool steamConnectionCheckFail = false;

	public static AudioMode audioModeOverride = AudioMode.None;

	private bool _threadedLoadingStarted;

	private static Thread _initializeThread;

	private static Task _initializeTask;

	private static List<WorkshopItem> availableModsToDownload = new List<WorkshopItem>();

	public static bool editSave = false;

	public static bool downloadWorkshopMods = false;

	public static bool disableDirectInput = false;

	public static bool dinputNoTimeout = false;

	private bool _doStart;

	public static int framesSinceFocusChange;

	public static int loseDevice = 0;

	public static bool _didReceiptCheck = false;

	public static bool _recordingStarted = false;

	public static bool _recordData = false;

	public int times;

	public static long framesBackInFocus = 0L;

	public static bool showingSaveTool = false;

	public static Form saveTool;

	public static bool closedCorruptSaveDialog = false;

	public static bool closedNoSpaceDialog = false;

	public static bool shouldPauseGameplay;

	public static bool exit = false;

	public static volatile bool pause = false;

	public static volatile bool paused = false;

	public bool _didInitialVsyncUpdate;

	public static bool specialSync;

	public static string modMemoryOffendersString = "";

	public static List<ModConfiguration> loadedModsWithAssemblies = new List<ModConfiguration>();

	private Timer _loadTimer = new Timer();

	private Timer _waitToStartLoadingTimer = new Timer();

	private bool _loggedConnectionCheckFailure;

	private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667L);

	private static double LowestSleepThreshold = 0.0;

	private long _previousTicks;

	private TimeSpan _accumulatedElapsedTime = TimeSpan.Zero;

	private bool _setCulture;

	public static bool autoPauseFade = true;

	private static MaterialPause _pauseMaterial;

	private bool _didFirstDraw;

	private static Recording _tempRecordingReference;

	private RenderTarget2D _screenshotTarget;

	private int _numShots;

	private int _loadingFramesRendered;

	private int waitFrames;

	private bool takingShot;

	public static bool doPauseFade = true;

	public static volatile int loadyBits = 0;

	public static volatile int totalLoadyBits = 365;

	private Timer _timeSinceLastLoadFrame = new Timer();

	private int deviceLostWait;

	int Frames;

	public static MonoMainCore core
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

	private static UIComponent _pauseMenu
	{
		get
		{
			return _core._pauseMenu;
		}
		set
		{
			_core._pauseMenu = value;
		}
	}

	public static UIComponent pauseMenu
	{
		get
		{
			if (_pauseMenu != null && !_pauseMenu.inWorld && !_pauseMenu.open)
			{
				return null;
			}
			return _pauseMenu;
		}
		set
		{
			if (_pauseMenu != value && _pauseMenu != null && _pauseMenu.open && !_pauseMenu.inWorld)
			{
				_pauseMenu.Close();
			}
			_pauseMenu = value;
		}
	}

	public static List<UIComponent> closeMenuUpdate => _core.closeMenuUpdate;

	public static RenderTarget2D screenCapture => _screenCapture;

	public static bool started => _started;

	public static int screenWidth => _screenWidth;

	public static int screenHeight => _screenHeight;

	public static int windowWidth => (int)Math.Round((float)screenWidth * Options.GetWindowScaleMultiplier());

	public static int windowHeight => (int)Math.Round((float)screenHeight * Options.GetWindowScaleMultiplier());

	public string screenMode
	{
		get
		{
			return Resolution.current.mode.ToString();
		}
		set
		{
		}
	}

	public bool canSyncFramerateWithVSync => Options.Data.vsync;

	public static Thread lazyLoadThread => _lazyLoadThread;

	public static Thread initializeThread => _initializeThread;

	public static Task initializeTask => _initializeTask;

	public static bool closeMenus
	{
		get
		{
			return _core.closeMenus;
		}
		set
		{
			_core.closeMenus = value;
		}
	}

	public static bool menuOpenedThisFrame
	{
		get
		{
			return _core.menuOpenedThisFrame;
		}
		set
		{
			_core.menuOpenedThisFrame = value;
		}
	}

	public static bool dontResetSelection
	{
		get
		{
			return _core.dontResetSelection;
		}
		set
		{
			_core.dontResetSelection = value;
		}
	}

	public static MaterialPause pauseMaterial => _pauseMaterial;

	public static bool FullMoon
	{
		get
		{
			DateTime moonTime = new DateTime(1900, 1, 1);
			double phase = (DateTime.UtcNow - moonTime).TotalDays % 29.530588853;
			if (DateTime.Now.Hour < 1 && phase > 13.0 && phase < 17.0)
			{
				return true;
			}
			return false;
		}
	}

    public static int MaximumGamepadCount = 4;

    public bool IsFocused
    {
        get => ((uint)SDL.SDL_GetWindowFlags(Window.Handle) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) > 0;
    }
    public static void RegisterEngineUpdatable(IEngineUpdatable pUpdatable)
	{
		core.engineUpdatables.Add(pUpdatable);
	}

	public static string GetOnlineString()
	{
		if (Network.isActive)
		{
			return "Online: 1 (" + (Network.activeNetwork.core.isServer ? "H" : "C") + "," + Network.activeNetwork.core.averagePing + ")\r\n";
		}
		return "Online: 0";
	}

	public static string GetLevelString()
	{
		if (Level.current == null)
		{
			return "null";
		}
		if (Level.current is XMLLevel)
		{
			XMLLevel lev = Level.current as XMLLevel;
			if (lev.level == "RANDOM")
			{
				return "RANDOM";
			}
			if (lev.data == null)
			{
				return Level.current.GetType().ToString();
			}
			return lev.data.GetPath();
		}
		return Level.current.GetType().ToString();
	}

	public static string GetExceptionString(UnhandledExceptionEventArgs e)
	{
		return GetExceptionString(e.ExceptionObject);
	}

	public static string GetExceptionString(object e)
	{
		string error = Program.ProcessExceptionString(e as Exception) + "\r\n";
		error = error.Replace(kCleanupString, "");
		try
		{
			DevConsole.FlushPendingLines();
			if (DevConsole.core.lines.Count > 0)
			{
				error += "Last 8 Lines of Console Output:\r\n";
				for (int i = 8; i >= 1; i--)
				{
					if (DevConsole.core.lines.Count - i >= 0)
					{
						DCLine line = DevConsole.core.lines.ElementAt(DevConsole.core.lines.Count - i);
						try
						{
							string er = line.line;
							string formatted = "";
							for (int l = 0; l < er.Length; l++)
							{
								if (er[l] == '|')
								{
									for (l++; l < er.Length && er[l] != '|'; l++)
									{
									}
									l++;
								}
								if (l < er.Length)
								{
									formatted += er[l];
								}
							}
							error = error + formatted + "\r\n";
						}
						catch (Exception)
						{
							error = error + line.line + "\r\n";
						}
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return error + GetDetails();
	}

	public static string GetDetails()
	{
		string error = "";
		foreach (Func<string> func in _extraExceptionDetails)
		{
			string add = "FIELD FAILED";
			try
			{
				add = func();
			}
			catch
			{
			}
			error += "\r\n";
			error += add;
		}
		return error;
	}

	/// <summary>
	/// Gives the local time zone's time. Unfortunately, MonoGame/Brute does not completely account for time zones on Switch for DateTime.Now.
	/// </summary>
	/// <returns>Time zone adjusted for local time.</returns>
	public static DateTime GetLocalTime()
	{
		return DateTime.Now;
	}

	public static string RequestCape(string data)
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(string.Format("http://www.wonthelp.info/DuckWeb/getCape.php?sendRequest=IWannaUseADangOlCape&id=" + data));
			obj.Credentials = CredentialCache.DefaultCredentials;
			HttpWebResponse response = (HttpWebResponse)obj.GetResponse();
			string stringResponse = "";
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using Stream dataStream = response.GetResponseStream();
				using StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
				stringResponse = reader.ReadToEnd();
			}
			response.Close();
			return stringResponse;
		}
		catch
		{
		}
		return "";
	}

	public static byte[] StringToByteArrayFastest(string hex)
	{
		if (hex.Length % 2 == 1)
		{
			throw new Exception("The binary key cannot have an odd number of digits");
		}
		byte[] arr = new byte[hex.Length >> 1];
		for (int i = 0; i < hex.Length >> 1; i++)
		{
			arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
		}
		return arr;
	}

	public static int GetHexVal(char hex)
	{
		return hex - ((hex < ':') ? 48 : 55);
	}

	public static byte[] StringToByteArray(string hex)
	{
		return (from x in Enumerable.Range(0, hex.Length)
			where x % 2 == 0
			select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
	}

	public static Texture2D RequestRandomDoodle()
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(string.Format("http://www.wonthelp.info/crappydoodle/getTotallyRandomImage2.php?sendRequest=crappyDoodles&id=" + Rando.Int(112215)));
			obj.Credentials = CredentialCache.DefaultCredentials;
			HttpWebResponse response = (HttpWebResponse)obj.GetResponse();
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (Stream dataStream = response.GetResponseStream())
				{
					using StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
					string[] parts = reader.ReadToEnd().Split('=');
					byte[] buffer = StringToByteArray(parts[1].Split('&')[0]);
					_ = parts[2];
					return ContentPack.LoadTexture2DFromStream(new MemoryStream(buffer), processPink: false);
				}
			}
			response.Close();
			return null;
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static WebCharData RequestRandomCharacter()
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(string.Format("http://www.wonthelp.info/mangaka/getTotallyRandomCharacter.php?sendRequest=charzone&id=" + Rando.Int(464)));
			obj.Credentials = CredentialCache.DefaultCredentials;
			HttpWebResponse response = (HttpWebResponse)obj.GetResponse();
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (Stream dataStream = response.GetResponseStream())
				{
					using StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
					string[] parts = reader.ReadToEnd().Split('&');
					string namee = parts[0].Split('=')[1];
					string quotee = parts[2].Split('=')[1];
					string swapped = parts[1].Split('=')[1].Replace('|', '+');
					int mod4 = swapped.Length % 4;
					if (mod4 > 0)
					{
						swapped += new string('=', 4 - mod4);
					}
					byte[] bytes = Convert.FromBase64String(swapped);
					Tex2D tex = new Tex2D(128, 128);
					Color[] colors = new Color[16384];
					int index = 0;
					for (int i = 0; i < bytes.Count(); i++)
					{
						if (index >= colors.Count())
						{
							break;
						}
						byte b = bytes[i];
						for (int j = 0; j < 8; j++)
						{
							if ((b & 0x80) != 0)
							{
								colors[index] = Color.Black;
							}
							else
							{
								colors[index] = Color.White;
							}
							b <<= 1;
							index++;
						}
					}
					tex.SetData(colors);
					return new WebCharData
					{
						image = tex,
						name = namee,
						quote = quotee
					};
				}
			}
			response.Close();
			return null;
		}
		catch (Exception)
		{
		}
		return null;
	}

	public void SaveShot()
	{
		Thread thread = new Thread(SaveShotThread);
		thread.CurrentCulture = CultureInfo.InvariantCulture;
		thread.Priority = ThreadPriority.BelowNormal;
		thread.IsBackground = true;
		thread.Start();
	}

	public void SaveShotThread()
	{
		RenderTarget2D shotToSave = saveShot;
		string d = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + " " + _numShots;
		_numShots++;
		d = d.Replace("/", "_");
		d = d.Replace(":", "-");
		d = d.Replace(" ", "");
		if (!Directory.Exists("screenshots"))
		{
			Directory.CreateDirectory("screenshots");
		}
		FileStream f = File.OpenWrite("screenshots/duckscreen-" + d + ".png");
		(shotToSave.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D).SaveAsPng(f, shotToSave.width, shotToSave.height);
		f.Close();
	}

	public static string TimeString(TimeSpan span, int places = 3, bool small = false)
	{
		if (!small)
		{
			return ((places > 2) ? (((span.Hours < 10) ? ("0" + Change.ToString(span.Hours)) : Change.ToString(span.Hours)) + ":") : "") + ((places > 1) ? (((span.Minutes < 10) ? ("0" + Change.ToString(span.Minutes)) : Change.ToString(span.Minutes)) + ":") : "") + ((span.Seconds < 10) ? ("0" + Change.ToString(span.Seconds)) : Change.ToString(span.Seconds));
		}
		int milis = (int)((float)span.Milliseconds / 1000f * 99f);
		return ((places > 2) ? (((span.Minutes < 10) ? ("0" + Change.ToString(span.Minutes)) : Change.ToString(span.Minutes)) + ":") : "") + ((places > 1) ? (((span.Seconds < 10) ? ("0" + Change.ToString(span.Seconds)) : Change.ToString(span.Seconds)) + ":") : "") + ((milis < 10) ? ("0" + Change.ToString(milis)) : Change.ToString(milis));
	}

	private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
		if (!noHidef && e.GraphicsDeviceInformation.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
		{
			e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
			hidef = true;
		}
		else
		{
			e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.Reach;
		}
		e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PlatformContents;
	}

	public MonoMain()
	{
		mainThread = Thread.CurrentThread;
		cultureCode = CultureInfo.CurrentCulture.LCID;
		startupAssemblies = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			where !assembly.IsDynamic
			select assembly.Location).ToArray();
		base.Content = new SynchronizedContentManager(base.Services);
		DG.SetVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString());
		graphics = new GraphicsDeviceManager(this);
		graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
		base.Content.RootDirectory = "Content";
		_adapterW = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
		_adapterH = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
		if (_adapterW >= 2048 || _adapterH >= 2048)
		{
			fourK = true;
		}
		int defaultWidth = 1280;
		if (defaultWidth > _adapterW)
		{
			defaultWidth = 1024;
		}
		if (defaultWidth > _adapterW)
		{
			defaultWidth = 640;
		}
		if (defaultWidth > _adapterW)
		{
			defaultWidth = 320;
		}
		if (_adapterW > 1920)
		{
			defaultWidth = 1920;
		}
		float aspect = (float)_adapterH / (float)_adapterW;
		if (aspect < 0.56f)
		{
			aspect = 0.5625f;
			_adapterH = (int)((float)_adapterW * aspect);
		}
		int defaultHeight = (int)(aspect * (float)defaultWidth);
		if (defaultHeight > 1200)
		{
			defaultHeight = 1200;
		}
		_screenWidth = defaultWidth;
		_screenHeight = defaultHeight;
		DuckFile.Initialize();
		Options.Load();
		Cloud.Initialize();
		instance = this;
		Resolution.Initialize(Window.Handle, graphics);
		Options.Load();
		Options.PostLoad();
		if (noFullscreen)
		{
			Options.LocalData.currentResolution = Options.LocalData.windowedResolution;
		}
		Graphics.InitializeBase(graphics, screenWidth, screenHeight);
		_waitToStartLoadingTimer.Start();
	}

	public static void ResetInfiniteLoopTimer()
	{
		_loopTimer.Reset();
	}

	public void InfiniteLoopDetector()
	{
		while (_infiniteLoopDetector != null)
		{
			Thread.Sleep(40);
			if (!started || !Graphics.inFocus)
			{
				ResetInfiniteLoopTimer();
			}
			if (!(_loopTimer.Elapsed.TotalSeconds > 5.0))
			{
				continue;
			}
			try
			{
				mainThread.Suspend();
				infiniteLoopDetails = "Infinite loop crash: ";
				try
				{
					infiniteLoopDetails += GetInfiniteLoopDetails();
				}
				catch (Exception)
				{
				}
				hadInfiniteLoop = true;
				mainThread.Resume();
				mainThread.Abort(new Exception(infiniteLoopDetails));
			}
			catch (Exception ex2)
			{
				throw ex2;
			}
		}
	}

	public static string GetInfiniteLoopDetails()
	{
		string stackString = new StackTrace(true).ToString();
		int idx = stackString.IndexOf("at Microsoft.Xna.Framework.Game.Tick");
		if (idx >= 0)
		{
			return stackString.Substring(0, idx);
		}
		return stackString;
	}

	protected override void LoadContent()
	{
		base.LoadContent();
	}

	protected override void Initialize()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		base.IsFixedTimeStep = true;
		Graphics.mouseVisible = false;
		base.Initialize();
		DuckGame.Content.InitializeBase(base.Content);
		Curve.System_Initialize();
		Rando.DoInitialize();
		NetRand.Initialize();
		base.InactiveSleepTime = new TimeSpan(0L);
		Graphics.Initialize(base.GraphicsDevice);
		Resolution.Set(Options.LocalData.currentResolution);
		Resolution.Apply();
		_screenCapture = new RenderTarget2D(Resolution.current.x, Resolution.current.y, pdepth: true);
		_duckRun = new SpriteMap("duck", 32, 32);
		_duckRun.AddAnimation("run", 1f, true, 1, 2, 3, 4, 5, 6);
		_duckRun.SetAnimation("run");
		_duckArm = new SpriteMap("duckArms", 16, 16);
		graphicsService = base.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
		Graphics.device.DeviceLost += DeviceLost;
		Graphics.device.DeviceResetting += DeviceResetting;
		Graphics.device.DeviceReset += DeviceReset;
		graphicsService.DeviceCreated += delegate
		{
			OnDeviceCreated();
		};
		if (infiniteLoopDebug)
		{
			_infiniteLoopDetector = new Thread(InfiniteLoopDetector);
			_infiniteLoopDetector.CurrentCulture = CultureInfo.InvariantCulture;
			_infiniteLoopDetector.Priority = ThreadPriority.Lowest;
			_infiniteLoopDetector.IsBackground = true;
			_infiniteLoopDetector.Start();
			_loopTimer.Start();
		}
		_canStartLoading = true;
	}

	private void PostCloudLogic()
	{
		atPostCloudLogic = true;
		DGSave.Initialize();
		Global.Initialize();
		Layer.InitializeLayers();
	}

	private void OnDeviceCreated()
	{
		Graphics.device = graphicsService.GraphicsDevice;
	}

	public void KillEverything()
	{
		closingGame = true;
		if (_killedEverything)
		{
			return;
		}
		DevConsole.Log(DCSection.General, "|DGRED|-----------KillEverything()-----------");
		_killedEverything = true;
		try
		{
			if (!Program.crashed)
			{
				Global.Save();
				Options.Save();
				Options.SaveLocalData();
				if (Network.isActive)
				{
					for (int i = 0; i < 5; i++)
					{
						Send.ImmediateUnreliableBroadcast(new NMClientClosedGame());
						Send.ImmediateUnreliableBroadcast(new NMClientClosedGame());
						Steam.Update();
						Thread.Sleep(16);
					}
				}
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Music.Terminate();
			cancelLazyLoad = true;
		}
		catch
		{
		}
		try
		{
			if (_lazyLoadThread != null && _lazyLoadThread.IsAlive)
			{
				_lazyLoadThread.Abort();
			}
			if (_initializeThread != null && _initializeThread.IsAlive)
			{
				_initializeThread.Abort();
			}
		}
		catch
		{
		}
		try
		{
			NetworkDebugger.TerminateThreads();
			Network.Terminate();
			Input.Terminate();
		}
		catch
		{
		}
		try
		{
			DeviceChangeNotifier.Stop();
		}
		catch
		{
		}
		try
		{
			while (Cloud.processing)
			{
				Cloud.Update();
			}
			Steam.Terminate();
		}
		catch (Exception)
		{
		}
		try
		{
			if (logFileOperations)
			{
				DevConsole.Log(DCSection.General, "Logging file operations finished.");
				DevConsole.SaveNetLog("duck_file_log.rtf");
			}
		}
		catch (Exception)
		{
		}
		if (_infiniteLoopDetector != null)
		{
			_infiniteLoopDetector = null;
		}
	}

	protected override void OnExiting(object sender, EventArgs args)
	{
		KillEverything();
		Process.GetCurrentProcess().Kill();
	}

	public static void FinishLazyLoad()
	{
		while (lazyLoadActions.Count > 0)
		{
			lazyLoadActions.Dequeue()();
		}
	}

	private static void ResultFetched(object value0, WorkshopQueryResult result)
	{
		if (result != null && result.details != null)
		{
			WorkshopItem item = result.details.publishedFile;
			int num = DuckFile.GetFiles(item.path).Count();
			int numDirectories = DuckFile.GetDirectories(item.path).Count();
			if ((num == 0 && numDirectories == 0) || (item.stateFlags & WorkshopItemState.Installed) == 0 || (item.stateFlags & WorkshopItemState.NeedsUpdate) != WorkshopItemState.None)
			{
				availableModsToDownload.Add(item);
			}
		}
	}

	internal void DownloadWorkshopItems()
	{
		//loadMessage = "Downloading workshop mods...";
		if (!Steam.IsInitialized())
		{
			return;
		}
		LoadingAction steamLoad = new LoadingAction();
		steamLoad.action = delegate
		{
			WorkshopQueryUser workshopQueryUser = Steam.CreateQueryUser(Steam.user.id, WorkshopList.Subscribed, WorkshopType.UsableInGame, WorkshopSortOrder.TitleAsc);
			workshopQueryUser.requiredTags.Add("Mod");
			workshopQueryUser.onlyQueryIDs = true;
			workshopQueryUser.QueryFinished += delegate
			{
				steamLoad.flag = true;
			};
			workshopQueryUser.ResultFetched += ResultFetched;
			workshopQueryUser.Request();
			Steam.Update();
		};
		steamLoad.waitAction = delegate
		{
			Steam.Update();
			return steamLoad.flag;
		};
		_thingsToLoad.Enqueue(steamLoad);
		steamLoad = new LoadingAction();
		steamLoad.action = delegate
		{
			totalLoadyBits = availableModsToDownload.Count;
			loadyBits = 0;
			foreach (WorkshopItem u in availableModsToDownload)
			{
				LoadingAction itemDownload = new LoadingAction();
				itemDownload.action = delegate
				{
					//loadMessage = "Downloading workshop mods (" + loadyBits + "/" + totalLoadyBits + ")";
					if (Steam.DownloadWorkshopItem(u))
					{
						itemDownload.context = u;
					}
					loadyBits++;
				};
				itemDownload.waitAction = delegate
				{
					Steam.Update();
					return (u == null || u.finishedProcessing) ? true : false;
				};
				steamLoad.actions.Enqueue(itemDownload);
			}
		};
		steamLoad.waitAction = delegate
		{
			Steam.Update();
			return steamLoad.flag;
		};
		_thingsToLoad.Enqueue(steamLoad);
	}

	Startup Startup;

    private void StartThreadedLoading()
	{
		Startup = new(this);

		_threadedLoadingStarted = true;
		currentActionQueue = _thingsToLoad;

		Startup.RunMainTasks();
		Startup.RunAsyncTasks();
    }

    internal void SetStarted()
	{
		_doStart = true;
    }

	private void Start()
	{
		ModLoader.PostLoadMods();
		OnStart();
		_started = true;
	}

	protected virtual void OnStart()
	{
	}

	protected override void UnloadContent()
	{
	}

	public static void StartRecording(string name)
	{
		_recordingStarted = true;
		_recordData = true;
		_ = _recordData;
	}

	public static void StartPlayback()
	{
		_recordingStarted = true;
		_recordData = false;
	}

	public static void StopRecording()
	{
		_recordingStarted = false;
	}

	private void DeviceLost(object obj, EventArgs args)
	{
		loseDevice = 1;
		SynchronizedContentManager.blockLoading = 2;
	}

	private void DeviceResetting(object obj, EventArgs args)
	{
		loseDevice = 1;
		SynchronizedContentManager.blockLoading = 2;
	}

	private void DeviceReset(object obj, EventArgs args)
	{
		loseDevice = 1;
		SynchronizedContentManager.blockLoading = 2;
	}

	protected override void Update(GameTime gameTime)
	{
		Frames++;

        if (showingSaveTool && saveTool == null && File.Exists("SaveTool.dll"))
		{
			saveTool = Activator.CreateInstance(Assembly.Load(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\SaveTool.dll")).GetType("SaveRecovery.SaveTool")) as Form;
			Graphics.mouseVisible = true;
			saveTool.ShowDialog();
			Program.crashed = true;
			Application.Exit();
		}
		if (Program.isLinux)
		{
			if (IsActive)
			{
				framesBackInFocus++;
				Graphics.mouseVisible = showingSaveTool;
			}
			else
			{
				framesBackInFocus = 0L;
				Graphics.mouseVisible = true;
			}
		}
		else if (IsActive && IsFocused)
		{
			framesBackInFocus++;
			Graphics.mouseVisible = showingSaveTool;
		}
		else
		{
			framesBackInFocus = 0L;
			Graphics.mouseVisible = true;
		}
		if (GraphicsDevice.IsDisposed)
		{
			base.Update(gameTime);
			return;
		}
		try
		{
			_loopTimer.Restart();
			RunUpdate(gameTime);
		}
		catch (Exception pException)
		{
			Program.HandleGameCrash(pException);
		}
	}

	public static void UpdatePauseMenu(bool hasFocus = true)
	{
		shouldPauseGameplay = true;
		if (Network.isActive && UIMatchmakerMark2.instance == null && (!Network.InLobby() || !(Level.current as TeamSelect2).MatchmakerOpen()))
		{
			shouldPauseGameplay = false;
		}
		if (_pauseMenu != null)
		{
			if (shouldPauseGameplay)
			{
				HUD.Update();
				_pauseMenu.Update();
				AutoUpdatables.MuteSounds();
			}
			else
			{
				_pauseMenu.Update();
				Input.ignoreInput = true;
			}
			if (_pauseMenu != null && !_pauseMenu.open)
			{
				_pauseMenu = null;
			}
		}
		else
		{
			shouldPauseGameplay = false;
		}
		for (int i = 0; i < closeMenuUpdate.Count; i++)
		{
			UIComponent uIComponent = closeMenuUpdate[i];
			uIComponent.Update();
			if (!uIComponent.animating)
			{
				closeMenuUpdate.RemoveAt(i);
				i--;
			}
		}
		menuOpenedThisFrame = false;
		dontResetSelection = false;
	}

	public static void RetakePauseCapture()
	{
		_didPauseCapture = false;
	}

	public static void CalculateModMemoryOffendersList()
	{
		List<ModConfiguration> list = loadedModsWithAssemblies.OrderByDescending((ModConfiguration x) => (x.content == null) ? (-1) : x.content.kilobytesPreAllocated).ToList();
		bool found = false;
		modMemoryOffendersString = "Mods taking up the most memory:\n";
		foreach (ModConfiguration m in list)
		{
			long allocated = m.content.kilobytesPreAllocated;
			if (allocated / 1000 > 20)
			{
				modMemoryOffendersString = modMemoryOffendersString + m.displayName + " (" + allocated / 1000 + "MB)(ID:" + m.workshopID + ")\n";
				found = true;
			}
		}
		modMemoryOffendersString += "\n";
		if (!found)
		{
			modMemoryOffendersString = "";
		}
	}

	public void RunUpdate(GameTime gameTime)
	{
		Graphics.frame++;
		Tasker.RunTasks();
		Graphics.GarbageDisposal(pLevelTransition: false);
		if (!disableSteam && !_started)
		{
			if (Cloud.processing)
			{
				Cloud.Update();
				return;
			}
			if (steamConnectionCheckFail)
			{
				if (_loggedConnectionCheckFailure)
				{
					_loggedConnectionCheckFailure = true;
					DevConsole.Log("|DGRED|Failed to initialize a connection to Steam.");
				}
			}
			else if (Steam.IsInitialized() && Steam.IsRunningInitializeProcedures())
			{
				//loadMessage = "Loading Steam";
				Steam.Update();
			}
		}
		if (_canStartLoading && !_threadedLoadingStarted && _didFirstDraw)
		{
			PostCloudLogic();
			StartThreadedLoading();
		}
		if (_doStart && !_started)
		{
			_doStart = false;
			Start();
		}
		if (!_started || Graphics.screenCapture != null)
		{
			return;
		}
		if (Graphics.inFocus)
		{
			Input.Update();
		}
		lock (LevelMetaData._completedPreviewTasks)
		{
			if (LevelMetaData._completedPreviewTasks.Count > 0)
			{
				foreach (LevelMetaData.SaveLevelPreviewTask task in LevelMetaData._completedPreviewTasks)
				{
					try
					{
						DuckFile.SaveString(task.levelString, task.savePath);
					}
					catch (Exception)
					{
					}
				}
				LevelMetaData._completedPreviewTasks.Clear();
			}
		}
		Cloud.Update();
		if (_started && !NetworkDebugger.enabled)
		{
			InputProfile.Update();
			Network.PreUpdate();
		}
		if (Keyboard.Pressed(Keys.F4) || (Keyboard.alt && Keyboard.Pressed(Keys.Enter)))
		{
			Options.Data.fullscreen = !Options.Data.fullscreen;
			Options.FullscreenChanged();
		}
		if (!Cloud.processing)
		{
			Steam.Update();
		}
		try
		{
			if (Keyboard.Pressed(Keys.F2))
			{
				Program.MakeNetLog();
			}
		}
		catch (Exception)
		{
		}
		if (exit || ((Keyboard.Down(Keys.LeftAlt) || Keyboard.Down(Keys.RightAlt)) && Keyboard.Down(Keys.F4)))
		{
			KillEverything();
			Exit();
			return;
		}
		TouchScreen.Update();
		if (!NetworkDebugger.enabled)
		{
			DevConsole.Update();
		}
		SFX.Update();
		Options.Update();
		InputProfile.repeat = Level.current is Editor || _pauseMenu != null || Editor.selectingLevel;
		Keyboard.repeat = Level.current is Editor || _pauseMenu != null || DevConsole.open || DuckNetwork.core.enteringText || Editor.enteringText;
		bool hasFocus = true;
		if (!NetworkDebugger.enabled)
		{
			UpdatePauseMenu(hasFocus);
		}
		else
		{
			shouldPauseGameplay = false;
		}
		if (transitionDirection != TransitionDirection.None)
		{
			if (transitionLevel != null)
			{
				Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
				if (Graphics.fade <= 0f)
				{
					Level.current = transitionLevel;
					transitionLevel = null;
					transitionDirection = TransitionDirection.None;
				}
			}
			else
			{
				Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.1f);
				if (Graphics.fade >= 1f)
				{
					transitionLevel = null;
					transitionDirection = TransitionDirection.None;
				}
			}
			shouldPauseGameplay = true;
		}
		RumbleManager.Update();
		if (!shouldPauseGameplay)
		{
			if (_pauseMenu == null)
			{
				_didPauseCapture = false;
			}
			if (!_recordingStarted || _recordData)
			{
				if (DevConsole.rhythmMode && Level.current is GameLevel)
				{
					TimeSpan s = Music.position;
					s += new TimeSpan(0, 0, 0, 0, 80);
					float bpm = 140f;
					RhythmMode.TickSound((float)(s.TotalMinutes * (double)bpm) % 1f / 1f);
					s = Music.position;
					s += new TimeSpan(0, 0, 0, 0, 40);
					bpm = 140f;
					RhythmMode.Tick((float)(s.TotalMinutes * (double)bpm) % 1f / 1f);
				}
				foreach (IEngineUpdatable engineUpdatable in core.engineUpdatables)
				{
					engineUpdatable.PreUpdate();
				}
				AutoUpdatables.Update();
				DuckGame.Content.Update();
				Music.Update();
				Level.UpdateLevelChange();
				Level.UpdateCurrentLevel();
				foreach (IEngineUpdatable engineUpdatable2 in core.engineUpdatables)
				{
					engineUpdatable2.Update();
				}
				OnUpdate();
			}
		}
		Graphics.RunRenderTasks();
		Input.ignoreInput = false;
		base.Update(gameTime);
		FPSCounter.Tick(0);
		if (!NetworkDebugger.enabled)
		{
			Network.PostUpdate();
		}
		foreach (IEngineUpdatable engineUpdatable3 in core.engineUpdatables)
		{
			engineUpdatable3.PostUpdate();
		}
	}

	protected virtual void OnUpdate()
	{
	}

	public static void RenderGame(RenderTarget2D target)
	{
		int w = Graphics.width;
		int height = Graphics.height;
		Graphics.SetRenderTarget(target);
		Viewport vp = default(Viewport);
		int x = (vp.Y = 0);
		vp.X = x;
		vp.Width = target.width;
		vp.Height = target.height;
		vp.MinDepth = 0f;
		vp.MaxDepth = 1f;
		Graphics.viewport = vp;
		Graphics.width = target.width;
		Graphics.height = target.height;
		Level.DrawCurrentLevel();
		Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
		instance.OnDraw();
		Graphics.screen.End();
		Graphics.width = w;
		Graphics.height = height;
		Graphics.SetRenderTarget(null);
	}

	[DllImport("ntdll.dll", SetLastError = true)]
	private static extern int NtQueryTimerResolution(out int MinimumResolution, out int MaximumResolution, out int CurrentResolution);

	/// <summary>
	/// Returns the current timer resolution in 100ns units
	/// </summary>
	public static int GetCurrentResolution()
	{
		NtQueryTimerResolution(out var _, out var _, out var current);
		return current;
	}

	/// <summary>
	/// Sleeps as long as possible without exceeding the specified period
	/// </summary>
	public static void SleepForNoMoreThan(double milliseconds)
	{
		if (LowestSleepThreshold == 0.0)
		{
			NtQueryTimerResolution(out var min, out var max, out var current);
			LowestSleepThreshold = 1.0 + (double)max / 10000.0;
			DevConsole.Log(DCSection.General, "TIMER RES(" + min + ", " + max + ", " + current + ")");
		}
		if (!(milliseconds < LowestSleepThreshold))
		{
			int sleepTime = (int)(milliseconds - (double)GetCurrentResolution());
			if (sleepTime >= 1)
			{
				Thread.Sleep(sleepTime);
			}
		}
	}

	private static int JulianDate(int d, int m, int y)
	{
		int yy = y - (12 - m) / 10;
		int mm = m + 9;
		if (mm >= 12)
		{
			mm -= 12;
		}
		int num = (int)(365.25 * (double)(yy + 4712));
		int k2 = (int)(30.6001 * (double)mm + 0.5);
		int k3 = (int)((double)(yy / 100 + 49) * 0.75) - 38;
		int j = num + k2 + d + 59;
		if (j > 2299160)
		{
			j -= k3;
		}
		return j;
	}

	private static MoonInfo MoonPhase(int d, int m, int y)
	{
		MoonInfo inf = new MoonInfo();
		int j = JulianDate(d, m, y);
		inf.phase = ((double)j + 4.867) / 29.53059;
		inf.phase -= Math.Floor(inf.phase);
		if (inf.phase < 0.5)
		{
			inf.age = inf.phase * 29.53059 + 14.765295;
		}
		else
		{
			inf.age = inf.phase * 29.53059 - 14.765295;
		}
		inf.age = Math.Floor(inf.age) + 1.0;
		return inf;
	}

	protected override void Draw(GameTime gameTime)
	{
		_ = started;
		framesSinceFocusChange++;
		if (loseDevice > 0)
		{
			Graphics.Clear(Color.Black);
			base.GraphicsDevice.SetRenderTarget(null);
			loseDevice--;
			base.Draw(gameTime);
		}
		else
		{
			if (base.GraphicsDevice.IsDisposed)
			{
				return;
			}
			Graphics.drawing = true;
			SynchronizedContentManager.blockLoading--;
			if (SynchronizedContentManager.blockLoading < 0)
			{
				SynchronizedContentManager.blockLoading = 0;
			}
			try
			{
				Graphics.device = base.GraphicsDevice;
				if (Resolution.Update())
				{
					Graphics.Clear(Color.Black);
					base.GraphicsDevice.SetRenderTarget(null);
					base.Draw(gameTime);
					Graphics.drawing = false;
					return;
				}
				RunDraw(gameTime);
				Graphics.SetRenderTargetToScreen();
				if (Graphics._screenBufferTarget != null)
				{
					_tempRecordingReference = Recorder.currentRecording;
					Recorder.currentRecording = null;
					Graphics.SetScreenTargetViewport();
					Graphics.Clear(Color.Black);
					Camera c = new Camera(0f, 0f, Graphics._screenBufferTarget.width, Graphics._screenBufferTarget.height);
					Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, c.getMatrix());
					Graphics.Draw(Graphics._screenBufferTarget, 0f, 0f);
					Graphics.screen.End();
					Recorder.currentRecording = _tempRecordingReference;
				}
				Graphics.UpdateScreenViewport();
				base.GraphicsDevice.SetRenderTarget(null);
				base.Draw(gameTime);
				Graphics.drawing = false;
			}
			catch (Exception pException)
			{
				Program.HandleGameCrash(pException);
			}
		}
	}

	protected void RunDraw(GameTime gameTime)
	{
		FPSCounter.Tick(1);
		_didFirstDraw = true;
		Graphics.frameFlipFlop = !Graphics.frameFlipFlop;
		if (Graphics.device.IsDisposed)
		{
			return;
		}
		Graphics.SetScissorRectangle(new Rectangle(0f, 0f, Graphics.width, Graphics.height));
		if (Recorder.currentRecording != null)
		{
			Recorder.currentRecording.NextFrame();
		}
		if (!_started)
		{
			_loadingFramesRendered++;
			Graphics.SetRenderTarget(null);
			_pauseMaterial = new MaterialPause();
			if (!_setCulture)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				_setCulture = true;
			}
			Graphics.Clear(new Color(0, 0, 0));
			Camera c = new Camera(0f, 0f, Graphics.width, Graphics.height);
			Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, c.getMatrix());
			Vec2 loadBarPos = new Vec2(50f, Graphics.height - 50);
			Vec2 loadBarSize = new Vec2(Graphics.width - 100, 20f);
			Graphics.DrawRect(loadBarPos, loadBarPos + loadBarSize, Color.DarkGray * 0.1f, 0.5f);
			float loaded = (float)loadyBits / (float)totalLoadyBits;
			if (loaded > 1f)
			{
				loaded = 1f;
			}

			var actualProgress = Startup?.CalculateProgress() ?? 0;
			Progress += (actualProgress - Progress) * 0.05f;
            Graphics.DrawRect(loadBarPos, loadBarPos + new Vec2(loadBarSize.x * Progress, loadBarSize.y), Color.White * 0.3f, 0.6f);
			//string message = loadMessage;
			if (Cloud.processing && Cloud.progress != 0f && Cloud.progress != 1f)
			{
				//message = "Synchronizing Steam Cloud... (" + (int)(Cloud.progress * 100f) + "%)";
			}
			for (int i = 0; i < LoadMessages.Count; i++)
			{
				Graphics.DrawString(LoadMessages[LoadMessages.Count - i - 1], loadBarPos + new Vec2(0, -24 - i * 24), Color.White, 1f, null, 2f);
            }
            _duckRun.speed = 0.15f;
			_duckRun.scale = new Vec2(4f, 4f);
			_duckRun.depth = 0.7f;
			_duckRun.color = new Color(80, 80, 80);
			{
				_duckRun.frame = (int)(Frames / 5f) % 6;
			}
			Vec2 duckPos = new Vec2(Graphics.width - _duckRun.width * 4 - 50, Graphics.height - _duckRun.height * 4 - 55);
			Graphics.Draw(_duckRun, duckPos.x, duckPos.y);
			_duckArm.frame = _duckRun.imageIndex;
			_duckArm.scale = new Vec2(4f, 4f);
			_duckArm.depth = 0.6f;
			_duckArm.color = new Color(80, 80, 80);
			Graphics.Draw(_duckArm, duckPos.x + 20f, duckPos.y + 56f);
			Graphics.screen.End();
			_timeSinceLastLoadFrame.Restart();
		}
		else
		{
			if (Level.current == null)
			{
				return;
			}
			Reflection.Render();
			if (!takingShot)
			{
				takingShot = true;
				if ((Keyboard.shift && Keyboard.Pressed(Keys.F12)) || waitFrames < 0)
				{
					if (_screenshotTarget == null)
					{
						_screenshotTarget = new RenderTarget2D(Graphics.width, Graphics.height, pdepth: true);
					}
					Graphics.screenCapture = _screenshotTarget;
					RunDraw(gameTime);
					waitFrames = 60 + Rando.Int(60);
					SFX.Play("ching");
				}
				takingShot = false;
			}
			if (_pauseMenu != null && !NetworkDebugger.enabled && !_didPauseCapture)
			{
				Graphics.screenCapture = _screenCapture;
				_didPauseCapture = true;
			}
			if (Graphics.screenCapture != null)
			{
				int w = Graphics.width;
				int height = Graphics.height;
				Graphics.SetRenderTarget(Graphics.screenCapture);
				Graphics.UpdateScreenViewport(pForceReset: true);
				HUD.hide = true;
				Level.DrawCurrentLevel();
				Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
				OnDraw();
				Graphics.screen.End();
				HUD.hide = false;
				Graphics.screenCapture = null;
				Graphics.width = w;
				Graphics.height = height;
				Graphics.SetRenderTarget(null);
			}
			if (_screenshotTarget != null)
			{
				saveShot = _screenshotTarget;
				_screenshotTarget = null;
				SaveShot();
			}
			if (Graphics.screenTarget != null)
			{
				int w2 = Graphics.width;
				int height2 = Graphics.height;
				Graphics.SetRenderTarget(Graphics.screenTarget);
				Graphics.UpdateScreenViewport();
				HUD.hide = true;
				Level.DrawCurrentLevel();
				Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
				OnDraw();
				Graphics.screen.End();
				HUD.hide = false;
				Graphics.width = w2;
				Graphics.height = height2;
				Graphics.SetRenderTarget(null);
				Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
				Graphics.Draw(Graphics.screenTarget, Vec2.Zero, null, Color.White, 0f, Vec2.Zero, Vec2.One, SpriteEffects.None);
				Graphics.screen.End();
				return;
			}
			bool menuShouldPauseGameplay = true;
			if (Network.isActive)
			{
				menuShouldPauseGameplay = false;
			}
			if (_pauseMenu != null && _didPauseCapture && Graphics.screenCapture == null)
			{
				Graphics.SetRenderTarget(null);
				Graphics.Clear(Color.Black * Graphics.fade);
				if (autoPauseFade)
				{
					_pauseMaterial.fade = Lerp.FloatSmooth(_pauseMaterial.fade, doPauseFade ? 0.6f : 0f, 0.1f, 1.1f);
					_pauseMaterial.dim = Lerp.FloatSmooth(_pauseMaterial.dim, doPauseFade ? 0.6f : 1f, 0.1f, 1.1f);
				}
				Graphics.SetFullViewport();
				new Vec2(Layer.HUD.camera.width / (float)_screenCapture.width, Layer.HUD.camera.height / (float)_screenCapture.height);
				Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, Matrix.Identity);
				Graphics.material = _pauseMaterial;
				Graphics.Draw(_screenCapture, new Vec2(0f, 0f), null, new Color(120, 120, 120), 0f, Vec2.Zero, new Vec2(1f, 1f), SpriteEffects.None, -0.9f);
				Graphics.material = null;
				Graphics.screen.End();
				Graphics.RestoreOldViewport();
				Layer.HUD.Begin(transparent: true);
				_pauseMenu.Draw();
				for (int i = 0; i < closeMenuUpdate.Count; i++)
				{
					closeMenuUpdate[i].Draw();
				}
				HUD.Draw();
				if (Level.current.drawsOverPauseMenu)
				{
					Level.current.PostDrawLayer(Layer.HUD);
				}
				Layer.HUD.End(transparent: true);
				Layer.Console.Begin(transparent: true);
				DevConsole.Draw();
				Level.current.PostDrawLayer(Layer.Console);
				Layer.Console.End(transparent: true);
				if (!menuShouldPauseGameplay && UIMatchmakerMark2.instance == null)
				{
					_didPauseCapture = false;
				}
			}
			else
			{
				if (autoPauseFade)
				{
					_pauseMaterial.fade = 0f;
					_pauseMaterial.dim = 0.6f;
				}
				Graphics.SetRenderTarget(null);
				Level.DrawCurrentLevel();
                Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Resolution.getTransformationMatrix());
				OnDraw();
				Graphics.screen.End();
				if (closeMenuUpdate.Count > 0)
				{
					Layer.HUD.Begin(transparent: true);
					foreach (UIComponent item in closeMenuUpdate)
					{
						item.DoDraw();
					}
					Layer.HUD.End(transparent: true);
				}
			}
			if (DevConsole.showFPS)
			{
				FPSCounter.Render(Graphics.device, 16f, 16f, 0, "UPS");
				FPSCounter.Render(Graphics.device, 100f, 16f, 1);
			}
		}
	}

	protected virtual void OnDraw()
	{
	}
}
