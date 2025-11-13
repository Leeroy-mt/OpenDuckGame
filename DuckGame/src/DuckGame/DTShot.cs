namespace DuckGame;

public class DTShot : DestroyType
{
	private Bullet _bullet;

	public Bullet bullet => _bullet;

	public Thing bulletOwner
	{
		get
		{
			if (_bullet == null)
			{
				return null;
			}
			return _bullet.owner;
		}
	}

	public Thing bulletFiredFrom
	{
		get
		{
			if (_bullet == null)
			{
				return null;
			}
			return _bullet.firedFrom;
		}
	}

	public DTShot(Bullet b)
		: base(b)
	{
		_bullet = b;
	}
}
