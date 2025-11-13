using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;

namespace DuckGame;

public class Speech
{
	public object _speech;

	public object speech => _speech;

	public void Initialize()
	{
		if (_speech == null)
		{
			_speech = new SpeechSynthesizer();
			(_speech as SpeechSynthesizer).SetOutputToDefaultAudioDevice();
			ApplyTTSSettings();
		}
	}

	public void Say(string pString)
	{
		if (_speech != null)
		{
			(speech as SpeechSynthesizer).SpeakAsync(pString);
		}
	}

	public void StopSaying()
	{
		if (_speech != null)
		{
			(speech as SpeechSynthesizer).SpeakAsyncCancelAll();
		}
	}

	public void SetSayVoice(string pName)
	{
		if (_speech == null)
		{
			return;
		}
		try
		{
			(speech as SpeechSynthesizer).SelectVoice(pName);
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.General, "|DGRED|SFX.SetSayVoice failed:" + ex.Message);
		}
	}

	public List<string> GetSayVoices()
	{
		if (_speech == null)
		{
			return new List<string>();
		}
		List<string> names = new List<string>();
		try
		{
			foreach (InstalledVoice s in (speech as SpeechSynthesizer).GetInstalledVoices().ToList())
			{
				names.Add(s.VoiceInfo.Name);
			}
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.General, "|DGRED|SFX.GetSayVoices failed:" + ex.Message);
		}
		return names;
	}

	public void ApplyTTSSettings()
	{
		if (_speech == null)
		{
			return;
		}
		try
		{
			if (!Program.isLinux && Options.Data.textToSpeech)
			{
				(speech as SpeechSynthesizer).SpeakAsyncCancelAll();
				List<InstalledVoice> voices = (speech as SpeechSynthesizer).GetInstalledVoices().ToList();
				if (Options.Data.textToSpeechVoice >= 0 && Options.Data.textToSpeechVoice < voices.Count)
				{
					(speech as SpeechSynthesizer).SelectVoice(voices[Options.Data.textToSpeechVoice].VoiceInfo.Name);
				}
				(speech as SpeechSynthesizer).Volume = Maths.Clamp((int)(Options.Data.textToSpeechVolume * 100f), 0, 100);
				(speech as SpeechSynthesizer).Rate = Maths.Clamp((int)Math.Round((Options.Data.textToSpeechRate - 0.5f) * 20f), -10, 10);
			}
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.General, "|DGRED|SFX.ApplyTTSSettings failed:" + ex.Message);
		}
	}

	public void SetOutputToDefaultAudioDevice()
	{
		(_speech as SpeechSynthesizer).SetOutputToDefaultAudioDevice();
	}
}
