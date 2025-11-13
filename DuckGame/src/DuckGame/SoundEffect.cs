using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Vorbis;

namespace DuckGame;

public class SoundEffect
{
	public string file;

	public bool streaming;

	public static float[] _songBuffer;

	private float[] _waveBuffer;

	public int dataSize;

	public WaveStream _decode;

	private Thread _decoderThread;

	private ISampleProvider _decoderReader;

	private int _decodedSamples;

	private int _totalSamples;

	private int kDecoderChunkSize = 22050;

	private static int kDecoderIndex = 0;

	private static object kDecoderHandle = new object();

	private int _decoderIndex;

	public float replaygainModifier = 1f;

	private Stream _stream;

	public static float DistanceScale { get; set; }

	public static float DopplerScale { get; set; }

	public static float MasterVolume { get; set; }

	public static float SpeedOfSound { get; set; }

	public TimeSpan Duration { get; }

	public bool IsDisposed { get; }

	public string Name { get; set; }

	public float[] data => _waveBuffer;

	public WaveFormat format { get; private set; }

	public int decodedSamples => _decodedSamples;

	public int totalSamples => _totalSamples;

	public static SoundEffect FromStream(Stream stream)
	{
		return FromStream(stream, "wav");
	}

	public static SoundEffect FromStream(Stream stream, string extension)
	{
		SoundEffect e = new SoundEffect();
		if (e.Platform_Construct(stream, extension))
		{
			return e;
		}
		DevConsole.Log(DCSection.General, "|DGRED|SoundEffect.FromStream Failed!");
		return null;
	}

	public static SoundEffect CreateStreaming(string pPath)
	{
		if (File.Exists(pPath))
		{
			return new SoundEffect
			{
				streaming = true,
				file = pPath
			};
		}
		DevConsole.Log(DCSection.General, "|DGRED|SoundEffect.CreateStreaming Failed (file not found)!");
		return null;
	}

	public SoundEffect(string pPath)
	{
		file = pPath;
		Platform_Construct(pPath);
	}

	public SoundEffect()
	{
	}

	public SoundEffectInstance CreateInstance()
	{
		return new SoundEffectInstance(this);
	}

	public int Decode(float[] pBuffer, int pOffset, int pCount)
	{
		return _decoderReader.Read(pBuffer, pOffset, pCount);
	}

	public void Rewind()
	{
		_decode.Seek(0L, SeekOrigin.Begin);
	}

	public bool Decoder_DecodeChunk()
	{
		if (_decoderReader == null)
		{
			return false;
		}
		lock (_decoderReader)
		{
			try
			{
				if (_decodedSamples + kDecoderChunkSize > _songBuffer.Length)
				{
					float[] newBuffer = new float[_songBuffer.Length * 2];
					Array.Copy(_songBuffer, newBuffer, _songBuffer.Length);
					_songBuffer = newBuffer;
				}
				int samplesRead = _decoderReader.Read(_songBuffer, _decodedSamples, kDecoderChunkSize);
				if (samplesRead > 0)
				{
					_decodedSamples += samplesRead;
				}
				else
				{
					dataSize = _decodedSamples;
					_decode.Dispose();
					_decode = null;
					_decoderReader = null;
				}
				return samplesRead > 0;
			}
			catch (Exception)
			{
			}
			return false;
		}
	}

	private void Thread_Decoder()
	{
		while (true)
		{
			lock (kDecoderHandle)
			{
				if (!Decoder_DecodeChunk() || _decoderIndex != kDecoderIndex)
				{
					break;
				}
			}
			Thread.Sleep(10);
		}
	}

	public void Dispose()
	{
		if (_decoderReader == null)
		{
			return;
		}
		lock (_decoderReader)
		{
			_decoderReader = null;
			_decode.Dispose();
			if (_stream != null)
			{
				_stream.Close();
				_stream.Dispose();
			}
		}
	}

	public bool Platform_Construct(Stream pStream, string pExtension)
	{
		pExtension = pExtension.Replace(".", "");
		_stream = pStream;
		WaveStream reader = null;
		switch (pExtension)
		{
		case "wav":
			reader = new WaveFileReader(pStream);
			if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm && reader.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
			{
				reader = WaveFormatConversionStream.CreatePcmStream(reader);
				reader = new BlockAlignReductionStream(reader);
			}
			break;
		case "mp3":
			reader = new Mp3FileReader(pStream);
			break;
		case "aiff":
			reader = new AiffFileReader(pStream);
			break;
		case "ogg":
		{
			reader = new VorbisWaveReader(pStream);
			float gainValue = 0f;
			try
			{
				byte[] startBuffer = new byte[1000];
				pStream.Position = 0L;
				pStream.Read(startBuffer, 0, 1000);
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
				pStream.Position = 0L;
			}
			catch (Exception)
			{
				gainValue = 0f;
			}
			float vol = 100f * (float)Math.Pow(10.0, gainValue / 20f);
			replaygainModifier = Math.Max(0f, Math.Min(1f, vol / 100f * 1.9f));
			break;
		}
		}
		if (reader != null)
		{
			PrepareReader(reader, pStream);
			return true;
		}
		return false;
	}

	private void PrepareReader(WaveStream reader, Stream pStream)
	{
		_decode = reader;
		_totalSamples = (int)(_decode.Length * 8 / _decode.WaveFormat.BitsPerSample);
		_decoderReader = new SampleChannel(_decode);
		if (_decoderReader.WaveFormat.SampleRate != 44100)
		{
			_decoderReader = new WdlResamplingSampleProvider(_decoderReader, 44100);
			_totalSamples *= _decoderReader.WaveFormat.BitsPerSample / _decode.WaveFormat.BitsPerSample;
		}
		format = _decoderReader.WaveFormat;
		dataSize = _totalSamples;
		if (reader is WaveFileReader)
		{
			if (pStream is FileStream)
			{
				streaming = true;
				return;
			}
			_waveBuffer = new float[_totalSamples];
			_decoderReader.Read(_waveBuffer, 0, _totalSamples);
			_decode.Dispose();
			_decoderReader = null;
			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}
			int allocAmount = _totalSamples * 4 / 1000;
			ContentPack.kTotalKilobytesAllocated += allocAmount;
			if (ContentPack.currentPreloadPack != null)
			{
				ContentPack.currentPreloadPack.kilobytesPreAllocated += allocAmount;
			}
		}
		else
		{
			if (!MonoMain.enableThreadedLoading)
			{
				return;
			}
			lock (kDecoderHandle)
			{
				if (_songBuffer == null)
				{
					_songBuffer = new float[_totalSamples];
				}
				_waveBuffer = _songBuffer;
				kDecoderIndex++;
				_decoderIndex = kDecoderIndex;
				Task.Factory.StartNew(Thread_Decoder);
			}
		}
	}

	public void Platform_Construct(string pPath)
	{
		byte[] data = File.ReadAllBytes(pPath);
		if (data == null)
		{
			AudioFileReader reader = new AudioFileReader(pPath);
			PrepareReader(reader, null);
		}
		else if (!Platform_Construct(new MemoryStream(data), Path.GetExtension(pPath)))
		{
			DevConsole.Log(DCSection.General, "Tried to read invalid sound format (" + pPath + ")");
		}
	}

	public float[] Platform_GetData()
	{
		return data;
	}
}
