using System;

namespace DuckGame;

public class Stats
{
    private static float _averageRoundTime;

    private static float _totalRoundTime;

    private static int _numberOfRounds;

    public static float averageRoundTime => _totalRoundTime / (float)numberOfRounds;

    public static float totalRoundTime => _averageRoundTime;

    public static int numberOfRounds => _numberOfRounds;

    public static int lastMatchLength
    {
        get
        {
            DateTime endTime = DateTime.Now;
            for (int i = Event.events.Count - 1; i > 0; i--)
            {
                Event e = Event.events[i];
                if (e is RoundEndEvent)
                {
                    endTime = e.timestamp;
                }
                else if (e is RoundStartEvent)
                {
                    return (int)(endTime - e.timestamp).TotalSeconds;
                }
            }
            return 99;
        }
    }

    public static void CalculateStats()
    {
        DateTime startTime = DateTime.Now;
        _totalRoundTime = 0f;
        _numberOfRounds = 0;
        foreach (Event e in Event.events)
        {
            if (e is RoundStartEvent)
            {
                startTime = e.timestamp;
            }
            else if (e is RoundEndEvent)
            {
                _totalRoundTime += (int)(e.timestamp - startTime).TotalSeconds;
                _numberOfRounds++;
            }
        }
    }
}
