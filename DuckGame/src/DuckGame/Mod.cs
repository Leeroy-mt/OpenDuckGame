using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

/// <summary>
/// The base class for mod information. Each mod has a custom instance of this class.
/// </summary>
public abstract class Mod
{
	/// <summary>
	/// Provides some mod debugging logic
	/// </summary>
	public static class Debug
	{
		[DllImport("kernel32.dll")]
		private static extern void OutputDebugString(string lpOutputString);

		/// <summary>
		/// Logs the specified line to any attached debuggers.
		/// If "-moddebug" is specified this will also output
		/// to the dev console in ~
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="objs">The format parameters.</param>
		public static void Log(string format, params object[] objs)
		{
			if (MonoMain.modDebugging)
			{
				string str = string.Format(format, objs);
				DevConsole.Log(DCSection.Mod, str);
			}
		}
	}

	private Dictionary<Type, List<Type>> _typeLists = new Dictionary<Type, List<Type>>();

	private uint _dataHash;

	private uint _thingHash;

	private uint _netMessageHash;

	private WorkshopMetaData _workshopData = new WorkshopMetaData();

	/// <summary>
	/// The property bag for this mod. Other mods may view and read from this collection.
	/// You must not edit this bag while the game is running, only during mod initialization.
	/// </summary>
	protected readonly PropertyBag _properties = new PropertyBag();

	private Priority _priority = Priority.Normal;

	public bool clientMod;

	private Tex2D _previewTexture;

	private Tex2D _screenshot;

	private Map<ushort, Type> _typeToMessageID = new Map<ushort, Type>();

	private Map<ushort, ConstructorInfo> _constructorToMessageID = new Map<ushort, ConstructorInfo>();

	public ushort currentMessageIDIndex = 1;

	private uint _identifierHash;

	/// <summary>
	/// A hash of all the Thing type names + NetMessage type names in this mod
	/// </summary>
	public uint dataHash
	{
		get
		{
			if (_dataHash == 0)
			{
				_dataHash = (thingHash + netMessageHash) % uint.MaxValue;
			}
			return _dataHash;
		}
	}

	/// <summary>
	/// A hash of all the Thing type names in this mod
	/// </summary>
	public uint thingHash
	{
		get
		{
			if (_thingHash == 0)
			{
				string thingHashTypeString = "";
				foreach (Type t in GetTypeList(typeof(Thing)))
				{
					thingHashTypeString += t.Name;
				}
				_thingHash = CRC32.Generate(thingHashTypeString);
			}
			return _thingHash;
		}
	}

	/// <summary>
	/// A hash of all the NetMessage type names in this mod
	/// </summary>
	public uint netMessageHash
	{
		get
		{
			if (_netMessageHash == 0)
			{
				string messageHashString = "";
				foreach (KeyValuePair<ushort, Type> item in typeToMessageID)
				{
					messageHashString += item.Value.Name;
				}
				_netMessageHash = CRC32.Generate(messageHashString);
			}
			return _netMessageHash;
		}
	}

	/// <summary>
	/// Used by the mod upload window, you shouldn't need this.
	/// </summary>
	public WorkshopMetaData workshopData => _workshopData;

	public string generateAndGetPathToScreenshot
	{
		get
		{
			string screenshotPath = configuration.directory;
			DuckFile.CreatePath(screenshotPath);
			screenshotPath += "screenshot.png";
			if (!File.Exists(screenshotPath))
			{
				if (configuration.modType == ModConfiguration.Type.MapPack && configuration.mapPack != null)
				{
					screenshotPath = configuration.mapPack.RegeneratePreviewImage(null);
				}
				else
				{
					screenshotPath = configuration.directory + "/content/";
					DuckFile.CreatePath(screenshotPath);
					screenshotPath += "screenshot.png";
				}
			}
			if (screenshotPath == null)
			{
				return "";
			}
			if (!File.Exists(screenshotPath))
			{
				File.Delete(screenshotPath);
				Tex2D texData = screenshot;
				Stream stream = DuckFile.Create(screenshotPath);
				((Texture2D)texData.nativeObject).SaveAsPng(stream, texData.width, texData.height);
				stream.Dispose();
			}
			return screenshotPath;
		}
	}

	/// <summary>
	/// The read-only property bag that this mod was initialized with.
	/// </summary>
	public IReadOnlyPropertyBag properties => _properties;

	/// <summary>
	/// The configuration class for this mod
	/// </summary>
	public ModConfiguration configuration { get; internal set; }

	/// <summary>
	/// The priority of this mod as compared to other mods.
	/// </summary>
	/// <value>
	/// The priority.
	/// </value>
	public virtual Priority priority => _priority;

	/// <summary>
	/// The workshop IDs of this mods parent mod. This is useful for DEV versions of mods, and will allow the parent mod's levels to be played in this mod and vice-versa.
	/// </summary>
	public virtual ulong workshopIDFacade => 0uL;

	/// <summary>
	/// All objects serialized and deserialized from this mod will use the namsepaceFacade instead of their actual namespace if this is set.
	/// For example, if your namespace is 'MyModDev', you could say namespaceFacade = 'MyModDev:MyMod' to drop the 'Dev' part during serialization.
	/// Therefore the format is 'MYNAMESPACENAME:FAKENAMESPACENAME'
	/// </summary>
	public virtual string namespaceFacade => null;

	/// <summary>
	///  All objects serialized and deserialized from this mod will use the assemblyNameFacade instead of the actual name of this assembly.
	/// For example, if your Assembly is named 'MyModDEV', you could say assemblyNameFacade = 'MyMod' to drop the 'DEV' part during serialization.
	/// </summary>
	public virtual string assemblyNameFacade => null;

	/// <summary>
	/// Gets the preview texture for this mod.
	/// </summary>
	/// <value>
	/// The preview texture.
	/// </value>
	public virtual Tex2D previewTexture
	{
		get
		{
			if (_previewTexture == null)
			{
				if (configuration.loaded)
				{
					if (configuration.contentDirectory != null)
					{
						_previewTexture = ContentPack.LoadTexture2D(GetPath("preview") + ".png", processPink: false);
					}
					if (_previewTexture == null)
					{
						_previewTexture = Content.Load<Tex2D>("notexture");
					}
				}
				else
				{
					_previewTexture = Content.Load<Tex2D>("none");
				}
			}
			return _previewTexture;
		}
		protected set
		{
			_previewTexture = value;
		}
	}

	/// <summary>
	/// Gets path for screenshot.png from Content folder.
	/// </summary>
	/// <value>
	/// Path for mod screenshot.png from Content folder.
	/// </value>
	public virtual Tex2D screenshot
	{
		get
		{
			if (_screenshot == null)
			{
				if (configuration.loaded)
				{
					if (configuration.contentDirectory != null)
					{
						string shotPath = GetPath("screenshot") + ".png";
						if (File.Exists(shotPath))
						{
							_screenshot = Content.Load<Tex2D>(shotPath);
						}
					}
					if (_screenshot == null)
					{
						_screenshot = Content.Load<Tex2D>("defaultMod");
					}
				}
				else
				{
					_screenshot = null;
				}
			}
			return _screenshot;
		}
	}

	public Map<ushort, Type> typeToMessageID => _typeToMessageID;

	public Map<ushort, ConstructorInfo> constructorToMessageID => _constructorToMessageID;

	public uint identifierHash
	{
		get
		{
			if (_identifierHash == 0 && configuration != null)
			{
				_identifierHash = CRC32.Generate(configuration.uniqueID);
			}
			return _identifierHash;
		}
	}

	public void System_RuinDatahash()
	{
		_dataHash = (uint)Rando.Int(9999999);
		DevConsole.Log("|DGRED|Mod.System_RuinDatahash called!");
	}

	public List<Type> GetTypeList(Type pType)
	{
		List<Type> list = null;
		if (!_typeLists.TryGetValue(pType, out list))
		{
			List<Type> list2 = (_typeLists[pType] = new List<Type>());
			list = list2;
		}
		return list;
	}

	/// <summary>
	/// Returns a formatted path that leads to the "asset" parameter in a given mod.
	/// </summary>
	public static string GetPath<T>(string asset) where T : Mod
	{
		return ModLoader.GetMod<T>().configuration.contentDirectory + asset.Replace('\\', '/');
	}

	/// <summary>
	/// Returns a formatted path that leads to the "asset" parameter in this mod.
	/// </summary>
	public string GetPath(string asset)
	{
		return configuration.contentDirectory + asset.Replace('\\', '/');
	}

	public void SetPriority(Priority pPriority)
	{
		_priority = pPriority;
	}

	/// <summary>
	/// The constructor for the Mod. Do not call any functions or use Reflection in here, as the core and mods
	/// may not be ready to use yet. Use the proper callbacks.
	/// </summary>
	protected Mod()
	{
	}

	/// <summary>
	/// Called on a mod when all mods and the core are finished being created
	/// and are ready to be initialized. You may use game functions and Reflection
	/// in here safely. Note that during this method, not all mods may have ran
	/// their pre-initialization routines and may not have sent their content to
	/// the core. Ideally, you will want to set up your properties here.
	/// </summary>
	protected virtual void OnPreInitialize()
	{
	}

	/// <summary>
	/// Called on a mod after all mods have finished their pre-initialization
	/// and have sent their content to the core.
	/// </summary>
	protected virtual void OnPostInitialize()
	{
	}

	/// <summary>
	/// Called on a mod after everything has been loaded and the first Level has been set
	/// </summary>
	protected virtual void OnStart()
	{
	}

	internal void InvokeOnPreInitialize()
	{
		OnPreInitialize();
	}

	internal void InvokeOnPostInitialize()
	{
		OnPostInitialize();
	}

	internal void InvokeStart()
	{
		OnStart();
	}
}
