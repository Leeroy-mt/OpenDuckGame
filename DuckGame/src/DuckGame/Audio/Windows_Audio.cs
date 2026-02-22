using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DuckGame;

public class Windows_Audio
{
    private class NotificationClientImplementation : IMMNotificationClient
    {
        private Windows_Audio _owner;

        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
        {
            _owner.LoseDevice();
        }

        public void OnDeviceAdded(string deviceId)
        {
        }

        public void OnDeviceRemoved(string deviceId)
        {
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
        }

        public NotificationClientImplementation(Windows_Audio pOwner)
        {
            _owner = pOwner;
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
            }
        }

        public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
        {
        }
    }

    private static IWavePlayer _output;

    private static MixingSampleProvider _mixer;

    private static AudioMode _mode = AudioMode.Wasapi;

    public static bool initialized = false;

    private static int _losingDevice = 0;

    private static AudioMode _forceMode = AudioMode.None;

    private bool _recreateNonExclusive;

    private bool _recreateAlternateAudio;

    private static ISampleProvider _currentMusic;

    private MMDeviceEnumerator deviceEnum;

    private NotificationClientImplementation notificationClient;

    private IMMNotificationClient notifyClient;

    public static AudioMode forceMode
    {
        get
        {
            return _forceMode;
        }
        set
        {
            _forceMode = value;
            if (SFX._audio != null)
            {
                ResetDevice();
            }
        }
    }

    public void Platform_Initialize()
    {
        ResetDevice();
    }

    public void Update()
    {
        if (initialized && SFX._audio != null && _losingDevice > 0 && (_output == null || _output.PlaybackState == PlaybackState.Stopped))
        {
            if (_output != null)
            {
                _output.Dispose();
                _output = null;
            }
            _losingDevice--;
            if (_losingDevice == 0)
            {
                RecreateDevice();
            }
        }
    }

    public void LoseDevice()
    {
        if (initialized && _output != null)
        {
            _output.Stop();
            _losingDevice = 60;
        }
    }

    public static void ResetDevice()
    {
        if (MonoMain.audioModeOverride != AudioMode.None)
        {
            _mode = MonoMain.audioModeOverride;
        }
        else
        {
            _mode = (AudioMode)Options.Data.audioMode;
        }
        if (_output != null)
        {
            SFX._audio.LoseDevice();
        }
        else
        {
            SFX._audio.RecreateDevice();
        }
    }

    private void RecreateDevice()
    {
        try
        {
            if (_output != null)
            {
                _output.Stop();
                _output.Dispose();
                _output = null;
            }
            if (_forceMode != AudioMode.None && !_recreateAlternateAudio && !_recreateNonExclusive)
            {
                _mode = _forceMode;
            }
            if (_mode == AudioMode.DirectSound)
            {
                _output = new DirectSoundOut();
            }
            else if (_mode == AudioMode.Wave || _recreateAlternateAudio)
            {
                _output = new WaveOutEvent
                {
                    DesiredLatency = 50,
                    NumberOfBuffers = 10
                };
            }
            else
            {
                if (notificationClient == null)
                {
                    notificationClient = new NotificationClientImplementation(this);
                    notifyClient = notificationClient;
                    if (deviceEnum == null)
                    {
                        deviceEnum = new MMDeviceEnumerator();
                    }
                    deviceEnum.RegisterEndpointNotificationCallback(notifyClient);
                }
                new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                _output = new WasapiOut((!_recreateNonExclusive && Options.Data.audioExclusiveMode) ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, 20);
            }
            if (_mixer == null)
            {
                _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                _mixer.ReadFully = true;
            }
            _output.Init(_mixer);
        }
        catch (Exception ex)
        {
            if (_recreateAlternateAudio)
            {
                initialized = false;
                _output = null;
                _mixer = null;
                return;
            }
            if (!_recreateNonExclusive)
            {
                _recreateNonExclusive = true;
                DevConsole.Log(DCSection.General, "|DGRED|Failed to create audio device, reattempting creation in non-exclusive mode:");
                DevConsole.Log(DCSection.General, ex.Message);
                RecreateDevice();
                _recreateNonExclusive = false;
                return;
            }
            _recreateAlternateAudio = true;
            DevConsole.Log(DCSection.General, "|DGRED|Failed to create audio device, reattempting creation in alternate mode:");
            DevConsole.Log(DCSection.General, ex.Message);
            RecreateDevice();
            _recreateNonExclusive = false;
            _recreateAlternateAudio = false;
        }
        if (_output != null)
        {
            initialized = true;
            _output.Play();
        }
    }

    public static void AddSound(ISampleProvider pSound, bool pIsMusic)
    {
        if (!initialized || pSound == null || _mixer.MixerInputs == null || _mixer == null || _output == null)
        {
            return;
        }
        if (_mixer.MixerInputs.Count() > 32)
        {
            lock (_mixer.MixerInputs)
            {
                ISampleProvider first = _mixer.MixerInputs.Where((ISampleProvider x) => x != _currentMusic).First();
                _mixer.RemoveMixerInput(first);
            }
        }
        _mixer.AddMixerInput(pSound);
        if (_losingDevice <= 0)
        {
            _output.Play();
        }
        if (pIsMusic)
        {
            _currentMusic = pSound;
        }
    }

    public static void RemoveSound(ISampleProvider pSound)
    {
        if (initialized && _mixer != null)
        {
            _mixer.RemoveMixerInput(pSound);
        }
    }

    public void Dispose()
    {
        if (initialized && notificationClient != null)
        {
            UnRegisterEndpointNotificationCallback(notificationClient);
            _output.Dispose();
        }
    }

    /// <summary>
    /// Registers a call back for Device Events
    /// </summary>
    /// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface</param>
    /// <returns></returns>
    public int RegisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
    {
        if (deviceEnum == null)
        {
            deviceEnum = new MMDeviceEnumerator();
        }
        return deviceEnum.RegisterEndpointNotificationCallback(client);
    }

    /// <summary>
    /// UnRegisters a call back for Device Events
    /// </summary>
    /// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface </param>
    /// <returns></returns>
    public int UnRegisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
    {
        if (deviceEnum == null)
        {
            deviceEnum = new MMDeviceEnumerator();
        }
        return deviceEnum.UnregisterEndpointNotificationCallback(client);
    }
}
