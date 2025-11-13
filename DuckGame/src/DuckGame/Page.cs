namespace DuckGame;

public class Page : Level
{
	protected CategoryState _state;

	public static float camOffset;

	public virtual void DeactivateAll()
	{
	}

	public virtual void ActivateAll()
	{
	}

	public virtual void TransitionOutComplete()
	{
	}

	public override void Update()
	{
		Layer.HUD.camera.x = camOffset;
		if (_state == CategoryState.OpenPage)
		{
			DeactivateAll();
			camOffset = Lerp.FloatSmooth(camOffset, 360f, 0.1f);
			if (camOffset > 330f)
			{
				TransitionOutComplete();
			}
		}
		else if (_state == CategoryState.Idle)
		{
			camOffset = Lerp.FloatSmooth(camOffset, -40f, 0.1f);
			if (camOffset < 0f)
			{
				camOffset = 0f;
			}
			if (camOffset == 0f)
			{
				ActivateAll();
			}
			else
			{
				DeactivateAll();
			}
		}
	}
}
