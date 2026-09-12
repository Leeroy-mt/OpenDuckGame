using DuckGame.Compatibility;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class SFX
{
    const int kMaxSounds = 32;

    #region Public Fields

    public static bool NoSoundcard;
    public static bool enabled = true;
    public static bool skip;

    public static Windows_Audio _audio;

    #endregion

    #region Private Fields

    static int _numProcessed;

    static float _volume = 1;

    static Speech _speech;

    static Dictionary<string, SoundEffect> _sounds = [];

    static Map<string, int> _soundHashmap = [];

    static Dictionary<string, MultiSoundUpdater> _multiSounds = [];

    static List<Sound> _soundPool = [];

    static List<Sound> _playedThisFrame = [];

    #endregion

    #region Public Properties

    public static bool hasTTS
    {
        get
        {
            if (Program.isLinux || speech == null)
                return false;

            return speech.GetSayVoices().Count > 0;
        }
    }

    public static float volume
    {
        get => Math.Min(1, Math.Max(0, _volume * _volume)) * 0.9f;
        set => _volume = Math.Min(1, Math.Max(0, value));
    }

    public static Speech speech
    {
        get
        {
            if (Program.isLinux)
                return null;

            if (_speech == null)
            {
                _speech = new Speech();
                _speech.Initialize();
                _speech.SetOutputToDefaultAudioDevice();
                _speech.ApplyTTSSettings();
            }

            return _speech;
        }
    }

    #endregion

    #region Public Methods

    public static void Say(string pString)
    {
        if (!Program.isLinux && speech != null)
            speech.Say(pString);
    }

    public static void StopSaying()
    {
        if (!Program.isLinux && speech != null)
            speech.StopSaying();
    }

    public static void SetSayVoice(string pName)
    {
        if (Program.isLinux || speech == null)
            return;

        try
        {
            speech.SetSayVoice(pName);
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, $"|DGRED|SFX.SetSayVoice failed:{ex.Message}");
        }
    }

    public static List<string> GetSayVoices()
    {
        if (Program.isLinux || speech == null)
            return [];

        return speech.GetSayVoices();
    }

    public static void ApplyTTSSettings()
    {
        if (!Program.isLinux && speech != null)
            speech.ApplyTTSSettings();
    }

    public static int RegisterSound(string pSound, SoundEffect pEffect)
    {
        var hash = NetFramework.GetHashCode(pSound);
        lock (_sounds)
        {
            _soundHashmap[pSound] = hash;
            _sounds[pSound] = pEffect;
            return hash;
        }
    }

    public static bool PoolSound(Sound s)
    {
        if (_soundPool.Count > kMaxSounds)
        {
            var unpooled = false;

            for (int i = 0; i < _soundPool.Count; i++)
            {
                if (!_soundPool[i].cannotBeCancelled)
                {
                    UnpoolSound(_soundPool[i]);
                    unpooled = true;
                    break;
                }
            }

            if (!unpooled)
                return false;
        }

        _soundPool.Add(s);

        return true;
    }

    public static void UnpoolSound(Sound s)
    {
        _soundPool.Remove(s);
        s.Unpooled();
    }

    public static void Initialize(IProgress<float> progress)
    {
        _audio = new Windows_Audio();
        _audio.Platform_Initialize();

        if (!Windows_Audio.initialized)
        {
            NoSoundcard = true;
            return;
        }

        SearchDir("Content/Audio/SFX", progress);
        NetSoundEffect.Initialize();
    }

    public static void Terminate()
    {
        _audio.Dispose();
    }

    public static void Update()
    {
        _playedThisFrame.Clear();

        for (int i = 0; i < _soundPool.Count; i++)
        {
            if (_soundPool[i].State != SoundState.Playing)
            {
                _soundPool[i].Stop();
                i--;
            }
        }

        foreach (var multiSound in _multiSounds)
            multiSound.Value.Update();

        _audio.Update();
    }

    /// <summary>
    /// Plays a sound effect, synchronized over the network (if the network is active)
    /// </summary>
    public static Sound PlaySynchronized(string sound, float vol = 1, float pitch = 0, float pan = 0, bool looped = false)
    {
        return PlaySynchronized(sound, vol, pitch, pan, looped, louderForMe: false);
    }

    /// <summary>
    /// Plays a sound effect, synchronized over the network (if the network is active)
    /// </summary>
    public static Sound PlaySynchronized(string sound, float vol, float pitch, float pan, bool looped, bool louderForMe)
    {
        if (!enabled)
            return new InvalidSound(sound, vol, pitch, pan, looped);

        if (Network.isActive)
            Send.Message(new NMSoundEffect(sound, louderForMe ? (vol * 0.7f) : vol, pitch));

        return Play(sound, vol, pitch, pan, looped);
    }

    public static Sound Play(string sound, float vol = 1, float pitch = 0, float pan = 0, bool looped = false)
    {
        if (!enabled || skip)
            return new InvalidSound(sound, vol, pitch, pan, looped);

        var s = _playedThisFrame.FirstOrDefault(x => x.name == sound);
        if (s == null)
        {
            try
            {
                s = Get(sound, vol, pitch, pan, looped);
                if (s != null)
                {
                    s.Play();
                    _playedThisFrame.Add(s);
                }
            }
            catch (Exception)
            {
                return new Sound(_sounds.FirstOrDefault().Key, 0, 0, 0, looped: false);
            }
        }

        return s;
    }

    public static Sound Play(int sound, float vol = 1, float pitch = 0, float pan = 0, bool looped = false)
    {
        if (_soundHashmap.TryGetKey(sound, out var foundSound))
            return Play(foundSound, vol, pitch, pan, looped);

        return new Sound(_sounds.FirstOrDefault().Key, 0, 0, 0, looped: false);
    }

    public static int SoundHash(string pSound)
    {
        _soundHashmap.TryGetValue(pSound, out var val);
        return val;
    }

    public static bool HasSound(string sound)
    {
        if (NoSoundcard)
            return false;

        if (!_sounds.TryGetValue(sound, out var effect))
        {
            if (!sound.Contains(':'))
                effect = Content.Load<SoundEffect>($"Audio/SFX/{sound}");

            if (effect == null && MonoMain.moddingEnabled && ModLoader.modsEnabled)
                effect = Content.Load<SoundEffect>(sound);

            RegisterSound(sound, effect);
        }

        return effect != null;
    }

    public static Sound Get(string sound, float vol = 1, float pitch = 0, float pan = 0, bool looped = false)
    {
        try
        {
            var voll = Math.Min(1, Math.Max(0, vol));

            if (HasSound(sound))
                return new Sound(sound, voll, pitch, pan, looped);

            return new InvalidSound(sound, voll, pitch, pan, looped);
        }
        catch
        {
            return new InvalidSound(sound, 0, pitch, pan, looped);
        }
    }

    public static MultiSound GetMultiSound(string single, string multi)
    {
        if (_multiSounds.ContainsKey(single + multi))
            return _multiSounds[single + multi].GetInstance();

        if (HasSound(single) && HasSound(multi))
        {
            MultiSoundUpdater newSound = new(single + multi, single, multi);
            _multiSounds[single + multi] = newSound;

            return newSound.GetInstance();
        }

        MultiSoundUpdater newSound2 = new("", "", "");
        _multiSounds[single + multi] = newSound2;
        return newSound2.GetInstance();
    }

    public static SoundEffectInstance GetInstance(string sound, float vol = 1, float pitch = 0, float pan = 0, bool looped = false)
    {
        var voll = Math.Min(1, Math.Max(0, vol));
        SoundEffectInstance soundEffectInstance = _sounds[sound].CreateInstance();
        soundEffectInstance.Volume = voll;
        soundEffectInstance.Pitch = pitch;
        soundEffectInstance.Pan = pan;
        soundEffectInstance.IsLooped = looped;
        return soundEffectInstance;
    }

    public static void StopAllSounds()
    {
        while (_soundPool.Count > 0)
            _soundPool[0].Stop();
    }

    public static void KillAllSounds()
    {
        while (_soundPool.Count > 0)
            _soundPool[0].Stop();
    }

    #endregion

    #region Private Methods

    static void SearchDir(string dir, IProgress<float> progress = null)
    {
        var files = Content.GetFiles(dir);
        for (int i = 0; i < files.Length; i++)
        {
            ProcessSoundEffect(files[i]);
            progress?.Report(i / (float)files.Length);
        }

        files = Content.GetDirectories(dir);
        for (int i = 0; i < files.Length; i++)
            SearchDir(files[i]);
    }

    static void ProcessSoundEffect(string path)
    {
        _numProcessed++;
        path = path.Replace('\\', '/');
        var start = path.IndexOf("Content/Audio/", 0);
        var fileName = path[(start + 8)..];
        fileName = fileName[..^4];
        SoundEffect soundEffect = Content.Load<SoundEffect>(fileName);

        if (soundEffect != null)
            RegisterSound(fileName[(fileName.IndexOf("/SFX/") + 5)..], soundEffect);
    }

    #endregion
}