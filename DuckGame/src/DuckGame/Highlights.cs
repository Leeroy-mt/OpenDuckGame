using System.Collections.Generic;

namespace DuckGame;

public class Highlights
{
	private static List<Recording> _recordings = new List<Recording>();

	public static float highlightRatingMultiplier = 1f;

	public static void Initialize()
	{
		MonoMain.loadMessage = "Loading Highlights";
		for (int i = 0; i < 6; i++)
		{
			_recordings.Add(new Recording());
		}
	}

	public static List<Recording> GetHighlights()
	{
		List<Recording> highlights = new List<Recording>();
		foreach (Recording r in _recordings)
		{
			if (Recorder.currentRecording != r && highlights.Count < 6)
			{
				r.Rewind();
				highlights.Add(r);
			}
		}
		return highlights;
	}

	public static void ClearHighlights()
	{
		foreach (Recording recording in _recordings)
		{
			recording.Reset();
		}
	}

	public static void FinishRound()
	{
		if (Network.isActive)
		{
			return;
		}
		if (_recordings.Count == 0)
		{
			Initialize();
		}
		highlightRatingMultiplier = 1f;
		Recording tape = Recorder.currentRecording;
		Recorder.currentRecording = null;
		if (tape != null)
		{
			float score = 0f;
			float lastRoundLength = Stats.lastMatchLength;
			tape.Rewind();
			while (!tape.StepForward())
			{
				score += tape.GetFrameTotal();
			}
			if (lastRoundLength < 5f)
			{
				score *= 1.3f;
			}
			if (lastRoundLength < 7f)
			{
				score *= 1.2f;
			}
			if (lastRoundLength < 10f)
			{
				score *= 1.1f;
			}
			tape.highlightScore = score;
		}
	}

	public static void StartRound()
	{
		if (Network.isActive)
		{
			return;
		}
		if (_recordings.Count == 0)
		{
			Initialize();
		}
		Recording worst = _recordings[0];
		foreach (Recording r in _recordings)
		{
			if (r.startFrame == r.endFrame)
			{
				worst = r;
				break;
			}
			if (r.highlightScore < worst.highlightScore)
			{
				worst = r;
			}
			if (r.highlightScore == worst.highlightScore && Rando.Float(1f) >= 0.5f)
			{
				worst = r;
			}
		}
		worst.Reset();
		Recorder.currentRecording = worst;
	}
}
