using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace DuckGame;

public class Content
{
	private static MultiMap<string, LevelData> _levels = new MultiMap<string, LevelData>();

	private static MultiMap<string, LevelData> _levelPreloadList = new MultiMap<string, LevelData>();

	private static Dictionary<string, MTEffect> _effects = new Dictionary<string, MTEffect>();

	private static Dictionary<Effect, MTEffect> _effectMap = new Dictionary<Effect, MTEffect>();

	private static List<MTEffect> _effectList = new List<MTEffect>();

	private static short _currentEffectIndex = 0;

	private static Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();

	private static Dictionary<string, Tex2D> _textures = new Dictionary<string, Tex2D>();

	private static Dictionary<object, Tex2D> _texture2DMap = new Dictionary<object, Tex2D>();

	private static List<Tex2D> _textureList = new List<Tex2D>();

	public static short _currentTextureIndex = 0;

	public static Tex2D invalidTexture;

	public static volatile bool readyToRenderPreview = false;

	private static volatile bool previewRendering = false;

	public static volatile bool renderingPreview = false;

	public static XMLLevel previewLevel;

	private static Camera _previewCamera;

	public static volatile bool cancelPreview = false;

	public static int customPreviewWidth = 0;

	public static int customPreviewHeight = 0;

	public static Vec2 customPreviewCenter = Vec2.Zero;

	private static LevelData _previewLevelData;

	private static LayerCore _previewLayerCore = null;

	private static string _previewPath = null;

	private static MTSpriteBatch _previewBatch;

	private static bool _previewBackground = false;

	private static Thread _previewThread;

	private static RenderTarget2D _currentPreviewTarget;

	private static LevelMetaData.PreviewPair _currentPreviewPair;

	public static bool doingTempSave = false;

	public static byte[] generatePreviewBytes;

	public static bool renderingToTarget = false;

	private static Dictionary<Type, string> _extensionList = new Dictionary<Type, string>
	{
		{
			typeof(Tex2D),
			"*.png"
		},
		{
			typeof(Texture2D),
			"*.png"
		},
		{
			typeof(SoundEffect),
			"*.wav"
		},
		{
			typeof(Song),
			"*.ogg"
		},
		{
			typeof(Level),
			"*.lev"
		},
		{
			typeof(Effect),
			"*.xnb"
		}
	};

	private static List<string> _texturesToProcess = new List<string>();

	private static ContentManager _base;

	private static string _path = "";

	private static Dictionary<string, Vec2> _spriteSizeDirectory = new Dictionary<string, Vec2>();

	public static object _loadLock = new object();

	public static Exception lastException = null;

	private static Dictionary<string, ParallaxBackground.Definition> _parallaxDefinitions = new Dictionary<string, ParallaxBackground.Definition>();

	public static List<MTEffect> effectList => _effectList;

	public static Dictionary<string, Tex2D> textures => _textures;

	public static List<Tex2D> textureList => _textureList;

	public static Thread previewThread => _previewThread;

	public static string path => _path;

	public static LevelData GetLevel(string guid, LevelLocation location = LevelLocation.Any)
	{
		if (guid != null && _levels.TryGetValue(guid, out var levs))
		{
			foreach (LevelData lev in levs)
			{
				if (lev.GetLocation() == location || location == LevelLocation.Any)
				{
					return lev;
				}
			}
		}
		return null;
	}

	public static List<LevelData> GetAllLevels(string guid)
	{
		if (_levels.TryGetValue(guid, out var levs))
		{
			return levs;
		}
		return new List<LevelData>();
	}

	public static List<LevelData> GetAllLevels()
	{
		List<LevelData> levs = new List<LevelData>();
		foreach (KeyValuePair<string, List<LevelData>> level in _levels)
		{
			levs.AddRange(level.Value);
		}
		return levs;
	}

	public static void MapLevel(string lev, LevelData dat, LevelLocation location)
	{
		lock (_levels)
		{
			if (_levels.TryGetValue(lev, out var levs))
			{
				LevelData found = null;
				foreach (LevelData l in levs)
				{
					if (l.GetLocation() == location)
					{
						found = l;
						break;
					}
				}
				if (found != null)
				{
					levs.Remove(found);
				}
			}
			dat.SetLocation(location);
			_levels.Add(lev, dat);
		}
	}

	private static void PreviewThread()
	{
		Level l = Level.activeLevel;
		Level l2 = Level.core.currentLevel;
		LayerCore oldLayerCore = Layer.core;
		try
		{
			renderingPreview = true;
			if (!_previewBackground)
			{
				Thing.skipLayerAdding = true;
			}
			XMLLevel lev = null;
			if (_previewLevelData == null)
			{
				lev = new XMLLevel(_previewPath);
			}
			else
			{
				lev = new XMLLevel(_previewLevelData);
				_previewLevelData = null;
			}
			if (cancelPreview)
			{
				return;
			}
			previewLevel = lev;
			previewLevel.ignoreVisibility = true;
			Level.skipInitialize = !_previewBackground;
			if (!_previewBackground)
			{
				previewLevel.isPreview = true;
			}
			_previewLayerCore = null;
			if (_previewBackground)
			{
				Layer.core = (_previewLayerCore = new LayerCore());
				Layer.core.InitializeLayers();
			}
			Level.core.currentLevel = previewLevel;
			Level.activeLevel = previewLevel;
			previewLevel.Initialize();
			Level.activeLevel = l;
			Level.core.currentLevel = l2;
			if (cancelPreview)
			{
				return;
			}
			Thing.skipLayerAdding = false;
			Level.skipInitialize = false;
			previewLevel.CalculateBounds();
			if (customPreviewWidth != 0)
			{
				_previewCamera = new Camera(0f, 0f, customPreviewWidth, customPreviewHeight);
			}
			else
			{
				_previewCamera = new Camera(0f, 0f, 1280f, 1280f * Graphics.aspect);
			}
			Vec2 topLeft = previewLevel.topLeft;
			Vec2 br = previewLevel.bottomRight;
			Vec2 center = (topLeft + br) / 2f;
			if (cancelPreview)
			{
				return;
			}
			_previewCamera.width /= 2f;
			_previewCamera.height /= 2f;
			if (customPreviewCenter != Vec2.Zero)
			{
				_previewCamera.center = customPreviewCenter;
			}
			else
			{
				_previewCamera.center = center;
			}
			readyToRenderPreview = true;
			if (_previewThread != null)
			{
				while (readyToRenderPreview)
				{
					if (cancelPreview)
					{
						return;
					}
				}
			}
			previewRendering = false;
			renderingPreview = false;
		}
		catch (Exception ex)
		{
			Program.LogLine(ex.ToString());
			renderingPreview = false;
			Thing.skipLayerAdding = false;
			Level.skipInitialize = false;
		}
		if (_previewBackground)
		{
			Level.activeLevel = l;
			Level.core.currentLevel = l2;
			Layer.core = oldLayerCore;
		}
	}

	private static void DoPreviewRender(bool pSaveMetadata)
	{
		MTSpriteBatch cur = Graphics.screen;
		Graphics.screen = _previewBatch;
		_ = Graphics.viewport;
		RenderTarget2D curTarget = Graphics.currentRenderTarget;
		Graphics.SetRenderTarget(_currentPreviewTarget);
		Graphics.viewport = new Viewport(0, 0, _currentPreviewTarget.width, _currentPreviewTarget.height);
		string curTileset0 = Custom.data[CustomType.Block][0];
		if (Custom.previewData[CustomType.Block][0] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Block][0].GetTileData(), 0, CustomType.Block);
		}
		string curTileset1 = Custom.data[CustomType.Block][1];
		if (Custom.previewData[CustomType.Block][1] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Block][1].GetTileData(), 1, CustomType.Block);
		}
		string curTileset2 = Custom.data[CustomType.Block][2];
		if (Custom.previewData[CustomType.Block][2] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Block][2].GetTileData(), 2, CustomType.Block);
		}
		string curBackground0 = Custom.data[CustomType.Background][0];
		if (Custom.previewData[CustomType.Background][0] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Background][0].GetTileData(), 0, CustomType.Background);
		}
		string curBackground1 = Custom.data[CustomType.Background][1];
		if (Custom.previewData[CustomType.Background][1] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Background][1].GetTileData(), 1, CustomType.Background);
		}
		string curBackground2 = Custom.data[CustomType.Background][2];
		if (Custom.previewData[CustomType.Background][2] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Background][2].GetTileData(), 2, CustomType.Background);
		}
		string curPlatform0 = Custom.data[CustomType.Platform][0];
		if (Custom.previewData[CustomType.Platform][0] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][0].GetTileData(), 0, CustomType.Platform);
		}
		string curPlatform1 = Custom.data[CustomType.Platform][1];
		if (Custom.previewData[CustomType.Platform][1] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][1].GetTileData(), 1, CustomType.Platform);
		}
		string curPlatform2 = Custom.data[CustomType.Platform][2];
		if (Custom.previewData[CustomType.Platform][2] != null)
		{
			Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][2].GetTileData(), 2, CustomType.Platform);
		}
		bool challenge = false;
		bool strange = true;
		bool arcade = false;
		Dictionary<string, int> invalidList = new Dictionary<string, int>();
		if (_previewBackground)
		{
			Level l = Level.activeLevel;
			Level l2 = Level.core.currentLevel;
			LayerCore oldLayerCore = Layer.core;
			if (_previewLayerCore != null)
			{
				Layer.core = _previewLayerCore;
			}
			Level.activeLevel = previewLevel;
			Level.core.currentLevel = previewLevel;
			try
			{
				Graphics.defaultRenderTarget = _currentPreviewTarget;
				Layer.HUD.visible = false;
				previewLevel.camera = _previewCamera;
				previewLevel.simulatePhysics = false;
				previewLevel.DoUpdate();
				previewLevel.DoUpdate();
				previewLevel.DoDraw();
				Layer.HUD.visible = true;
				Graphics.defaultRenderTarget = null;
				Level.activeLevel = l;
				Level.core.currentLevel = l2;
				Layer.core = oldLayerCore;
			}
			catch (Exception ex)
			{
				Layer.HUD.visible = true;
				Graphics.defaultRenderTarget = null;
				Level.activeLevel = l;
				Level.core.currentLevel = l2;
				Layer.core = oldLayerCore;
				throw ex;
			}
		}
		else
		{
			Graphics.Clear(Color.Black);
			Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, _previewCamera.getMatrix());
			foreach (Thing t in previewLevel.things)
			{
				if (t.layer == Layer.Game || t.layer == Layer.Blocks || t.layer == null)
				{
					t.Draw();
				}
				if (pSaveMetadata)
				{
					if (t is ChallengeMode)
					{
						challenge = true;
					}
					else if (t is SpawnPoint)
					{
						strange = false;
					}
					else if (t is ArcadeMode)
					{
						arcade = true;
					}
					if (!ContentProperties.GetBag(t.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true))
					{
						if (!invalidList.ContainsKey(t.editorName))
						{
							invalidList[t.editorName] = 1;
						}
						else
						{
							invalidList[t.editorName]++;
						}
					}
				}
				Graphics.material = null;
			}
			if (previewLevel.things.Count == 1)
			{
				_ = previewLevel.things.First() is ImportMachine;
			}
			Graphics.screen.End();
		}
		Graphics.screen = cur;
		Graphics.SetRenderTarget(curTarget);
		Custom.data[CustomType.Block][0] = curTileset0;
		Custom.data[CustomType.Block][1] = curTileset1;
		Custom.data[CustomType.Block][2] = curTileset2;
		Custom.data[CustomType.Background][0] = curBackground0;
		Custom.data[CustomType.Background][1] = curBackground1;
		Custom.data[CustomType.Background][2] = curBackground2;
		Custom.data[CustomType.Platform][0] = curPlatform0;
		Custom.data[CustomType.Platform][1] = curPlatform1;
		Custom.data[CustomType.Platform][2] = curPlatform2;
		if (pSaveMetadata && !doingTempSave)
		{
			LevelMetaData metaData = Editor.ReadLevelMetadata(previewLevel.data);
			if (metaData != null && metaData.guid != null)
			{
				_currentPreviewPair = metaData.SavePreview(_currentPreviewTarget, invalidList, strange, challenge, arcade);
			}
		}
	}

	public static LevelMetaData.PreviewPair GeneratePreview(LevelData levelData, bool pRefresh = false, RenderTarget2D pCustomPreviewTarget = null)
	{
		_previewLevelData = levelData;
		return GeneratePreview((string)null, pRefresh, pCustomPreviewTarget);
	}

	public static LevelMetaData.PreviewPair GeneratePreview(string levelPath, bool pRefresh = false, RenderTarget2D pCustomPreviewTarget = null)
	{
		if (generatePreviewBytes != null)
		{
			_previewLevelData = DuckFile.LoadLevel(generatePreviewBytes);
			generatePreviewBytes = null;
		}
		bool background = false;
		if (pCustomPreviewTarget != null)
		{
			background = true;
		}
		else if (!pRefresh && levelPath != null)
		{
			LevelMetaData d = null;
			d = ((_previewLevelData == null) ? Editor.ReadLevelMetadata(levelPath) : Editor.ReadLevelMetadata(_previewLevelData));
			if (d != null)
			{
				LevelMetaData.PreviewPair previewPair = d.LoadPreview();
				if (previewPair != null)
				{
					return previewPair;
				}
			}
		}
		DevConsole.Log(DCSection.General, "Generating preview data for (" + levelPath + ")...");
		_previewBackground = background;
		readyToRenderPreview = false;
		if (_previewThread != null && _previewThread.IsAlive)
		{
			cancelPreview = true;
			int wait = 250;
			while (_previewThread.IsAlive)
			{
				Tasker.RunTasks();
				Thread.Sleep(2);
				wait--;
			}
			readyToRenderPreview = false;
		}
		_previewThread = null;
		cancelPreview = false;
		Thing.skipLayerAdding = false;
		Level.skipInitialize = false;
		if (_previewBatch == null)
		{
			_previewBatch = new MTSpriteBatch(Graphics.device);
		}
		_previewPath = levelPath;
		if (pCustomPreviewTarget != null)
		{
			_currentPreviewTarget = pCustomPreviewTarget;
		}
		else
		{
			_currentPreviewTarget = new RenderTarget2D(320, 200);
		}
		renderingToTarget = true;
		renderingPreview = true;
		readyToRenderPreview = true;
		PreviewThread();
		DoPreviewRender(pCustomPreviewTarget == null);
		renderingPreview = false;
		readyToRenderPreview = false;
		renderingToTarget = false;
		return _currentPreviewPair;
	}

	public static void SetTextureAtIndex(short index, Tex2D tex)
	{
		while (index >= _textureList.Count)
		{
			_textureList.Add(null);
			_currentTextureIndex++;
		}
		_textureList[index] = tex;
		_texture2DMap[tex.nativeObject] = tex;
		_textures[tex.textureName] = tex;
		tex.SetTextureIndex(index);
	}

	public static Tex2D AssignTextureIndex(Tex2D tex)
	{
		Tex2D val = null;
		_texture2DMap.TryGetValue(tex, out val);
		if (val == null)
		{
			tex.SetTextureIndex(_currentTextureIndex);
			_currentTextureIndex++;
			_textureList.Add(tex);
			_texture2DMap[tex] = tex;
		}
		return val;
	}

	public static Tex2D GetTex2D(object tex)
	{
		return GetTex2D((Texture2D)tex);
	}

	public static Tex2D GetTex2D(Texture2D tex)
	{
		if (tex == null)
		{
			return null;
		}
		Tex2D val = null;
		_texture2DMap.TryGetValue(tex, out val);
		if (val == null)
		{
			val = new Tex2D(tex, "", _currentTextureIndex);
			_currentTextureIndex++;
			_textureList.Add(val);
			_texture2DMap[tex] = val;
		}
		return val;
	}

	public static void SetEffectAtIndex(short index, MTEffect e)
	{
		while (index > _effectList.Count)
		{
			_effectList.Add(null);
			_currentEffectIndex++;
		}
		_effectList[index] = e;
		_effectMap[e.effect] = e;
		_effects[e.effectName] = e;
		e.SetEffectIndex(index);
	}

	public static MTEffect GetMTEffect(Effect e)
	{
		MTEffect val = null;
		_effectMap.TryGetValue(e, out val);
		if (val == null)
		{
			val = new MTEffect(e, "", _currentEffectIndex);
			_currentEffectIndex++;
			_effectList.Add(val);
			_effectMap[e] = val;
		}
		return val;
	}

	public static Tex2D GetTex2DFromIndex(short index)
	{
		return _textureList[index];
	}

	public static MTEffect GetMTEffectFromIndex(short index)
	{
		if (index < 0)
		{
			return null;
		}
		return _effectList[index];
	}

	public static List<string> GetFiles<T>(string path)
	{
		List<string> files = new List<string>();
		string ext = null;
		if (_extensionList.TryGetValue(typeof(T), out ext))
		{
			GetFilesInternal<T>(path, files, ext);
		}
		return files;
	}

	public static List<string> GetFilesInternal<T>(string path, List<string> files, string ext)
	{
		string[] files2 = DuckFile.GetFiles(path, ext);
		foreach (string f in files2)
		{
			files.Add(f);
		}
		files2 = GetDirectories(path);
		for (int i = 0; i < files2.Length; i++)
		{
			GetFilesInternal<T>(files2[i], files, ext);
		}
		return files;
	}

	private static void SearchDirLevels(string dir, LevelLocation location)
	{
		string[] array = ((location == LevelLocation.Content) ? GetFiles(dir) : DuckFile.GetFiles(dir, "*.*"));
		for (int i = 0; i < array.Length; i++)
		{
			ProcessLevel(array[i], location);
		}
		array = ((location == LevelLocation.Content) ? GetDirectories(dir) : DuckFile.GetDirectories(dir));
		for (int i = 0; i < array.Length; i++)
		{
			SearchDirLevels(array[i], location);
		}
	}

	public static void ReloadLevels(string s)
	{
		SearchDirLevels("Content/levels/" + s, LevelLocation.Content);
	}

	private static void SearchDirTextures(string dir, bool reverse = false)
	{
		if (reverse)
		{
			foreach (string item in DG.Reverse(GetFiles(dir)))
			{
				ProcessTexture(item);
			}
			{
				foreach (string d in DG.Reverse(GetDirectories(dir)))
				{
					if (!d.EndsWith("Audio") && !d.EndsWith("Shaders"))
					{
						SearchDirTextures(d, reverse);
					}
				}
				return;
			}
		}
		string[] files = GetFiles(dir);
		for (int i = 0; i < files.Length; i++)
		{
			ProcessTexture(files[i]);
		}
		files = GetDirectories(dir);
		foreach (string d2 in files)
		{
			if (!d2.EndsWith("Audio") && !d2.EndsWith("Shaders"))
			{
				SearchDirTextures(d2);
			}
		}
	}

	private static void SearchDirEffects(string dir)
	{
		string[] files = GetFiles(dir);
		for (int i = 0; i < files.Length; i++)
		{
			ProcessEffect(files[i]);
		}
		files = GetDirectories(dir);
		for (int i = 0; i < files.Length; i++)
		{
			SearchDirEffects(files[i]);
		}
	}

	public static string GetLevelID(string path, LevelLocation loc = LevelLocation.Content)
	{
		if (!path.EndsWith(".lev"))
		{
			path += ".lev";
		}
		foreach (KeyValuePair<string, List<LevelData>> level in _levels)
		{
			foreach (LevelData dat in level.Value)
			{
				if ((dat.GetLocation() == loc || loc == LevelLocation.Any) && dat.GetPath().EndsWith("/" + path))
				{
					return dat.metaData.guid;
				}
			}
		}
		string text = Content.path + "/levels/" + path;
		if (!path.EndsWith(".lev"))
		{
			path += ".lev";
		}
		LevelData lev = DuckFile.LoadLevel(text);
		if (lev != null)
		{
			MapLevel(lev.metaData.guid, lev, loc);
			return lev.metaData.guid;
		}
		return "";
	}

	public static List<string> GetLevels(string dir, LevelLocation location)
	{
		return GetLevels(dir, location, pRecursive: true, pOnline: false, pEightPlayer: false);
	}

	public static List<string> GetLevels(string dir, LevelLocation location, bool pRecursive, bool pOnline, bool pEightPlayer, bool pAllowNonRestrictedEightPlayer = false, bool pSkipFilters = false)
	{
		List<string> levels = new List<string>();
		foreach (KeyValuePair<string, List<LevelData>> levDat in _levels)
		{
			foreach (LevelData dat in levDat.Value)
			{
				if ((dat.GetLocation() == location || location == LevelLocation.Any) && (pSkipFilters || ((!pOnline || dat.metaData.online) && (!pEightPlayer || dat.metaData.eightPlayer) && (pEightPlayer || !dat.metaData.eightPlayer || (pAllowNonRestrictedEightPlayer && !dat.metaData.eightPlayerRestricted)))))
				{
					string path = dat.GetPath();
					int index = path.IndexOf(dir + "/");
					if (index >= 0 && (pRecursive || path.LastIndexOf("/") == index + dir.Length))
					{
						levels.Add(levDat.Key);
					}
				}
			}
		}
		return levels;
	}

	public static void ProcessLevel(string path, LevelLocation location)
	{
		MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
		{
			try
			{
				Main.SpecialCode = (("Loading Level " + path != null) ? path : "null");
				if (path.EndsWith(".lev"))
				{
					LoadLevelData(path, location);
					MonoMain.loadyBits++;
				}
			}
			catch (Exception ex)
			{
				LogLevelFailure(ex.ToString());
			}
		}));
	}

	private static LevelData LoadLevelData(string pPath, LevelLocation pLocation)
	{
		pPath = pPath.Replace('\\', '/');
		LevelData dat = null;
		dat = ((pLocation != LevelLocation.Content) ? DuckFile.LoadLevel(pPath) : DuckFile.LoadLevel(DuckFile.ReadEntireStream(DuckFile.OpenStream(pPath))));
		if (dat != null)
		{
			dat.SetPath(pPath);
			pPath = pPath.Substring(0, pPath.Length - 4);
			pPath.Substring(pPath.IndexOf("/levels/") + 8);
			if (dat.metaData.guid != null)
			{
				MapLevel(dat.metaData.guid, dat, pLocation);
			}
			return dat;
		}
		return null;
	}

	private static void LogLevelFailure(string s)
	{
		try
		{
			Program.LogLine("Level Load Failure (Did not cause crash)\n================================================\n " + s + "\n================================================\n");
		}
		catch (Exception)
		{
		}
	}

	private static void ProcessTexture(string path)
	{
		if (path.EndsWith(".xnb"))
		{
			path = path.Replace('\\', '/');
			if (path.StartsWith("Content/"))
			{
				path = path.Substring(8);
			}
			path = path.Substring(0, path.Length - 4);
			MonoMain.loadMessage = "Loading Textures (" + path + ")";
			MonoMain.lazyLoadActions.Enqueue(delegate
			{
				Load<Tex2D>(path);
			});
			MonoMain.lazyLoadyBits++;
		}
	}

	private static void ProcessEffect(string path)
	{
		try
		{
			if (path.EndsWith(".xnb"))
			{
				path = path.Replace('\\', '/');
				if (path.StartsWith("Content/"))
				{
					path = path.Substring(8);
				}
				path = path.Substring(0, path.Length - 4);
				Load<MTEffect>(path);
				MonoMain.lazyLoadyBits++;
			}
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.General, "|DGRED|Failed to load shader (" + path + "):");
			DevConsole.Log(DCSection.General, "|DGRED|" + ex.Message);
		}
	}

	public static void InitializeBase(ContentManager manager)
	{
		_base = manager;
		invalidTexture = Load<Tex2D>("notexture");
		_path = Directory.GetCurrentDirectory() + "/Content/";
	}

	public static void InitializeLevels()
	{
		MonoMain.loadMessage = "Loading Levels";
		SearchDirLevels("Content/levels", LevelLocation.Content);
		if (!Steam.IsInitialized())
		{
			return;
		}
		LoadingAction steamLoad = new LoadingAction();
		steamLoad.action = delegate
		{
			WorkshopQueryUser workshopQueryUser = Steam.CreateQueryUser(Steam.user.id, WorkshopList.Subscribed, WorkshopType.UsableInGame, WorkshopSortOrder.TitleAsc);
			workshopQueryUser.requiredTags.Add("Map");
			workshopQueryUser.onlyQueryIDs = true;
			workshopQueryUser.QueryFinished += delegate
			{
				steamLoad.flag = true;
			};
			workshopQueryUser.ResultFetched += delegate(object sender, WorkshopQueryResult result)
			{
				WorkshopItem publishedFile = result.details.publishedFile;
				if ((publishedFile.stateFlags & WorkshopItemState.Installed) != WorkshopItemState.None)
				{
					SearchDirLevels(publishedFile.path, LevelLocation.Workshop);
				}
			};
			workshopQueryUser.Request();
			Steam.Update();
		};
		steamLoad.waitAction = delegate
		{
			Steam.Update();
			return steamLoad.flag;
		};
		MonoMain.currentActionQueue.Enqueue(steamLoad);
	}

	public static Vec2 GetTextureSize(string pName)
	{
		Vec2 size = Vec2.Zero;
		if (_spriteSizeDirectory.TryGetValue(pName, out size))
		{
			return size;
		}
		return Vec2.Zero;
	}

	public static void InitializeTextureSizeDictionary()
	{
		try
		{
			if (File.Exists(DuckFile.contentDirectory + "texture_size_directory.dat"))
			{
				string[] array = File.ReadAllLines(DuckFile.contentDirectory + "texture_size_directory.dat");
				for (int i = 0; i < array.Length; i++)
				{
					string[] subParts = array[i].Split(',');
					_spriteSizeDirectory[subParts[0].Trim().Replace('\\', '/')] = new Vec2(Convert.ToSingle(subParts[1]), Convert.ToSingle(subParts[2]));
				}
			}
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.General, "|DGRED|Error initializing texture_size_directory.dat:");
			DevConsole.Log(DCSection.General, "|DGRED|" + ex.Message);
		}
	}

	public static void Initialize(bool reverse)
	{
		MonoMain.loadMessage = "Loading Textures";
		SearchDirTextures("Content/", reverse);
	}

	public static void Initialize()
	{
		Initialize(reverse: false);
	}

	public static void InitializeEffects()
	{
		MonoMain.loadMessage = "Loading Effects";
		SearchDirEffects("Content/Shaders");
	}

	public static string[] GetFiles(string path, string filter = "*.*")
	{
		path = path.Replace('\\', '/');
		path = path.Trim('/');
		string cur = Directory.GetCurrentDirectory() + "/";
		cur = cur.Replace('\\', '/');
		List<string> dirs = new List<string>();
		foreach (string d in DuckFile.GetFilesNoCloud(path, filter))
		{
			if (!Path.GetFileName(d).Contains("._"))
			{
				string fix = d.Replace('\\', '/');
				int index = fix.IndexOf(cur);
				if (index != -1)
				{
					fix = fix.Remove(index, cur.Length);
				}
				dirs.Add(fix);
			}
		}
		return dirs.ToArray();
	}

	public static string[] GetDirectories(string path, string filter = "*.*")
	{
		path = path.Replace('\\', '/');
		path = path.Trim('/');
		List<string> dirs = new List<string>();
		foreach (string d in DuckFile.GetDirectoriesNoCloud(path))
		{
			if (!Path.GetFileName(d).Contains("._"))
			{
				dirs.Add(d);
			}
		}
		return dirs.ToArray();
	}

	public static void Update()
	{
	}

	public static ParallaxBackground.Definition LoadParallaxDefinition(string pName)
	{
		try
		{
			pName.Replace(".png", ".txt");
			if (!pName.EndsWith(".txt"))
			{
				pName += ".txt";
			}
			ParallaxBackground.Definition def = null;
			if (_parallaxDefinitions.TryGetValue(pName, out def))
			{
				return def;
			}
			string fullPath = pName;
			if (!pName.Contains(":"))
			{
				fullPath = DuckFile.contentDirectory + pName;
			}
			string[] parts = null;
			if (ReskinPack.active.Count > 0)
			{
				parts = ReskinPack.LoadAsset<string[]>(pName);
			}
			if (parts == null && File.Exists(fullPath))
			{
				parts = File.ReadAllLines(fullPath);
			}
			if (parts != null)
			{
				try
				{
					def = new ParallaxBackground.Definition();
					string[] array = parts;
					foreach (string s in array)
					{
						if (s.StartsWith("[") || string.IsNullOrWhiteSpace(s))
						{
							continue;
						}
						string[] p = s.Split(',');
						ParallaxBackground.Definition.Zone zone = new ParallaxBackground.Definition.Zone
						{
							index = Convert.ToInt32(p[0].Trim()),
							distance = Convert.ToSingle(p[1].Trim()),
							speed = Convert.ToSingle(p[2].Trim()),
							moving = Convert.ToBoolean(p[3].Trim())
						};
						if (p.Length > 4)
						{
							zone.sprite = new Sprite(p[4].Trim());
							if (p.Length > 6)
							{
								zone.sprite.position = new Vec2(Convert.ToSingle(p[5].Trim()), Convert.ToSingle(p[6].Trim()));
							}
							if (p.Length > 7)
							{
								zone.sprite.depth = Convert.ToSingle(p[7].Trim());
							}
						}
						if (zone.sprite != null)
						{
							def.sprites.Add(zone);
						}
						else
						{
							def.zones.Add(zone);
						}
					}
					return def;
				}
				catch (Exception ex)
				{
					DevConsole.Log(DCSection.General, "|DGRED|LoadParallaxDefinition error (" + pName + "):");
					DevConsole.Log(DCSection.General, "|DGRED|" + ex.Message);
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static T Load<T>(string name)
	{
		if (ReskinPack.active.Count > 0)
		{
			try
			{
				if (typeof(T) == typeof(Tex2D))
				{
					Texture2D tex = ReskinPack.LoadAsset<Texture2D>(name);
					if (tex != null)
					{
						lock (_loadLock)
						{
							Vec2 originalSize = GetTextureSize(name);
							Tex2D t = ((!(originalSize != Vec2.Zero) || ((float)tex.Width == originalSize.x && (float)tex.Height == originalSize.y)) ? new Tex2D(tex, name, _currentTextureIndex) : new BigBoyTex2D(tex, name, _currentTextureIndex)
							{
								scaleFactor = originalSize.x / (float)tex.Width
							});
							_currentTextureIndex++;
							_textureList.Add(t);
							_textures[name] = t;
							_texture2DMap[tex] = t;
							return (T)(object)t;
						}
					}
				}
				else
				{
					T skinret = ReskinPack.LoadAsset<T>(name);
					if (skinret != null)
					{
						return skinret;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		if (typeof(T) == typeof(Tex2D))
		{
			Tex2D t2 = null;
			lock (_textures)
			{
				_textures.TryGetValue(name, out t2);
			}
			if (t2 == null)
			{
				Texture2D t2d = null;
				bool modLoad = false;
				if (MonoMain.moddingEnabled && ModLoader.accessibleMods.Count() > 1 && name.Length > 1 && name[1] == ':')
				{
					modLoad = true;
				}
				if (!modLoad)
				{
					try
					{
						t2d = _base.Load<Texture2D>(name);
					}
					catch (Exception ex2)
					{
						modLoad = MonoMain.moddingEnabled && ModLoader.modsEnabled;
						lastException = ex2;
					}
				}
				if (modLoad)
				{
					foreach (Mod mod in ModLoader.accessibleMods)
					{
						if (mod.configuration != null && mod.configuration.content != null)
						{
							t2d = mod.configuration.content.Load<Texture2D>(name);
						}
						if (t2d != null)
						{
							break;
						}
					}
				}
				else if (t2d == null)
				{
					try
					{
						t2d = ContentPack.LoadTexture2D(name);
					}
					catch (Exception ex3)
					{
						lastException = ex3;
					}
				}
				if (t2d == null)
				{
					t2d = invalidTexture;
					Main.SpecialCode = "Couldn't load texture " + name;
				}
				lock (_loadLock)
				{
					t2 = new Tex2D(t2d, name, _currentTextureIndex);
					_currentTextureIndex++;
					_textureList.Add(t2);
					_textures[name] = t2;
					_texture2DMap[t2d] = t2;
				}
			}
			return (T)(object)t2;
		}
		if (typeof(T) == typeof(MTEffect))
		{
			MTEffect t3 = null;
			lock (_effects)
			{
				_effects.TryGetValue(name, out t3);
			}
			if (t3 == null)
			{
				Effect e = null;
				lock (_loadLock)
				{
					e = _base.Load<Effect>(name);
				}
				lock (_loadLock)
				{
					t3 = new MTEffect(e, name, _currentEffectIndex);
					_currentEffectIndex++;
					_effectList.Add(t3);
					_effects[name] = t3;
					_effectMap[e] = t3;
				}
			}
			return (T)(object)t3;
		}
		if (typeof(T) == typeof(SoundEffect))
		{
			SoundEffect sound = null;
			lock (_sounds)
			{
				_sounds.TryGetValue(name, out sound);
			}
			if (sound == null)
			{
				if (!name.Contains(":") && !name.EndsWith(".wav"))
				{
					lock (_loadLock)
					{
						try
						{
							string fullName = DuckFile.contentDirectory + name + ".wav";
							sound = SoundEffect.FromStream(new MemoryStream(File.ReadAllBytes(fullName)));
							if (sound != null)
							{
								sound.file = fullName;
							}
						}
						catch (Exception ex4)
						{
							lastException = ex4;
						}
					}
				}
				if (sound == null && MonoMain.moddingEnabled && ModLoader.modsEnabled)
				{
					foreach (Mod mod2 in ModLoader.accessibleMods)
					{
						if (mod2.configuration != null && mod2.configuration.content != null)
						{
							sound = mod2.configuration.content.Load<SoundEffect>(name);
						}
						if (sound != null)
						{
							break;
						}
					}
				}
			}
			if (sound == null)
			{
				Main.SpecialCode = "Couldn't load sound (" + sound?.ToString() + ")";
			}
			else
			{
				_sounds[name] = sound;
			}
			return (T)(object)sound;
		}
		if (typeof(T) == typeof(Song))
		{
			if (MonoMain.moddingEnabled && ModLoader.modsEnabled)
			{
				foreach (Mod mod3 in ModLoader.accessibleMods)
				{
					if (mod3.configuration != null && mod3.configuration.content != null)
					{
						Song song = mod3.configuration.content.Load<Song>(name);
						if (song != null)
						{
							return (T)(object)song;
						}
					}
				}
			}
			return default(T);
		}
		if (typeof(T) == typeof(Microsoft.Xna.Framework.Media.Song))
		{
			return (T)(object)_base.Load<Microsoft.Xna.Framework.Media.Song>(name);
		}
		return _base.Load<T>(name);
	}
}
