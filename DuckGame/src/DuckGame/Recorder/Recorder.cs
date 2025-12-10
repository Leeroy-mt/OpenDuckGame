namespace DuckGame;

public class Recorder
{
    public static Recording _currentRecording;

    public static FileRecording _globalRecording;

    public static Recording currentRecording
    {
        get
        {
            return _currentRecording;
        }
        set
        {
            _currentRecording = value;
        }
    }

    public static FileRecording globalRecording
    {
        get
        {
            return _globalRecording;
        }
        set
        {
            _globalRecording = value;
        }
    }

    public static void LogVelocity(float velocity)
    {
        if (_currentRecording != null)
        {
            _currentRecording.LogVelocity(velocity);
        }
    }

    public static void LogCoolness(int val)
    {
        if (_currentRecording != null)
        {
            _currentRecording.LogCoolness(val);
        }
    }

    public static void LogDeath()
    {
        if (_currentRecording != null)
        {
            _currentRecording.LogDeath();
        }
    }

    public static void LogAction(int num = 1)
    {
        if (_currentRecording != null)
        {
            _currentRecording.LogAction(num);
        }
    }

    public static void LogBonus()
    {
        if (_currentRecording != null)
        {
            _currentRecording.LogBonus();
        }
    }
}
