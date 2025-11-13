using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace DuckGame;

public class OggPlayer
{
	private DynamicSoundEffectInstance _instance;

	private byte[] _buffer;

	private float[] _floatBuffer;

	private bool _iSaidStop;

	private float _replaygainModifier = 1f;

	private float _volume = 1f;

	private bool _shouldLoop;

	private bool _valid = true;

	private Thread _decoderThread;

	private bool _killDecodingThread;

	private bool _initialized;

	private VorbisReader _activeSong;

	private VorbisReader _decoderSong;

	private VorbisReader _streamerSong;

	private object _decoderDataMutex = new object();

	private object _decoderMutex = new object();

	private object _streamingMutex = new object();

	private float[] _decodedData;

	private int _samplesDecoded;

	private int _totalSamplesToDecode;

	private int _decodedSamplePosition;

	private const int kDecoderChunkSize = 176400;

	public SoundState state
	{
		get
		{
			if (_instance == null || !_valid)
			{
				return SoundState.Stopped;
			}
			if (!_iSaidStop)
			{
				return _instance.State;
			}
			return SoundState.Stopped;
		}
	}

	public float volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
			if (_instance != null)
			{
				lock (_instance)
				{
					ApplyVolume();
				}
			}
		}
	}

	public bool looped
	{
		get
		{
			return _shouldLoop;
		}
		set
		{
			_shouldLoop = value;
		}
	}

	public TimeSpan position
	{
		get
		{
			if (_activeSong != null && _valid && _totalSamplesToDecode > 0 && _decodedSamplePosition < _totalSamplesToDecode)
			{
				return new TimeSpan(0, 0, 0, 0, (int)((float)(_decodedSamplePosition / _totalSamplesToDecode) / 44100f) * 500);
			}
			return default(TimeSpan);
		}
	}

	private void ApplyVolume()
	{
		if (_valid && _instance != null && _instance.State == SoundState.Playing)
		{
			_instance.Volume = MathHelper.Clamp(_volume, 0f, 1f) * _replaygainModifier;
		}
	}

	public void Terminate()
	{
		if (_valid)
		{
			_instance.Dispose();
		}
		try
		{
			if (_decoderThread != null)
			{
				_decoderThread.Abort();
			}
		}
		catch (Exception)
		{
		}
		_killDecodingThread = true;
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			try
			{
				_instance = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
				_buffer = new byte[_instance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500.0))];
				_floatBuffer = new float[_buffer.Length / 2];
				_instance.BufferNeeded += Thread_Stream;
				_decoderThread = new Thread(Thread_Decoder);
				_decoderThread.CurrentCulture = CultureInfo.InvariantCulture;
				_decoderThread.Priority = ThreadPriority.BelowNormal;
				_decoderThread.IsBackground = true;
				_decoderThread.Start();
			}
			catch
			{
				DevConsole.Log(DCSection.General, "Music player failed to initialize.");
				_valid = false;
			}
			_initialized = true;
		}
	}

	private void Thread_Decoder_LoadNewSong()
	{
		if (_decoderSong == _activeSong)
		{
			return;
		}
		lock (_decoderDataMutex)
		{
			lock (_decoderMutex)
			{
				if (_decoderSong != _activeSong)
				{
					if (_decoderSong != null)
					{
						_decoderSong.Dispose();
					}
					_decoderSong = _activeSong;
					_streamerSong = _activeSong;
					_decodedSamplePosition = 0;
					_samplesDecoded = 0;
					if (_decoderSong != null)
					{
						_totalSamplesToDecode = (int)(_decoderSong.TotalSamples * 2);
						_decodedData = new float[_totalSamplesToDecode];
					}
				}
			}
		}
	}

	private bool Thread_Decoder_DecodeChunk()
	{
		lock (_decoderMutex)
		{
			if (_decoderSong != null && volume != 0f)
			{
				int samplesToRead = Math.Min(176400, _totalSamplesToDecode - _samplesDecoded);
				if (samplesToRead > 0)
				{
					_decoderSong.ReadSamples(_decodedData, _samplesDecoded, samplesToRead);
					_samplesDecoded += samplesToRead;
					return true;
				}
			}
		}
		return false;
	}

	private void Thread_Decoder()
	{
		while (!_killDecodingThread)
		{
			Thread_Decoder_LoadNewSong();
			if (!Thread_Decoder_DecodeChunk())
			{
				Thread.Sleep(200);
			}
			else
			{
				Thread.Sleep(20);
			}
		}
	}

	public void SetOgg(MemoryStream ogg)
	{
		if (!_valid)
		{
			return;
		}
		try
		{
			lock (_streamingMutex)
			{
				Stop();
				float gainValue = 0f;
				try
				{
					byte[] startBuffer = new byte[1000];
					ogg.Position = 0L;
					ogg.Read(startBuffer, 0, 1000);
					string meta = Encoding.ASCII.GetString(startBuffer);
					int replayIDX = meta.IndexOf("replaygain_track_gain");
					if (replayIDX >= 0)
					{
						for (; meta[replayIDX] != '=' && replayIDX < meta.Length; replayIDX++)
						{
						}
						replayIDX++;
						string val = "";
						for (; meta[replayIDX] != 'd' && replayIDX < meta.Length; replayIDX++)
						{
							val += meta[replayIDX];
						}
						gainValue = Convert.ToSingle(val);
					}
				}
				catch (Exception)
				{
					gainValue = 0f;
				}
				_activeSong = new VorbisReader(ogg, false);
				float vol = 100f * (float)Math.Pow(10.0, gainValue / 20f);
				_replaygainModifier = Math.Max(0f, Math.Min(1f, vol / 100f * 1.9f));
				Thread_Decoder_LoadNewSong();
				Thread_Decoder_DecodeChunk();
			}
		}
		catch (Exception ex2)
		{
			DevConsole.Log(DCSection.General, "OggPlayer.SetOgg failed with exception:");
			DevConsole.Log(DCSection.General, ex2.Message);
			_activeSong = null;
		}
	}

	public void Play()
	{
		if (_instance == null)
		{
			return;
		}
		lock (_instance)
		{
			if (_valid)
			{
				_instance.Play();
				ApplyVolume();
				_iSaidStop = false;
			}
		}
	}

	public void Pause()
	{
		if (_instance == null)
		{
			return;
		}
		lock (_instance)
		{
			if (_valid)
			{
				_instance.Pause();
			}
		}
	}

	public void Resume()
	{
		if (_instance == null)
		{
			return;
		}
		lock (_instance)
		{
			if (_valid)
			{
				_instance.Resume();
				ApplyVolume();
				_iSaidStop = false;
			}
		}
	}

	public void Stop()
	{
		if (_instance == null)
		{
			return;
		}
		lock (_instance)
		{
			if (_valid)
			{
				_instance.Stop();
				_iSaidStop = true;
			}
		}
	}

	public void Update()
	{
	}

	private void Thread_Stream(object sender, EventArgs e)
	{
		lock (_streamingMutex)
		{
			int samplesRead = 0;
			lock (_decoderDataMutex)
			{
				Thread_Decoder_LoadNewSong();
				if (volume == 0f || !_valid || _decoderSong == null)
				{
					for (int i = 0; i < _buffer.Count(); i++)
					{
						_buffer[i] = 0;
					}
					_instance.SubmitBuffer(_buffer, 0, _buffer.Count());
					return;
				}
				while (_samplesDecoded - _decodedSamplePosition < _floatBuffer.Length && Thread_Decoder_DecodeChunk())
				{
				}
				samplesRead = Math.Min(_totalSamplesToDecode - _decodedSamplePosition, _floatBuffer.Length);
				if (samplesRead > 0)
				{
					Array.Copy(_decodedData, _decodedSamplePosition, _floatBuffer, 0, samplesRead);
					_decodedSamplePosition += samplesRead;
				}
				if (samplesRead == 0)
				{
					if (_shouldLoop)
					{
						_decodedSamplePosition = 0;
						Array.Copy(_decodedData, _decodedSamplePosition, _floatBuffer, 0, _floatBuffer.Length);
						_decodedSamplePosition += _floatBuffer.Length;
						samplesRead = _floatBuffer.Length;
					}
					else
					{
						for (int j = 0; j < _floatBuffer.Length / 2; j++)
						{
							_floatBuffer[j * 2] = 0f;
							_floatBuffer[j * 2 + 1] = 0f;
						}
						samplesRead = _floatBuffer.Length;
						Stop();
					}
				}
			}
			if (samplesRead > 0)
			{
				for (int k = 0; k < samplesRead; k++)
				{
					short sValue = (short)Math.Max(Math.Min(32767f * _floatBuffer[k], 32767f), -32768f);
					_buffer[k * 2] = (byte)(sValue & 0xFF);
					_buffer[k * 2 + 1] = (byte)((sValue >> 8) & 0xFF);
				}
				_instance.SubmitBuffer(_buffer, 0, samplesRead * 2);
			}
		}
	}
}
