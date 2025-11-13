namespace DuckGame;

public class ContinueCountdown : Thing
{
	public StateBinding _timerBinding = new StateBinding("timer");

	public float timer = 5f;

	public ContinueCountdown(float time = 5f)
	{
		timer = time;
	}

	public void UpdateTimer()
	{
		if (base.isServerForObject)
		{
			timer -= Maths.IncFrameTimer();
		}
		if (timer < 0f)
		{
			timer = 0f;
		}
	}
}
