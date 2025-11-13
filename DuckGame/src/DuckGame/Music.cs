using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace DuckGame;

public class Music
{
	private static Dictionary<string, MemoryStream> _songs = new Dictionary<string, MemoryStream>();

	private static Dictionary<string, Queue<string>> _recentSongs = new Dictionary<string, Queue<string>>();

	private static float _fadeSpeed;

	private static float _volume = 1f;

	private static float _volumeMult = 1f;

	private static float _masterVolume = 0.65f;

	private static string _currentSong = "";

	private static string _pendingSong = "";

	private static string[] _songList;

	private static Random _musicPickGen = new Random();

	private static SoundEffect _currentMusic = null;

	public static MusicInstance _musicPlayer = null;

	private static bool _alternateLoop = false;

	private static string _alternateSong = "";

	private static HashSet<string> _processedSongs = new HashSet<string>();

	public static Dictionary<string, MemoryStream> songs => _songs;

	public static bool stopped
	{
		get
		{
			if (_musicPlayer.State != SoundState.Stopped)
			{
				return _musicPlayer.State == SoundState.Paused;
			}
			return true;
		}
	}

	public static float volumeMult
	{
		get
		{
			return _volumeMult;
		}
		set
		{
			_volumeMult = value;
			volume = _volume;
		}
	}

	public static float volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
			if (_musicPlayer != null)
			{
				_musicPlayer.Volume = _volume * (_masterVolume * _masterVolume) * _volumeMult;
			}
		}
	}

	public static float masterVolume
	{
		get
		{
			return _masterVolume;
		}
		set
		{
			_masterVolume = value;
			volume = _volume;
		}
	}

	public static string currentSong => _currentSong;

	public static string pendingSong => _pendingSong;

	public static TimeSpan position => new TimeSpan(0, 0, 0, 0, (int)(_musicPlayer.Platform_GetProgress() * (float)_musicPlayer.Platform_GetLengthInMilliseconds()));

	public static bool finished => _musicPlayer.State == SoundState.Stopped;

	public static void Reset()
	{
		_recentSongs.Clear();
	}

	public static void Initialize()
	{
		_musicPlayer = new MusicInstance(null);
		_songList = Content.GetFiles("Content/Audio/Music/InGame");
	}

	public static void PreloadSongs()
	{
	}

	public static void Terminate()
	{
		foreach (KeyValuePair<string, MemoryStream> song2 in _songs)
		{
			song2.Value.Close();
		}
	}

	public static string RandomTrack(string folder, string ignore = "")
	{
		if (DevConsole.rhythmMode)
		{
			return "InGame/comic.ogg";
		}
		string[] songList = _songList;
		if (ReskinPack.active.Count > 0)
		{
			List<string> moreSongs = new List<string>();
			foreach (ReskinPack p in ReskinPack.active)
			{
				moreSongs.AddRange(DuckFile.GetFiles(p.contentPath + "/Audio/Music/InGame"));
			}
			if (moreSongs.Count > 0)
			{
				songList = moreSongs.ToArray();
			}
		}
		if (songList.Length == 0)
		{
			return "";
		}
		Random oldRando = Rando.generator;
		Rando.generator = _musicPickGen;
		List<string> songs = new List<string>();
		string[] array = songList;
		foreach (string song in array)
		{
			string s = folder + "/" + Path.GetFileNameWithoutExtension(song);
			if (s != ignore)
			{
				songs.Add(s);
			}
		}
		if (songs.Count == 0)
		{
			songs.Add(folder + "/" + Path.GetFileNameWithoutExtension(songList[0]));
		}
		Queue<string> recentSongs = null;
		if (!_recentSongs.TryGetValue(folder, out recentSongs))
		{
			recentSongs = new Queue<string>();
			_recentSongs[folder] = recentSongs;
		}
		if (recentSongs.Count > 0 && recentSongs.Count > songs.Count - 5)
		{
			recentSongs.Dequeue();
		}
		List<string> validSongs = new List<string>();
		validSongs.AddRange(songs);
		string curSong = "";
		while (curSong == "")
		{
			if (songs.Count == 0 && recentSongs.Count > 0)
			{
				curSong = recentSongs.Dequeue();
				if (!validSongs.Contains(curSong))
				{
					curSong = "";
				}
				continue;
			}
			if (songs.Count == 0)
			{
				curSong = validSongs[0];
				continue;
			}
			curSong = songs[Rando.Int(songs.Count() - 1)];
			if (curSong == ignore && songs.Count > 1)
			{
				songs.Remove(curSong);
				curSong = "";
			}
			else
			{
				if (!recentSongs.Contains(curSong))
				{
					continue;
				}
				if (Rando.Float(1f) > 0.25f)
				{
					songs.Remove(curSong);
					if (songs.Count > 0)
					{
						curSong = "";
					}
				}
				else
				{
					curSong = "";
				}
			}
		}
		recentSongs.Enqueue(curSong);
		Rando.generator = oldRando;
		return curSong;
	}

	public static string FindSong(string song)
	{
		string[] songList = _songList;
		for (int i = 0; i < songList.Length; i++)
		{
			string shortSong = Path.GetFileNameWithoutExtension(songList[i]);
			if (shortSong.ToLower() == song.ToLower())
			{
				return "InGame/" + shortSong;
			}
		}
		return "Challenging";
	}

	public static void Play(string music, bool looping = true, float crossFadeTime = 0f)
	{
		if (Load(music))
		{
			_musicPlayer.Play();
			_musicPlayer.IsLooped = looping;
		}
	}

	public static void Play(Song music, bool looping = true)
	{
	}

	public static bool Load(string music, bool looping = true, float crossFadeTime = 0f)
	{
		_currentSong = music;
		_musicPlayer.Stop();
		if (!music.Contains(":") && !music.EndsWith(".wav"))
		{
			try
			{
				string fullName = "Audio/Music/" + music;
				try
				{
					_currentMusic = ReskinPack.LoadAsset<SoundEffect>(fullName + ".ogg", pMusic: true);
					if (_currentMusic == null)
					{
						_currentMusic = ReskinPack.LoadAsset<SoundEffect>(fullName + ".mp3", pMusic: true);
					}
				}
				catch (Exception)
				{
				}
				if (_currentMusic == null)
				{
					fullName = DuckFile.contentDirectory + fullName;
					_currentMusic = new SoundEffect(fullName + ".ogg");
				}
			}
			catch (Exception ex2)
			{
				DevConsole.Log(DCSection.General, "|DGRED|Failed to load music (" + music + "):");
				DevConsole.Log(DCSection.General, "|DGRED|" + ex2.Message);
			}
		}
		else
		{
			_currentMusic = new SoundEffect(music);
		}
		_musicPlayer.SetData(_currentMusic);
		return true;
	}

	public static void PlayLoaded()
	{
		_musicPlayer.Play();
	}

	public static void CancelLooping()
	{
		_musicPlayer.IsLooped = false;
	}

	public static void LoadAlternateSong(string music, bool looping = true, float crossFadeTime = 0f)
	{
		_alternateLoop = looping;
		_pendingSong = music;
		_alternateSong = music;
	}

	public static void SwitchSongs()
	{
		try
		{
			Play(_pendingSong, _alternateLoop);
		}
		catch
		{
		}
		_pendingSong = null;
	}

	public static void Pause()
	{
		_musicPlayer.Pause();
	}

	public static void Resume()
	{
		_musicPlayer.Resume();
	}

	public static void Stop()
	{
		_musicPlayer.Stop();
		_currentSong = "";
	}

	public static void FadeOut(float duration)
	{
		_fadeSpeed = duration / 60f;
	}

	public static void FadeIn(float duration)
	{
		_fadeSpeed = 0f - duration / 60f;
	}

	private static void SearchDir(string dir)
	{
		string[] files = Content.GetFiles(dir);
		for (int i = 0; i < files.Length; i++)
		{
			ProcessSong(files[i]);
		}
		files = Content.GetDirectories(dir);
		for (int i = 0; i < files.Length; i++)
		{
			SearchDir(files[i]);
		}
	}

	private static void ProcessSong(string path)
	{
		if (ReskinPack.context != null)
		{
			if (ReskinPack.context.hasIngameMusic && !path.Contains(":") && path.Contains("Audio/Music/InGame"))
			{
				return;
			}
			string p = path;
			if (p.StartsWith("Content"))
			{
				p = p.Substring(7, p.Length - 7);
			}
			p = ReskinPack.context.contentPath + p;
			if (DuckFile.FileExists(p))
			{
				path = p;
			}
		}
		path = path.Replace('\\', '/');
		if (!_processedSongs.Contains(path))
		{
			_processedSongs.Add(path);
			try
			{
				MemoryStream sound = OggSong.Load(path, !path.Contains(":"));
				path = path.Substring(0, path.Length - 4);
				string shortName = path.Substring(path.IndexOf("/Music/") + 7);
				_songs[shortName] = sound;
			}
			catch (Exception)
			{
				DevConsole.Log(DCSection.General, "Failed to load song: " + path);
			}
			MonoMain.loadyBits++;
		}
	}

	public static void Update()
	{
	}
}
