using System.Collections.Generic;

namespace DuckGame;

public class NetSoundEffect
{
    public delegate void Function();

    public static Dictionary<string, NetSoundEffect> _sfxDictionary = new Dictionary<string, NetSoundEffect>();

    public static Dictionary<ushort, NetSoundEffect> _sfxIndexDictionary = new Dictionary<ushort, NetSoundEffect>();

    private static ushort kCurrentSfxIndex = 0;

    private static List<NetSoundEffect> _soundsPlayedThisFrame = new List<NetSoundEffect>();

    public ushort sfxIndex;

    public Function function;

    public FieldBinding pitchBinding;

    public FieldBinding appendBinding;

    private List<string> _sounds = new List<string>();

    private List<string> _rareSounds = new List<string>();

    public float volume = 1f;

    public float pitch;

    public float pitchVariationHigh;

    public float pitchVariationLow;

    private int _localIndex;

    private int _index;

    public int index
    {
        get
        {
            return _index;
        }
        set
        {
            _index = value;
            if (_localIndex != value)
            {
                _localIndex = value;
                PlaySound();
            }
        }
    }

    public static void Register(string pName, NetSoundEffect pEffect)
    {
        pEffect.sfxIndex = kCurrentSfxIndex;
        _sfxDictionary[pName] = pEffect;
        _sfxIndexDictionary[kCurrentSfxIndex] = pEffect;
        kCurrentSfxIndex++;
    }

    public static void Initialize()
    {
        Register("duckJump", new NetSoundEffect("jump")
        {
            volume = 0.5f
        });
        Register("duckDisarm", new NetSoundEffect("disarm")
        {
            volume = 0.3f
        });
        Register("duckTinyMotion", new NetSoundEffect("tinyMotion"));
        Register("duckSwear", new NetSoundEffect(new List<string> { "cutOffQuack", "cutOffQuack2" }, new List<string> { "quackBleep" })
        {
            pitchVariationLow = -0.05f,
            pitchVariationHigh = 0.05f
        });
        Register("duckScream", new NetSoundEffect("quackYell01", "quackYell02", "quackYell03"));
        Register("itemBoxHit", new NetSoundEffect("hitBox")
        {
            volume = 1f
        });
        Register("bananaSplat", new NetSoundEffect("smallSplat")
        {
            pitchVariationLow = -0.2f,
            pitchVariationHigh = 0.2f
        });
        Register("bananaEat", new NetSoundEffect("smallSplat")
        {
            pitchVariationLow = -0.6f,
            pitchVariationHigh = 0.6f
        });
        Register("bananaSlip", new NetSoundEffect("slip")
        {
            pitchVariationLow = -0.2f,
            pitchVariationHigh = 0.2f
        });
        Register("mineDoubleBeep", new NetSoundEffect("doubleBeep"));
        Register("minePullPin", new NetSoundEffect("pullPin"));
        Register("oldPistolClick", new NetSoundEffect("click")
        {
            volume = 1f,
            pitch = 0.5f
        });
        Register("oldPistolSwipe", new NetSoundEffect("swipe")
        {
            volume = 0.6f,
            pitch = -0.3f
        });
        Register("oldPistolSwipe2", new NetSoundEffect("swipe")
        {
            volume = 0.7f
        });
        Register("oldPistolLoad", new NetSoundEffect("shotgunLoad"));
        Register("pelletGunClick", new NetSoundEffect("click")
        {
            volume = 1f,
            pitch = 0.5f
        });
        Register("pelletGunSwipe", new NetSoundEffect("swipe")
        {
            volume = 0.4f,
            pitch = 0.3f
        });
        Register("pelletGunSwipe2", new NetSoundEffect("swipe")
        {
            volume = 0.5f,
            pitch = 0.4f
        });
        Register("pelletGunLoad", new NetSoundEffect("loadLow")
        {
            volume = 0.7f,
            pitchVariationLow = -0.05f,
            pitchVariationHigh = 0.05f
        });
        Register("sniperLoad", new NetSoundEffect("loadSniper"));
        Register("flowerHappyQuack", new NetSoundEffect("happyQuack01")
        {
            pitchVariationLow = -0.1f,
            pitchVariationHigh = 0.1f
        });
        Register("equipmentTing", new NetSoundEffect("ting2"));
    }

    public static void Update()
    {
        if (_soundsPlayedThisFrame.Count > 0)
        {
            Send.Message(new NMNetSoundEvents(_soundsPlayedThisFrame));
            _soundsPlayedThisFrame.Clear();
        }
    }

    public static NetSoundEffect Get(string pSound)
    {
        NetSoundEffect se = null;
        _sfxDictionary.TryGetValue(pSound, out se);
        return se;
    }

    public static NetSoundEffect Get(ushort pSound)
    {
        NetSoundEffect se = null;
        _sfxIndexDictionary.TryGetValue(pSound, out se);
        return se;
    }

    public static void Play(string pSound)
    {
        NetSoundEffect se = null;
        if (_sfxDictionary.TryGetValue(pSound, out se))
        {
            PlayAndSynchronize(se);
        }
    }

    public static void Play(string pSound, float pPitchOffset)
    {
        NetSoundEffect se = null;
        if (_sfxDictionary.TryGetValue(pSound, out se))
        {
            PlayAndSynchronize(se, pPitchOffset);
        }
    }

    public static void Play(ushort pSound)
    {
        NetSoundEffect se = null;
        if (_sfxIndexDictionary.TryGetValue(pSound, out se))
        {
            PlayAndSynchronize(se);
        }
    }

    private static void PlayAndSynchronize(NetSoundEffect pSound, float pPitchOffset = 0f)
    {
        pSound.Play(1f, pPitchOffset);
        _soundsPlayedThisFrame.Add(pSound);
    }

    public NetSoundEffect()
    {
    }

    public NetSoundEffect(params string[] sounds)
    {
        _sounds = new List<string>(sounds);
    }

    public NetSoundEffect(List<string> sounds, List<string> rareSounds)
    {
        _sounds = sounds;
        _rareSounds = rareSounds;
    }

    public void Play(float vol = 1f, float pit = 0f)
    {
        PlaySound(vol, pit);
        _index++;
        _localIndex = _index;
    }

    private void PlaySound(float vol = 1f, float pit = 0f)
    {
        if (function != null)
        {
            function();
        }
        vol *= volume;
        pit += pitch;
        pit += Rando.Float(pitchVariationLow, pitchVariationHigh);
        if (pit < -1f)
        {
            pit = -1f;
        }
        if (pit > 1f)
        {
            pit = 1f;
        }
        if (_sounds.Count > 0)
        {
            if (pitchBinding != null)
            {
                pit = (float)(int)(byte)pitchBinding.value / 255f;
            }
            string append = "";
            if (appendBinding != null)
            {
                append = ((byte)appendBinding.value).ToString();
            }
            if (_rareSounds.Count > 0 && Rando.Float(1f) > 0.9f)
            {
                SFX.Play(_rareSounds[Rando.Int(_rareSounds.Count - 1)] + append, vol, pit);
            }
            else
            {
                SFX.Play(_sounds[Rando.Int(_sounds.Count - 1)] + append, vol, pit);
            }
        }
    }
}
