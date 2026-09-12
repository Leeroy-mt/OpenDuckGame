using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class Music
{
    public static MusicInstance _musicPlayer;

    #region Private Fields

    static bool _alternateLoop;

    static float _fadeSpeed;
    static float _volume = 1;
    static float _volumeMult = 1;
    static float _masterVolume = 0.65f;

    static string _currentSong = "";
    static string _pendingSong = "";
    static string _alternateSong = "";

    static Random _musicPickGen = new();
    static SoundEffect _currentMusic;

    static string[] _songList;

    static Dictionary<string, MemoryStream> _songs = [];
    static Dictionary<string, Queue<string>> _recentSongs = [];
    static HashSet<string> _processedSongs = [];

    #endregion

    #region Public Properties

    public static bool stopped
    {
        get
        {
            if (_musicPlayer.State != SoundState.Stopped)
                return _musicPlayer.State == SoundState.Paused;
            return true;
        }
    }
    public static bool finished => _musicPlayer.State == SoundState.Stopped;

    public static float volumeMult
    {
        get => _volumeMult;
        set
        {
            _volumeMult = value;
            volume = _volume;
        }
    }
    public static float volume
    {
        get => _volume;
        set
        {
            _volume = value;
            _musicPlayer?.Volume = _volume * (_masterVolume * _masterVolume) * _volumeMult;
        }
    }
    public static float masterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            volume = _volume;
        }
    }

    public static string currentSong => _currentSong;

    public static string pendingSong => _pendingSong;

    public static TimeSpan position => new(0, 0, 0, 0, (int)(_musicPlayer.Platform_GetProgress() * _musicPlayer.Platform_GetLengthInMilliseconds()));

    public static Dictionary<string, MemoryStream> songs => _songs;

    #endregion

    #region Public Methods

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
        foreach (var song2 in _songs)
            song2.Value.Close();
    }

    public static string RandomTrack(string folder, string ignore = "")
    {
        if (DevConsole.rhythmMode)
            return "InGame/comic.ogg";

        var songList = _songList;
        if (ReskinPack.active.Count > 0)
        {
            List<string> moreSongs = [];

            foreach (ReskinPack p in ReskinPack.active)
                moreSongs.AddRange(DuckFile.GetFiles(p.contentPath + "/Audio/Music/InGame"));

            if (moreSongs.Count > 0)
                songList = moreSongs.ToArray();
        }

        if (songList.Length == 0)
            return "";

        var oldRando = Rando.Generator;
        Rando.Generator = _musicPickGen;
        List<string> songs = [];
        var array = songList;
        foreach (var song in array)
        {
            var s = $"{folder}/{Path.GetFileNameWithoutExtension(song)}";
            if (s != ignore)
                songs.Add(s);
        }

        if (songs.Count == 0)
            songs.Add($"{folder}/{Path.GetFileNameWithoutExtension(songList[0])}");

        if (!_recentSongs.TryGetValue(folder, out var recentSongs))
        {
            recentSongs = new Queue<string>();
            _recentSongs[folder] = recentSongs;
        }

        if (recentSongs.Count > 0 && recentSongs.Count > songs.Count - 5)
            recentSongs.Dequeue();

        List<string> validSongs = [.. songs];
        var curSong = "";
        while (curSong == "")
        {
            if (songs.Count == 0 && recentSongs.Count > 0)
            {
                curSong = recentSongs.Dequeue();
                if (!validSongs.Contains(curSong))
                    curSong = "";
                continue;
            }

            if (songs.Count == 0)
            {
                curSong = validSongs[0];
                continue;
            }

            curSong = songs[Rando.Int(songs.Count - 1)];
            if (curSong == ignore && songs.Count > 1)
            {
                songs.Remove(curSong);
                curSong = "";
            }
            else
            {
                if (!recentSongs.Contains(curSong))
                    continue;

                if (Rando.Float(1) > 0.25f)
                {
                    songs.Remove(curSong);
                    if (songs.Count > 0)
                        curSong = "";
                }
                else
                {
                    curSong = "";
                }
            }
        }
        recentSongs.Enqueue(curSong);
        Rando.Generator = oldRando;
        return curSong;
    }

    public static string FindSong(string song)
    {
        var songList = _songList;
        for (int i = 0; i < songList.Length; i++)
        {
            var shortSong = Path.GetFileNameWithoutExtension(songList[i]);
            if (shortSong.ToLower() == song.ToLower())
                return $"InGame/{shortSong}";
        }
        return "Challenging";
    }

    public static void Play(string music, bool looping = true, float crossFadeTime = 0)
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

    public static bool Load(string music, bool looping = true, float crossFadeTime = 0)
    {
        _currentSong = music;
        _musicPlayer.Stop();
        if (!music.Contains(':') && !music.EndsWith(".wav"))
        {
            try
            {
                var fullName = $"Audio/Music/{music}";
                try
                {
                    _currentMusic = ReskinPack.LoadAsset<SoundEffect>($"{fullName}.ogg", pMusic: true);
                    _currentMusic ??= ReskinPack.LoadAsset<SoundEffect>($"{fullName}.mp3", pMusic: true);
                }
                catch
                {
                }

                if (_currentMusic == null)
                {
                    fullName = $"{DuckFile.contentDirectory}{fullName}";
                    _currentMusic = new SoundEffect($"{fullName}.ogg");
                }
            }
            catch (Exception ex2)
            {
                DevConsole.Log(DCSection.General, $"|DGRED|Failed to load music ({music}):");
                DevConsole.Log(DCSection.General, $"|DGRED|{ex2.Message}");
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

    public static void LoadAlternateSong(string music, bool looping = true, float crossFadeTime = 0)
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
        _fadeSpeed = duration / 60;
    }

    public static void FadeIn(float duration)
    {
        _fadeSpeed = 0 - duration / 60;
    }

    public static void Update()
    {
    }

    #endregion

    #region Private Methods

    static void SearchDir(string dir)
    {
        var files = Content.GetFiles(dir);
        for (int i = 0; i < files.Length; i++)
            ProcessSong(files[i]);

        files = Content.GetDirectories(dir);
        for (int i = 0; i < files.Length; i++)
            SearchDir(files[i]);
    }

    static void ProcessSong(string path)
    {
        if (ReskinPack.context != null)
        {
            if (ReskinPack.context.hasIngameMusic && !path.Contains(':') && path.Contains("Audio/Music/InGame"))
                return;

            string p = path;
            if (p.StartsWith("Content"))
                p = p[7..];

            p = ReskinPack.context.contentPath + p;
            if (DuckFile.FileExists(p))
                path = p;
        }

        path = path.Replace('\\', '/');
        if (!_processedSongs.Contains(path))
        {
            _processedSongs.Add(path);
            try
            {
                var soundStream = OggSong.Load(path, !path.Contains(':'));
                path = path[..^4];
                var shortName = path[(path.IndexOf("/Music/") + 7)..];
                _songs[shortName] = soundStream;
            }
            catch
            {
                DevConsole.Log(DCSection.General, $"Failed to load song: {path}");
            }
        }
    }

    #endregion
}