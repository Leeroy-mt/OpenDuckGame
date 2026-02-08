using DuckGame.Compatibility;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class SFX
{
    private static Speech _speech;

    private static Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();

    private static Map<string, int> _soundHashmap = new Map<string, int>();

    private static Dictionary<string, MultiSoundUpdater> _multiSounds = new Dictionary<string, MultiSoundUpdater>();

    private static List<Sound> _soundPool = new List<Sound>();

    private const int kMaxSounds = 32;

    private static List<Sound> _playedThisFrame = new List<Sound>();

    public static Windows_Audio _audio;

    public static bool NoSoundcard = false;

    private static float _volume = 1f;

    public static bool enabled = true;

    public static bool skip = false;

    private static int _numProcessed = 0;

    public static Speech speech
    {
        get
        {
            if (Program.isLinux)
            {
                return null;
            }
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

    public static bool hasTTS
    {
        get
        {
            if (Program.isLinux || speech == null)
            {
                return false;
            }
            return speech.GetSayVoices().Count > 0;
        }
    }

    public static float volume
    {
        get
        {
            return Math.Min(1f, Math.Max(0f, _volume * _volume)) * 0.9f;
        }
        set
        {
            _volume = Math.Min(1f, Math.Max(0f, value));
        }
    }

    public static void Say(string pString)
    {
        if (!Program.isLinux && speech != null)
        {
            speech.Say(pString);
        }
    }

    public static void StopSaying()
    {
        if (!Program.isLinux && speech != null)
        {
            speech.StopSaying();
        }
    }

    public static void SetSayVoice(string pName)
    {
        if (Program.isLinux || speech == null)
        {
            return;
        }
        try
        {
            speech.SetSayVoice(pName);
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "|DGRED|SFX.SetSayVoice failed:" + ex.Message);
        }
    }

    public static List<string> GetSayVoices()
    {
        if (Program.isLinux || speech == null)
        {
            return new List<string>();
        }
        return speech.GetSayVoices();
    }

    public static void ApplyTTSSettings()
    {
        if (!Program.isLinux && speech != null)
        {
            speech.ApplyTTSSettings();
        }
    }

    public static int RegisterSound(string pSound, SoundEffect pEffect)
    {
        int hash = NetFramework.GetHashCode(pSound);
        lock (_sounds)
        {
            _soundHashmap[pSound] = hash;
            _sounds[pSound] = pEffect;
            return hash;
        }
    }

    public static bool PoolSound(Sound s)
    {
        if (_soundPool.Count > 32)
        {
            bool unpooled = false;
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
            {
                return false;
            }
        }
        _soundPool.Add(s);
        return true;
    }

    public static void UnpoolSound(Sound s)
    {
        _soundPool.Remove(s);
        s.Unpooled();
    }

    public static void Initialize()
    {
        _audio = new Windows_Audio();
        _audio.Platform_Initialize();
        if (!Windows_Audio.initialized)
        {
            NoSoundcard = true;
            return;
        }
        //MonoMain.loadMessage = "Loading SFX";
        SearchDir("Content/Audio/SFX");
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
        foreach (KeyValuePair<string, MultiSoundUpdater> multiSound in _multiSounds)
        {
            multiSound.Value.Update();
        }
        _audio.Update();
    }

    /// <summary>
    /// Plays a sound effect, synchronized over the network (if the network is active)
    /// </summary>
    public static Sound PlaySynchronized(string sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        return PlaySynchronized(sound, vol, pitch, pan, looped, louderForMe: false);
    }

    /// <summary>
    /// Plays a sound effect, synchronized over the network (if the network is active)
    /// </summary>
    public static Sound PlaySynchronized(string sound, float vol, float pitch, float pan, bool looped, bool louderForMe)
    {
        if (!enabled)
        {
            return new InvalidSound(sound, vol, pitch, pan, looped);
        }
        if (Network.isActive)
        {
            Send.Message(new NMSoundEffect(sound, louderForMe ? (vol * 0.7f) : vol, pitch));
        }
        return Play(sound, vol, pitch, pan, looped);
    }

    public static Sound Play(string sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        if (!enabled || skip)
        {
            return new InvalidSound(sound, vol, pitch, pan, looped);
        }
        Sound s = _playedThisFrame.FirstOrDefault((Sound x) => x.name == sound);
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
                return new Sound(_sounds.FirstOrDefault().Key, 0f, 0f, 0f, looped: false);
            }
        }
        return s;
    }

    public static Sound Play(int sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        string foundSound = null;
        if (_soundHashmap.TryGetKey(sound, out foundSound))
        {
            return Play(foundSound, vol, pitch, pan, looped);
        }
        return new Sound(_sounds.FirstOrDefault().Key, 0f, 0f, 0f, looped: false);
    }

    public static int SoundHash(string pSound)
    {
        int val = 0;
        _soundHashmap.TryGetValue(pSound, out val);
        return val;
    }

    public static bool HasSound(string sound)
    {
        if (NoSoundcard)
        {
            return false;
        }
        SoundEffect effect = null;
        if (!_sounds.TryGetValue(sound, out effect))
        {
            if (!sound.Contains(":"))
            {
                effect = Content.Load<SoundEffect>("Audio/SFX/" + sound);
            }
            if (effect == null && MonoMain.moddingEnabled && ModLoader.modsEnabled)
            {
                effect = Content.Load<SoundEffect>(sound);
            }
            RegisterSound(sound, effect);
        }
        return effect != null;
    }

    public static Sound Get(string sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        try
        {
            float voll = Math.Min(1f, Math.Max(0f, vol));
            if (HasSound(sound))
            {
                return new Sound(sound, voll, pitch, pan, looped);
            }
            return new InvalidSound(sound, voll, pitch, pan, looped);
        }
        catch (Exception)
        {
            return new InvalidSound(sound, 0f, pitch, pan, looped);
        }
    }

    public static MultiSound GetMultiSound(string single, string multi)
    {
        if (_multiSounds.ContainsKey(single + multi))
        {
            return _multiSounds[single + multi].GetInstance();
        }
        if (HasSound(single) && HasSound(multi))
        {
            MultiSoundUpdater newSound = new MultiSoundUpdater(single + multi, single, multi);
            _multiSounds[single + multi] = newSound;
            return newSound.GetInstance();
        }
        MultiSoundUpdater newSound2 = new MultiSoundUpdater("", "", "");
        _multiSounds[single + multi] = newSound2;
        return newSound2.GetInstance();
    }

    public static SoundEffectInstance GetInstance(string sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        float voll = Math.Min(1f, Math.Max(0f, vol));
        SoundEffectInstance soundEffectInstance = _sounds[sound].CreateInstance();
        soundEffectInstance.Volume = voll;
        soundEffectInstance.Pitch = pitch;
        soundEffectInstance.Pan = pan;
        soundEffectInstance.IsLooped = looped;
        return soundEffectInstance;
    }

    private static void SearchDir(string dir)
    {
        string[] files = Content.GetFiles(dir);
        for (int i = 0; i < files.Length; i++)
        {
            ProcessSoundEffect(files[i]);
        }
        files = Content.GetDirectories(dir);
        for (int i = 0; i < files.Length; i++)
        {
            SearchDir(files[i]);
        }
    }

    public static void StopAllSounds()
    {
        while (_soundPool.Count > 0)
        {
            _soundPool[0].Stop();
        }
    }

    public static void KillAllSounds()
    {
        while (_soundPool.Count > 0)
        {
            _soundPool[0].Stop();
        }
    }

    private static void ProcessSoundEffect(string path)
    {
        _numProcessed++;
        path = path.Replace('\\', '/');
        int start = path.IndexOf("Content/Audio/", 0);
        string fileName = path.Substring(start + 8);
        fileName = fileName.Substring(0, fileName.Length - 4);
        MonoMain.lazyLoadActions.Enqueue(delegate
        {
            SoundEffect soundEffect = Content.Load<SoundEffect>(fileName);
            if (soundEffect != null)
            {
                RegisterSound(fileName.Substring(fileName.IndexOf("/SFX/") + 5), soundEffect);
            }
        });
    }
}
