using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class ContentPack
{
	public static long kTotalKilobytesAllocated;

	public long kilobytesPreAllocated;

	public List<string> levels = new List<string>();

	protected Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

	protected Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();

	protected Dictionary<string, Song> _songs = new Dictionary<string, Song>();

	protected ModConfiguration _modConfig;

	public static ContentPack currentPreloadPack;

	private long _beginCalculatingAllocatedBytes;

	public ContentPack(ModConfiguration modConfiguration)
	{
		_modConfig = modConfiguration;
	}

	public void ImportAsset(string path, byte[] data)
	{
		try
		{
			string assetName = path.Substring(0, path.Length - 4);
			if (path.EndsWith(".png"))
			{
				Texture2D tex = TextureConverter.LoadPNGWithPinkAwesomeness(Graphics.device, new Bitmap(new MemoryStream(data)), process: true);
				_textures[assetName] = tex;
				Content.textures[assetName] = tex;
			}
			else if (path.EndsWith(".wav"))
			{
				SoundEffect snd = SoundEffect.FromStream(new MemoryStream(data));
				if (snd != null)
				{
					snd.file = path;
					_sounds[assetName] = snd;
					SFX.RegisterSound(assetName, snd);
				}
				else
				{
					DevConsole.Log(DCSection.General, "|DGRED|Failed to load sound effect! (" + path + ")");
				}
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Called when the mod is loaded to preload content. This is only called if preload is set to true.
	/// </summary>
	public virtual void PreloadContent()
	{
		PreloadContentPaths();
	}

	/// <summary>
	/// Called when the mod is loaded to preload the paths to all content. Does not actually load content, and is only called if PreloadContent is disabled (looks like that's a lie, this function loads the content).
	/// </summary>
	public virtual void PreloadContentPaths()
	{
		List<string> files = Content.GetFiles<Texture2D>(_modConfig.contentDirectory);
		_ = _modConfig.contentDirectory.Length;
		foreach (string s in files)
		{
			Texture2D tex = LoadTexture2DInternal(s);
			string texName = s.Substring(0, s.Length - 4);
			_textures[texName] = tex;
			Content.textures[texName] = tex;
		}
		foreach (string s2 in Content.GetFiles<SoundEffect>(_modConfig.contentDirectory))
		{
			MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
			{
				currentPreloadPack = this;
				SoundEffect soundEffect = LoadSoundInternal(s2);
				string text = s2.Substring(0, s2.Length - 4);
				_sounds[text] = soundEffect;
				SFX.RegisterSound(text, soundEffect);
			}));
		}
		string levDir = _modConfig.contentDirectory + "/Levels";
		if (DuckFile.DirectoryExists(levDir))
		{
			levels = Content.GetFiles<Level>(levDir);
		}
		MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
		{
			currentPreloadPack = null;
			if (kilobytesPreAllocated / 1000 > 20)
			{
				MonoMain.CalculateModMemoryOffendersList();
			}
		}));
	}

	private static Texture2D LoadTexture2DInternal(string file, bool processPink = true)
	{
		Texture2D t = null;
		try
		{
			return TextureConverter.LoadPNGWithPinkAwesomeness(Graphics.device, file, processPink);
		}
		catch (Exception ex)
		{
			throw new Exception("PNG Load Fail(" + Path.GetFileName(file) + "): " + ex.Message, ex);
		}
	}

	public static Texture2D LoadTexture2DFromStream(Stream data, bool processPink = true)
	{
		Texture2D t = null;
		try
		{
			return TextureConverter.LoadPNGWithPinkAwesomeness(Graphics.device, data, processPink);
		}
		catch (Exception ex)
		{
			throw new Exception("PNG Load Fail: " + ex.Message, ex);
		}
	}

	public static PNGData LoadPNGDataFromStream(Stream data, bool processPink = true)
	{
		PNGData t = null;
		try
		{
			return TextureConverter.LoadPNGDataWithPinkAwesomeness(data, processPink);
		}
		catch (Exception ex)
		{
			throw new Exception("PNG Load Fail: " + ex.Message, ex);
		}
	}

	public static Texture2D LoadTexture2D(string name, bool processPink = true)
	{
		Texture2D t = null;
		if (!name.EndsWith(".png"))
		{
			name += ".png";
		}
		if (File.Exists(name))
		{
			try
			{
				t = LoadTexture2DInternal(name);
			}
			catch (Exception ex)
			{
				DevConsole.Log(DCSection.General, "LoadTexure2D Error (" + name + "): " + ex.Message);
			}
		}
		return t;
	}

	internal SoundEffect LoadSoundInternal(string file)
	{
		SoundEffect s = null;
		try
		{
			s = ((new FileInfo(file).Length <= 5000000) ? SoundEffect.FromStream(new MemoryStream(File.ReadAllBytes(file)), Path.GetExtension(file)) : SoundEffect.FromStream(new FileStream(file, FileMode.Open), Path.GetExtension(file)));
			if (s != null)
			{
				s.file = file;
			}
		}
		catch (Exception)
		{
		}
		return s;
	}

	internal SoundEffect LoadSoundEffect(string name)
	{
		SoundEffect s = null;
		if (Path.GetExtension(name) == "")
		{
			name += ".wav";
		}
		if (File.Exists(name))
		{
			s = LoadSoundInternal(name);
		}
		return s;
	}

	internal Song LoadSongInternal(string file)
	{
		Song s = null;
		try
		{
			MemoryStream stream = OggSong.Load(file, localContent: false);
			if (stream != null)
			{
				s = new Song(stream, file);
			}
		}
		catch
		{
		}
		return s;
	}

	internal Song LoadSong(string name)
	{
		Song s = null;
		if (!name.EndsWith(".ogg"))
		{
			name += ".ogg";
		}
		if (File.Exists(name))
		{
			s = LoadSongInternal(name);
		}
		return s;
	}

	/// <summary>
	/// Loads content from the content pack. Currently supports Texture2D(png) and SoundEffect(wav) in
	/// "mySound" "customSounds/mySound" path format. You should usually use Content.Load&lt;&gt;().
	/// </summary>
	public virtual T Load<T>(string name)
	{
		if (typeof(T) == typeof(Texture2D))
		{
			Texture2D t = null;
			if (_textures.TryGetValue(name, out t))
			{
				return (T)(object)t;
			}
			Texture2D tex = LoadTexture2D(name, _modConfig == null || _modConfig.processPinkTransparency);
			_textures[name] = tex;
			return (T)(object)tex;
		}
		if (typeof(T) == typeof(SoundEffect))
		{
			SoundEffect s = null;
			if (_sounds.TryGetValue(name, out s))
			{
				return (T)(object)s;
			}
			SoundEffect snd = LoadSoundEffect(name);
			_sounds[name] = snd;
			return (T)(object)snd;
		}
		if (typeof(T) == typeof(Song))
		{
			Song s2 = null;
			if (_songs.TryGetValue(name, out s2))
			{
				return (T)(object)s2;
			}
			Song song = LoadSong(name);
			_songs[name] = song;
			return (T)(object)song;
		}
		return default(T);
	}
}
