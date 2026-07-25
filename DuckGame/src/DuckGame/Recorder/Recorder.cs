namespace DuckGame;

public class Recorder
{
    public static Recording currentRecording { get; set; }

    #region Public Methods

    public static void LogVelocity(float velocity)
    {
        currentRecording?.LogVelocity(velocity);
    }

    public static void LogCoolness(int val)
    {
        currentRecording?.LogCoolness(val);
    }

    public static void LogDeath()
    {
        currentRecording?.LogDeath();
    }

    public static void LogAction(int num = 1)
    {
        currentRecording?.LogAction(num);
    }

    public static void LogBonus()
    {
        currentRecording?.LogBonus();
    }

    #endregion
}