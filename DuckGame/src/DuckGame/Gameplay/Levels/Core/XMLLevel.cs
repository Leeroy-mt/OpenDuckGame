using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class XMLLevel : Level
{
	public bool collectItems;

	public List<Thing> levelItems = new List<Thing>();

	public Texture2D preview;

	private bool _customLevel;

	private bool _clientLevel;

	private bool _customLoad;

	private uint _checksum;

	private string _loadString = "";

	private LevelData _data;

	private byte[] _compressedData;

	private MemoryStream _compressedDataReceived;

	public string synchronizedLevelName;

	public bool ignoreVisibility;

	public volatile bool cancelLoading;

	public bool onlineEnabled;

	private ushort specialSyncIndex;

	public bool customLevel => _customLevel;

	public bool clientLevel => _clientLevel;

	public uint checksum => _checksum;

	public LevelData data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public byte[] compressedData => _compressedData;

	public MemoryStream compressedDataReceived => _compressedDataReceived;

	private void InitializeSeed()
	{
		if (NetworkDebugger.enabled && NetworkDebugger.Recorder.active != null)
		{
			seed = NetworkDebugger.Recorder.active.seed;
		}
		else
		{
			seed = Rando.Int(2147483646);
		}
	}

	public XMLLevel(string level)
	{
		InitializeSeed();
		if (level.EndsWith(".client"))
		{
			isCustomLevel = true;
			_customLevel = true;
			_clientLevel = true;
			_customLoad = true;
		}
		if (level.EndsWith(".custom"))
		{
			DevConsole.Log(DCSection.General, "Loading Level " + level);
			isCustomLevel = true;
			_customLevel = true;
			level = level.Substring(0, level.Length - 7);
			if (Network.isActive)
			{
				LevelData lev = Content.GetLevel(level);
				_checksum = lev.GetChecksum();
				_data = lev;
				_customLoad = true;
				if (Network.isServer)
				{
					_compressedData = GetCompressedLevelData(lev, level);
				}
			}
		}
		if (level == "WORKSHOP")
		{
			_customLevel = true;
			isCustomLevel = true;
			level = level.Substring(0, level.Length - 7);
			LevelData lev2 = RandomLevelDownloader.GetNextLevel();
			_checksum = lev2.GetChecksum();
			_data = lev2;
			_customLoad = true;
			if (Network.isServer && Network.isActive)
			{
				MemoryStream stream = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(new GZipStream(stream, CompressionMode.Compress));
				binaryWriter.Write(lev2.metaData.guid.ToString());
				BitBuffer levData = lev2.GetData();
				binaryWriter.Write(levData.lengthInBytes);
				binaryWriter.Write(levData.buffer, 0, levData.lengthInBytes);
				binaryWriter.Close();
				_compressedData = stream.ToArray();
			}
		}
		_level = level;
	}

	public XMLLevel(LevelData level)
	{
		InitializeSeed();
		_data = level;
	}

	public static byte[] GetCompressedLevelData(LevelData pLevel, string pLevelName)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(new GZipStream(memoryStream, CompressionMode.Compress));
		binaryWriter.Write(pLevelName);
		BitBuffer levData = pLevel.GetData();
		binaryWriter.Write(levData.lengthInBytes);
		binaryWriter.Write(levData.buffer, 0, levData.lengthInBytes);
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	public bool ApplyLevelData(ReceivedLevelInfo info)
	{
		base.waitingOnNewData = false;
		_data = info.data;
		_level = info.name;
		_customLevel = true;
		isCustomLevel = true;
		string onlineDir = DuckFile.onlineLevelDirectory;
		if (NetworkDebugger.currentIndex != 0)
		{
			onlineDir = onlineDir.Insert(onlineDir.Length - 1, NetworkDebugger.currentIndex.ToString());
		}
		string docName = _level;
		if (docName.EndsWith(".custom"))
		{
			docName = docName.Substring(0, base.level.Length - 7);
		}
		DuckFile.EnsureDownloadFileSpaceAvailable();
		return DuckFile.SaveChunk(_data, onlineDir + docName + ".lev");
	}

	public string ProcessLevelPath(string path)
	{
		bool online = false;
		if (path.EndsWith(".online"))
		{
			isCustomLevel = true;
			string shortPath = path.Substring(0, path.Length - 7);
			string onlineDir = DuckFile.onlineLevelDirectory;
			if (NetworkDebugger.currentIndex != 0)
			{
				onlineDir = onlineDir.Insert(onlineDir.Length - 1, NetworkDebugger.currentIndex.ToString());
			}
			string newPath = onlineDir + shortPath + ".lev";
			if (File.Exists(newPath))
			{
				return newPath;
			}
			online = true;
		}
		if (online || path.EndsWith(".custom"))
		{
			isCustomLevel = true;
			string shortPath2 = path.Substring(0, path.Length - 7);
			string levelDir = DuckFile.levelDirectory;
			if (NetworkDebugger.currentIndex != 0)
			{
				levelDir = levelDir.Insert(levelDir.Length - 1, NetworkDebugger.currentIndex.ToString());
			}
			string newPath2 = levelDir + shortPath2 + ".lev";
			if (File.Exists(newPath2))
			{
				return newPath2;
			}
			return null;
		}
		_data = Content.GetLevel(path);
		if (_data != null)
		{
			return path;
		}
		string customLevelPath = DuckFile.levelDirectory + path + ".lev";
		if (File.Exists(customLevelPath))
		{
			return customLevelPath;
		}
		customLevelPath = Editor.initialDirectory + "/" + path + ".lev";
		if (File.Exists(customLevelPath))
		{
			return customLevelPath;
		}
		return null;
	}

	private LevelData LoadLevelDoc()
	{
		if (_data != null)
		{
			return _data;
		}
		if (_level == "WORKSHOP")
		{
			return RandomLevelDownloader.GetNextLevel();
		}
		LevelData dat = null;
		if (!_level.Contains("_tempPlayLevel"))
		{
			_loadString = _level;
			dat = Content.GetLevel(_level);
			if (dat == null)
			{
				bool completePath = false;
				if (_level.Contains(":/") || _level.Contains(":\\"))
				{
					completePath = true;
				}
				if (completePath)
				{
					_loadString = _level;
					if (!_loadString.EndsWith(".lev"))
					{
						_loadString += ".lev";
					}
				}
				else
				{
					_loadString = DuckFile.levelDirectory + _level;
					if (!_loadString.EndsWith(".lev"))
					{
						_loadString += ".lev";
					}
				}
				dat = DuckFile.LoadLevel(_loadString);
				if (dat == null && !completePath)
				{
					_loadString = Editor.initialDirectory + "/" + _level + ".lev";
					if (!_loadString.EndsWith(".lev"))
					{
						_loadString += ".lev";
					}
					dat = DuckFile.LoadLevel(_loadString);
				}
				if (dat == null)
				{
					dat = DuckFile.LoadLevel(_level);
				}
				if (this is GameLevel)
				{
					_customLoad = true;
				}
			}
		}
		else
		{
			if (this is GameLevel && _level.ToLowerInvariant().Contains(DuckFile.levelDirectory.ToLowerInvariant()))
			{
				_customLoad = true;
			}
			_level = _level.Replace(Directory.GetCurrentDirectory() + "\\", "");
			dat = DuckFile.LoadLevel(_level);
		}
		return dat;
	}

	public override void Initialize()
	{
		AutoBlock._kBlockIndex = 0;
		if (base.level == "RANDOM" || cancelLoading)
		{
			return;
		}
		if (_data == null)
		{
			_data = LoadLevelDoc();
		}
		if (cancelLoading || _data == null)
		{
			return;
		}
		_id = _data.metaData.guid;
		if ((base.level == "WORKSHOP" || _customLoad || _customLevel) && !bareInitialize)
		{
			Global.PlayCustomLevel(_id);
		}
		Custom.ClearCustomData();
		Custom.previewData[CustomType.Block][0] = null;
		Custom.previewData[CustomType.Block][1] = null;
		Custom.previewData[CustomType.Block][2] = null;
		Custom.previewData[CustomType.Background][0] = null;
		Custom.previewData[CustomType.Background][1] = null;
		Custom.previewData[CustomType.Background][2] = null;
		Custom.previewData[CustomType.Platform][0] = null;
		Custom.previewData[CustomType.Platform][1] = null;
		Custom.previewData[CustomType.Platform][2] = null;
		Custom.previewData[CustomType.Parallax][0] = null;
		if (_data.customData != null)
		{
			if (_data.customData.customTileset01Data != null)
			{
				Custom.previewData[CustomType.Block][0] = _data.customData.customTileset01Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Block][0].GetTileData(), 0, CustomType.Block);
			}
			if (_data.customData.customTileset02Data != null)
			{
				Custom.previewData[CustomType.Block][1] = _data.customData.customTileset02Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Block][1].GetTileData(), 1, CustomType.Block);
			}
			if (_data.customData.customTileset03Data != null)
			{
				Custom.previewData[CustomType.Block][2] = _data.customData.customTileset03Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Block][2].GetTileData(), 2, CustomType.Block);
			}
			if (_data.customData.customBackground01Data != null)
			{
				Custom.previewData[CustomType.Background][0] = _data.customData.customBackground01Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Background][0].GetTileData(), 0, CustomType.Background);
			}
			if (_data.customData.customBackground02Data != null)
			{
				Custom.previewData[CustomType.Background][1] = _data.customData.customBackground02Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Background][1].GetTileData(), 1, CustomType.Background);
			}
			if (_data.customData.customBackground03Data != null)
			{
				Custom.previewData[CustomType.Background][2] = _data.customData.customBackground03Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Background][2].GetTileData(), 2, CustomType.Background);
			}
			if (_data.customData.customPlatform01Data != null)
			{
				Custom.previewData[CustomType.Platform][0] = _data.customData.customPlatform01Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][0].GetTileData(), 0, CustomType.Platform);
			}
			if (_data.customData.customPlatform02Data != null)
			{
				Custom.previewData[CustomType.Platform][1] = _data.customData.customPlatform02Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][1].GetTileData(), 1, CustomType.Platform);
			}
			if (_data.customData.customPlatform03Data != null)
			{
				Custom.previewData[CustomType.Platform][2] = _data.customData.customPlatform03Data;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][2].GetTileData(), 2, CustomType.Platform);
			}
			if (_data.customData.customParallaxData != null)
			{
				Custom.previewData[CustomType.Parallax][0] = _data.customData.customParallaxData;
				Custom.ApplyCustomData(Custom.previewData[CustomType.Parallax][0].GetTileData(), 0, CustomType.Parallax);
			}
		}
		if (cancelLoading)
		{
			return;
		}
		if (!bareInitialize && !isPreview)
		{
			preview = Editor.LoadPreview(_data.previewData.preview);
		}
		Random oldGen = Rando.generator;
		Rando.generator = new Random(seed);
		if (!bareInitialize && !isPreview)
		{
			GhostManager.context.ResetGhostIndex(networkIndex);
		}
		Thing.loadingLevel = _data;
		_ = _data.metaData.version;
		onlineEnabled = _data.metaData.online;
		bool willBeOnline = true;
		int idx = 0;
		foreach (BinaryClassChunk elly in _data.objects.objects)
		{
			if (cancelLoading)
			{
				return;
			}
			Thing t = Thing.LoadThing(elly);
			if (t == null || (_data.metaData.version < 1 && Thing.CheckForBozoData(t)))
			{
				continue;
			}
			if (!ContentProperties.GetBag(t.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true) || (t.serverOnly && !Network.isServer))
			{
				willBeOnline = false;
				if (Network.isActive)
				{
					continue;
				}
			}
			if (!bareInitialize || t is ArcadeMachine)
			{
				if (!t.visibleInGame && !ignoreVisibility)
				{
					t.visible = false;
				}
				if (Network.isActive)
				{
					if (t is ThingContainer)
					{
						foreach (Thing th in (t as ThingContainer).things)
						{
							NetPrepare(th);
						}
					}
					NetPrepare(t);
				}
				AddThing(t);
			}
			idx++;
		}
		Rando.generator = oldGen;
		if (willBeOnline)
		{
			onlineEnabled = true;
		}
		_things.RefreshState();
		Thing.loadingLevel = null;
	}

	private void NetPrepare(Thing pThing)
	{
		if (!bareInitialize && !isPreview)
		{
			if (pThing.isStateObject)
			{
				pThing.PrepareForHost();
			}
			else if (!(pThing is IDontMove))
			{
				specialSyncIndex++;
				pThing.specialSyncIndex = specialSyncIndex;
			}
		}
	}
}
